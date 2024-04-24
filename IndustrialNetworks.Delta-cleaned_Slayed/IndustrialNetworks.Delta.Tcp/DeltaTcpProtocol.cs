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

namespace NetStudio.Delta.Tcp;

public class DeltaTcpProtocol : DeltaTcpBuilder, IDeltaMaster
{
	private readonly INetworkAdapter adapter;

	private static int TransactionId;

	private const int FORMAT_READ = 9;

	private const int FORMAT_WRITE = 8;

	public Author Author => new Author();

	public DeltaTcpProtocol(INetworkAdapter adapter)
	{
		this.adapter = adapter;
	}

	public DeltaTcpProtocol(string IP = "127.0.0.1", int port = 502, int sendTimeout = 500, int receiveTimeout = 500)
	{
		adapter = new EthernetAdapter(IP, port, sendTimeout, receiveTimeout);
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
				int num = 3;
				int num2 = 0;
				int num3 = 0;
				byte[] array = Array.Empty<byte>();
				int num4 = 9 + 2 * RP.Quantity;
				lock (adapter)
				{
					do
					{
						try
						{
							TransactionId++;
							RP.SendBytes[0] = (byte)(TransactionId >> 8);
							RP.SendBytes[0] = (byte)TransactionId;
							num3++;
							num2 = adapter.Write(RP.SendBytes);
							if (RP.ReceivingDelay > 0)
							{
								Thread.Sleep(RP.ReceivingDelay);
							}
							array = adapter.Read(num4);
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
					while ((num2 != RP.SendBytes.Length || array.Length < num4 || (array.Length >= num4 && RP.StationNo != array[6])) && num3 <= num);
				}
				if (array.Length >= 9 && RP.StationNo == array[6])
				{
					if (3 != array[7] && 4 != array[7])
					{
						iPSResult.Status = CommStatus.Error;
						iPSResult.Message = GetErrorMessage(array[8]);
					}
					else
					{
						int num5 = array[8];
						iPSResult.Values = new byte[num5];
						Array.Copy(array, 9, iPSResult.Values, 0, iPSResult.Values.Length);
						iPSResult.Status = CommStatus.Success;
						iPSResult.Message = "Read request successfully.";
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
				int num = 0;
				int num2 = 0;
				byte[] array = Array.Empty<byte>();
				int num3 = RP.Quantity / 8 + ((RP.Quantity % 8 != 0) ? 1 : 0);
				int num4 = 9 + num3;
				lock (adapter)
				{
					do
					{
						try
						{
							num2++;
							num = adapter.Write(RP.SendBytes);
							if (RP.ReceivingDelay > 0)
							{
								Thread.Sleep(RP.ReceivingDelay);
							}
							array = adapter.Read(num4);
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
					while ((num != RP.SendBytes.Length || array.Length < num4 || (array.Length >= num4 && RP.StationNo != array[6])) && num2 <= RP.ConnectRetries);
				}
				if (array.Length >= 9 && RP.StationNo == array[6])
				{
					if (1 != array[7] && 2 != array[7])
					{
						iPSResult.Status = CommStatus.Error;
						iPSResult.Message = GetErrorMessage(array[8]);
					}
					else
					{
						int num5 = array[8];
						iPSResult.Values = new byte[num5];
						Array.Copy(array, 9, iPSResult.Values, 0, iPSResult.Values.Length);
						iPSResult.Status = CommStatus.Success;
						iPSResult.Message = "Read request successfully.";
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

	public async Task<IPSResult> WriteCoilAsync(byte stationNo, int address, BOOL[] values)
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
				int num2 = 0;
				int num3 = 0;
				byte[] array = Array.Empty<byte>();
				byte[] values2 = BOOL.ToBytes(values);
				byte[] array2 = WriteMultipleMessage(TransactionId++, stationNo, address, 15, values.Length, values2);
				lock (adapter)
				{
					do
					{
						try
						{
							num3++;
							num2 = adapter.Write(array2);
							Thread.Sleep(10);
							array = adapter.Read(8);
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
					while ((num2 != array2.Length || array.Length < 8 || (array.Length >= 8 && stationNo != array[6])) && num3 <= num);
				}
				if (array.Length != 0 && (array.Length < 8 || array.Length > 8) && stationNo != array[6])
				{
					iPSResult.Status = CommStatus.Error;
					iPSResult.Message = "The communication frame is not in the correct format.";
				}
				else if (array.Length == 8)
				{
					if (15 == array[7])
					{
						iPSResult.Status = CommStatus.Success;
						iPSResult.Message = "Write data: successfully.";
					}
					else
					{
						iPSResult.Status = CommStatus.Error;
						iPSResult.Message = "The communication frame is not in the correct format.";
					}
				}
				else
				{
					iPSResult.Status = CommStatus.Error;
					iPSResult.Message = GetErrorMessage(array[8]);
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
				int num = 0;
				int num2 = 0;
				byte[] array = Array.Empty<byte>();
				byte[] array2 = WriteMessage(TransactionId++, WP.StationNo, WP.Address.BitAddress, 5, WP.DataDec);
				lock (adapter)
				{
					do
					{
						try
						{
							num2++;
							num = adapter.Write(array2);
							if (WP.ReceivingDelay > 0)
							{
								Thread.Sleep(WP.ReceivingDelay);
							}
							array = adapter.Read(8);
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
					while ((num != array2.Length || array.Length < 8 || (array.Length >= 8 && WP.StationNo != array[6])) && num2 <= WP.ConnectRetries);
				}
				if (array.Length >= 9 && WP.StationNo == array[6])
				{
					if (5 == array[7])
					{
						iPSResult.Status = CommStatus.Success;
						iPSResult.Message = "Write data: successfully.";
					}
					else
					{
						iPSResult.Status = CommStatus.Error;
						iPSResult.Message = GetErrorMessage(array[8]);
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
				WP.Quantity = ((WP.Memory == "HC") ? (WP.DataDec.Length / 4) : (WP.DataDec.Length / 2));
				int wordAddress = WP.Address.WordAddress;
				int num = 0;
				int num2 = 0;
				byte[] array = Array.Empty<byte>();
				byte[] array2 = WriteMultipleMessage(TransactionId++, WP.StationNo, wordAddress, 16, WP.Quantity, WP.DataDec);
				lock (adapter)
				{
					do
					{
						try
						{
							num2++;
							num = adapter.Write(array2);
							if (WP.ReceivingDelay > 0)
							{
								Thread.Sleep(WP.ReceivingDelay);
							}
							array = adapter.Read(8);
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
					while ((num != array2.Length || array.Length < 8 || (array.Length >= 8 && WP.StationNo != array[6])) && num2 <= WP.ConnectRetries);
				}
				if (array.Length >= 9 && WP.StationNo == array[6])
				{
					if (16 == array[7])
					{
						iPSResult.Status = CommStatus.Success;
						iPSResult.Message = "Write data: successfully.";
					}
					else
					{
						iPSResult.Status = CommStatus.Error;
						iPSResult.Message = GetErrorMessage(array[8]);
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
