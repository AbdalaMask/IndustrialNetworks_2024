using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NetStudio.Common.Authors;
using NetStudio.Common.DataTypes;
using NetStudio.Common.IndusCom;
using NetStudio.Common.Manager;
using NetStudio.Common.Security;

namespace NetStudio.Modbus.RTU;

public class ModbusRtuProtocol : ModbusRtuBuilder, IModbusProtocol
{
	private readonly INetworkAdapter adapter;

	private const int FORMAT_READ = 5;

	private const int FORMAT_WRITE = 8;

	public Author Author => new Author();

	public ModbusRtuProtocol(INetworkAdapter adapter)
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
				int num = 5 + 2 * RP.Quantity;
				int num2 = 0;
				int num3 = 0;
				byte[] array = Array.Empty<byte>();
				lock (adapter)
				{
					do
					{
						try
						{
							num3++;
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
					while ((num2 != RP.SendBytes.Length || array.Length < num || (array.Length >= num && RP.StationNo != array[0])) && num3 <= RP.ConnectRetries);
				}
				if (array.Length >= 5 && RP.StationNo == array[0])
				{
					if (3 != array[1] && 4 != array[1])
					{
						iPSResult.Status = CommStatus.Error;
						iPSResult.Message = GetErrorMessage(array[2]);
					}
					else
					{
						byte b = array[2];
						iPSResult.Values = new byte[b];
						Array.Copy(array, 3, iPSResult.Values, 0, iPSResult.Values.Length);
						iPSResult.Status = CommStatus.Success;
						iPSResult.Message = "Read request successfully.";
					}
				}
				else
				{
					iPSResult.Status = CommStatus.Timeout;
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
				int num2 = 5 + num;
				int num3 = 0;
				int num4 = 0;
				byte[] array = Array.Empty<byte>();
				lock (adapter)
				{
					do
					{
						try
						{
							num4++;
							num3 = adapter.Write(RP.SendBytes);
							if (RP.ReceivingDelay > 0)
							{
								Thread.Sleep(RP.ReceivingDelay);
							}
							array = adapter.Read(num2);
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
					while ((num3 != RP.SendBytes.Length || array.Length < num2 || (array.Length >= num2 && RP.StationNo != array[0])) && num4 <= RP.ConnectRetries);
				}
				if (array.Length >= 5 && RP.StationNo == array[0])
				{
					if (1 != array[1] && 2 != array[1])
					{
						iPSResult.Status = CommStatus.Error;
						iPSResult.Message = GetErrorMessage(array[2]);
					}
					else
					{
						iPSResult.Values = new byte[array.Length - 5];
						Array.Copy(array, 3, iPSResult.Values, 0, iPSResult.Values.Length);
						iPSResult.Status = CommStatus.Success;
						iPSResult.Message = "Read request successfully.";
					}
				}
				else if (array.Length == num2)
				{
					iPSResult.Status = CommStatus.Timeout;
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
				byte[] values2 = BOOL.ToBytes(values);
				int num2 = 0;
				int num3 = 0;
				byte[] array = Array.Empty<byte>();
				byte[] array2 = WriteMessage(stationNo, 15, address, values.Length, values2);
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
					while ((num2 != array2.Length || array.Length < 8 || (array.Length >= 8 && stationNo != array[0])) && num3 <= num);
				}
				if (array.Length >= 8 && (array.Length < 8 || stationNo == array[0]))
				{
					if (15 == array[1])
					{
						iPSResult.Status = CommStatus.Success;
						iPSResult.Message = "Write data: successfully.";
					}
					else
					{
						iPSResult.Status = CommStatus.Error;
						iPSResult.Message = GetErrorMessage(array[2]);
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
				int num = 0;
				int num2 = 0;
				byte[] array = Array.Empty<byte>();
				byte[] array2 = WriteMessage(WP.StationNo, 5, WP.Address, WP.DataDec);
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
					while ((num != array2.Length || array.Length < 8 || (array.Length >= 8 && WP.StationNo != array[0])) && num2 <= WP.ConnectRetries);
				}
				if (array.Length >= 5 && WP.StationNo == array[0])
				{
					if (5 == array[1])
					{
						iPSResult.Status = CommStatus.Success;
						iPSResult.Message = "Write data: successfully.";
					}
					else
					{
						iPSResult.Status = CommStatus.Error;
						iPSResult.Message = GetErrorMessage(array[2]);
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
				int num = 0;
				int num2 = 0;
				byte[] array = Array.Empty<byte>();
				byte[] array2 = WriteMessage(WP.StationNo, 16, WP.Address, WP.Quantity, WP.DataDec);
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
					while ((num != array2.Length || array.Length < 8 || (array.Length >= 8 && WP.StationNo != array[0])) && num2 <= WP.ConnectRetries);
				}
				if (array.Length >= 5 && WP.StationNo == array[0])
				{
					if (16 == array[1])
					{
						iPSResult.Status = CommStatus.Success;
						iPSResult.Message = "Write data: successfully.";
					}
					else
					{
						iPSResult.Status = CommStatus.Error;
						iPSResult.Message = GetErrorMessage(array[2]);
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
