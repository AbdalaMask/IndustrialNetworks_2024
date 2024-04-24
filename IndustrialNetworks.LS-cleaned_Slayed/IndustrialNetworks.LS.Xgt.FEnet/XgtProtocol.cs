using System;
using System.IO;
using System.Threading.Tasks;
using NetStudio.Common.Authors;
using NetStudio.Common.IndusCom;
using NetStudio.Common.Manager;
using NetStudio.Common.Security;

namespace NetStudio.LS.Xgt.FEnet;

public class XgtProtocol : XgtBuilder
{
	private INetworkAdapter adapter;

	private const int FORMAT_READ = 32;

	private const int FORMAT_WRITE = 9;

	public Author Author => new Author();

	public XgtProtocol(INetworkAdapter adapter)
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
				int num = 32 + 2 * RP.Quantity;
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
								Task.Delay(RP.ReceivingDelay);
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
					while ((num2 != RP.SendBytes.Length || array.Length < num || (array.Length >= num && (array[0] != 76 || array[1] != 83))) && num3 <= RP.ConnectRetries);
				}
				if (num2 == RP.SendBytes.Length && array.Length >= num && array.Length != 0 && array.Length > 32)
				{
					int num4 = BitConverter.ToUInt16(new byte[2]
					{
						array[30],
						array[31]
					});
					if (32 + num4 >= array.Length)
					{
						iPSResult.Values = new byte[num4];
						for (int i = 0; i < num4; i++)
						{
							iPSResult.Values[i] = array[i + 32];
						}
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

	public async Task<IPSResult> WriteAsync(WritePacket WP)
	{
	
		return await Task.Run(delegate
		{
			IPSResult iPSResult = new IPSResult
			{
				Status = CommStatus.Error,
				Message = "Write data: failure."
			};
			byte[] array = Array.Empty<byte>();
			int num = 0;
			int num2 = 0;
			int num3 = 30;
			byte[] array2 = WritingDirectVariableIndividually(WP);
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
							Task.Delay(WP.ReceivingDelay);
						}
						array = adapter.Read(num3);
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
				while ((num != array2.Length || array.Length < num3 || (array.Length >= num3 && array.Length > 1 && (array[0] != 76 || array[1] != 83))) && num2 <= WP.ConnectRetries);
				iPSResult.Status = CommStatus.Success;
				iPSResult.Message = "Write data: successfully.";
				return iPSResult;
			}
		});
	}
}
