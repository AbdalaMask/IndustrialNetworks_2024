using System;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;

namespace NetStudio.Common.Historiant;

public class LoggingModeConverter : EnumConverter
{
	public LoggingModeConverter()
		: base(typeof(LoggingMode))
	{
	}

	public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
	{
		string text = value.ToString();
		if (destinationType == typeof(string) && text.StartsWith("On change"))
		{
			return Extensions.GetEnumFromDescription<LoggingMode>(value.ToString());
		}
		return value;
	}

	public string GetDescription(LoggingMode loggingMode_0)
	{
		MemberInfo[] member = loggingMode_0.GetType().GetMember(loggingMode_0.ToString());
		if (member != null && member.Length != 0)
		{
			object[] customAttributes = member[0].GetCustomAttributes(typeof(DescriptionAttribute), inherit: false);
			if (customAttributes != null && customAttributes.Length != 0)
			{
				return ((DescriptionAttribute)customAttributes[0]).Description;
			}
		}
		return loggingMode_0.ToString();
	}

	public LoggingMode GetValue(string description)
	{
		foreach (LoggingMode value in Enum.GetValues(typeof(LoggingMode)))
		{
			if (GetDescription(value) == description)
			{
				return value;
			}
		}
		throw new ArgumentOutOfRangeException("description", "Argument description must match a Description attribute on an enum value of " + typeof(LoggingMode).FullName);
	}
}
