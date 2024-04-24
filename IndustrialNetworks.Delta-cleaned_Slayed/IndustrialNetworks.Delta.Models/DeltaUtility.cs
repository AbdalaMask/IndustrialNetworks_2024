using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NetStudio.Common.DataTypes;
using NetStudio.Common.IndusCom;
using NetStudio.Common.Manager;

namespace NetStudio.Delta.Models;

public class DeltaUtility
{
	public static class AhAsSeries
	{
		public static readonly Dictionary<string, IpsAddress> Memories = new Dictionary<string, IpsAddress>
		{
			{
				"X",
				new IpsAddress(32768, 24576)
			},
			{
				"Y",
				new IpsAddress(40960, 40960)
			},
			{
				"M",
				new IpsAddress(0, 0)
			},
			{
				"SM",
				new IpsAddress(0, 16384)
			},
			{
				"SR",
				new IpsAddress(49152, 0)
			},
			{
				"D",
				new IpsAddress(0, 0)
			},
			{
				"S",
				new IpsAddress(0, 20480)
			},
			{
				"T",
				new IpsAddress(57344, 57344)
			},
			{
				"C",
				new IpsAddress(61440, 61440)
			},
			{
				"HC",
				new IpsAddress(64512, 64512)
			},
			{
				"E",
				new IpsAddress(65024, 0)
			}
		};

		public static bool IsBitOnlyMemory(string memory)
		{
			if (!(memory == "M") && !(memory == "SM"))
			{
				return memory == "S";
			}
			return true;
		}

		public static bool IsBitOnlyMemory(Tag tg)
		{
			string text = string.Join("", from char_0 in tg.Address
				where char.IsLetter(char_0)
				select (char_0));
			if (!(text == "M") && !(text == "SM"))
			{
				return text == "S";
			}
			return true;
		}

		public static bool IsBitWordMemory(string memory)
		{
			switch (memory)
			{
			default:
				return memory == "HC";
			case "X":
			case "Y":
			case "T":
			case "C":
				return true;
			}
		}

		public static bool IsBitMemory(string memory)
		{
			switch (memory)
			{
			default:
				return memory == "HC";
			case "X":
			case "Y":
			case "M":
			case "SM":
			case "S":
			case "T":
			case "C":
				return true;
			}
		}

		public static ValidateResult TagValidate(Tag tg)
		{
			ValidateResult validateResult = new ValidateResult
			{
				Status = ValidateStatus.Invalid
			};
			string memory = GetMemory(tg);
			if (tg.DataType == DataType.BOOL)
			{
				if (!tg.Address.StartsWith("X") && !tg.Address.StartsWith("Y"))
				{
					if (tg.Address.Contains('.'))
					{
						validateResult.Message = "Data type and Address are not match.";
					}
				}
				else if (!tg.Address.Contains('.'))
				{
					validateResult.Message = "Data type and Address are not match.";
				}
			}
			else if (((tg.Address.StartsWith("X") || tg.Address.StartsWith("Y")) && tg.Address.Contains('.')) || IsBitOnlyMemory(memory))
			{
				validateResult.Message = "Data type and Address are not match.";
			}
			switch (tg.DataType)
			{
			default:
				throw new NotSupportedException($"{tg.DataType}: {"This data type is not supported."}");
			case DataType.INT:
			case DataType.UINT:
			case DataType.WORD:
			case DataType.TIME16:
				if (tg.Address.StartsWith("HC"))
				{
					validateResult.Message = "Data type and Address are not match.";
				}
				break;
			case DataType.BOOL:
			case DataType.DINT:
			case DataType.UDINT:
			case DataType.DWORD:
			case DataType.REAL:
			case DataType.TIME32:
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

		public static decimal GetWordAddress(Tag tg)
		{
			string text = string.Join("", from char_0 in tg.Address
				where char.IsLetter(char_0)
				select (char_0));
			string text2 = tg.Address.Substring(text.Length);
			int num = 0;
			if (tg.DataType == DataType.BOOL)
			{
				if (IsBitOnlyMemory(text))
				{
					goto IL_00e6;
				}
				switch (text)
				{
				case "HC":
				case "T":
				case "C":
					goto IL_00e6;
				}
				if (tg.Address.Contains("."))
				{
					num = int.Parse(text2.Split('.')[0]);
					int.Parse(text2.Split('.')[1]);
				}
				else
				{
					num = int.Parse(text2) / 16;
				}
			}
			else
			{
				num = int.Parse(text2);
			}
			goto IL_00f6;
			IL_00e6:
			int.Parse(text2);
			goto IL_00f6;
			IL_00f6:
			IpsAddress ipsAddress = Memories[text];
			return num + ipsAddress.WordAddress;
		}

		public static int GetBitAddress(Tag tg)
		{
			string text = string.Join("", from char_0 in tg.Address
				where char.IsLetter(char_0)
				select (char_0));
			string text2 = tg.Address.Substring(text.Length);
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			if (tg.DataType == DataType.BOOL)
			{
				if (IsBitOnlyMemory(text))
				{
					goto IL_00fb;
				}
				switch (text)
				{
				case "HC":
				case "T":
				case "C":
					goto IL_00fb;
				}
				if (tg.Address.Contains("."))
				{
					num = int.Parse(text2.Split('.')[0]);
					num3 = int.Parse(text2.Split('.')[1]);
				}
				else
				{
					num = int.Parse(text2) / 16;
				}
				num2 = 16 * num + num3;
			}
			goto IL_0102;
			IL_00fb:
			num2 = int.Parse(text2);
			goto IL_0102;
			IL_0102:
			IpsAddress ipsAddress = Memories[text];
			return num2 + ipsAddress.BitAddress;
		}

		public static IpsAddress GetIpsAddress(Tag tg)
		{
			string text = string.Join("", from char_0 in tg.Address
				where char.IsLetter(char_0)
				select (char_0));
			string text2 = tg.Address.Substring(text.Length);
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			if (tg.DataType == DataType.BOOL)
			{
				if (IsBitOnlyMemory(text))
				{
					goto IL_00f9;
				}
				switch (text)
				{
				case "HC":
				case "T":
				case "C":
					goto IL_00f9;
				}
				if (tg.Address.Contains("."))
				{
					num = int.Parse(text2.Split('.')[0]);
					num3 = int.Parse(text2.Split('.')[1]);
				}
				else
				{
					num = int.Parse(text2) / 16;
				}
				num2 = 16 * num + num3;
			}
			else
			{
				num = int.Parse(text2);
			}
			goto IL_010a;
			IL_00f9:
			num2 = int.Parse(text2);
			goto IL_010a;
			IL_010a:
			IpsAddress ipsAddress = Memories[text];
			return new IpsAddress(num + ipsAddress.WordAddress, num2 + ipsAddress.BitAddress);
		}

		public static int GetBitIndexInWord(Tag tg)
		{
			string text = string.Join("", from char_0 in tg.Address
				where char.IsLetter(char_0)
				select (char_0));
			string text2 = tg.Address.Substring(text.Length);
			if (tg.DataType == DataType.BOOL && tg.Address.Contains("."))
			{
				return int.Parse(text2.Split('.')[1]);
			}
			return int.Parse(text2) % 16;
		}
	}

	public static class DvpSeries
	{
		public static readonly Dictionary<string, IpsAddress> Memories = new Dictionary<string, IpsAddress>
		{
			{
				"S",
				new IpsAddress(0, 0)
			},
			{
				"X",
				new IpsAddress(0, 1024)
			},
			{
				"Y",
				new IpsAddress(40960, 1280)
			},
			{
				"T",
				new IpsAddress(1536, 1536)
			},
			{
				"M",
				new IpsAddress(0, 2048)
			},
			{
				"C",
				new IpsAddress(3584, 3584)
			},
			{
				"D",
				new IpsAddress(4096, 0)
			}
		};

		public static readonly Dictionary<string, int> Address = new Dictionary<string, int>
		{
			{ "S1", 0 },
			{ "S2", 256 },
			{ "S3", 512 },
			{ "S4", 768 },
			{ "X", 1024 },
			{ "Y", 1280 },
			{ "T", 1536 },
			{ "M1", 2048 },
			{ "M2", 2304 },
			{ "M3", 2560 },
			{ "M4", 2816 },
			{ "M5", 3072 },
			{ "M6", 3328 },
			{ "M7", 45056 },
			{ "M8", 45312 },
			{ "M9", 45568 },
			{ "M10", 45824 },
			{ "M11", 46080 },
			{ "M12", 46336 },
			{ "M13", 46592 },
			{ "M14", 46848 },
			{ "M15", 47104 },
			{ "M16", 47360 },
			{ "C1", 3584 },
			{ "C2", 3784 },
			{ "D1", 4096 },
			{ "D2", 4352 },
			{ "D3", 4608 },
			{ "D4", 4864 },
			{ "D5", 5120 },
			{ "D6", 5376 },
			{ "D7", 5632 },
			{ "D8", 5888 },
			{ "D9", 6144 },
			{ "D10", 6400 },
			{ "D11", 6656 },
			{ "D12", 6912 },
			{ "D13", 7168 },
			{ "D14", 7424 },
			{ "D15", 7680 },
			{ "D16", 7936 },
			{ "D17", 36864 },
			{ "D18", 36864 },
			{ "D19", 36864 },
			{ "D20", 36864 },
			{ "D21", 37120 },
			{ "D22", 37376 },
			{ "D23", 37632 },
			{ "D24", 37888 },
			{ "D25", 38144 },
			{ "D26", 38400 },
			{ "D27", 38656 },
			{ "D28", 38912 },
			{ "D29", 39168 },
			{ "D30", 39424 },
			{ "D31", 39680 },
			{ "D32", 39936 },
			{ "D33", 40192 },
			{ "D34", 40448 },
			{ "D35", 40704 },
			{ "D36", 40960 },
			{ "D37", 41216 },
			{ "D38", 41472 },
			{ "D39", 41728 },
			{ "D40", 41984 },
			{ "D41", 42240 },
			{ "D42", 42496 },
			{ "D43", 42752 }
		};

		public static bool IsBitOnlyMemory(Tag tg)
		{
			string text = string.Join("", from char_0 in tg.Address
				where char.IsLetter(char_0)
				select (char_0));
			if (!(text == "M") && !(text == "SM"))
			{
				return text == "S";
			}
			return true;
		}

		public static bool IsBitMemory(string memory)
		{
			switch (memory)
			{
			default:
				return memory == "C";
			case "X":
			case "Y":
			case "M":
			case "S":
			case "T":
				return true;
			}
		}

		public static ValidateResult TagValidate(Tag tg)
		{
			ValidateResult validateResult = new ValidateResult
			{
				Status = ValidateStatus.Invalid
			};
			GetMemory(tg);
			if (tg.DataType == DataType.BOOL && tg.Address.Contains('.'))
			{
				validateResult.Message = "Data type and Address are not match.";
			}
			switch (tg.DataType)
			{
			default:
				throw new NotSupportedException($"{tg.DataType}: {"This data type is not supported."}");
			case DataType.INT:
			case DataType.UINT:
			case DataType.WORD:
			case DataType.TIME16:
				if (tg.Address.StartsWith("HC"))
				{
					validateResult.Message = "Data type and Address are not match.";
				}
				break;
			case DataType.BOOL:
			case DataType.DINT:
			case DataType.UDINT:
			case DataType.DWORD:
			case DataType.REAL:
			case DataType.TIME32:
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

		public static int GetWordAddress(Tag tg)
		{
			string text = tg.Address.Substring(0, 1);
			int num = 0;
			if (text == "X")
			{
				return 1024 + Convert.ToInt32(tg.Address.Substring(text.Length), 8);
			}
			if (text == "Y")
			{
				return 1280 + Convert.ToInt32(tg.Address.Substring(text.Length), 8);
			}
			int num2 = int.Parse(tg.Address.Substring(text.Length));
			switch (text)
			{
			default:
				throw new NotSupportedException("Data type and Address are not match.");
			case "D":
			{
				int num7 = num2;
				if (num7 >= 0 && num7 <= 255)
				{
					num = 4096;
					break;
				}
				int num8 = num7;
				if (num8 >= 256 && num8 <= 511)
				{
					num = 4352;
					break;
				}
				int num9 = num7;
				if (num9 >= 512 && num9 <= 767)
				{
					num = 4608;
					break;
				}
				int num10 = num7;
				if (num10 >= 768 && num10 <= 1023)
				{
					num = 4864;
					break;
				}
				int num11 = num7;
				if (num11 >= 1024 && num11 <= 1279)
				{
					num = 5120;
					break;
				}
				int num12 = num7;
				if (num12 >= 1280 && num12 <= 1535)
				{
					num = 5376;
					break;
				}
				int num13 = num7;
				if (num13 >= 1536 && num13 <= 1791)
				{
					num = 5632;
					break;
				}
				int num14 = num7;
				if (num14 >= 1792 && num14 <= 2047)
				{
					num = 5888;
					break;
				}
				int num15 = num7;
				if (num15 >= 2048 && num15 <= 2303)
				{
					num = 6144;
					break;
				}
				int num16 = num7;
				if (num16 >= 2304 && num16 <= 2559)
				{
					num = 6400;
					break;
				}
				int num17 = num7;
				if (num17 >= 2560 && num17 <= 2815)
				{
					num = 6656;
					break;
				}
				int num18 = num7;
				if (num18 >= 2816 && num18 <= 3071)
				{
					num = 6912;
					break;
				}
				int num19 = num7;
				if (num19 >= 3072 && num19 <= 3327)
				{
					num = 7168;
					break;
				}
				int num20 = num7;
				if (num20 >= 3328 && num20 <= 3583)
				{
					num = 7424;
					break;
				}
				int num21 = num7;
				if (num21 >= 3584 && num21 <= 3839)
				{
					num = 7680;
					break;
				}
				int num22 = num7;
				if (num22 >= 3840 && num22 <= 4095)
				{
					num = 7936;
					break;
				}
				int num23 = num7;
				if (num23 >= 4096 && num23 <= 4351)
				{
					num = 36864;
					break;
				}
				int num24 = num7;
				if (num24 >= 4352 && num24 <= 4607)
				{
					num = 37120;
					break;
				}
				int num25 = num7;
				if (num25 >= 4608 && num25 <= 4863)
				{
					num = 37376;
					break;
				}
				int num26 = num7;
				if (num26 >= 4864 && num26 <= 5119)
				{
					num = 37632;
					break;
				}
				int num27 = num7;
				if (num27 >= 5120 && num27 <= 5375)
				{
					num = 37888;
					break;
				}
				int num28 = num7;
				if (num28 >= 5376 && num28 <= 5631)
				{
					num = 38144;
					break;
				}
				int num29 = num7;
				if (num29 >= 5632 && num29 <= 5887)
				{
					num = 38400;
					break;
				}
				int num30 = num7;
				if (num30 >= 5888 && num30 <= 6143)
				{
					num = 38656;
					break;
				}
				int num31 = num7;
				if (num31 >= 6144 && num31 <= 6399)
				{
					num = 38912;
					break;
				}
				int num32 = num7;
				if (num32 >= 6400 && num32 <= 6655)
				{
					num = 39168;
					break;
				}
				int num33 = num7;
				if (num33 >= 6656 && num33 <= 6911)
				{
					num = 39424;
					break;
				}
				int num34 = num7;
				if (num34 >= 6912 && num34 <= 7167)
				{
					num = 39680;
					break;
				}
				int num35 = num7;
				if (num35 >= 7168 && num35 <= 7423)
				{
					num = 39936;
					break;
				}
				int num36 = num7;
				if (num36 >= 7424 && num36 <= 7679)
				{
					num = 40192;
					break;
				}
				int num37 = num7;
				if (num37 >= 7680 && num37 <= 7935)
				{
					num = 40448;
					break;
				}
				int num38 = num7;
				if (num38 >= 7936 && num38 <= 8191)
				{
					num = 40704;
					break;
				}
				int num39 = num7;
				if (num39 >= 8192 && num39 <= 8447)
				{
					num = 40960;
					break;
				}
				int num40 = num7;
				if (num40 >= 8448 && num40 <= 8703)
				{
					num = 41216;
					break;
				}
				int num41 = num7;
				if (num41 >= 8704 && num41 <= 8959)
				{
					num = 41472;
					break;
				}
				int num42 = num7;
				if (num42 >= 8960 && num42 <= 9215)
				{
					num = 41728;
					break;
				}
				int num43 = num7;
				if (num43 >= 9216 && num43 <= 9471)
				{
					num = 41984;
					break;
				}
				int num44 = num7;
				if (num44 >= 9472 && num44 <= 9727)
				{
					num = 42240;
					break;
				}
				int num45 = num7;
				if (num45 >= 9728 && num45 <= 9983)
				{
					num = 42496;
					break;
				}
				int num46 = num7;
				if (num46 >= 9984 && num46 <= 9999)
				{
					num = 42752;
				}
				break;
			}
			case "C":
				if (num2 >= 0 && num2 <= 199)
				{
					num = 3584;
				}
				else if (num2 >= 200 && num2 <= 255)
				{
					num = 3784;
				}
				break;
			case "T":
				if (num2 >= 0 && num2 <= 255)
				{
					num = 1536;
				}
				break;
			case "M":
			{
				int num47 = num2;
				if (num47 >= 0 && num47 <= 255)
				{
					num = 2048;
					break;
				}
				int num48 = num47;
				if (num48 >= 256 && num48 <= 511)
				{
					num = 2304;
					break;
				}
				int num49 = num47;
				if (num49 >= 512 && num49 <= 767)
				{
					num = 2560;
					break;
				}
				int num50 = num47;
				if (num50 >= 768 && num50 <= 1023)
				{
					num = 2816;
					break;
				}
				int num51 = num47;
				if (num51 >= 1024 && num51 <= 1279)
				{
					num = 3072;
					break;
				}
				int num52 = num47;
				if (num52 >= 1280 && num52 <= 1535)
				{
					num = 3328;
					break;
				}
				int num53 = num47;
				if (num53 >= 1536 && num53 <= 1791)
				{
					num = 45056;
					break;
				}
				int num54 = num47;
				if (num54 >= 1792 && num54 <= 2047)
				{
					num = 45312;
					break;
				}
				int num55 = num47;
				if (num55 >= 2048 && num55 <= 2303)
				{
					num = 45568;
					break;
				}
				int num56 = num47;
				if (num56 >= 2304 && num56 <= 2559)
				{
					num = 45824;
					break;
				}
				int num57 = num47;
				if (num57 >= 2560 && num57 <= 2815)
				{
					num = 46080;
					break;
				}
				int num58 = num47;
				if (num58 >= 2816 && num58 <= 3071)
				{
					num = 46336;
					break;
				}
				int num59 = num47;
				if (num59 >= 3072 && num59 <= 3327)
				{
					num = 46592;
					break;
				}
				int num60 = num47;
				if (num60 >= 3328 && num60 <= 3583)
				{
					num = 46848;
					break;
				}
				int num61 = num47;
				if (num61 >= 3584 && num61 <= 3839)
				{
					num = 47104;
					break;
				}
				int num62 = num47;
				if (num62 >= 3840 && num62 <= 4095)
				{
					num = 47360;
				}
				break;
			}
			case "S":
			{
				int num3 = num2;
				if (num3 >= 0 && num3 <= 255)
				{
					num = 0;
					break;
				}
				int num4 = num3;
				if (num4 >= 256 && num4 <= 511)
				{
					num = 256;
					break;
				}
				int num5 = num3;
				if (num5 >= 512 && num5 <= 767)
				{
					num = 512;
					break;
				}
				int num6 = num3;
				if (num6 >= 768 && num6 <= 1023)
				{
					num = 768;
				}
				break;
			}
			}
			return num + num2;
		}

		public static IpsAddress GetIpsAddress(Tag tg)
		{
			string text = string.Join("", from char_0 in tg.Address
				where char.IsLetter(char_0)
				select (char_0));
			string text2 = tg.Address.Substring(text.Length);
			int num = 0;
			int num2 = 0;
			if (tg.DataType == DataType.BOOL)
			{
				num2 = ((!IsOctal(text)) ? int.Parse(text2) : Convert.ToInt32(text2, 8));
			}
			else
			{
				num = int.Parse(text2);
			}
			IpsAddress ipsAddress = Memories[text];
			return new IpsAddress(num + ipsAddress.WordAddress, num2 + ipsAddress.BitAddress);
		}
	}

	public static string GetMemory(Tag tg)
	{
		return string.Join("", from char_0 in tg.Address
			where char.IsLetter(char_0)
			select (char_0));
	}

	public static void IncrementAddress(Tag tg)
	{
		string memory = GetMemory(tg);
		decimal num = decimal.Parse(tg.Address.Substring(memory.Length));
		if (memory == "HC" && tg.DataType != 0)
		{
			++num;
		}
		else
		{
			switch (tg.DataType)
			{
			case DataType.BOOL:
				if (tg.DataType == DataType.BOOL && tg.Address.Contains("."))
				{
					decimal value = Math.Truncate(num);
					int num2 = int.Parse(tg.Address.Split('.')[1]) + 1;
					if (num2 > 15)
					{
						++value;
						num2 = 0;
					}
					num = decimal.Parse($"{value}.{num2}");
				}
				else if (IsOctal(memory))
				{
					num = Conversion.IncrementOctal((int)num);
				}
				else
				{
					num += 1m;
				}
				break;
			default:
				throw new NotSupportedException("This data type is not supported.");
			case DataType.LINT:
			case DataType.ULINT:
			case DataType.LWORD:
			case DataType.LREAL:
				num += 4m;
				break;
			case DataType.INT:
			case DataType.UINT:
			case DataType.WORD:
			case DataType.TIME16:
				num += 1m;
				break;
			case DataType.DINT:
			case DataType.UDINT:
			case DataType.DWORD:
			case DataType.REAL:
			case DataType.TIME32:
				num += 2m;
				break;
			case DataType.STRING:
				num += 16m;
				break;
			}
		}
		tg.Address = $"{memory}{num}";
		tg.Name = tg.Address;
	}

	public static int GetSizeOfDataType(Tag tg)
	{
        switch (tg.DataType)
		{
		case DataType.BOOL:
			return 1;
		default:
			throw new NotSupportedException($"{tg.DataType}: " + "This data type is not supported.");
		case DataType.LINT:
		case DataType.ULINT:
		case DataType.LWORD:
		case DataType.LREAL:
			return 4;
		case DataType.INT:
		case DataType.UINT:
		case DataType.WORD:
		case DataType.TIME16:
			return 1;
		case DataType.DINT:
		case DataType.UDINT:
		case DataType.DWORD:
		case DataType.REAL:
		case DataType.TIME32:
			return 2;
		}
	}

	public static bool IsOctal(string memory)
	{
		if (!(memory == "X"))
		{
			return memory == "Y";
		}
		return true;
	}

	public static string ToByteOrder(string dataHex)
	{
		int length = dataHex.Length;
		return length switch
		{
			16 => dataHex.Substring(12, 2) + dataHex.Substring(14, 2) + dataHex.Substring(8, 2) + dataHex.Substring(10, 2) + dataHex.Substring(4, 2) + dataHex.Substring(6, 2) + dataHex.Substring(0, 2) + dataHex.Substring(2, 2), 
			8 => dataHex.Substring(4, 2) + dataHex.Substring(6, 2) + dataHex.Substring(0, 2) + dataHex.Substring(2, 2), 
			4 => dataHex.Substring(0, 2) + dataHex.Substring(2, 2), 
			_ => throw new InvalidDataException($"Byte order is undefined with the number of bytes={length}. The number of valid bytes is: 2, 4, 8 bytes"), 
		};
	}

	public static byte[] Sort(byte[] values)
	{
		int num = values.Length;
		return num switch
		{
			8 => new byte[8]
			{
				values[1],
				values[0],
				values[3],
				values[2],
				values[5],
				values[4],
				values[7],
				values[6]
			}, 
			4 => new byte[4]
			{
				values[1],
				values[0],
				values[3],
				values[2]
			}, 
			2 => new byte[2]
			{
				values[1],
				values[0]
			}, 
			_ => throw new InvalidDataException($"Byte order is undefined with the number of bytes={num}. The number of valid bytes is: 2, 4, 8 bytes"), 
		};
	}
}
