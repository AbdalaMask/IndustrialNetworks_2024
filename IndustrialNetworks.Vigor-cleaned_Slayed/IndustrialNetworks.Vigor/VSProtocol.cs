using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NetStudio.Common;
using NetStudio.Common.Authors;
using NetStudio.Common.DataTypes;
using NetStudio.Common.IndusCom;
using NetStudio.Common.Manager;
using NetStudio.Common.Security;
using NetStudio.Vigor.Enums;

namespace NetStudio.Vigor;

public class VSProtocol : VSBuilder
{
	private INetworkAdapter adapter;

	private const int FORMAT_READ = 10;

	private const int FORMAT_WRITE = 10;

	public Author Author => new Author();

	public VSProtocol(INetworkAdapter adapter)
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

	public async Task<IPSResult> ReadAsync(ReadPacket RP)
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
				int num = 10 + RP.NumOfBytes;
				byte[] array = Array.Empty<byte>();
				int num2 = 0;
				int num3 = 0;
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
						catch (TimeoutException ex)
						{
							if (num3 >= RP.ConnectRetries)
							{
								iPSResult.Status = CommStatus.Timeout;
								iPSResult.Message = ex.Message;
								return iPSResult;
							}
						}
					}
					while ((num2 != RP.SendBytes.Length || array.Length < num || (array.Length >= num && array[1] != 6)) && num3 <= RP.ConnectRetries);
				}
				if (array[5] == 0)
				{
					int num4 = array[4] * 8 + array[3] - 1;
					int num5 = array.Length - 10;
					byte[] array2 = new byte[num5];
					Array.Copy(array, 6, array2, 0, num5);
					iPSResult.Values = ((num4 == num5) ? array2 : SubOnCode10H(array2));
					iPSResult.Status = CommStatus.Success;
				}
				else
				{
					iPSResult.Status = CommStatus.Error;
					iPSResult.Message = Validate.Errors[(ErrorCode)array[5]];
				}
			}
			catch (TimeoutException ex2)
			{
				iPSResult.Status = CommStatus.Timeout;
				iPSResult.Message = ex2.Message;
			}
			catch (Exception ex3)
			{
				iPSResult.Status = CommStatus.Error;
				iPSResult.Message = ex3.Message;
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
				byte[] array = WriteMsg(WP);
				byte[] array2 = Array.Empty<byte>();
				int num = 0;
				int num2 = 0;
				lock (adapter)
				{
					do
					{
						try
						{
							num2++;
							num = adapter.Write(array);
							if (WP.ReceivingDelay > 0)
							{
								Thread.Sleep(WP.ReceivingDelay);
							}
							array2 = adapter.Read(10);
						}
						catch (TimeoutException ex)
						{
							if (num2 >= WP.ConnectRetries)
							{
								iPSResult.Status = CommStatus.Timeout;
								iPSResult.Message = ex.Message;
								return iPSResult;
							}
						}
					}
					while ((num != array.Length || array2.Length < 10 || (array2.Length >= 10 && array2[1] != 6)) && num2 <= WP.ConnectRetries);
				}
				if (array2[5] == 0)
				{
					iPSResult.Status = CommStatus.Success;
					iPSResult.Message = "Write data: successfully.";
				}
				else
				{
					iPSResult.Status = CommStatus.Error;
					iPSResult.Message = Validate.Errors[(ErrorCode)array2[5]];
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

	public async Task<IPSResult> WriteDataAsync(byte stationNo, FunctionCode functionCode, DeviceCode deviceCode, string deviceId, byte[] data)
	{
		
		 
		IPSResult result = new IPSResult
		{
			Status = CommStatus.Error,
			Message = "Write data: failure."
		};
		await Task.Run(delegate
		{
			try
			{
				int numOfBytes = 7 + data.Length;
				byte[] array = Conversion.HexStringToBytes(WriteByDeviceId(stationNo, numOfBytes, functionCode, deviceCode, deviceId, data));
				string.Join(" ", array.Select((byte byte_0) => byte_0.ToString("X2")));
				byte[] array2 = Array.Empty<byte>();
				lock (adapter)
				{
					adapter.Write(array);
					array2 = adapter.Read(10);
				}
				if (array2 != null && array2.Length >= 10)
				{
					if (array2[5] == 0)
					{
						result.Status = CommStatus.Success;
						result.Message = "Write data: successfully.";
					}
					else
					{
						result.Status = CommStatus.Error;
						result.Message = Validate.Errors[(ErrorCode)array2[5]];
					}
				}
				else
				{
					result.Status = CommStatus.Error;
					result.Message = "Write data: failure.";
				}
			}
			catch (TimeoutException ex)
			{
				result.Status = CommStatus.Timeout;
				result.Message = ex.Message;
			}
			catch (Exception ex2)
			{
				result.Status = CommStatus.Error;
				result.Message = ex2.Message;
			}
		});
		return result;
	}

	public async Task<string> ReadStringAsync(byte stationNo, DeviceCode deviceCode, string deviceId, int length, Encoding encoding)
	{
		_ = string.Empty;
		byte[] data = Conversion.HexToBytes(ReadByDeviceId(stationNo, 7, FunctionCode.WordDeviceRead, deviceCode, deviceId, length));
		await adapter.WriteAsync(data);
		int size = length + 10;
		byte[] array = await adapter.ReadAsync(size);
		if (array[5] == 0)
		{
			int num = array[4] * 8 + array[3] - 1;
			int num2 = array.Length - 10;
			byte[] array2 = new byte[num2];
			Array.Copy(array, 6, array2, 0, num2);
			string text = Conversion.BytesToString((num == num2) ? array2 : SubOnCode10H(array2), encoding);
			if (text.Length > length)
			{
				return text.Left(length);
			}
			return text;
		}
		throw new Exception(Validate.Errors[(ErrorCode)array[5]]);
	}

	public async Task<bool> WriteStringAsync(byte stationNo, DeviceCode deviceCode, string deviceId, string value, Encoding encoding)
	{
		byte[] array = Conversion.StringToBytes(value, encoding);
		int numOfBytes = 7 + array.Length;
		byte[] data = Conversion.HexStringToBytes(WriteByDeviceId(stationNo, numOfBytes, FunctionCode.WordDeviceWrite, deviceCode, deviceId, array));
		await adapter.WriteAsync(data);
		byte[] array2 = await adapter.ReadAsync();
		if (array2[5] != 0)
		{
			throw new Exception(Validate.Errors[(ErrorCode)array2[5]]);
		}
		return true;
	}
}
