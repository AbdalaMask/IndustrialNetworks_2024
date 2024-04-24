using System;
using System.IO;
using System.Xml.Serialization;

namespace NetStudio.Common.Files;

public class XmlObjectManager : GInterface0
{
	public void Write<T>(string fileProject, T objXML)
	{
		if (string.IsNullOrEmpty(fileProject) || string.IsNullOrWhiteSpace(fileProject))
		{
			throw new ArgumentNullException();
		}
		XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
		StreamWriter streamWriter = new StreamWriter(fileProject);
		xmlSerializer.Serialize(streamWriter, objXML);
		streamWriter.Close();
	}

	public T Read<T>(string fileProject)
	{
		XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
		FileStream fileStream = new FileStream(fileProject, FileMode.Open);
		T result = (T)xmlSerializer.Deserialize(fileStream);
		fileStream.Close();
		return result;
	}

	public void WriteEncrypt<T>(string fileProject, T objXML)
	{
		throw new NotImplementedException();
	}

	public T ReadDecrypt<T>(string fileProject)
	{
		throw new NotImplementedException();
	}

	public string GetFileName(string filePath)
	{
		if (!string.IsNullOrEmpty(filePath) && !string.IsNullOrWhiteSpace(filePath))
		{
			string[] array = filePath.Split('\\');
			if (array != null && array.Length != 0)
			{
				string[] array2 = array[^1].Split('.');
				if (array2 != null && array2.Length != 0)
				{
					return array2[0];
				}
			}
		}
		return string.Empty;
	}
}
