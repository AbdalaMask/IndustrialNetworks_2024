using System;
using System.ServiceModel;
using System.Threading.Tasks;
using NetStudio.Common.Manager;
using NetStudio.DriverComm.Interfaces;
using NetStudio.DriverComm.Models;
using NetStudio.DriverComm.Services;

namespace NetStudio.DriverComm;

public class ClientHelper
{
	private static IDriverServerManager? driverServerManager;

	private static InstanceContext? instanceContext;

	public static string IP { get; set; }

	public static int Port { get; set; }

	public static string UserName { get; set; }

	public static string Password { get; set; }

	public static ClientInfo ClientInfo { get; set; }

	public static IndustrialProtocol IndustrialProtocol { get; set; }

	public static IAppSettingManager AppSettings
	{
		get
		{
			NetTcpBinding netTcpBindingWithWindows = CommUtility.GetNetTcpBindingWithWindows();
			EndpointAddress remoteAddress = new EndpointAddress($"net.tcp://{IP}:{Port}/NetStudio/{"Settings"}");
			ChannelFactory<IAppSettingManager> channelFactory = new ChannelFactory<IAppSettingManager>(netTcpBindingWithWindows, remoteAddress);
			if (!string.IsNullOrEmpty(UserName) && !string.IsNullOrEmpty(Password) && !IP.Contains("127.0.0.1") && !IP.Contains("localhost"))
			{
				channelFactory.Credentials.Windows.ClientCredential.UserName = UserName;
				channelFactory.Credentials.Windows.ClientCredential.Password = Password;
			}
			return channelFactory.CreateChannel();
		}
	}

	public static IEditManger Editor
	{
		get
		{
			NetTcpBinding netTcpBindingWithWindows = CommUtility.GetNetTcpBindingWithWindows();
			EndpointAddress remoteAddress = new EndpointAddress($"net.tcp://{IP}:{Port}/NetStudio/{"EditManger"}");
			ChannelFactory<IEditManger> channelFactory = new ChannelFactory<IEditManger>(netTcpBindingWithWindows, remoteAddress);
			if (!string.IsNullOrEmpty(UserName) && !string.IsNullOrEmpty(Password) && !IP.Contains("127.0.0.1") && !IP.Contains("localhost"))
			{
				channelFactory.Credentials.Windows.ClientCredential.UserName = UserName;
				channelFactory.Credentials.Windows.ClientCredential.Password = Password;
			}
			return channelFactory.CreateChannel();
		}
	}

	public static IDriverServerManager Driver
	{
		get
		{
			if (driverServerManager == null || Status != CommunicationState.Opened)
			{
				instanceContext = new InstanceContext(new DataChangeEvent());
				NetTcpBinding netTcpBindingWithWindows = CommUtility.GetNetTcpBindingWithWindows();
				EndpointAddress remoteAddress = new EndpointAddress($"net.tcp://{IP}:{Port}/NetStudio/{"Driver"}");
				DuplexChannelFactory<IDriverServerManager> duplexChannelFactory = new DuplexChannelFactory<IDriverServerManager>(instanceContext, netTcpBindingWithWindows, remoteAddress);
				if (!string.IsNullOrEmpty(UserName) && !string.IsNullOrEmpty(Password) && !IP.Contains("127.0.0.1") && !IP.Contains("localhost"))
				{
					duplexChannelFactory.Credentials.Windows.ClientCredential.UserName = UserName;
					duplexChannelFactory.Credentials.Windows.ClientCredential.Password = Password;
				}
				driverServerManager = duplexChannelFactory.CreateChannel();
				Status = CommunicationState.Opened;
			}
			return driverServerManager;
		}
	}

	public static CommunicationState Status { get; set; }

	public static async Task<CommunicationState> GetConnectionStatusAsync()
	{
		if (driverServerManager == null)
		{
			return CommunicationState.Closed;
		}
		ApiResponse response = new ApiResponse
		{
			Success = false,
			Message = "Disconnect"
		};
		try
		{
			response = await driverServerManager.CheckConnectAsync();
		}
		catch (Exception ex)
		{
			response = response ?? new ApiResponse();
			response.Success = false;
			response.Message = ex.Message;
		}
		Status = (response.Success ? CommunicationState.Opened : CommunicationState.Closed);
		if (ClientDataSource.OnCommunicationStateChanged != null)
		{
			ClientDataSource.OnCommunicationStateChanged(Status);
		}
		return Status;
	}

	static ClientHelper()
	{
		IP = "127.0.0.1";
		Port = 5012;
		UserName = string.Empty;
		Password = string.Empty;
		ClientInfo = new ClientInfo();
		IndustrialProtocol = null;
		driverServerManager = null;
		instanceContext = null;
		Status = CommunicationState.Closed;
	}
}
