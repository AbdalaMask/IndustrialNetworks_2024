using System.Xml.Serialization;
using NetStudio.Common.DataTypes;

namespace NetStudio.IPS.Entity;

[XmlInclude(typeof(REAL))]
[XmlInclude(typeof(ULINT))]
[XmlInclude(typeof(WORD))]
[XmlInclude(typeof(DINT))]
[XmlInclude(typeof(UINT))]
[XmlInclude(typeof(DWORD))]
[XmlInclude(typeof(BYTE))]
[XmlInclude(typeof(LINT))]
[XmlInclude(typeof(BOOL))]
[XmlInclude(typeof(LWORD))]
[XmlInclude(typeof(LREAL))]
[XmlInclude(typeof(INT))]
[XmlInclude(typeof(UDINT))]
public class RtTag
{
	public int Id { get; set; }

	public string TagName { get; set; }

	public string Address { get; set; }

	public DataType DataType { get; set; }

	public dynamic Value { get; set; }

	public string? Description { get; set; }
}
