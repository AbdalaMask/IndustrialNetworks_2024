using System.ServiceModel;
using System.Threading.Tasks;
using NetStudio.Common.AsrsLink;
using NetStudio.Common.Manager;
using NetStudio.DriverComm.Models;

namespace NetStudio.DriverComm.Interfaces;

[ServiceContract]
public interface IEditManger
{
	[OperationContract(Name = "Read")]
	ApiResponse Read();

	[OperationContract(Name = "Write")]
	ApiResponse Write(IndustrialProtocol data);

	[OperationContract(Name = "ReadAsync")]
	Task<ApiResponse> ReadAsync();

	[OperationContract(Name = "WriteAsync")]
	Task<ApiResponse> WriteAsync(IndustrialProtocol data);

	[OperationContract(Name = "TestConnection")]
	ApiResponse TestConnection(string connectionString);

	[OperationContract(Name = "TestConnectionAsync")]
	Task<ApiResponse> TestConnectionAsync(string connectionString);

	[OperationContract(Name = "GetTableNames")]
	ApiResponse GetTableNames(SqlRequestInfo rstInfo);

	[OperationContract(Name = "GetTableNamesAsync")]
	Task<ApiResponse> GetTableNamesAsync(SqlRequestInfo rstInfo);

	[OperationContract(Name = "GetColumnsInfo")]
	ApiResponse GetColumnsInfo(SqlRequestInfo rstInfo);

	[OperationContract(Name = "GetColumnsInfoAsync")]
	Task<ApiResponse> GetColumnsInfoAsync(SqlRequestInfo rstInfo);
}
