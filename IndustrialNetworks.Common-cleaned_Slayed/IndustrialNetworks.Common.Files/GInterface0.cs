namespace NetStudio.Common.Files;

public interface GInterface0
{
	void Write<T>(string fileProject, T objXML);

	T Read<T>(string fileProject);

	void WriteEncrypt<T>(string fileProject, T objXML);

	T ReadDecrypt<T>(string fileProject);

	string GetFileName(string filePath);
}
