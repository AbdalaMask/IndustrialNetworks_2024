using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NetStudio.Common.Authors;
using NetStudio.Common.IndusCom;
using NetStudio.Common.Manager;
using NetStudio.Common.Security;
using NetStudio.Siemens.Models;

namespace NetStudio.Siemens.NetTcp;

public class TcpProtocol : IS7Protocol
{
	private INetworkAdapter adapter;

	private ushort maxDataBytes;

	private const int BytesFormatToRead = 25;

	private Tsap tsap_src;

	private Tsap tsap_des;

	private TcpBuilder builder;

	public Author Author => new Author();

	public TcpProtocol(INetworkAdapter adapter, CPUType cputype_0)
	{
		this.adapter = adapter;
		Tsap[] single = Tsap.GetSingle(cputype_0);
		tsap_src = single[0];
		tsap_des = single[1];
		builder = new TcpBuilder();
	}

	public TcpProtocol(INetworkAdapter adapter, Tsap tsap_src, Tsap tsap_des)
	{
		this.adapter = adapter;
		this.tsap_src = tsap_src;
		this.tsap_des = tsap_des;
		builder = new TcpBuilder();
	}

	 

	public bool Connect()
	{
		
		if (tsap_src != null && tsap_des != null)
		{
			bool num = adapter.Connect();
			IPSResult iPSResult = ConnectionRequest();
			if (iPSResult.Status == CommStatus.Success)
			{
				iPSResult = CreateConnectionSetup();
			}
			if (num)
			{
				return iPSResult.Status == CommStatus.Success;
			}
			return false;
		}
		throw new InvalidDataException("TSAP: Invalid");
	}

	public bool Reconnect()
	{
		
		if (tsap_src != null && tsap_des != null)
		{
			if (adapter.Connected)
			{
				adapter.Disconnect();
			}
			bool num = adapter.Connect();
			IPSResult iPSResult = ConnectionRequest();
			if (iPSResult.Status == CommStatus.Success)
			{
				iPSResult = CreateConnectionSetup();
			}
			if (num)
			{
				return iPSResult.Status == CommStatus.Success;
			}
			return false;
		}
		throw new InvalidDataException("TSAP: Invalid");
	}

	public bool Disconnect()
	{
		return adapter.Disconnect();
	}

	public async Task<bool> ConnectAsync()
	{
		
		if (tsap_src == null || tsap_des == null)
		{
			throw new InvalidDataException("TSAP: Invalid");
		}
		bool num = await adapter.ConnectAsync();
		IPSResult iPSResult = ConnectionRequest();
		if (iPSResult.Status == CommStatus.Success)
		{
			iPSResult = CreateConnectionSetup();
		}
		return num && iPSResult.Status == CommStatus.Success;
	}

	public async Task<bool> ReconnectAsync()
	{
		
		if (tsap_src != null && tsap_des != null)
		{
			if (adapter.Connected)
			{
				adapter.Disconnect();
			}
			bool num = await adapter.ConnectAsync();
			IPSResult iPSResult = ConnectionRequest();
			if (iPSResult.Status == CommStatus.Success)
			{
				iPSResult = CreateConnectionSetup();
			}
			return num && iPSResult.Status == CommStatus.Success;
		}
		throw new InvalidDataException("TSAP: Invalid");
	}

	public async Task<bool> DisconnectAsync()
	{
		return await adapter.DisconnectAsync();
	}

	private IPSResult ConnectionRequest()
	{
		IPSResult iPSResult = new IPSResult
		{
			Status = CommStatus.Error,
			Message = "Read request failed."
		};
		int num = 3;
		int num2 = 7;
		int num3 = 0;
		int num4 = 0;
		byte[] array = Array.Empty<byte>();
		byte[] connectionRequestMessage = TcpBuilder.GetConnectionRequestMessage(tsap_src, tsap_des);
		lock (adapter)
		{
			do
			{
				try
				{
					num3 = adapter.Write(connectionRequestMessage);
					Thread.Sleep(10);
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
				num4++;
			}
			while ((num3 != connectionRequestMessage.Length || array.Length < num2) && num4 <= num);
		}
		if (array.Length < num2)
		{
			iPSResult.Status = CommStatus.Error;
			iPSResult.Message = "The communication frame is not in the correct format.";
			return iPSResult;
		}
		string errorMessage = builder.GetErrorMessage(array);
		if (!string.IsNullOrEmpty(errorMessage))
		{
			throw new Exception(errorMessage);
		}
		iPSResult.Status = CommStatus.Success;
		iPSResult.Message = "Connection request successfully.";
		return iPSResult;
	}

	private IPSResult CreateConnectionSetup()
	{
		IPSResult iPSResult = new IPSResult
		{
			Status = CommStatus.Error,
			Message = "Read request failed."
		};
		int num = 3;
		int num2 = 7;
		int num3 = 0;
		int num4 = 0;
		byte[] array = Array.Empty<byte>();
		byte[] connectionSetupMessage = builder.GetConnectionSetupMessage();
		lock (adapter)
		{
			do
			{
				try
				{
					num3 = adapter.Write(connectionSetupMessage);
					Thread.Sleep(10);
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
				num4++;
			}
			while ((num3 != connectionSetupMessage.Length || array.Length < num2) && num4 <= num);
		}
		if (array.Length < num2)
		{
			iPSResult.Status = CommStatus.Error;
			iPSResult.Message = "The communication frame is not in the correct format.";
			return iPSResult;
		}
		string errorMessage = builder.GetErrorMessage(array);
		if (string.IsNullOrEmpty(errorMessage))
		{
			maxDataBytes = Convert.ToUInt16(array[25] * 256 + array[26] - 20);
			iPSResult.Status = CommStatus.Success;
			iPSResult.Message = "Create connection setup successfully.";
			return iPSResult;
		}
		throw new Exception(errorMessage);
	}

	public bool Start()
	{
		return OperationModeChanged(OperationMode.START);
	}

	public bool Stop()
	{
		return OperationModeChanged(OperationMode.STOP);
	}

	public bool RunMode()
	{
		return OperationModeChanged(OperationMode.RUN);
	}

	private bool OperationModeChanged(OperationMode mode)
	{
		byte[] operationModeMessage = TcpBuilder.GetOperationModeMessage(mode);
		int size = 7;
		byte[] array;
		lock (adapter)
		{
			adapter.Write(operationModeMessage);
			Thread.Sleep(10);
			array = adapter.Read(size);
		}
		byte[] array2 = new byte[array[2] * 256 + array[3] - 7];
		Array.Copy(array, 7, array2, 0, array2.Length);
		string errorMessage = builder.GetErrorMessage(array2);
		if (!string.IsNullOrEmpty(errorMessage))
		{
			throw new Exception(errorMessage);
		}
		return true;
	}

	public async Task<IPSResult> ReadAsync(ReadPacket RP)
	{
		
		IPSResult result = new IPSResult
		{
			Status = CommStatus.Error,
			Message = "Read request failed."
		};
		if (RP.Quantity <= maxDataBytes)
		{
			return await Task.Run(delegate
			{
				try
				{
					int num = 0;
					int num2 = 0;
					byte[] array = Array.Empty<byte>();
					int num3 = 25 + RP.Quantity;
					lock (adapter)
					{
						do
						{
							try
							{
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
									result.Status = CommStatus.Timeout;
									result.Message = ex.Message;
									return result;
								}
							}
							num2++;
						}
						while ((num != RP.SendBytes.Length || array.Length < num3) && num2 <= RP.ConnectRetries);
					}
					if (array.Length < num3)
					{
						result.Status = CommStatus.Error;
						result.Message = "The communication frame is not in the correct format.";
						return result;
					}
					string empty = string.Empty;
					if (array[21] != byte.MaxValue)
					{
						empty = builder.GetErrorMessage(array[21]);
					}
					empty = builder.GetErrorMessage(array);
					if (!string.IsNullOrEmpty(empty))
					{
						result.Message = empty;
						return result;
					}
					int num4 = (256 * array[23] + array[24]) / 8;
					if (num4 == RP.Quantity)
					{
						result.Values = new byte[num4];
						Array.Copy(array, 25, result.Values, 0, num4);
						result.Status = CommStatus.Success;
						result.Message = "Read request successfully.";
					}
				}
				catch (Exception ex2)
				{
					result.Message = ex2.Message;
				}
				return result;
			});
		}
		result = await ReadBigAsync(RP);
		return result;
	}

	private async Task<IPSResult> ReadBigAsync(ReadPacket RP)
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
				int num = RP.Quantity;
				int num2 = 0;
				decimal address = RP.Address;
				List<byte> list = new List<byte>();
				byte[] array = Array.Empty<byte>();
				do
				{
					num2 = ((num <= maxDataBytes) ? num : maxDataBytes);
					int num3 = 25 + num2;
					ReadPacket readPacket = new ReadPacket
					{
						DBNumber = RP.DBNumber,
						Memory = RP.Memory,
						Address = address,
						Quantity = num2
					};
					int num4 = 0;
					int num5 = 0;
					readPacket.SendBytes = builder.ReadDataMessage(readPacket);
					lock (adapter)
					{
						do
						{
							try
							{
								num4 = adapter.Write(readPacket.SendBytes);
								if (RP.ReceivingDelay > 0)
								{
									Thread.Sleep(RP.ReceivingDelay);
								}
								array = adapter.Read(num3);
							}
							catch (Exception ex)
							{
								if (num5 >= RP.ConnectRetries)
								{
									iPSResult.Status = CommStatus.Timeout;
									iPSResult.Message = ex.Message;
									return iPSResult;
								}
							}
							num5++;
						}
						while ((num4 != readPacket.SendBytes.Length || array.Length < num3) && num5 <= RP.ConnectRetries);
					}
					if (array.Length < num3)
					{
						iPSResult.Status = CommStatus.Error;
						iPSResult.Message = "The communication frame is not in the correct format.";
						return iPSResult;
					}
					string empty = string.Empty;
					if (array[21] != byte.MaxValue)
					{
						empty = builder.GetErrorMessage(array[21]);
					}
					empty = builder.GetErrorMessage(array);
					if (!string.IsNullOrEmpty(empty))
					{
						iPSResult.Status = CommStatus.Error;
						iPSResult.Message = empty;
						return iPSResult;
					}
					int num6 = (256 * array[23] + array[24]) / 8;
					if (num6 == num2)
					{
						byte[] array2 = new byte[num6];
						Array.Copy(array, 25, array2, 0, num6);
						iPSResult.Status = CommStatus.Success;
						iPSResult.Message = "Read request successfully.";
						list.AddRange(array2);
					}
					address += (decimal)readPacket.Quantity;
					num -= num2;
				}
				while (num > 0);
				iPSResult.Values = list.ToArray();
			}
			catch (Exception ex2)
			{
				iPSResult.Status = CommStatus.Error;
				iPSResult.Message = ex2.Message;
			}
			return iPSResult;
		});
	}

	public async Task<IPSResult> WriteAsync(WritePacket WP)
	{
	
		return await Task.Run(delegate
		{
			IPSResult iPSResult = new IPSResult
			{
				Status = CommStatus.Error,
				Message = "Write data: failure."
			};
			try
			{
				byte[] array = (WP.IsBit ? builder.WriteBitMessage(WP) : builder.WriteDataMessage(WP));
				byte[] array2 = Array.Empty<byte>();
				int num = 22;
				int num2 = 0;
				int num3 = 0;
				lock (adapter)
				{
					do
					{
						try
						{
							num2 = adapter.Write(array);
							if (WP.ReceivingDelay > 0)
							{
								Thread.Sleep(WP.ReceivingDelay);
							}
							array2 = adapter.Read(num);
						}
						catch (Exception ex)
						{
							if (num3 >= WP.ConnectRetries)
							{
								iPSResult.Status = CommStatus.Timeout;
								iPSResult.Message = ex.Message;
								return iPSResult;
							}
						}
						num3++;
					}
					while ((num2 != array.Length || array2.Length < num) && num3 <= WP.ConnectRetries);
				}
				if (array2.Length < num)
				{
					iPSResult.Status = CommStatus.Error;
					iPSResult.Message = "The communication frame is not in the correct format.";
					return iPSResult;
				}
				string empty = string.Empty;
				if (array2[21] != byte.MaxValue)
				{
					empty = builder.GetErrorMessage(array2[21]);
				}
				empty = builder.GetErrorMessage(array2);
				if (string.IsNullOrEmpty(empty))
				{
					iPSResult.Status = CommStatus.Success;
					iPSResult.Message = "Write data: successfully.";
				}
				else
				{
					iPSResult.Status = CommStatus.Error;
					iPSResult.Message = empty;
				}
			}
			catch (Exception ex2)
			{
				iPSResult.Status = CommStatus.Error;
				iPSResult.Message = ex2.Message;
			}
			return iPSResult;
		});
	}
}
