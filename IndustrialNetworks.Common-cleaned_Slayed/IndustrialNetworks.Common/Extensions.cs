using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace NetStudio.Common;

public static class Extensions
{
	public static string Left(this string value, int length)
	{
		return value.Substring(0, length);
	}

	public static string Right(this string value, int length)
	{
		return value.Substring(value.Length - length);
	}

	public static string GetDescription<T>(this T source)
	{
		ref T reference = ref source;
		T val = default(T);
		object obj;
		if (val == null)
		{
			val = reference;
			reference = ref val;
			if (val == null)
			{
				obj = null;
				goto IL_0067;
			}
		}
		obj = reference.GetType().GetField(source?.ToString());
		goto IL_0067;
		IL_0067:
		FieldInfo fieldInfo = (FieldInfo)obj;
		if (fieldInfo != null)
		{
			DescriptionAttribute[] array = (DescriptionAttribute[])fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), inherit: false);
			if (array != null && array.Length != 0)
			{
				return array[0].Description;
			}
		}
		ref T reference2 = ref source;
		val = default(T);
		if (val == null)
		{
			val = reference2;
			reference2 = ref val;
			if (val == null)
			{
				return null;
			}
		}
		return reference2.ToString();
	}

	public static string GetDescription(this Enum source)
	{
		FieldInfo field = source.GetType().GetField(source.ToString());
		if (field != null)
		{
			DescriptionAttribute[] array = (DescriptionAttribute[])field.GetCustomAttributes(typeof(DescriptionAttribute), inherit: false);
			if (array != null && array.Length != 0)
			{
				return array[0].Description;
			}
		}
		return source.ToString();
	}

	public static List<string> GetDescriptions(this Enum source)
	{
		List<string> list = new List<string>();
		FieldInfo[] fields = source.GetType().GetFields();
		if (fields != null && fields.Length != 0)
		{
			FieldInfo[] array = fields;
			for (int i = 0; i < array.Length; i++)
			{
				DescriptionAttribute[] array2 = (DescriptionAttribute[])array[i].GetCustomAttributes(typeof(DescriptionAttribute), inherit: false);
				if (array2 != null && array2.Length != 0)
				{
					list.Add(array2[0].Description);
				}
			}
		}
		return list;
	}

	public static T GetEnumFromDescription<T>(string description) where T : Enum
	{
		FieldInfo[] fields = typeof(T).GetFields();
		int num = 0;
		FieldInfo fieldInfo;
		while (true)
		{
			if (num < fields.Length)
			{
				fieldInfo = fields[num];
				if (Attribute.GetCustomAttribute(fieldInfo, typeof(DescriptionAttribute)) is DescriptionAttribute descriptionAttribute)
				{
					if (descriptionAttribute.Description == description)
					{
						return (T)fieldInfo.GetValue(null);
					}
				}
				else if (fieldInfo.Name == description)
				{
					break;
				}
				num++;
				continue;
			}
			throw new ArgumentException("Not found.", "description");
		}
		return (T)fieldInfo.GetValue(null);
	}

	public static Dictionary<T, string> GetDictionaryByEnum<T>() where T : Enum
	{
		return Enum.GetValues(typeof(T)).Cast<T>().ToDictionary((T gparam_0) => gparam_0, (T gparam_0) => gparam_0.GetDescription());
	}

	public static Dictionary<int, string> GetEnumAsDictionary<T>() where T : Enum
	{
		Array values = Enum.GetValues(typeof(T));
		Dictionary<int, string> dictionary = new Dictionary<int, string>();
		foreach (object item in values)
		{
			if (!dictionary.ContainsKey((int)item))
			{
				dictionary.Add((int)item, item.GetDescription());
			}
		}
		return dictionary;
	}

	public static Dictionary<T, string> GetDictionary<T>() where T : Enum
	{
		Array values = Enum.GetValues(typeof(T));
		Dictionary<T, string> dictionary = new Dictionary<T, string>();
		foreach (object item in values)
		{
			if (!dictionary.ContainsKey((T)item))
			{
				dictionary.Add((T)item, item.GetDescription());
			}
		}
		return dictionary;
	}
}
