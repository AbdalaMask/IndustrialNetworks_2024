using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NetStudio.Common.Authors;
using NetStudio.Common.IndusCom;
using NetStudio.Common.Manager;
using NetStudio.Common.Security;

namespace NetStudio.Mitsubishi.SLMP;

public sealed class SLMPProtocol
{
	private INetworkAdapter adapter;

	private const int MC3E_BYTES_FORMAT = 11;

	private const int MC4E_BYTES_FORMAT = 15;

	public Author Author => new Author();

	public Frame FrameMC { get; set; }

	public SLMPProtocol(INetworkAdapter adapter, Frame frame)
	{
		this.adapter = adapter;
		FrameMC = frame;
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
		
		IPSResult result = new IPSResult
		{
			Status = CommStatus.Error,
			Message = "Read request failed."
		};
		return await Task.Run(delegate
		{
			if (FrameMC == Frame.Frame3E)
			{
				if (RP.IsBit)
				{
					result = ReadBitMC3E(RP);
				}
				else
				{
					result = ReadDataMC3E(RP);
				}
			}
			else if (RP.IsBit)
			{
				result = ReadBitMC4E(RP);
			}
			else
			{
				result = ReadDataMC4E(RP);
			}
			return result;
		});
	}

	public async Task<IPSResult> WriteAsync(WritePacket WP)
	{
	
		IPSResult result = new IPSResult
		{
			Status = CommStatus.Error,
			Message = "Write data: failure."
		};
		return await Task.Run(delegate
		{
			if (FrameMC == Frame.Frame3E)
			{
				result = WriteDataMC3E(WP);
			}
			else
			{
				result = WriteDataMC4E(WP);
			}
			return result;
		});
	}

	public IPSResult ReadDataMC3E(ReadPacket RP)
	{
		IPSResult iPSResult = new IPSResult
		{
			Status = CommStatus.Error,
			Message = "Read request failed."
		};
		try
		{
			int size = 11 + RP.NumOfBytes;
			int num = 0;
			int num2 = 0;
			byte[] array = Array.Empty<byte>();
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
						array = adapter.Read(size);
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
				while ((num != RP.SendBytes.Length || array.Length < 11) && num2 <= RP.ConnectRetries);
			}
			if (num == RP.SendBytes.Length)
			{
				if (array.Length >= 11)
				{
					int num3 = BitConverter.ToUInt16(new byte[2]
					{
						array[7],
						array[8]
					});
					string text = SLMPUtility.CheckErrorCodes(BitConverter.ToUInt16(new byte[2]
					{
						array[9],
						array[10]
					}));
					if (string.IsNullOrEmpty(text))
					{
						num3 -= 2;
						byte[] array2 = new byte[num3];
						Array.Copy(array, 11, array2, 0, array2.Length);
						iPSResult.Values = new byte[array.Length - 11];
						Array.Copy(array, 11, iPSResult.Values, 0, iPSResult.Values.Length);
						iPSResult.Status = CommStatus.Success;
						iPSResult.Message = "Read request successfully.";
					}
					else
					{
						iPSResult.Message = text;
					}
				}
				else
				{
					iPSResult.Message = "An unknown error.";
				}
			}
		}
		catch (Exception ex2)
		{
			iPSResult.Status = CommStatus.Error;
			iPSResult.Message = ex2.Message;
		}
		return iPSResult;
	}

	public IPSResult WriteDataMC3E(WritePacket WP)
	{
		IPSResult iPSResult = new IPSResult
		{
			Status = CommStatus.Error,
			Message = "Write data: failure."
		};
		try
		{
			int size = 11;
			int num = 0;
			int num2 = 0;
			byte[] array = Array.Empty<byte>();
			lock (adapter)
			{
				do
				{
					try
					{
						num2++;
						num = adapter.Write(WP.Data);
						if (WP.ReceivingDelay > 0)
						{
							Thread.Sleep(WP.ReceivingDelay);
						}
						array = adapter.Read(size);
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
				while ((num != WP.Data.Length || array.Length < 11) && num2 <= WP.ConnectRetries);
			}
			if (num == WP.Data.Length)
			{
				if (array.Length >= 11)
				{
					byte[] array2 = new byte[BitConverter.ToUInt16(new byte[2]
					{
						array[7],
						array[8]
					})];
					Array.Copy(array, 9, array2, 0, array2.Length);
					string text = SLMPUtility.CheckErrorCodes(BitConverter.ToUInt16(array2));
					if (string.IsNullOrEmpty(text))
					{
						iPSResult.Status = CommStatus.Success;
						iPSResult.Message = "Write data: successfully.";
					}
					else
					{
						iPSResult.Message = text;
					}
				}
				else
				{
					iPSResult.Message = "An unknown error.";
				}
			}
		}
		catch (Exception ex2)
		{
			iPSResult.Status = CommStatus.Error;
			iPSResult.Message = ex2.Message;
		}
		return iPSResult;
	}

	public IPSResult ReadBitMC3E(ReadPacket RP)
	{
		IPSResult iPSResult = new IPSResult
		{
			Status = CommStatus.Error,
			Message = "Read request failed."
		};
		try
		{
			int size = 11 + RP.NumOfBytes;
			int num = 0;
			int num2 = 0;
			byte[] array = Array.Empty<byte>();
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
						array = adapter.Read(size);
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
				while ((num != RP.SendBytes.Length || array.Length < 11) && num2 <= RP.ConnectRetries);
			}
			if (num == RP.SendBytes.Length)
			{
				if (array.Length >= 11)
				{
					int num3 = BitConverter.ToUInt16(new byte[2]
					{
						array[7],
						array[8]
					});
					string text = SLMPUtility.CheckErrorCodes(BitConverter.ToUInt16(new byte[2]
					{
						array[9],
						array[10]
					}));
					if (string.IsNullOrEmpty(text))
					{
						num3 -= 2;
						iPSResult.Values = new byte[num3];
						Array.Copy(array, 11, iPSResult.Values, 0, iPSResult.Values.Length);
						iPSResult.Status = CommStatus.Success;
						iPSResult.Message = "Read request successfully.";
					}
					else
					{
						iPSResult.Message = text;
					}
				}
				else
				{
					iPSResult.Message = "An unknown error.";
				}
			}
		}
		catch (Exception ex2)
		{
			iPSResult.Status = CommStatus.Error;
			iPSResult.Message = ex2.Message;
		}
		return iPSResult;
	}

	public IPSResult ReadDataMC4E(ReadPacket RP)
	{
		IPSResult iPSResult = new IPSResult
		{
			Status = CommStatus.Error,
			Message = "Read request failed."
		};
		try
		{
			int size = 15 + RP.NumOfBytes;
			int num = 0;
			int num2 = 0;
			byte[] array = Array.Empty<byte>();
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
						array = adapter.Read(size);
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
				while ((num != RP.SendBytes.Length || array.Length < 15) && num2 <= RP.ConnectRetries);
			}
			if (num == RP.SendBytes.Length)
			{
				if (array.Length >= 15)
				{
					int num3 = BitConverter.ToUInt16(new byte[2]
					{
						array[11],
						array[12]
					});
					string text = SLMPUtility.CheckErrorCodes(BitConverter.ToUInt16(new byte[2]
					{
						array[13],
						array[14]
					}));
					if (string.IsNullOrEmpty(text))
					{
						num3 -= 2;
						iPSResult.Values = new byte[array.Length - 15];
						Array.Copy(array, 15, iPSResult.Values, 0, iPSResult.Values.Length);
						iPSResult.Status = CommStatus.Success;
						iPSResult.Message = "Read request successfully.";
					}
					else
					{
						iPSResult.Message = text;
					}
				}
				else
				{
					iPSResult.Message = "An unknown error.";
				}
			}
		}
		catch (Exception ex2)
		{
			iPSResult.Status = CommStatus.Error;
			iPSResult.Message = ex2.Message;
		}
		return iPSResult;
	}

	public IPSResult WriteDataMC4E(WritePacket WP)
	{
		IPSResult iPSResult = new IPSResult
		{
			Status = CommStatus.Error,
			Message = "Write data: failure."
		};
		try
		{
			int size = 15;
			int num = 0;
			int num2 = 0;
			byte[] array = Array.Empty<byte>();
			lock (adapter)
			{
				do
				{
					try
					{
						num2++;
						num = adapter.Write(WP.Data);
						if (WP.ReceivingDelay > 0)
						{
							Thread.Sleep(WP.ReceivingDelay);
						}
						array = adapter.Read(size);
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
				while ((num != WP.Data.Length || array.Length < 15) && num2 <= WP.ConnectRetries);
			}
			if (num == WP.Data.Length)
			{
				if (array.Length >= 15)
				{
					byte[] array2 = new byte[BitConverter.ToUInt16(new byte[2]
					{
						array[11],
						array[12]
					})];
					Array.Copy(array, 13, array2, 0, array2.Length);
					string text = SLMPUtility.CheckErrorCodes(BitConverter.ToUInt16(array2));
					if (string.IsNullOrEmpty(text))
					{
						iPSResult.Status = CommStatus.Success;
						iPSResult.Message = "Write data: successfully.";
					}
					else
					{
						iPSResult.Message = text;
					}
				}
				else
				{
					iPSResult.Message = "An unknown error.";
				}
			}
		}
		catch (Exception ex2)
		{
			iPSResult.Status = CommStatus.Error;
			iPSResult.Message = ex2.Message;
		}
		return iPSResult;
	}

	public IPSResult ReadBitMC4E(ReadPacket RP)
	{
		IPSResult iPSResult = new IPSResult
		{
			Status = CommStatus.Error,
			Message = "Read request failed."
		};
		try
		{
			int size = 15 + RP.NumOfBytes;
			int num = 0;
			int num2 = 0;
			byte[] array = Array.Empty<byte>();
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
						array = adapter.Read(size);
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
				while ((num != RP.SendBytes.Length || array.Length < 15) && num2 <= RP.ConnectRetries);
			}
			if (num == RP.SendBytes.Length)
			{
				if (array.Length >= 15)
				{
					int num3 = BitConverter.ToUInt16(new byte[2]
					{
						array[11],
						array[12]
					});
					string text = SLMPUtility.CheckErrorCodes(BitConverter.ToUInt16(new byte[2]
					{
						array[13],
						array[14]
					}));
					if (string.IsNullOrEmpty(text))
					{
						num3 -= 2;
						iPSResult.Values = new byte[num3];
						Array.Copy(array, 15, iPSResult.Values, 0, iPSResult.Values.Length);
						iPSResult.Status = CommStatus.Success;
						iPSResult.Message = "Read request successfully.";
					}
					else
					{
						iPSResult.Message = text;
					}
				}
				else
				{
					iPSResult.Message = "An unknown error.";
				}
			}
		}
		catch (Exception ex2)
		{
			iPSResult.Status = CommStatus.Error;
			iPSResult.Message = ex2.Message;
		}
		return iPSResult;
	}
}
