using System;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;

namespace NetStudio.Common.Historiant;

public class EnumDescriptionConverter<T> : TypeConverter
{
	public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
	{
		if (!(sourceType == typeof(T)))
		{
			return sourceType == typeof(string);
		}
		return true;
	}

	public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
	{
		if (!(destinationType == typeof(T)))
		{
			return destinationType == typeof(string);
		}
		return true;
	}

	public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		Type type = context.Instance.GetType();
		if (type == typeof(string))
		{
			return GetValue((string)context.Instance);
		}
		if (!(type is T))
		{
			throw new ArgumentException("Type converting from not supported: " + type.FullName);
		}
		return GetDescription((T)context.Instance);
	}

	public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
	{
		Type type = value.GetType();
		if (type == typeof(string) && destinationType == typeof(T))
		{
			return GetValue((string)value);
		}
		if (!(type == typeof(T)) || !(destinationType == typeof(string)))
		{
			throw new ArgumentException("Type converting from not supported: " + type.FullName);
		}
		return GetDescription((T)value);
	}

	public string GetDescription(T gparam_0)
	{
		MemberInfo[] member = gparam_0.GetType().GetMember(gparam_0.ToString());
		if (member != null && member.Length != 0)
		{
			object[] customAttributes = member[0].GetCustomAttributes(typeof(DescriptionAttribute), inherit: false);
			if (customAttributes != null && customAttributes.Length != 0)
			{
				return ((DescriptionAttribute)customAttributes[0]).Description;
			}
		}
		return gparam_0.ToString();
	}

	public T GetValue(string description)
	{
		foreach (T value in Enum.GetValues(typeof(T)))
		{
			if (GetDescription(value) == description)
			{
				return value;
			}
		}
		throw new ArgumentOutOfRangeException("description", "Argument description must match a Description attribute on an enum value of " + typeof(T).FullName);
	}
}
