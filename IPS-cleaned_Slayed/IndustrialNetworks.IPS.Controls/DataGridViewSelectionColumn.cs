using System.Windows.Forms;

namespace NetStudio.IPS.Controls;

public class DataGridViewSelectionColumn : DataGridViewButtonColumn
{
	public string LabelText { get; set; } = "           ";


	public string ButtonText { get; set; } = "...";


	public DataGridViewSelectionColumn()
	{
		CellTemplate = new DataGridViewAllocationControlCell();
	}

	public override object Clone()
	{
		DataGridViewSelectionColumn obj = (DataGridViewSelectionColumn)base.Clone();
		obj.LabelText = LabelText;
		obj.ButtonText = ButtonText;
		return obj;
	}
}
