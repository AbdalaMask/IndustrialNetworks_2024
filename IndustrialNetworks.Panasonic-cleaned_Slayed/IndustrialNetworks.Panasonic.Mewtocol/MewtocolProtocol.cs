using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NetStudio.Common.Authors;
using NetStudio.Common.IndusCom;
using NetStudio.Common.Manager;
using NetStudio.Common.Security;
using NetStudio.Panasonic.Mewtocol.Codes;

namespace NetStudio.Panasonic.Mewtocol;

public class MewtocolProtocol : MessageBuilder
{
	public const int Timer_Resolution = 10;

	private INetworkAdapter adapter;

	private const int FORMAT_READ = 9;

	private const int FORMAT_WRITE = 9;

	public Author Author => new Author();

	public MewtocolProtocol(INetworkAdapter adapter)
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
				int num = 9 + 4 * RP.Quantity;
				int num2 = 0;
				int num3 = 0;
				string text = string.Empty;
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
								Thread.Sleep(RP.ReceivingDelay);
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
					while ((num2 != RP.SendMsg.Length || text.Length < num || (text.Length >= num && text[0] != '<')) && num3 <= RP.ConnectRetries);
				}
				if (text.Length >= num && (text.Length < num || text[0] == '<'))
				{
					switch (text[3])
					{
					case '$':
						iPSResult.Values_Hex = text.Substring(6, 4 * RP.Quantity);
						iPSResult.Status = CommStatus.Success;
						iPSResult.Message = "Read request successfully.";
						break;
					default:
						iPSResult.Message = "An unknown error.";
						break;
					case '!':
					{
						string key = text.Substring(4, 2);
						iPSResult.Message = MessageBuilder.Errors[key];
						break;
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

	public IPSResult Write(WritePacket WP)
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
			string text = string.Empty;
			lock (adapter)
			{
				do
				{
					try
					{
						num2++;
						num = adapter.Write(WP.SendMsg);
						if (WP.ReceivingDelay > 0)
						{
							Thread.Sleep(WP.ReceivingDelay);
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
				while ((num != WP.SendMsg.Length || text.Length < 9 || (text.Length >= 9 && text[0] != '<')) && num2 <= WP.ConnectRetries);
			}
			if (text.Length >= 9 && (text.Length < 9 || text[0] == '<'))
			{
				switch (text[3])
				{
				case '$':
					iPSResult.Status = CommStatus.Success;
					iPSResult.Message = "Write data: successfully.";
					break;
				default:
					iPSResult.Message = "An unknown error.";
					break;
				case '!':
				{
					string key = text.Substring(4, 2);
					iPSResult.Message = MessageBuilder.Errors[key];
					break;
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
	}

	public IPSResult WriteBit(WritePacket WP)
	{
		return WP.AreaCode switch
		{
			AreaCode.OperationMode => RemoteControl(WP), 
			AreaCode.Contact => WriteContact(WP), 
			_ => throw new NotSupportedException(), 
		};
	}

	public IPSResult WriteContact(WritePacket WP)
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
			string text = string.Empty;
			lock (adapter)
			{
				do
				{
					try
					{
						num2++;
						num = adapter.Write(WP.SendMsg);
						if (WP.ReceivingDelay > 0)
						{
							Thread.Sleep(WP.ReceivingDelay);
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
				while ((num != WP.SendMsg.Length || text.Length < 9 || (text.Length >= 9 && text[0] != '%')) && num2 <= WP.ConnectRetries);
			}
			if (text.Length >= 9 && (text.Length < 9 || text[0] == '%'))
			{
				switch (text[3])
				{
				case '$':
					iPSResult.Status = CommStatus.Success;
					iPSResult.Message = "Write data: successfully.";
					break;
				default:
					iPSResult.Message = "An unknown error.";
					break;
				case '!':
				{
					string key = text.Substring(4, 2);
					iPSResult.Message = MessageBuilder.Errors[key];
					break;
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
	}

	public IPSResult RemoteControl(WritePacket WP)
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
			string text = string.Empty;
			int length = 10;
			lock (adapter)
			{
				do
				{
					try
					{
						num2++;
						num = adapter.Write(WP.SendMsg);
						if (WP.ReceivingDelay > 0)
						{
							Thread.Sleep(WP.ReceivingDelay);
						}
						text = adapter.ReadString(length);
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
				while ((num != WP.SendMsg.Length || text.Length < 9 || (text.Length >= 9 && text[0] != '%')) && num2 <= WP.ConnectRetries);
			}
			if (text.Length >= 9 && (text.Length < 9 || text[0] == '%'))
			{
				switch (text[3])
				{
				case '$':
					iPSResult.Status = CommStatus.Success;
					iPSResult.Message = "Write data: successfully.";
					break;
				default:
					iPSResult.Message = "An unknown error.";
					break;
				case '!':
				{
					string key = text.Substring(4, 2);
					iPSResult.Message = MessageBuilder.Errors[key];
					break;
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
	}
}
