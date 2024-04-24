using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NetStudio.Common.IndusCom;

[JsonDerivedType(typeof(EthernetAdapter))]
[JsonDerivedType(typeof(SerialAdapter))]
[JsonPolymorphic(UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FallBackToNearestAncestor)]
public interface INetworkAdapter
{
	bool Connected { get; }

	int SendTimeout { get; set; }

	int ReceiveTimeout { get; set; }

	string ErrorMessage { get; }

	bool Connect();

	Task<bool> ConnectAsync();

	bool Disconnect();

	Task<bool> DisconnectAsync();

	byte[] Read();

	Task<byte[]> ReadAsync();

	byte[] Read(int size);

	Task<byte[]> ReadAsync(int size);

	string ReadString();

	Task<string> ReadStringAsync();

	string ReadString(int length);

	Task<string> ReadStringAsync(int length);

	void SetSendTimeout(int timeout);

	void SetReceiveTimeout(int timeout);

	int Write(byte[] data);

	Task<int> WriteAsync(byte[] data);

	int Write(string data);

	Task<int> WriteAsync(string data);
}
