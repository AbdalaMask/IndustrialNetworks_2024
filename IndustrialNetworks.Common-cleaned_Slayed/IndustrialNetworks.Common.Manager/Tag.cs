using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using System.Xml.Serialization;
using NetStudio.Common.DataTypes;

namespace NetStudio.Common.Manager;

public class Tag : ICloneable, INotifyPropertyChanged
{
	private dynamic value;

	private DataType dataType = DataType.INT;

	private TimeSpan time = DateTime.Now.TimeOfDay;

	private TagStatus state;

	public int ChannelId { get; set; }

	public int DeviceId { get; set; }

	public int GroupId { get; set; }

	public int Id { get; set; }

	public string Name { get; set; }

	public string Address { get; set; }

	public TagMode Mode { get; set; }

	[XmlIgnore]
	[JsonIgnore]
	public string ModeName => Mode.GetDescription();

	public string? Description { get; set; }

	[JsonIgnore]
	[XmlIgnore]
	public dynamic Value
	{
		get
		{
			return value;
		}
		set
		{
			if ((object)value == null)
			{
				SetDefaultValue(TagStatus.Good);
			}
			else if (!$"{(object?)this.value}".Equals($"{(object?)value}", StringComparison.CurrentCultureIgnoreCase))
			{
				Time = DateTime.Now.TimeOfDay;
				this.value = value;
				NotifyPropertyChanged("Value");
			}
		}
	}

	public dynamic? Value2 { get; set; }

	public DataType DataType
	{
		get
		{
			return dataType;
		}
		set
		{
			dataType = value;
			SetDefaultValue();
		}
	}

	[XmlIgnore]
	[JsonIgnore]
	public TimeSpan Time
	{
		get
		{
			return time;
		}
		set
		{
			time = value;
			NotifyPropertyChanged("Time");
		}
	}

	[JsonIgnore]
	[XmlIgnore]
	public TagStatus Status
	{
		get
		{
			return state;
		}
		set
		{
			if (state != value)
			{
				state = value;
				NotifyPropertyChanged("Status");
			}
		}
	}

	public ushort Resolution { get; set; } = 1;


	public bool IsScaling { get; set; }

	public int AImin { get; set; }

	public int AImax { get; set; }

	public float RLmin { get; set; }

	public float RLmax { get; set; }

	public float Offset { get; set; }

	public bool IsOperator { get; set; }

	[JsonIgnore]
	[XmlIgnore]
	public int WordAddress { get; set; }

	[JsonIgnore]
	[XmlIgnore]
	public int ByteAddress { get; set; }

	[XmlIgnore]
	[JsonIgnore]
	public int BitAddress { get; set; }

	[JsonIgnore]
	[XmlIgnore]
	public string FullName { get; set; }

	public event PropertyChangedEventHandler? PropertyChanged;

	private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
	{
		this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	public object Clone()
	{
		return MemberwiseClone();
	}

	public void SetDefaultValue(TagStatus tagStatus = TagStatus.Bad)
	{
		Status = tagStatus;
		switch (DataType)
		{
		default:
			Value = "XXXXX";
			break;
		case DataType.BOOL:
			Value = false;
			break;
		case DataType.BYTE:
			Value = (byte)0;
			break;
		case DataType.INT:
			Value = (short)0;
			break;
		case DataType.UINT:
			Value = (ushort)0;
			break;
		case DataType.WORD:
			Value = new string("0000");
			break;
		case DataType.DINT:
			Value = 0;
			break;
		case DataType.UDINT:
			Value = 0u;
			break;
		case DataType.DWORD:
			Value = new string("00000000");
			break;
		case DataType.REAL:
			Value = 0f;
			break;
		case DataType.LINT:
			Value = 0L;
			break;
		case DataType.ULINT:
			Value = 0uL;
			break;
		case DataType.LWORD:
			Value = new string("0000000000000000");
			break;
		case DataType.LREAL:
			Value = 0.0;
			break;
		case DataType.TIME16:
		case DataType.TIME32:
			Value = TimeSpan.FromMilliseconds(0.0);
			break;
		case DataType.STRING:
			Value = string.Empty;
			break;
		}
	}

	public string GetAddressLabel(Manufacturer manufacturer)
	{
        if (manufacturer == Manufacturer.PANASONIC)
		{
			return "FP Address:";
		}
		return "Address:";
	}

	public dynamic GetValueString(string valueString)
	{
		if (!IsScaling && Offset != 0f)
		{
			return DataType switch
			{
				DataType.BYTE => new BYTE($"{(byte)(double.Parse(valueString) * (double)Offset)}"), 
				DataType.INT => new INT($"{(short)(double.Parse(valueString) * (double)Offset)}"), 
				DataType.UINT => new UINT($"{(ushort)(double.Parse(valueString) * (double)Offset)}"), 
				DataType.WORD => $"{(ushort)(double.Parse(valueString, NumberStyles.HexNumber) * (double)Offset)}", 
				DataType.DINT => new DINT($"{(int)(double.Parse(valueString) * (double)Offset)}"), 
				DataType.UDINT => new UDINT($"{(uint)(double.Parse(valueString) * (double)Offset)}"), 
				DataType.DWORD => $"{(uint)(double.Parse(valueString, NumberStyles.HexNumber) * (double)Offset)}", 
				DataType.LINT => new LINT($"{(long)(double.Parse(valueString) * (double)Offset)}"), 
				DataType.ULINT => new ULINT($"{(ulong)(double.Parse(valueString) * (double)Offset)}"), 
				DataType.LWORD => $"{(ulong)(double.Parse(valueString, NumberStyles.HexNumber) * (double)Offset)}", 
				_ => 0, 
			};
		}
		return 0;
	}

	public T GetValue<T>(string valueString) where T : struct
	{
		if (IsScaling || Offset == 0f)
		{
			return Value;
		}
		return DataType switch
		{
			DataType.BYTE => (T)(object)new BYTE($"{(byte)(double.Parse(valueString) * (double)Offset)}"), 
			DataType.INT => (T)(object)new INT($"{(short)(double.Parse(valueString) * (double)Offset)}"), 
			DataType.UINT => (T)(object)new UINT($"{(ushort)(double.Parse(valueString) * (double)Offset)}"), 
			DataType.WORD => (T)(object)new UINT($"{(ushort)(double.Parse(valueString, NumberStyles.HexNumber) * (double)Offset)}"), 
			DataType.DINT => (T)(object)new DINT($"{(int)(double.Parse(valueString) * (double)Offset)}"), 
			DataType.UDINT => (T)(object)new UDINT($"{(uint)(double.Parse(valueString) * (double)Offset)}"), 
			DataType.DWORD => (T)(object)new DWORD($"{(uint)(double.Parse(valueString, NumberStyles.HexNumber) * (double)Offset)}"), 
			DataType.LINT => (T)(object)new LINT($"{(long)(double.Parse(valueString) * (double)Offset)}"), 
			DataType.ULINT => (T)(object)new ULINT($"{(ulong)(double.Parse(valueString) * (double)Offset)}"), 
			DataType.LWORD => (T)(object)new ULINT($"{(ulong)(double.Parse(valueString, NumberStyles.HexNumber) * (double)Offset)}"), 
			_ => default(T), 
		};
	}
}
