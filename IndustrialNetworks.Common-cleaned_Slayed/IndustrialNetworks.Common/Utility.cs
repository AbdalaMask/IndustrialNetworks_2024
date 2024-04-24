using System;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using NetStudio.Common.DataTypes;
using NetStudio.Common.Manager;

namespace NetStudio.Common;

public static class Utility
{
	private static readonly Regex validIpV4AddressRegex = new Regex("^(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\\.){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])$", RegexOptions.IgnoreCase);

	public static int ConnectRetries { get; private set; } = 3000;


	public static ValidateResult TagValidate(Tag tg)
	{
		ValidateResult validateResult = new ValidateResult
		{
			Status = ValidateStatus.Invalid
		};
		switch (tg.DataType)
		{
		case DataType.BOOL:
			if (tg.Address.Contains("."))
			{
				string[] array2 = tg.Address.Split('.');
				string text = ("00" + array2[1]).Right(2);
				tg.Address = array2[0] + "." + text;
			}
			else
			{
				tg.Address += ".00";
			}
			break;
		default:
			throw new NotSupportedException($"{tg.DataType}: {"This data type is not supported."}");
		case DataType.INT:
		case DataType.UINT:
		case DataType.WORD:
		case DataType.DINT:
		case DataType.UDINT:
		case DataType.DWORD:
		case DataType.REAL:
		case DataType.TIME16:
		case DataType.TIME32:
			if (tg.Address.Contains("."))
			{
				string[] array = tg.Address.Split('.');
				tg.Address = array[0];
			}
			break;
		}
		if (string.IsNullOrEmpty(validateResult.Message))
		{
			validateResult.Status = ValidateStatus.Valid;
		}
		else
		{
			validateResult.Status = ValidateStatus.Invalid;
		}
		return validateResult;
	}

	public static float Interpolation(Tag tg, dynamic iaCurrent)
	{
		return (float)Math.Round((float)(iaCurrent - tg.AImin) / (float)(tg.AImax - tg.AImin) * (tg.RLmax - tg.RLmin) + tg.RLmin, 1);
	}

	public static float Interpolation(short iaCurrent, ushort iaMin, ushort iaMax, float rlMin, float rlMax)
	{
		return (float)Math.Round((float)((iaCurrent - iaMin) / (iaMax - iaMin)) * (rlMax - rlMin) + rlMin, 1);
	}

	public static float Interpolation(ushort iaCurrent, ushort iaMin, ushort iaMax, float rlMin, float rlMax)
	{
		return (float)Math.Round((float)(iaCurrent - iaMin) / (float)(iaMax - iaMin) * (rlMax - rlMin) + rlMin, 1);
	}

	public static bool IsIpV4AddressValid(string address)
	{
		if (!string.IsNullOrWhiteSpace(address))
		{
			return validIpV4AddressRegex.IsMatch(address.Trim());
		}
		return false;
	}

	public static bool IsIpV6AddressValid(string address)
	{
		if (!string.IsNullOrWhiteSpace(address) && IPAddress.TryParse(address, out IPAddress address2))
		{
			return address2.AddressFamily == AddressFamily.InterNetworkV6;
		}
		return false;
	}
}
