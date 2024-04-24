using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NetStudio.Common.IndusCom;
using NetStudio.Common.Manager;
using NetStudio.Common.Security;
using NetStudio.Omron.Models;

namespace NetStudio.Omron.HostLink;

public class HostLinkFinsProtocol : HostLinkFinsBuilder
{
	private INetworkAdapter adapter;

	private Validate validate;

	public HostLinkFinsProtocol(INetworkAdapter adapter)
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
				if (string.IsNullOrEmpty(RP.SendMsg))
				{
					throw new ArgumentNullException();
				}
				string text = string.Empty;
				int num = 0;
				int num2 = 0;
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
							text = adapter.ReadString(RP.NumOfRecvBytes);
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
					while ((num != RP.SendMsg.Length || text.Length < RP.NumOfRecvBytes || (text.Length >= RP.NumOfRecvBytes && text[0] != '@')) && num2 <= RP.ConnectRetries);
				}
				if (num == RP.SendMsg.Length && text.Length >= RP.NumOfRecvBytes && (text.Length <= 0 || text[0] == '@'))
				{
					string code = text.Substring(5, 2);
					validate.EndCode(code);
					if (RP.NumOfRecvBytes == text.Length)
					{
						iPSResult.Values_Hex = text.Substring(23, RP.NumOfchars);
					}
					else
					{
						int num3 = RP.NumOfRecvBytes - text.Length;
						iPSResult.Values_Hex = text.Substring(23, RP.NumOfchars - num3);
					}
					iPSResult.Status = CommStatus.Success;
					iPSResult.Message = "Read request successfully.";
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

	public async Task<IPSResult> Write(int unitNo, string memoryAreaCode, int wordAddress, int bitAddress, int numOfElements, string dataHex)
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
				string text = WriteMsg(unitNo, memoryAreaCode, wordAddress, bitAddress, numOfElements, dataHex);
				string text2 = string.Empty;
				int num = 0;
				int num2 = 0;
				int num3 = 27;
				int num4 = 3;
				lock (adapter)
				{
					do
					{
						try
						{
							num2++;
							num = adapter.Write(text);
							text2 = adapter.ReadString(num3);
						}
						catch (Exception ex)
						{
							if (num2 >= num4)
							{
								iPSResult.Status = CommStatus.Timeout;
								iPSResult.Message = ex.Message;
								return iPSResult;
							}
						}
					}
					while ((num != text.Length || text2.Length < num3 || (text2.Length >= num3 && text2[0] != '@')) && num2 <= num4);
				}
				if (text2.Length > 0 && text2[0] == '@')
				{
					string code = text2.Substring(19, 2);
					validate.EndCode(code);
					iPSResult.Status = CommStatus.Success;
					iPSResult.Message = "Write data: successfully.";
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

	public async Task<IPSResult> ReadCpuMode(ReadPacket RP)
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
				string text = ReadOperationModeMsg(RP.StationNo);
				string text2 = string.Empty;
				int num = 0;
				int num2 = 0;
				lock (adapter)
				{
					do
					{
						try
						{
							num2++;
							num = adapter.Write(text);
							if (RP.ReceivingDelay > 0)
							{
								Thread.Sleep(RP.ReceivingDelay);
							}
							text2 = adapter.ReadString(RP.NumOfRecvBytes);
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
					while ((num != text.Length || text2.Length < RP.NumOfRecvBytes || (text2.Length >= RP.NumOfRecvBytes && text2[0] != '@')) && num2 <= RP.ConnectRetries);
				}
				if (text2.Length > 0 && text2[0] == '@')
				{
					string code = text2.Substring(19, 2);
					validate.EndCode(code);
					iPSResult.Values_Hex = text2.Substring(23, 4);
					iPSResult.Status = CommStatus.Success;
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

	public async Task<IPSResult> WriteCpuMode(int unitNo, Mode mode)
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
				string text = OperationModeMsg(unitNo, mode);
				string text2 = string.Empty;
				int num2 = 0;
				int num3 = 0;
				int num4 = 27;
				lock (adapter)
				{
					do
					{
						try
						{
							num3++;
							num2 = adapter.Write(text);
							text2 = adapter.ReadString(num4);
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
					while ((num2 != text.Length || text2.Length < num4 || (text2.Length >= num4 && text2[0] != '@')) && num3 <= num);
				}
				if (text2.Length > 0 && text2[0] == '@')
				{
					string code = text2.Substring(19, 2);
					validate.EndCode(code);
					iPSResult.Status = CommStatus.Success;
					iPSResult.Message = "Write data: successfully.";
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
