using System;
using System.Security.Authentication;
using System.ServiceModel;

namespace NetStudio.DriverComm;

public class CommUtility
{
	public const string DriverService = "Driver";

	public const string EditMangerService = "EditManger";

	public const string SettingService = "Settings";

	public static NetTcpBinding GetNetTcpBindingSimple()
	{
		return new NetTcpBinding
		{
			CloseTimeout = TimeSpan.FromSeconds(15.0),
			OpenTimeout = TimeSpan.FromSeconds(15.0),
			SendTimeout = TimeSpan.FromDays(24.0),
			ReceiveTimeout = TimeSpan.FromSeconds(15.0)
		};
	}

	public static NetTcpBinding GetNetTcpBindingWithWindows()
	{
		return new NetTcpBinding(SecurityMode.Transport)
		{
			Security = 
			{
				Transport = 
				{
					ClientCredentialType = TcpClientCredentialType.Windows
				}
			},
			TransferMode = TransferMode.Buffered,
			MaxConnections = 10,
			ReceiveTimeout = TimeSpan.FromMinutes(20.0),
			SendTimeout = TimeSpan.FromMinutes(20.0),
			OpenTimeout = TimeSpan.FromSeconds(15.0),
			CloseTimeout = TimeSpan.FromSeconds(15.0),
			MaxReceivedMessageSize = 2000000L,
			MaxBufferPoolSize = 2000000L,
			MaxBufferSize = 2000000
		};
	}

	public static NetTcpBinding GetNetTcpBindingWithSecurity()
	{
		NetTcpBinding netTcpBinding = new NetTcpBinding(SecurityMode.Transport);
		netTcpBinding.Security.Message.ClientCredentialType = MessageCredentialType.None;
		netTcpBinding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Certificate;
		netTcpBinding.Security.Transport.SslProtocols = SslProtocols.Tls12;
		netTcpBinding.SendTimeout = new TimeSpan(72, 0, 0);
		netTcpBinding.ReceiveTimeout = new TimeSpan(72, 0, 0);
		netTcpBinding.MaxReceivedMessageSize = 524288000L;
		netTcpBinding.MaxBufferSize = (int)netTcpBinding.MaxReceivedMessageSize;
		netTcpBinding.ReaderQuotas.MaxStringContentLength = (int)netTcpBinding.MaxReceivedMessageSize;
		return netTcpBinding;
	}
}
