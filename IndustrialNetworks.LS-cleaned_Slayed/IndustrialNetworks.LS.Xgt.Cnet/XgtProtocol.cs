using System;
using System.IO;
using System.Threading.Tasks;
using NetStudio.Common.Authors;
using NetStudio.Common.DataTypes;
using NetStudio.Common.IndusCom;
using NetStudio.Common.Manager;
using NetStudio.Common.Security;

namespace NetStudio.LS.Xgt.Cnet;

public class XgtProtocol : XgtBuilder
{
	private INetworkAdapter adapter;

	private const int FORMAT_READ = 13;

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
				int num = 13 + 4 * RP.Quantity;
				string text = string.Empty;
				int num2 = 0;
				int num3 = 0;
				lock (adapter)
				{
					do
					{
						try
						{
							num3++;
							num2 = adapter.Write(RP.SendMsg);
							if (RP.ReceivingDelay > 0)
							{
								Task.Delay(RP.ReceivingDelay);
							}
							text = adapter.ReadString(num);
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
					while ((num2 != RP.SendMsg.Length || text.Length < num || (text.Length >= num && text[0] != '\u0006')) && num3 <= RP.ConnectRetries);
				}
				if (num2 == RP.SendMsg.Length && text.Length >= num && text.Length != 0)
				{
					if ((byte)(BYTE)BYTE.GetByteFromHex(text.Substring(1, 2)) == RP.StationNo)
					{
						switch (text[0])
						{
						default:
							iPSResult.Message = "An unknown error.";
							break;
						case '\u0015':
						{
							STRING key = text.Substring(6, 4);
							iPSResult.Message = XgtBuilder.Errors[key];
							break;
						}
						case '\u0006':
						{
							DINT dINT = 2 * (ushort)UINT.Parse(text.Substring(8, 2), base.ByteOrder);
							iPSResult.Values_Hex = text.Substring(10, dINT);
							iPSResult.Values = BYTE.GetBytesFromHex(iPSResult.Values_Hex);
							iPSResult.Status = CommStatus.Success;
							break;
						}
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

	public async Task<IPSResult> WriteAsync(WritePacket WP)
	{
	
		return await Task.Run(delegate
		{
			IPSResult iPSResult = new IPSResult
			{
				Status = CommStatus.Error,
				Message = "Write data: failure."
			};
			string text = string.Empty;
			int num = 0;
			int num2 = 0;
			string text2 = ((!WP.IsBit) ? WritingTheDirectVariableContinuously(WP) : WritingDirectVariableIndividually(WP));
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
							Task.Delay(WP.ReceivingDelay);
						}
						text = adapter.ReadString(9);
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
				while ((num != text2.Length || text.Length < 9 || (text.Length >= 9 && text[0] != '\u0006')) && num2 <= WP.ConnectRetries);
			}
			if ((byte)(BYTE)BYTE.GetByteFromHex(text.Substring(1, 2)) == WP.StationNo)
			{
				switch (text[0])
				{
				default:
					iPSResult.Message = "An unknown error.";
					break;
				case '\u0015':
				{
					STRING key = text.Substring(6, 4);
					iPSResult.Message = XgtBuilder.Errors[key];
					break;
				}
				case '\u0006':
					iPSResult.Status = CommStatus.Success;
					iPSResult.Message = "Write data: successfully.";
					break;
				}
			}
			return iPSResult;
		});
	}
}
