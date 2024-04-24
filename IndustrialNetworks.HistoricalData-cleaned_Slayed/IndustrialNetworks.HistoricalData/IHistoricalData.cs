using NetStudio.Common.Historiant;

namespace NetStudio.HistoricalData;

public interface IHistoricalData
{
	HDResult Create(DataLog dataLog);

	HDResult Update(DataLog dataLog);

	HDResult Delete(DataLog dataLog);

	HDResult GetSingle(DataLog dataLog);

	HDResult GetAll();
}
