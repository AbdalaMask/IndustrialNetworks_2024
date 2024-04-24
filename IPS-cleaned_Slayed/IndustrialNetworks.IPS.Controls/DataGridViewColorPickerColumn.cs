using System.Windows.Forms;

namespace NetStudio.IPS.Controls;

public class DataGridViewColorPickerColumn : DataGridViewColumn
{
	public DataGridViewColorPickerColumn()
		: base(new DataGridViewColorPickerCell())
	{
	}
}
