using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace NetStudio.IPS.Controls;

public class DataGridLog : DataGridView
{
	protected override void OnCreateControl()
	{
		base.OnCreateControl();
		base.VirtualMode = true;
		base.RowHeadersVisible = false;
		Dock = DockStyle.Fill;
		base.ReadOnly = true;
		base.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
		base.MultiSelect = false;
		base.AllowUserToAddRows = false;
		base.AllowUserToDeleteRows = false;
		base.AllowUserToOrderColumns = false;
		base.BackgroundColor = Color.White;
		base.BorderStyle = BorderStyle.Fixed3D;
		base.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
		base.GridColor = Color.GhostWhite;
		base.AutoGenerateColumns = false;
		base.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.Empty;
		base.ColumnHeadersDefaultCellStyle.SelectionForeColor = Color.Empty;
		typeof(DataGridLog).InvokeMember("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.SetProperty, null, this, new object[1] { true });
		base.DataError += DataGrid_DataError;
		base.SelectionChanged += delegate
		{
			ClearSelection();
		};
	}

	private void DataGrid_DataError(object? sender, DataGridViewDataErrorEventArgs e)
	{
	}
}
