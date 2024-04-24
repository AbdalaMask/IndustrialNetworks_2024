using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace NetStudio.Database.SqlServer;

public static class Extensions
{
	public static DataTable ToDataTable<T>(this IEnumerable<T> pItems)
	{
		DataTable dataTable = new DataTable();
		T[] array = (pItems as T[]) ?? pItems.ToArray();
		PropertyInfo[] properties = array.First().GetType().GetProperties();
		foreach (PropertyInfo propertyInfo in properties)
		{
			if (IsColumn(propertyInfo))
			{
				dataTable.Columns.Add(propertyInfo.Name, propertyInfo.PropertyType);
			}
		}
		T[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			T val = array2[i];
			DataRow dataRow = dataTable.NewRow();
			properties = val.GetType().GetProperties();
			foreach (PropertyInfo propertyInfo2 in properties)
			{
				if (IsColumn(propertyInfo2))
				{
					propertyInfo2.GetValue(val, null);
					dataRow[propertyInfo2.Name] = propertyInfo2.GetValue(val, null);
				}
			}
			dataTable.Rows.Add(dataRow);
		}
		return dataTable;
	}

	private static bool IsColumn(PropertyInfo prop)
	{
		if (!(prop.PropertyType.FullName == "System.Byte") && !(prop.PropertyType.FullName == "System.SByte") && !(prop.PropertyType.FullName == "System.Int32") && !(prop.PropertyType.FullName == "System.UInt32") && !(prop.PropertyType.FullName == "System.Int16") && !(prop.PropertyType.FullName == "System.UInt16") && !(prop.PropertyType.FullName == "System.Int64") && !(prop.PropertyType.FullName == "System.UInt64") && !(prop.PropertyType.FullName == "System.Single") && !(prop.PropertyType.FullName == "System.Double") && !(prop.PropertyType.FullName == "System.Char") && !(prop.PropertyType.FullName == "System.Boolean") && !(prop.PropertyType.FullName == "System.Object") && !(prop.PropertyType.FullName == "System.String") && !(prop.PropertyType.FullName == "System.Decimal") && !(prop.PropertyType.FullName == "System.DateTime") && !(prop.PropertyType.FullName == "System.Guid"))
		{
			return false;
		}
		return true;
	}
}
