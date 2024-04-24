using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NetStudio.Common.Authors;
using NetStudio.Common.DataTypes;
using NetStudio.Common.IndusCom;
using NetStudio.Common.Manager;
using NetStudio.Common.Security;
using NetStudio.Delta.Models;

namespace NetStudio.Delta.Ascii;

public class DeltaAsciiProtocol : DeltaAsciiBuilder, IDeltaMaster
{
	private INetworkAdapter adapter;

	private const int FORMAT_READ = 11;

	private const int FORMAT_WRITE = 17;

	public Author Author => new Author();

	public DeltaAsciiProtocol(INetworkAdapter adapter)
	{
		this.adapter = adapter;
	}

	 

	public bool Connect()
	{
		
		return adapter.Connect();
	}

	public bool Reconnect()
	{
		
		bool flag = adapter.Disconnect();
		return adapter.Connect() && flag;
	}

	public bool Disconnect()
	{
		return adapter.Disconnect();
	}

	public async Task<bool> ConnectAsync()
	{
		
		return await adapter.ConnectAsync();
	}

	public async Task<bool> ReconnectAsync()
	{
		
		bool disconnect = await adapter.DisconnectAsync();
		return await adapter.ConnectAsync() && disconnect;
	}

	public async Task<bool> DisconnectAsync()
	{
		return await adapter.DisconnectAsync();
	}

	public async Task<IPSResult> ReadRegisterAsync(ReadPacket RP)
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
				 
				int num = 0;
				int num2 = 0;
				string text = string.Empty;
				int num3 = 11 + 4 * RP.Quantity;
				lock (adapter)
				{
					do
					{
						try
						{
							num2++;
							num = adapter.Write(RP.SendMsg);
							if (RP.ReceivingDelay > 0)
							{
								Thread.Sleep(RP.ReceivingDelay);
							}
							text = adapter.ReadString(num3);
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
					while ((num != RP.SendMsg.Length || text.Length < num3 || (text.Length >= num3 && text[0] != ':')) && num2 <= RP.ConnectRetries);
				}
				if (text.Length >= 11 && text[0] == ':')
				{
					if (BYTE.GetByteFromHex(text.Substring(1, 2)) == RP.StationNo)
					{
						byte byteFromHex = BYTE.GetByteFromHex(text.Substring(3, 2));
						if (3 != byteFromHex && 4 != byteFromHex)
						{
							byte byteFromHex2 = BYTE.GetByteFromHex(text.Substring(5, 2));
							iPSResult.Message = GetErrorMessage(byteFromHex2);
							iPSResult.Status = CommStatus.Error;
						}
						else
						{
							byte byteFromHex3 = BYTE.GetByteFromHex(text.Substring(5, 2));
							int length = 2 * byteFromHex3;
							string string_ = text.Substring(7, length);
							iPSResult.Values = BYTE.GetBytesFromHex(string_);
							iPSResult.Status = CommStatus.Success;
							iPSResult.Message = "Read request successfully.";
						}
					}
				}
				else
				{
					iPSResult.Status = CommStatus.Error;
					iPSResult.Message = "The communication frame is not in the correct format.";
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

	public async Task<IPSResult> ReadStatusAsync(ReadPacket RP)
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
				 
				int num = RP.Quantity / 8 + ((RP.Quantity % 8 != 0) ? 1 : 0);
				int num2 = 11 + 2 * num;
				int num3 = 0;
				int num4 = 0;
				string text = string.Empty;
				lock (adapter)
				{
					do
					{
						try
						{
							num4++;
							num3 = adapter.Write(RP.SendMsg);
							if (RP.ReceivingDelay > 0)
							{
								Thread.Sleep(RP.ReceivingDelay);
							}
							text = adapter.ReadString(num2);
						}
						catch (Exception ex)
						{
							if (num4 >= RP.ConnectRetries)
							{
								iPSResult.Status = CommStatus.Timeout;
								iPSResult.Message = ex.Message;
								return iPSResult;
							}
						}
					}
					while ((num3 != RP.SendMsg.Length || text.Length < num2 || (text.Length >= num2 && text[0] != ':')) && num4 <= RP.ConnectRetries);
				}
				if (text.Length >= 11 && text[0] == ':')
				{
					if (BYTE.GetByteFromHex(text.Substring(1, 2)) == RP.StationNo)
					{
						byte byteFromHex = BYTE.GetByteFromHex(text.Substring(3, 2));
						if (1 != byteFromHex && 2 != byteFromHex)
						{
							byte byteFromHex2 = BYTE.GetByteFromHex(text.Substring(5, 2));
							iPSResult.Message = GetErrorMessage(byteFromHex2);
							iPSResult.Status = CommStatus.Error;
						}
						else
						{
							byte byteFromHex3 = BYTE.GetByteFromHex(text.Substring(5, 2));
							int length = 2 * byteFromHex3;
							string string_ = text.Substring(7, length);
							iPSResult.Values = BYTE.GetBytesFromHex(string_);
							iPSResult.Status = CommStatus.Success;
							iPSResult.Message = "Read request successfully.";
						}
					}
				}
				else
				{
					iPSResult.Status = CommStatus.Error;
					iPSResult.Message = "The communication frame is not in the correct format.";
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

	public async Task<IPSResult> WriteCoilAsync(byte slaveAddr, int address, BOOL[] values)
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
				int num = 3;
				string hex_value = BOOL.ToHex(values);
				int num2 = 0;
				int num3 = 0;
				string text = string.Empty;
				string text2 = WriteMultipleMessage(slaveAddr, address, 15, values.Length, hex_value);
				lock (adapter)
				{
					do
					{
						try
						{
							num3++;
							num2 = adapter.Write(text2);
							Thread.Sleep(10);
							text = adapter.ReadString(17);
						}
						catch (Exception ex)
						{
							if (num3 >= num)
							{
								iPSResult.Status = CommStatus.Timeout;
								iPSResult.Message = ex.Message;
								return iPSResult;
							}
						}
					}
					while ((num2 != text2.Length || text.Length < 17 || (text.Length >= 17 && text[0] != ':')) && num3 <= num);
				}
				if (text.Length >= 11 && text[0] == ':')
				{
					if (BYTE.GetByteFromHex(text.Substring(1, 2)) == slaveAddr)
					{
						byte byteFromHex = BYTE.GetByteFromHex(text.Substring(3, 2));
						if (15 == byteFromHex)
						{
							iPSResult.Status = CommStatus.Success;
							iPSResult.Message = "Write data: successfully.";
						}
						else
						{
							byte byteFromHex2 = BYTE.GetByteFromHex(text.Substring(5, 2));
							iPSResult.Message = GetErrorMessage(byteFromHex2);
							iPSResult.Status = CommStatus.Error;
						}
					}
				}
				else
				{
					iPSResult.Status = CommStatus.Error;
					iPSResult.Message = "The communication frame is not in the correct format.";
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

	public async Task<IPSResult> WriteRegisterAsync(WritePacket WP)
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
				string hex_value = BYTE.ToHex(WP.DataDec);
				int num = 0;
				int num2 = 0;
				string text = string.Empty;
				WP.Quantity = ((WP.Memory == "HC") ? (WP.DataDec.Length / 4) : (WP.DataDec.Length / 2));
				string text2 = WriteMultipleMessage(WP.StationNo, WP.Address.WordAddress, 16, WP.Quantity, hex_value);
				lock (adapter)
				{
					do
					{
						try
						{
							num2++;
							num = adapter.Write(text2);
							if (WP.ReceivingDelay > 0)
							{
								Thread.Sleep(WP.ReceivingDelay);
							}
							text = adapter.ReadString(17);
						}
						catch (Exception ex)
						{
							if (num2 >= WP.ConnectRetries)
							{
								iPSResult.Status = CommStatus.Timeout;
								iPSResult.Message = ex.Message;
								return iPSResult;
							}
						}
					}
					while ((num != text2.Length || text.Length < 17 || (text.Length >= 17 && text[0] != ':')) && num2 <= WP.ConnectRetries);
				}
				if (text.Length >= 17 && text[0] == ':')
				{
					if (BYTE.GetByteFromHex(text.Substring(1, 2)) == WP.StationNo)
					{
						byte byteFromHex = BYTE.GetByteFromHex(text.Substring(3, 2));
						if (16 == byteFromHex)
						{
							iPSResult.Status = CommStatus.Success;
							iPSResult.Message = "Write data: successfully.";
						}
						else
						{
							byte byteFromHex2 = BYTE.GetByteFromHex(text.Substring(5, 2));
							iPSResult.Message = GetErrorMessage(byteFromHex2);
							iPSResult.Status = CommStatus.Error;
						}
					}
				}
				else
				{
					iPSResult.Status = CommStatus.Error;
					iPSResult.Message = "The communication frame is not in the correct format.";
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

	public async Task<IPSResult> WriteCoilAsync(WritePacket WP)
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
				WP.ValueHex = BYTE.ToHex(WP.DataDec);
				int num = 0;
				int num2 = 0;
				string text = string.Empty;
				string text2 = WriteMessage(WP.StationNo, WP.Address.BitAddress, 5, WP.ValueHex);
				lock (adapter)
				{
					do
					{
						try
						{
							num2++;
							num = adapter.Write(text2);
							if (WP.ReceivingDelay > 0)
							{
								Thread.Sleep(WP.ReceivingDelay);
							}
							text = adapter.ReadString(17);
						}
						catch (Exception ex)
						{
							if (num2 >= WP.ConnectRetries)
							{
								iPSResult.Status = CommStatus.Timeout;
								iPSResult.Message = ex.Message;
								return iPSResult;
							}
						}
					}
					while ((num != text2.Length || text.Length < 17 || (text.Length >= 17 && text[0] != ':')) && num2 <= WP.ConnectRetries);
				}
				if (text.Length >= 17 && text[0] == ':')
				{
					if (BYTE.GetByteFromHex(text.Substring(1, 2)) == WP.StationNo)
					{
						byte byteFromHex = BYTE.GetByteFromHex(text.Substring(3, 2));
						if (5 == byteFromHex)
						{
							iPSResult.Status = CommStatus.Success;
							iPSResult.Message = "Write data: successfully.";
						}
						else
						{
							byte byteFromHex2 = BYTE.GetByteFromHex(text.Substring(5, 2));
							iPSResult.Message = GetErrorMessage(byteFromHex2);
							iPSResult.Status = CommStatus.Error;
						}
					}
				}
				else
				{
					iPSResult.Status = CommStatus.Error;
					iPSResult.Message = "The communication frame is not in the correct format.";
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
