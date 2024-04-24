using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using NetStudio.Common.IndusCom;
using NetStudio.Common.Manager;
using NetStudio.Common.Security;
using NetStudio.Omron.Models;

namespace NetStudio.Omron.Fins;

public class FinsProtocol : FinsBuilder
{
	private INetworkAdapter adapter;

	private Validate validate;

	public FinsProtocol(INetworkAdapter adapter)
	{
		this.adapter = adapter;
		validate = new Validate();
	}

	 

	public bool Connect()
	{
		
		bool flag = adapter.Connect();
		if (adapter is EthernetAdapter && ((EthernetAdapter)adapter).ProtocolType == ProtocolType.Tcp)
		{
			IPSResult iPSResult = OnInitializeTcp();
			if (iPSResult != null)
			{
				if (flag)
				{
					return iPSResult.Status == CommStatus.Success;
				}
				return false;
			}
		}
		return flag;
	}

	public bool Disconnect()
	{
		if (adapter != null)
		{
			return adapter.Disconnect();
		}
		return false;
	}

	public bool Reconnect()
	{
		
		adapter.Disconnect();
		bool flag = adapter.Connect();
		if (adapter is EthernetAdapter && ((EthernetAdapter)adapter).ProtocolType == ProtocolType.Tcp)
		{
			IPSResult iPSResult = OnInitializeTcp();
			if (iPSResult != null)
			{
				if (flag)
				{
					return iPSResult.Status == CommStatus.Success;
				}
				return false;
			}
		}
		return flag;
	}

	public async Task<bool> ConnectAsync()
	{
		
		bool flag = await adapter.ConnectAsync();
		if (adapter is EthernetAdapter && ((EthernetAdapter)adapter).ProtocolType == ProtocolType.Tcp)
		{
			IPSResult iPSResult = OnInitializeTcp();
			if (iPSResult != null)
			{
				return flag && iPSResult.Status == CommStatus.Success;
			}
		}
		return flag;
	}

	public async Task<bool> ReconnectAsync()
	{
		
		adapter.Disconnect();
		bool flag = await adapter.ConnectAsync();
		if (adapter is EthernetAdapter && ((EthernetAdapter)adapter).ProtocolType == ProtocolType.Tcp)
		{
			IPSResult iPSResult = OnInitializeTcp();
			if (iPSResult != null)
			{
				return flag && iPSResult.Status == CommStatus.Success;
			}
		}
		return flag;
	}

	public async Task<bool> DisconnectAsync()
	{
		if (adapter == null)
		{
			return false;
		}
		return await adapter.DisconnectAsync();
	}

	public IPSResult OnInitializeTcp()
	{
		IPSResult iPSResult = new IPSResult
		{
			Status = CommStatus.Error,
			Message = "Failed to connect device."
		};
		try
		{
			int num = 3;
			int num2 = 24;
			int num3 = 0;
			int num4 = 0;
			byte[] array = Array.Empty<byte>();
			byte[] array2 = OnInitializeTcpMsg(new byte[4]);
			lock (adapter)
			{
				do
				{
					try
					{
						num4++;
						num3 = adapter.Write(array2);
						array = adapter.Read(num2);
					}
					catch (Exception ex)
					{
						if (num4 >= num)
						{
							iPSResult.Status = CommStatus.Timeout;
							iPSResult.Message = ex.Message;
							return iPSResult;
						}
					}
				}
				while ((num3 != array2.Length || array.Length < num2) && num4 <= num);
			}
			if (array.Length < num2)
			{
				iPSResult.Status = CommStatus.Error;
				iPSResult.Message = "The communication frame is not in the correct format.";
				return iPSResult;
			}
			if (array[19] == 0 || array[19] == byte.MaxValue)
			{
				iPSResult.Status = CommStatus.Error;
				iPSResult.Message = "Message contained an Invalid Local Node ID.";
				return iPSResult;
			}
			base.SA1 = array[19];
			if (array[23] == 0 || array[23] == byte.MaxValue)
			{
				iPSResult.Status = CommStatus.Error;
				iPSResult.Message = "Message contained an Invalid Remote Node ID.";
				return iPSResult;
			}
			base.DA1 = array[23];
			iPSResult.Status = CommStatus.Success;
			iPSResult.Message = "Read request successfully.";
		}
		catch (Exception ex2)
		{
			iPSResult.Status = CommStatus.Error;
			iPSResult.Message = ex2.Message;
		}
		return iPSResult;
	}

	public async Task<IPSResult> ReadTcpAsync(ReadPacket RP)
	{
		
		return await Task.Run(delegate
		{
			IPSResult iPSResult = new IPSResult
			{
				Status = CommStatus.Error,
				Message = "Read request failed."
			};
			try
			{
				byte[] array = Array.Empty<byte>();
				int num = 16 + RP.NumOfRecvBytes;
				int num2 = 0;
				int num3 = 0;
				RP.SendBytes[20] = base.DA1;
				RP.SendBytes[23] = base.SA1;
				lock (adapter)
				{
					do
					{
						try
						{
							num3++;
							RP.SendBytes[25]++;
							num2 = adapter.Write(RP.SendBytes);
							if (RP.ReceivingDelay > 0)
							{
								Thread.Sleep(RP.ReceivingDelay);
							}
							array = adapter.Read(num);
						}
						catch (Exception ex)
						{
							if (num3 >= RP.ConnectRetries)
							{
								iPSResult.Status = CommStatus.Timeout;
								iPSResult.Message = ex.Message;
								return iPSResult;
							}
						}
					}
					while ((num2 != RP.SendBytes.Length || array.Length < num || (array.Length >= num && (array[0] != 70 || array[1] != 73 || array[2] != 78 || array[3] != 83))) && num3 <= RP.ConnectRetries);
				}
				if (array.Length == 0)
				{
					iPSResult.Message = "No Data was .";
					return iPSResult;
				}
				if (array.Length < 16)
				{
					iPSResult.Message = "FINS length error. Please Illegal response length check.";
					return iPSResult;
				}
				if (array[0] != 70 || array[1] != 73 || array[2] != 78 || array[3] != 83)
				{
					iPSResult.Message = "The TCP Header was Invalid.";
					return iPSResult;
				}
				if (array[11] == 3 || array[15] != 0)
				{
					Validate.FinsTcp(array[15]);
				}
				if (RP.SendBytes[25] != array[25])
				{
					iPSResult.Status = CommStatus.Error;
					iPSResult.Message = "Illegal SID error. Please SID Check.";
					return iPSResult;
				}
				iPSResult.Values = ((Memory<byte>)array).Slice(30, RP.NumOfRecvBytes).ToArray();
				iPSResult.Status = CommStatus.Success;
				iPSResult.Message = "Read request successfully.";
			}
			catch (Exception ex2)
			{
				iPSResult.Status = CommStatus.Error;
				iPSResult.Message = ex2.Message;
			}
			return iPSResult;
		});
	}

	public IPSResult WriteTcp(byte memoryAreaCode, int wordAddress, int bitAddress, int numOfElements, byte[] values)
	{
		IPSResult iPSResult = new IPSResult
		{
			Status = CommStatus.Error,
			Message = "Write data: failure."
		};
		try
		{
			int num = 3;
			int num2 = 0;
			int num3 = 0;
			byte[] array = WriteTcpMsg(memoryAreaCode, wordAddress, bitAddress, numOfElements, values);
			byte[] array2 = Array.Empty<byte>();
			lock (adapter)
			{
				do
				{
					try
					{
						num3++;
						num2 = adapter.Write(array);
						Thread.Sleep(10);
						array2 = adapter.Read(30);
					}
					catch (Exception ex)
					{
						Thread.Sleep(10);
						if (num3 >= num)
						{
							iPSResult.Status = CommStatus.Timeout;
							iPSResult.Message = ex.Message;
							return iPSResult;
						}
					}
				}
				while ((num2 != array.Length || array2.Length < 30 || (array2.Length >= 30 && (array2[0] != 70 || array2[1] != 73 || array2[2] != 78 || array2[3] != 83))) && num3 <= num);
			}
			if (array2.Length == 0)
			{
				iPSResult.Message = "No Data was .";
				return iPSResult;
			}
			if (array2.Length < 30)
			{
				iPSResult.Status = CommStatus.Error;
				iPSResult.Message = "FINS length error. Please Illegal response length check.";
				return iPSResult;
			}
			if (array2[0] != 70 || array2[1] != 73 || array2[2] != 78 || array2[3] != 83)
			{
				iPSResult.Message = "The TCP Header was Invalid.";
				return iPSResult;
			}
			if (array2[11] == 3 || array2[15] != 0)
			{
				Validate.FinsTcp(array2[15]);
			}
			iPSResult.Status = CommStatus.Success;
			iPSResult.Message = "Write data: successfully.";
		}
		catch (Exception ex2)
		{
			iPSResult.Status = CommStatus.Error;
			iPSResult.Message = ex2.Message;
		}
		return iPSResult;
	}

	public IPSResult ReadUdp(ReadPacket RP)
	{
		IPSResult iPSResult = new IPSResult
		{
			Status = CommStatus.Error,
			Message = "Read request failed."
		};
		try
		{
			int num = 0;
			int num2 = 0;
			int num3 = 14 + RP.NumOfRecvBytes;
			byte[] array = Array.Empty<byte>();
			lock (adapter)
			{
				do
				{
					try
					{
						num2++;
						RP.SendBytes[9]++;
						num = adapter.Write(RP.SendBytes);
						if (RP.ReceivingDelay > 0)
						{
							Thread.Sleep(RP.ReceivingDelay);
						}
						array = adapter.Read(num3);
					}
					catch (Exception ex)
					{
						if (num2 >= RP.ConnectRetries)
						{
							iPSResult.Status = CommStatus.Timeout;
							iPSResult.Message = ex.Message;
							return iPSResult;
						}
					}
				}
				while ((num != RP.SendBytes.Length || array.Length < num3) && num2 <= RP.ConnectRetries);
			}
			if (array.Length < 14)
			{
				iPSResult.Status = CommStatus.Error;
				iPSResult.Message = "FINS length error. Please Illegal response length check.";
				return iPSResult;
			}
			if (RP.SendBytes[3] != array[6] || RP.SendBytes[4] != array[7] || RP.SendBytes[5] != array[8])
			{
				iPSResult.Status = CommStatus.Error;
				iPSResult.Message = "Illegal source address error. Please Destination address check.";
				return iPSResult;
			}
			if (RP.SendBytes[9] != array[9])
			{
				iPSResult.Status = CommStatus.Error;
				iPSResult.Message = "Illegal SID error. Please SID Check.";
				return iPSResult;
			}
			iPSResult.Values = ((Memory<byte>)array).Slice(14, RP.NumOfRecvBytes).ToArray();
			iPSResult.Status = CommStatus.Success;
			iPSResult.Message = "Read request successfully.";
		}
		catch (Exception ex2)
		{
			iPSResult.Status = CommStatus.Error;
			iPSResult.Message = ex2.Message;
		}
		return iPSResult;
	}

	public IPSResult WriteUdp(byte memoryAreaCode, int wordAddress, int bitAddress, int numOfElements, byte[] values)
	{
		IPSResult iPSResult = new IPSResult
		{
			Status = CommStatus.Error,
			Message = "Write data: failure."
		};
		try
		{
			int num = 3;
			int num2 = 0;
			int num3 = 0;
			byte[] array = WriteUdpMsg(memoryAreaCode, wordAddress, bitAddress, numOfElements, values);
			byte[] array2 = Array.Empty<byte>();
			lock (adapter)
			{
				do
				{
					try
					{
						num3++;
						num2 = adapter.Write(array);
						Thread.Sleep(10);
						array2 = adapter.Read(14);
					}
					catch (Exception ex)
					{
						Thread.Sleep(10);
						if (num3 >= num)
						{
							iPSResult.Status = CommStatus.Timeout;
							iPSResult.Message = ex.Message;
							return iPSResult;
						}
					}
				}
				while ((num2 != array.Length || array2.Length < 14) && num3 <= num);
			}
			if (array2.Length < 14)
			{
				iPSResult.Status = CommStatus.Error;
				iPSResult.Message = "FINS length error. Please Illegal response length check.";
				return iPSResult;
			}
			if (array[3] != array2[6] || array[4] != array2[7] || array[5] != array2[8])
			{
				iPSResult.Status = CommStatus.Error;
				iPSResult.Message = "Illegal source address error. Please Destination address check.";
				return iPSResult;
			}
			if (array[9] != array2[9])
			{
				iPSResult.Status = CommStatus.Error;
				iPSResult.Message = "Illegal SID error. Please SID Check.";
				return iPSResult;
			}
			iPSResult.Status = CommStatus.Success;
			iPSResult.Message = "Write data: successfully.";
		}
		catch (Exception ex2)
		{
			iPSResult.Status = CommStatus.Error;
			iPSResult.Message = ex2.Message;
		}
		return iPSResult;
	}

	public IPSResult ReadCpuMode(ReadPacket RP)
	{
		IPSResult iPSResult = new IPSResult
		{
			Status = CommStatus.Error,
			Message = "Read request failed."
		};
		try
		{
			string data = ReadOperationModeMsg(RP.StationNo);
			string text = string.Empty;
			lock (adapter)
			{
				adapter.Write(data);
				if (RP.ReceivingDelay > 0)
				{
					Thread.Sleep(RP.ReceivingDelay);
				}
				text = adapter.ReadString(RP.NumOfRecvBytes);
			}
			string code = text.Substring(19, 2);
			validate.EndCode(code);
			iPSResult.Values_Hex = text.Substring(23, 4);
			iPSResult.Status = CommStatus.Success;
		}
		catch (Exception ex)
		{
			iPSResult.Status = CommStatus.Error;
			iPSResult.Message = ex.Message;
		}
		return iPSResult;
	}

	public IPSResult WriteCpuMode(int unitNo, Mode mode)
	{
		IPSResult iPSResult = new IPSResult
		{
			Status = CommStatus.Error,
			Message = "Write data: failure."
		};
		try
		{
			string data = OperationModeMsg(unitNo, mode);
			string text = string.Empty;
			lock (adapter)
			{
				adapter.Write(data);
				Thread.Sleep(10);
				text = adapter.ReadString(27);
			}
			string code = text.Substring(19, 2);
			validate.EndCode(code);
		}
		catch (Exception ex)
		{
			iPSResult.Status = CommStatus.Error;
			iPSResult.Message = ex.Message;
		}
		return iPSResult;
	}
}
