using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NetStudio.Common.Authors;
using NetStudio.Common.IndusCom;
using NetStudio.Common.Manager;
using NetStudio.Common.Security;
using NetStudio.Fatek.Models;

namespace NetStudio.Fatek;

public class FatekProtocol : FatekBuilder
{
	private INetworkAdapter adapter;

	private const int FORMAT_READ = 9;

	private const int FORMAT_WRITE = 9;

	public Author Author => new Author();

	public FatekProtocol(INetworkAdapter adapter)
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

	public async Task<IPSResult> GetPlcStatus(int stationNo)
	{
		IPSResult result = new IPSResult
		{
			Status = CommStatus.Error,
			Message = "Read request failed."
		};
		await Task.Run(delegate
		{
			try
			{
				string plcStatusMsg = GetPlcStatusMsg(stationNo);
				lock (adapter)
				{
					adapter.Write(plcStatusMsg);
					string text = adapter.ReadString(9);
					if (text != null && text.Length > 0 && text[0] == '\u0002')
					{
						string key = text.Substring(5, 1);
						if (FatekUtility.Errors.ContainsKey(key))
						{
							result.Message = FatekUtility.Errors[key];
						}
						else
						{
							string s = text.Substring(6, 2);
							result.Values = Convert.FromHexString(s);
							result.Status = CommStatus.Success;
							result.Message = "Read request successfully.";
						}
					}
				}
			}
			catch (Exception ex)
			{
				result.Status = CommStatus.Timeout;
				result.Message = ex.Message;
			}
		});
		return result;
	}

	public async Task<IPSResult> PlcControl(int stationNo, ControlCode ctrl)
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
				string plcControlMsg = GetPlcControlMsg(stationNo, ctrl);
				lock (adapter)
				{
					adapter.Write(plcControlMsg);
					string text = adapter.ReadString(9);
					if (text != null && text.Length > 0 && text[0] == '\u0002')
					{
						string key = text.Substring(5, 1);
						if (FatekUtility.Errors.ContainsKey(key))
						{
							result.Message = FatekUtility.Errors[key];
						}
						else
						{
							result.Status = CommStatus.Success;
							result.Message = "Write data: successfully.";
						}
					}
				}
			}
			catch (Exception ex)
			{
				result.Status = CommStatus.Timeout;
				result.Message = ex.Message;
			}
		});
		return result;
	}

	public async Task<IPSResult> SingleDiscreteControl(int stationNo, RunningCode runCode, string discreteNo)
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
				string text = string.Empty;
				int num2 = 0;
				int num3 = 0;
				string singleDiscreteControlMsg = GetSingleDiscreteControlMsg(stationNo, runCode, discreteNo);
				lock (adapter)
				{
					do
					{
						try
						{
							num3++;
							num2 = adapter.Write(singleDiscreteControlMsg);
							text = adapter.ReadString(9);
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
					while ((num2 != singleDiscreteControlMsg.Length || text.Length < 9 || (text.Length >= 9 && text[0] != '\u0002')) && num3 <= num);
				}
				if (text.Length >= 9 && (text.Length < 9 || text[0] == '\u0002'))
				{
					if (text[0] == '\u0002')
					{
						string key = text.Substring(5, 1);
						if (FatekUtility.Errors.ContainsKey(key))
						{
							iPSResult.Message = FatekUtility.Errors[key];
						}
						else
						{
							iPSResult.Status = CommStatus.Success;
							iPSResult.Message = "Write data: successfully.";
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
				iPSResult.Status = CommStatus.Timeout;
				iPSResult.Message = ex2.Message;
			}
			return iPSResult;
		});
	}

	public async Task<IPSResult> GetEnableOrDisable(int stationNo, ushort discreteNo, int numOfPoints)
	{
		IPSResult result = new IPSResult
		{
			Status = CommStatus.Error,
			Message = "Read request failed."
		};
		await Task.Run(delegate
		{
			try
			{
				string enableOrDisableMsg = GetEnableOrDisableMsg(stationNo, discreteNo, numOfPoints);
				lock (adapter)
				{
					adapter.Write(enableOrDisableMsg);
					string text = adapter.ReadString(9);
					if (text != null && text.Length > 0 && text[0] == '\u0002')
					{
						string key = text.Substring(5, 1);
						if (FatekUtility.Errors.ContainsKey(key))
						{
							result.Message = FatekUtility.Errors[key];
						}
						else
						{
							string s = text.Substring(6, 2);
							result.Values = Convert.FromHexString(s);
							result.Status = CommStatus.Success;
							result.Message = "Read request successfully.";
						}
					}
				}
			}
			catch (Exception ex)
			{
				result.Status = CommStatus.Timeout;
				result.Message = ex.Message;
			}
		});
		return result;
	}

	public async Task<IPSResult> ReadBits(int stationNo, string discreteNo, int numOfPoints)
	{
		
		IPSResult result = new IPSResult
		{
			Status = CommStatus.Error,
			Message = "Read request failed."
		};
		await Task.Run(delegate
		{
			try
			{
				string data = ReadBitsMsg(stationNo, discreteNo, numOfPoints);
				lock (adapter)
				{
					adapter.Write(data);
					string text = adapter.ReadString(numOfPoints + 9);
					if (text != null && text.Length > 0 && text[0] == '\u0002')
					{
						string key = text.Substring(5, 1);
						if (FatekUtility.Errors.ContainsKey(key))
						{
							result.Message = FatekUtility.Errors[key];
						}
						else
						{
							result.Values_Hex = text.Substring(6, numOfPoints);
							result.Status = CommStatus.Success;
							result.Message = "Read request successfully.";
						}
					}
				}
			}
			catch (Exception ex)
			{
				result.Status = CommStatus.Timeout;
				result.Message = ex.Message;
			}
		});
		return result;
	}

	public async Task<IPSResult> WriteBits(int stationNo, string discreteNo, bool[] values)
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
				int num = values.Length;
				string values2 = string.Join("", values.Select((bool bool_0) => (!bool_0) ? "0" : "1"));
				string data = WriteBitsMsg(stationNo, discreteNo, num, values2);
				lock (adapter)
				{
					adapter.Write(data);
					string text = adapter.ReadString(num + 9);
					if (text != null && text.Length > 0 && text[0] == '\u0002')
					{
						string key = text.Substring(5, 1);
						if (FatekUtility.Errors.ContainsKey(key))
						{
							result.Message = FatekUtility.Errors[key];
						}
						else
						{
							result.Values_Hex = text.Substring(6, num);
							result.Status = CommStatus.Success;
							result.Message = "Read request successfully.";
						}
					}
				}
			}
			catch (Exception ex)
			{
				result.Status = CommStatus.Timeout;
				result.Message = ex.Message;
			}
		});
		return result;
	}

	public async Task<IPSResult> ReadRegisters(ReadPacket RP)
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
				string text = string.Empty;
				int num = 0;
				int num2 = 0;
				int num3 = 4 * RP.Quantity;
				int num4 = num3 + 9;
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
								Task.Delay(RP.ReceivingDelay);
							}
							text = adapter.ReadString(num4);
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
					while ((num != RP.SendMsg.Length || text.Length < num4 || (text.Length >= num4 && text[0] != '\u0002')) && num2 <= RP.ConnectRetries);
				}
				if (text.Length >= 9 && (text.Length < num4 || text[0] == '\u0002'))
				{
					if (text[0] == '\u0002')
					{
						string key = text.Substring(5, 1);
						if (FatekUtility.Errors.ContainsKey(key))
						{
							iPSResult.Message = FatekUtility.Errors[key];
						}
						else
						{
							iPSResult.Values_Hex = text.Substring(6, num3);
							iPSResult.Status = CommStatus.Success;
							iPSResult.Message = "Read request successfully.";
						}
					}
					else
					{
						iPSResult.Message = "An unknown error.";
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

	public async Task<IPSResult> WriteRegisters(WritePacket WP)
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
				string text = string.Empty;
				int num = 0;
				int num2 = 0;
				string text2 = WriteRegistersMsg(WP);
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
					while ((num != text2.Length || text.Length < 9 || (text.Length >= 9 && text[0] != '\u0002')) && num2 <= WP.ConnectRetries);
				}
				if (text.Length >= 9 && (text.Length < 9 || text[0] == '\u0002'))
				{
					if (text[0] == '\u0002')
					{
						string key = text.Substring(5, 1);
						if (FatekUtility.Errors.ContainsKey(key))
						{
							iPSResult.Message = FatekUtility.Errors[key];
						}
						else
						{
							iPSResult.Values_Hex = text.Substring(6, WP.Quantity);
							iPSResult.Status = CommStatus.Success;
							iPSResult.Message = "Write data: successfully.";
						}
					}
					else
					{
						iPSResult.Message = "An unknown error.";
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
				iPSResult.Status = CommStatus.Timeout;
				iPSResult.Message = ex2.Message;
			}
			return iPSResult;
		});
	}
}
