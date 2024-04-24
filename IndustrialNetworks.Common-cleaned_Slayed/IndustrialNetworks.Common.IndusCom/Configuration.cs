namespace NetStudio.Common.IndusCom;

public class Configuration
{
	private int _DelayTimeToRead;

	private int _SendTimeout = 500;

	private int _ReceiveTimeout = 500;

	public int ConnectionType { get; set; }

	public int ProtocolType { get; set; }

	public int DelayTimeToRead
	{
		get
		{
			return _DelayTimeToRead;
		}
		set
		{
			if (value < 0)
			{
				_DelayTimeToRead = 0;
			}
			else if (value < 250)
			{
				_DelayTimeToRead = value;
			}
			else
			{
				_DelayTimeToRead = 250;
			}
		}
	}

	public int SendTimeout
	{
		get
		{
			return _SendTimeout;
		}
		set
		{
			if (value < 0)
			{
				_SendTimeout = 0;
			}
			else if (value > 2000)
			{
				_SendTimeout = 2000;
			}
			else
			{
				_SendTimeout = value;
			}
		}
	}

	public int ReceiveTimeout
	{
		get
		{
			return _ReceiveTimeout;
		}
		set
		{
			if (value < 0)
			{
				_ReceiveTimeout = 0;
			}
			else if (value > 2000)
			{
				_ReceiveTimeout = 2000;
			}
			else
			{
				_ReceiveTimeout = value;
			}
		}
	}

	public string IP { get; set; }

	public int Port { get; set; }

	public string PortName { get; set; }

	public int BaudRate { get; set; }

	public int DataBits { get; set; }

	public int Parity { get; set; }

	public int StopBits { get; set; }

	public int Handshake { get; set; }

	public Configuration()
	{
		IP = "127.0.0.1";
		Port = 502;
		PortName = "COM10";
		BaudRate = 9600;
		DataBits = 8;
		StopBits = 1;
		base.MemberwiseClone();
	}
}
