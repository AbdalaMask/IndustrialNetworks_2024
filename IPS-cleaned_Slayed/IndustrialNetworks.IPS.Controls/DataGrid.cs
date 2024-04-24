using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace NetStudio.IPS.Controls;

public class DataGrid : DataGridView
{
	protected override void OnCreateControl()
	{
		base.OnCreateControl();
		base.VirtualMode = true;
		base.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
		base.MultiSelect = false;
		base.AllowUserToOrderColumns = false;
		base.BackgroundColor = Color.LightGray;
		base.BorderStyle = BorderStyle.Fixed3D;
		base.AutoGenerateColumns = false;
		base.DefaultCellStyle.SelectionBackColor = Color.LightGoldenrodYellow;
		base.DefaultCellStyle.SelectionForeColor = Color.Black;
		base.RowHeadersDefaultCellStyle.SelectionBackColor = Color.Empty;
		base.RowsDefaultCellStyle.BackColor = Color.LightGray;
		base.AlternatingRowsDefaultCellStyle.BackColor = Color.DarkGray;
		base.EnableHeadersVisualStyles = true;
		base.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
		base.ColumnHeadersDefaultCellStyle.BackColor = Color.Black;
		base.RowHeadersDefaultCellStyle.BackColor = Color.Black;
		SetDoubleBuffered(this, setting: true);
		base.RowPostPaint += DataGrid_RowPostPaint;
		base.DataError += DataGrid_DataError;
	}

	private void SetDoubleBuffered(DataGridView dataGridView_0, bool setting)
	{
		Type type = dataGridView_0.GetType();
		if (type != null)
		{
			PropertyInfo property = type.GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);
			if (property != null)
			{
				property.SetValue(dataGridView_0, setting, null);
			}
		}
	}

	private void DataGrid_RowPostPaint(object? sender, DataGridViewRowPostPaintEventArgs e)
	{
		try
		{
			if (sender is DataGridView dataGridView && e.RowIndex >= 0)
			{
				string text = $"{e.RowIndex + 1}";
				StringFormat format = new StringFormat
				{
					Alignment = StringAlignment.Center,
					LineAlignment = StringAlignment.Center
				};
				Size size = TextRenderer.MeasureText(text, dataGridView.Font);
				if (dataGridView.RowHeadersWidth < size.Width + 40)
				{
					dataGridView.RowHeadersWidth = size.Width + 40;
				}
				Rectangle rectangle = new Rectangle(e.RowBounds.Left, e.RowBounds.Top, dataGridView.RowHeadersWidth, e.RowBounds.Height);
				e.Graphics.DrawString(text, dataGridView.Font, SystemBrushes.ControlText, rectangle, format);
			}
		}
		catch (Exception)
		{
		}
	}

	public void DataGrid_DataError(object? sender, DataGridViewDataErrorEventArgs e)
	{
	}
}
