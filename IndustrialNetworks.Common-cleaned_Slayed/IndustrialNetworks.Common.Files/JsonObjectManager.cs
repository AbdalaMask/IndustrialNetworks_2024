using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace NetStudio.Common.Files;

public class JsonObjectManager
{
	public void Write<T>(string fileName, T data)
	{
		if (string.IsNullOrEmpty(fileName) || string.IsNullOrWhiteSpace(fileName))
		{
			throw new ArgumentNullException();
		}
		string contents = JsonSerializer.Serialize(data);
		File.WriteAllText(fileName, contents);
	}

	public T Read<T>(string fileName)
	{
		if (!string.IsNullOrEmpty(fileName) && !string.IsNullOrWhiteSpace(fileName))
		{
			if (!File.Exists(fileName))
			{
				throw new FileNotFoundException("File not found: " + fileName + ".");
			}
			if (!fileName.Contains(".json", StringComparison.CurrentCultureIgnoreCase))
			{
				throw new FormatException("The file is not in the correct format: " + fileName + ".");
			}
			try
			{
				return JsonSerializer.Deserialize<T>(File.ReadAllText(fileName));
			}
			catch (FormatException ex)
			{
				throw new FormatException(fileName + ": " + ex.Message);
			}
		}
		throw new ArgumentNullException();
	}

	public async Task WriteAsync<T>(string fileName, T data)
	{
		if (!string.IsNullOrEmpty(fileName) && !string.IsNullOrWhiteSpace(fileName))
		{
			string contents = JsonSerializer.Serialize(data);
			await File.WriteAllTextAsync(fileName, contents);
			return;
		}
		throw new ArgumentNullException();
	}

	public async Task<T> ReadAsync<T>(string fileName)
	{
		if (!string.IsNullOrEmpty(fileName) && !string.IsNullOrWhiteSpace(fileName))
		{
			if (!File.Exists(fileName))
			{
				throw new FileNotFoundException("File not found: " + fileName + ".");
			}
			if (!fileName.Contains(".json", StringComparison.CurrentCultureIgnoreCase))
			{
				throw new FormatException("The file is not in the correct format: " + fileName + ".");
			}
			try
			{
				return JsonSerializer.Deserialize<T>(await File.ReadAllTextAsync(fileName));
			}
			catch (FormatException ex)
			{
				throw new FormatException(fileName + ": " + ex.Message);
			}
		}
		throw new ArgumentNullException();
	}

	public string GetFileName(string filePath)
	{
		if (!string.IsNullOrEmpty(filePath) && !string.IsNullOrWhiteSpace(filePath))
		{
			if (!File.Exists(filePath))
			{
				return string.Empty;
			}
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
