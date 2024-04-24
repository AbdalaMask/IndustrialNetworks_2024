using System;

namespace NetStudio.Common.AsrsLink;

public class SqlColumnInfo : ICloneable
{
	public int Id { get; set; }

	public string ColumnName { get; set; }

	public string DataType { get; set; }

	public int MaxLength { get; set; }

	public short Precision { get; set; }

	public short Scale { get; set; }

	public bool IsNullable { get; set; }

	public bool PrimaryKey { get; set; }

	public object Clone()
	{
		return MemberwiseClone();
	}
}
