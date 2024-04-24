using System;
using System.Collections.Generic;
using NetStudio.Vigor.Enums;

namespace NetStudio.Vigor;

internal static class Validate
{
	public static readonly Dictionary<ErrorCode, string> Errors = new Dictionary<ErrorCode, string>
	{
		{
			ErrorCode.CommunicationIsNormal,
			"Communication is normal; no error."
		},
		{
			ErrorCode.Error_CheckSum,
			"Communication SUM Check Error."
		},
		{
			ErrorCode.Error_NumberOfDataBystesIsZero,
			"The number of data bytes or the number of components is 0."
		},
		{
			ErrorCode.Error_NumberOfDataBitsExceedsTheRange,
			"The number of data bits exceeds the range."
		},
		{
			ErrorCode.Error_ASCIIConversion,
			"Error ASCII conversion."
		},
		{
			ErrorCode.Error_FunctionCodeIsNotExisted,
			"The command / function code is not existed."
		}
	};

	public static bool HasError(ErrorCode errorCode)
	{
		if (!Errors.ContainsKey(errorCode))
		{
			throw new Exception(Errors[errorCode]);
		}
		return !Errors.ContainsKey(errorCode);
	}
}
