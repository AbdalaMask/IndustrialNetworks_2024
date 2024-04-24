using System;
using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace NetStudio.Common.AsrsLink;

public class AsrsRow : ICloneable
{
	public int Id { get; set; }

	public short TableId { get; set; }

	public string ColumnId { get; set; }

	public string ColumnName { get; set; }

	public string ValueOfId { get; set; }

	public int TagId { get; set; }

	public int GroupId { get; set; }

	public int DeviceId { get; set; }

	public int ChannelId { get; set; }

	public OperatingMode Mode { get; set; }

	public AsrsTrigger Trigger { get; set; } = AsrsTrigger.Positive;


	public string TagName { get; set; }

	[XmlIgnore]
	[JsonIgnore]
	public string LinkToSqlCommandText { get; set; }

	[XmlIgnore]
	[JsonIgnore]
	public string LinkToPlcCommandText { get; set; }

	[XmlIgnore]
	[JsonIgnore]
	public dynamic Value { get; set; }

	public object Clone()
	{
		return MemberwiseClone();
	}

	public override string ToString()
	{
		return $"{ColumnId}.{ColumnName}.{ValueOfId}.{Mode}.{Trigger}.{TagName}";
	}
}
