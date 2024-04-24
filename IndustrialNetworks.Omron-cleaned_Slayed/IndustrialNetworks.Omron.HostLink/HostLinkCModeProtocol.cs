using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NetStudio.Common.IndusCom;
using NetStudio.Common.Manager;
using NetStudio.Common.Security;
using NetStudio.Omron.Models;

namespace NetStudio.Omron.HostLink;

public class HostLinkCModeProtocol : HostLinkCModeBuilder
{
	private INetworkAdapter adapter;

	private Validate validate;

	private const int FORMAT_READ = 11;

	private const int FORMAT_WRITE = 11;

	public HostLinkCModeProtocol(INetworkAdapter adapter)
	{
		this.adapter = adapter;
		validate = new Validate();
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

	public async Task<IPSResult> Read(ReadPacket RP)
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
				if (string.IsNullOrEmpty(RP.SendMsg))
				{
					throw new ArgumentNullException();
				}
				int num = RP.NumOfWords * 4;
				int length = 11 + num + 11;
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
								Thread.Sleep(RP.ReceivingDelay);
							}
							text = adapter.ReadString(length);
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
					while ((num2 != RP.SendMsg.Length || text.Length < RP.NumOfRecvBytes) && num3 <= RP.ConnectRetries);
				}
				if (text.Length < 11)
				{
					iPSResult.Status = CommStatus.Error;
					iPSResult.Message = "The communication frame is not in the correct format.";
				}
				else
				{
					string code = text.Substring(5, 2);
					validate.EndCode(code);
					iPSResult.Values_Hex = text.Substring(7, num);
					iPSResult.Status = CommStatus.Success;
					iPSResult.Message = "Read request successfully.";
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

	public async Task<IPSResult> ReadBig(ReadPacket RP)
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
				int length = RP.NumOfWords * 4;
				int numOfWords = RP.NumOfWords;
				int num = 0;
				int num2 = 0;
				if (numOfWords > 30)
				{
					num2 = 30;
					num = 130;
				}
				else
				{
					num2 = numOfWords;
					num = num2 * 4 + 11;
				}
				string text = string.Empty;
				lock (adapter)
				{
					adapter.Write(RP.SendMsg);
					if (RP.ReceivingDelay > 0)
					{
						Thread.Sleep(RP.ReceivingDelay);
					}
					text = adapter.ReadString(num);
					string code = text.Substring(5, 2);
					validate.EndCode(code);
					for (numOfWords -= num2; numOfWords > 0; numOfWords -= num2)
					{
						adapter.Write("\r");
						num2 = ((numOfWords > 31) ? 31 : numOfWords);
						num = num2 * 4 + 3;
						string text2 = adapter.ReadString(num);
						text = text.Substring(0, text.Length - 3) + text2;
					}
				}
				iPSResult.Values_Hex = text.Substring(7, length);
				iPSResult.Status = CommStatus.Success;
				iPSResult.Message = "Read request successfully.";
			}
			catch (TimeoutException ex)
			{
				iPSResult.Status = CommStatus.Timeout;
				iPSResult.Message = ex.Message;
			}
			catch (Exception ex2)
			{
				iPSResult.Status = CommStatus.Error;
				iPSResult.Message = ex2.Message;
			}
			return iPSResult;
		});
	}

	public IPSResult Write(int unitNo, string header, int startAddress, string dataHex)
	{
		IPSResult iPSResult = new IPSResult
		{
			Status = CommStatus.Error,
			Message = "Write data: failure."
		};
		try
		{
			int num = 3;
			string text = startAddress.ToString("D4") + dataHex;
			string text2 = WriteMsg(unitNo, header, text);
			string text3 = string.Empty;
			int num2 = 0;
			int num3 = 0;
			lock (adapter)
			{
				do
				{
					try
					{
						num3++;
						num2 = adapter.Write(text2);
						text3 = adapter.ReadString(11);
					}
					catch (TimeoutException ex)
					{
						Thread.Sleep(3);
						if (num3 >= num)
						{
							iPSResult.Status = CommStatus.Timeout;
							iPSResult.Message = ex.Message;
							return iPSResult;
						}
					}
				}
				while ((num2 != text2.Length || text3.Length < 11) && num3 <= num);
			}
			if (text3.Length < 11)
			{
				iPSResult.Status = CommStatus.Error;
				iPSResult.Message = "The communication frame is not in the correct format.";
			}
			else
			{
				string code = text3.Substring(5, 2);
				validate.EndCode(code);
				iPSResult.Status = CommStatus.Success;
				iPSResult.Message = "Write data: successfully.";
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
	}

	public IPSResult ReadCpuMode(int unitNo)
	{
		IPSResult iPSResult = new IPSResult
		{
			Status = CommStatus.Error,
			Message = "Read request failed."
		};
		try
		{
			string data = ReadMsg(unitNo, "MS");
			adapter.Write(data);
			Thread.Sleep(10);
			string text = adapter.ReadString(15);
			string code = text.Substring(5, 2);
			validate.EndCode(code);
			iPSResult.Values_Hex = text.Substring(7, 4);
			iPSResult.Status = CommStatus.Success;
			iPSResult.Message = "Read request successfully.";
		}
		catch (TimeoutException ex)
		{
			iPSResult.Status = CommStatus.Timeout;
			iPSResult.Message = ex.Message;
		}
		catch (Exception ex2)
		{
			iPSResult.Status = CommStatus.Error;
			iPSResult.Message = ex2.Message;
		}
		return iPSResult;
	}

	public IPSResult WriteMode(int unitNo, Mode mode)
	{
		IPSResult iPSResult = new IPSResult
		{
			Status = CommStatus.Error,
			Message = "Write data: failure."
		};
		try
		{
            string data = WriteMsg(unitNo, "SC", mode switch
			{
				Mode.RUN => "03", 
				Mode.MONITOR => "02", 
				_ => "00", 
			});
			adapter.Write(data);
			Thread.Sleep(10);
			string code = adapter.ReadString(11).Substring(5, 2);
			validate.EndCode(code);
			iPSResult.Status = CommStatus.Success;
			iPSResult.Message = "Write data: successfully.";
		}
		catch (TimeoutException ex)
		{
			iPSResult.Status = CommStatus.Timeout;
			iPSResult.Message = ex.Message;
		}
		catch (Exception ex2)
		{
			iPSResult.Status = CommStatus.Error;
			iPSResult.Message = ex2.Message;
		}
		return iPSResult;
	}

	public IPSResult ReadErrors(int unitNo)
	{
		IPSResult iPSResult = new IPSResult
		{
			Status = CommStatus.Error,
			Message = "Write data: failure."
		};
		try
		{
			string data = ReadMsg(unitNo, "MF", "00");
			adapter.Write(data);
			Thread.Sleep(10);
			string text = adapter.ReadString(19);
			validate.EndCode(text.Substring(5, 2));
			iPSResult.Values_Hex = text.Substring(7, 8);
			iPSResult.Status = CommStatus.Success;
			iPSResult.Message = "Read request successfully.";
		}
		catch (TimeoutException ex)
		{
			iPSResult.Status = CommStatus.Timeout;
			iPSResult.Message = ex.Message;
		}
		catch (Exception ex2)
		{
			iPSResult.Status = CommStatus.Error;
			iPSResult.Message = ex2.Message;
		}
		return iPSResult;
	}

	public IPSResult ReadModel(int unitNo)
	{
		IPSResult iPSResult = new IPSResult
		{
			Status = CommStatus.Error,
			Message = "Write data: failure."
		};
		try
		{
			string data = ReadMsg(unitNo, "MM");
			adapter.Write(data);
			Thread.Sleep(10);
			string text = adapter.ReadString(13);
			string code = text.Substring(5, 2);
			validate.EndCode(code);
			string modelCode = text.Substring(7, 2);
			iPSResult.Values_Hex = GetModelMsg(modelCode);
			iPSResult.Status = CommStatus.Success;
			iPSResult.Message = "Read request successfully.";
		}
		catch (TimeoutException ex)
		{
			iPSResult.Status = CommStatus.Timeout;
			iPSResult.Message = ex.Message;
		}
		catch (Exception ex2)
		{
			iPSResult.Status = CommStatus.Error;
			iPSResult.Message = ex2.Message;
		}
		return iPSResult;
	}
}
