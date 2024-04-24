using System.Windows.Forms;

namespace NetStudio.IPS.Controls;

public class DataGridViewButtonTextBoxColumn : DataGridViewTextBoxColumn
{
	public DataGridViewButtonTextBoxColumn()
	{
		CellTemplate = new DataGridViewButtonTextBoxCell();
	}
}
