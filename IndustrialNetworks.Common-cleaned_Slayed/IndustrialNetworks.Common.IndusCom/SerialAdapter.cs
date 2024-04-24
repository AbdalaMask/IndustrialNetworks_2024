using System;
using System.IO.Ports;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NetStudio.Common.IndusCom;

public class SerialAdapter : IDisposable, INetworkAdapter
{
	private const int _delayTimeout = 2;

	private SerialPort _serialPort;

	public string PortName { get; set; } = "COM1";


	public int BaudRate { get; set; } = 115200;


	public int DataBits { get; set; } = 8;


	public Parity Parity { get; set; }

	public StopBits StopBits { get; set; } = StopBits.One;


	public Handshake Handshake { get; set; }

	public int SendTimeout { get; set; } = 500;


	public int ReceiveTimeout { get; set; } = 500;


	public int WaitingTime { get; set; }

	public bool Connected
	{
		get
		{
			if (_serialPort != null)
			{
				return _serialPort.IsOpen;
			}
			return false;
		}
	}

	public string ErrorMessage => "Lost connection to device.";

	public SerialAdapter()
	{
	}

	public SerialAdapter(SerialPort serialPort)
	{
		PortName = serialPort.PortName;
		BaudRate = serialPort.BaudRate;
		DataBits = serialPort.DataBits;
		Parity = serialPort.Parity;
		StopBits = serialPort.StopBits;
		SendTimeout = serialPort.WriteTimeout;
		ReceiveTimeout = serialPort.ReadTimeout;
	}

	public SerialAdapter(string port = "COM1", int baudRate = 19200, Parity parity = Parity.None, int dataBits = 8, StopBits stopBits = StopBits.One)
	{
		PortName = port;
		BaudRate = baudRate;
		DataBits = dataBits;
		Parity = parity;
		StopBits = stopBits;
		Handshake = Handshake.None;
		SendTimeout = 500;
		ReceiveTimeout = 500;
	}

	public bool Connect()
	{
		_serialPort = _serialPort ?? new SerialPort
		{
			PortName = PortName,
			BaudRate = BaudRate,
			DataBits = DataBits,
			Parity = Parity,
			StopBits = StopBits,
			Handshake = Handshake,
			ReadTimeout = ((ReceiveTimeout < 50) ? 50 : ReceiveTimeout),
			WriteTimeout = ((SendTimeout < 50) ? 50 : SendTimeout)
		};
		if (_serialPort.IsOpen)
		{
			_serialPort.Close();
		}
		_serialPort.Open();
		return true;
	}

	public bool Disconnect()
	{
		if (_serialPort != null)
		{
			_serialPort.Close();
		}
		return true;
	}

	public async Task<bool> ConnectAsync()
	{
		return await Task.Factory.StartNew((Func<bool>)Connect);
	}

	public async Task<bool> DisconnectAsync()
	{
		return await Task.Factory.StartNew((Func<bool>)Disconnect);
	}

	public void SetReceiveTimeout(int timeout)
	{
		ReceiveTimeout = timeout;
	}

	public void SetSendTimeout(int timeout)
	{
		SendTimeout = timeout;
	}

	public override string ToString()
	{
		return $"[{PortName}, {BaudRate}, {DataBits}, {Parity}, {StopBits}, {Handshake}]";
	}

	public void Dispose()
	{
		Disconnect();
	}

	public byte[] Read()
	{
		if (_serialPort == null || !_serialPort.IsOpen)
		{
			throw new TimeoutException();
		}
		byte[] array = new byte[_serialPort.BytesToRead];
		_serialPort.Read(array, 0, array.Length);
		_serialPort.DiscardInBuffer();
		return array;
	}

	public byte[] Read(int size)
	{
		if (_serialPort != null && _serialPort.IsOpen)
		{
			int num = 0;
			while (_serialPort.BytesToRead < size)
			{
				Thread.Sleep(2);
				if (num * 2 < _serialPort.ReadTimeout)
				{
					num++;
					continue;
				}
				if (_serialPort.BytesToRead != 0)
				{
					break;
				}
				throw new TimeoutException();
			}
			byte[] array = new byte[_serialPort.BytesToRead];
			_serialPort.Read(array, 0, array.Length);
			return array;
		}
		throw new TimeoutException();
	}

	public int Write(byte[] data)
	{
		if (_serialPort == null || !_serialPort.IsOpen)
		{
			throw new TimeoutException();
		}
		_serialPort.DiscardOutBuffer();
		_serialPort.Write(data, 0, data.Length);
		return data.Length;
	}

	public async Task<byte[]> ReadAsync()
	{
		return await Task.Run(() => Read());
	}

	public async Task<byte[]> ReadAsync(int size)
	{
		return await Task.Run(() => Read(size));
	}

	public async Task<int> WriteAsync(byte[] data)
	{
		
		return await Task.Run(() => Write(data));
	}

	public string ReadString()
	{
		if (_serialPort == null || !_serialPort.IsOpen)
		{
			throw new TimeoutException();
		}
		byte[] array = new byte[_serialPort.BytesToRead];
		_serialPort.Read(array, 0, array.Length);
		string @string = Encoding.ASCII.GetString(array);
		_serialPort.DiscardInBuffer();
		return @string;
	}

	public string ReadString(int length)
	{
		if (_serialPort != null && _serialPort.IsOpen)
		{
			int num = 0;
			while (_serialPort.BytesToRead < length)
			{
				Thread.Sleep(2);
				if (num * 2 < _serialPort.ReadTimeout)
				{
					num++;
					continue;
				}
				if (_serialPort.BytesToRead != 0)
				{
					break;
				}
				throw new TimeoutException();
			}
			string result = _serialPort.ReadExisting();
			_serialPort.DiscardOutBuffer();
			_serialPort.DiscardInBuffer();
			return result;
		}
		throw new TimeoutException();
	}

	public async Task<string> ReadStringAsync()
	{
		return await Task.Run(() => ReadString());
	}

	public async Task<string> ReadStringAsync(int length)
	{
		return await Task.Run(() => ReadString(length));
	}

	public int Write(string data)
	{
		if (_serialPort == null || !_serialPort.IsOpen)
		{
			throw new TimeoutException();
		}
		_serialPort.Write(data);
		return data.Length;
	}

	public async Task<int> WriteAsync(string data)
	{
		 
		return await Task.Run(() => Write(data));
	}
}
