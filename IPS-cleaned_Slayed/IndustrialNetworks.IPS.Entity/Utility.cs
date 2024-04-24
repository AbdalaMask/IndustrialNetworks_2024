namespace NetStudio.IPS.Entity;

internal class Utility
{
	public static string GetProjectName(string fileProject)
	{
		if (!string.IsNullOrEmpty(fileProject) && !string.IsNullOrWhiteSpace(fileProject))
		{
			string[] array = fileProject.Split('\\');
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
