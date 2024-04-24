using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NetStudio.Common.IndusCom;

public class EthernetAdapter : IDisposable, INetworkAdapter
{
	private const int _delayTimeout = 2;

	private Socket _socket;

	public ProtocolType ProtocolType { get; set; } = ProtocolType.Tcp;


	public string IP { get; set; }

	public int Port { get; set; }

	public int SendTimeout { get; set; }

	public int ReceiveTimeout { get; set; }

	public bool Connected
	{
		get
		{
			if (_socket != null)
			{
				return _socket.Connected;
			}
			return false;
		}
	}

	public string ErrorMessage => "Lost connection to device.";

	public EthernetAdapter()
	{
        IP = "127.0.0.1";
		SendTimeout = 1000;
		ReceiveTimeout = 1000;
		base.MemberwiseClone();
	}

	public EthernetAdapter(ProtocolType type)
	{
        IP = "127.0.0.1";
		SendTimeout = 1000;
		ReceiveTimeout = 1000;
		base.MemberwiseClone();
		ProtocolType = type;
	}

	public EthernetAdapter(string ipAddress = "127.0.0.1", int port = 502, int sendTimeout = 500, int receiveTimeout = 500, ProtocolType type = ProtocolType.Tcp)
	{
        IP = "127.0.0.1";
		SendTimeout = 1000;
		ReceiveTimeout = 1000;
		base.MemberwiseClone();
		IP = ipAddress;
		Port = port;
		SendTimeout = sendTimeout;
		ReceiveTimeout = receiveTimeout;
		ProtocolType = type;
	}

	public bool Connect()
	{
		if (ProtocolType == ProtocolType.Tcp)
		{
			_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
			{
				SendTimeout = SendTimeout,
				ReceiveTimeout = ReceiveTimeout
			};
		}
		else
		{
			_socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp)
			{
				SendTimeout = SendTimeout,
				ReceiveTimeout = ReceiveTimeout
			};
		}
		IAsyncResult asyncResult = _socket.BeginConnect(IP, Port, null, null);
		asyncResult.AsyncWaitHandle.WaitOne(3000, exitContext: true);
		if (_socket.Connected)
		{
			_socket.EndConnect(asyncResult);
		}
		else
		{
			_socket.Close();
		}
		return _socket.Connected;
	}

	public bool Disconnect()
	{
		if (_socket != null)
		{
			if (_socket.Connected)
			{
				_socket.Shutdown(SocketShutdown.Both);
			}
			_socket.Close();
		}
		return true;
	}

	public async Task<bool> ConnectAsync()
	{
		return await Task.Run((Func<bool>)Connect);
	}

	public async Task<bool> DisconnectAsync()
	{
		return await Task.Run((Func<bool>)Disconnect);
	}

	public void SetReceiveTimeout(int timeout)
	{
		ReceiveTimeout = timeout;
	}

	public void SetSendTimeout(int timeout)
	{
		SendTimeout = timeout;
	}

	private void ConfigureTcpSocket(Socket tcpSocket)
	{
		tcpSocket.ExclusiveAddressUse = true;
		tcpSocket.LingerState = new LingerOption(enable: true, 10);
		tcpSocket.NoDelay = true;
		tcpSocket.ReceiveBufferSize = 8192;
		tcpSocket.ReceiveTimeout = ReceiveTimeout;
		tcpSocket.SendBufferSize = 8192;
		tcpSocket.SendTimeout = SendTimeout;
		tcpSocket.Ttl = 42;
	}

	public override string ToString()
	{
		return $"[IP Address={IP}, Port={Port}]";
	}

	public void Dispose()
	{
		Disconnect();
	}

	public byte[] Read()
	{
		if (_socket != null && _socket.Connected)
		{
			int num = 0;
			while (_socket.Available <= 0)
			{
				Thread.Sleep(2);
				if (num * 2 < _socket.ReceiveTimeout)
				{
					num++;
					continue;
				}
				if (_socket.Available != 0)
				{
					break;
				}
				throw new TimeoutException();
			}
			byte[] array = new byte[_socket.Available];
			_socket.Receive(array, 0, array.Length, SocketFlags.None);
			return array;
		}
		throw new TimeoutException();
	}

	public byte[] Read(int size)
	{
		if (_socket != null && _socket.Connected)
		{
			int num = 0;
			while (_socket.Available < size)
			{
				Thread.Sleep(2);
				if (num * 2 < _socket.ReceiveTimeout)
				{
					num++;
					continue;
				}
				if (_socket.Available != 0)
				{
					break;
				}
				throw new TimeoutException();
			}
			byte[] array = new byte[_socket.Available];
			_socket.Receive(array, 0, array.Length, SocketFlags.None);
			return array;
		}
		throw new TimeoutException();
	}

	public async Task<byte[]> ReadAsync()
	{
		if (_socket != null && _socket.Connected)
		{
			int count = 0;
			while (_socket.Available <= 0)
			{
				await Task.Delay(2);
				if (count * 2 < _socket.ReceiveTimeout)
				{
					count++;
					continue;
				}
				if (_socket.Available != 0)
				{
					break;
				}
				throw new TimeoutException();
			}
			byte[] recvBuffer = new byte[_socket.Available];
			await _socket.ReceiveAsync(new ArraySegment<byte>(recvBuffer), SocketFlags.None);
			return recvBuffer;
		}
		throw new TimeoutException();
	}

	public async Task<byte[]> ReadAsync(int size)
	{
		if (_socket != null && _socket.Connected)
		{
			int count = 0;
			while (_socket.Available < size)
			{
				await Task.Delay(2);
				if (count * 2 < ReceiveTimeout)
				{
					count++;
					continue;
				}
				if (_socket.Available != 0)
				{
					break;
				}
				throw new TimeoutException();
			}
			byte[] recvBuffer = new byte[_socket.Available];
			await _socket.ReceiveAsync(new ArraySegment<byte>(recvBuffer), SocketFlags.None);
			return recvBuffer;
		}
		throw new TimeoutException();
	}

	public int Write(byte[] data)
	{
		if (_socket == null || !_socket.Connected)
		{
			throw new TimeoutException();
		}
		return _socket.Send(data, SocketFlags.None);
	}

	public async Task<int> WriteAsync(byte[] data)
	{
		if (_socket == null || !_socket.Connected)
		{
			throw new TimeoutException();
		}
		return await _socket.SendAsync(data, SocketFlags.None);
	}

	public string ReadString()
	{
		if (_socket != null && _socket.Connected)
		{
			int num = 0;
			while (_socket.Available <= 0)
			{
				Thread.Sleep(2);
				if (num * 2 < _socket.ReceiveTimeout)
				{
					num++;
					continue;
				}
				if (_socket.Available != 0)
				{
					break;
				}
				throw new TimeoutException();
			}
			byte[] array = new byte[_socket.Available];
			_socket.Receive(new ArraySegment<byte>(array), SocketFlags.None);
			return Encoding.ASCII.GetString(array);
		}
		throw new TimeoutException();
	}

	public string ReadString(int length)
	{
		if (_socket != null && _socket.Connected)
		{
			int num = 0;
			while (_socket.Available < length)
			{
				Thread.Sleep(2);
				if (num * 2 < _socket.ReceiveTimeout)
				{
					num++;
					continue;
				}
				if (_socket.Available != 0)
				{
					break;
				}
				throw new TimeoutException();
			}
			byte[] array = new byte[_socket.Available];
			_socket.Receive(new ArraySegment<byte>(array), SocketFlags.None);
			return Encoding.ASCII.GetString(array);
		}
		throw new TimeoutException();
	}

	public async Task<string> ReadStringAsync()
	{
		if (_socket != null && _socket.Connected)
		{
			int count = 0;
			while (_socket.Available <= 0)
			{
				await Task.Delay(2);
				if (count * 2 < _socket.ReceiveTimeout)
				{
					count++;
					continue;
				}
				if (_socket.Available != 0)
				{
					break;
				}
				throw new TimeoutException();
			}
			byte[] recvBuffer = new byte[_socket.Available];
			await _socket.ReceiveAsync(new ArraySegment<byte>(recvBuffer), SocketFlags.None);
			return Encoding.ASCII.GetString(recvBuffer);
		}
		throw new TimeoutException();
	}

	public async Task<string> ReadStringAsync(int length)
	{
		if (_socket != null && _socket.Connected)
		{
			int count = 0;
			while (_socket.Available < length)
			{
				await Task.Delay(2);
				if (count * 2 < _socket.ReceiveTimeout)
				{
					count++;
					continue;
				}
				if (_socket.Available != 0)
				{
					break;
				}
				throw new TimeoutException();
			}
			byte[] recvBuffer = new byte[_socket.Available];
			await _socket.ReceiveAsync(new ArraySegment<byte>(recvBuffer), SocketFlags.None);
			return Encoding.ASCII.GetString(recvBuffer);
		}
		throw new TimeoutException();
	}

	public int Write(string data)
	{
		if (_socket == null || !_socket.Connected)
		{
			throw new TimeoutException();
		}
		byte[] bytes = Encoding.ASCII.GetBytes(data);
		return _socket.Send(bytes, SocketFlags.None);
	}

	public async Task<int> WriteAsync(string data)
	{
		if (_socket != null && _socket.Connected)
		{
			byte[] bytes = Encoding.ASCII.GetBytes(data);
			return await _socket.SendAsync(bytes, SocketFlags.None);
		}
		throw new TimeoutException();
	}
}
