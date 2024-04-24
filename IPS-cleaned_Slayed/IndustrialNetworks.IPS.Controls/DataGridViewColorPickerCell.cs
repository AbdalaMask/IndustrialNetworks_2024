using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace NetStudio.IPS.Controls;

public class DataGridViewColorPickerCell : DataGridViewTextBoxCell
{
	private DateTime selectedDateTime;

	private static Type valueType = typeof(byte);

	private static Type editType = typeof(DataGridViewColourPickerEditor);

	private StringAlignment verticalAlignment;

	private StringAlignment horizontalAlignment;

	private const int RECTCOLOR_LEFT = 0;

	private const int RECTCOLOR_TOP = 0;

	private const int RECTCOLOR_WIDTH = 20;

	private const int RECTTEXT_MARGIN = 0;

	private const int RECTTEXT_LEFT = 20;

	private static StringFormat stringFormat_0;

	public override Type EditType => editType;

	public override Type ValueType => valueType;

	public DateTime SelectedDateTime
	{
		get
		{
			return selectedDateTime;
		}
		set
		{
			selectedDateTime = value;
		}
	}

	private DataGridViewColourPickerEditor DataGridViewColourPickerEditor_0 => base.DataGridView.EditingControl as DataGridViewColourPickerEditor;

	[DefaultValue("Center")]
	public StringAlignment VerticalAlignment
	{
		get
		{
			return verticalAlignment;
		}
		set
		{
			verticalAlignment = value;
		}
	}

	[DefaultValue("Near")]
	public StringAlignment HorizontalAlignment
	{
		get
		{
			return horizontalAlignment;
		}
		set
		{
			horizontalAlignment = value;
		}
	}

	protected override void Paint(Graphics graphics, Rectangle clipBounds, Rectangle cellBounds, int rowIndex, DataGridViewElementStates cellStatus, object value, object formattedValue, string errorText, DataGridViewCellStyle cellStyle, DataGridViewAdvancedBorderStyle advancedBorderStyle, DataGridViewPaintParts paintParts)
	{
		try
		{
			base.Paint(graphics, clipBounds, cellBounds, rowIndex, cellStatus, value, formattedValue, errorText, cellStyle, advancedBorderStyle, DataGridViewPaintParts.All);
			if (base.DataGridView != null)
			{
				if (stringFormat_0 == null)
				{
					stringFormat_0 = new StringFormat();
					stringFormat_0.LineAlignment = StringAlignment.Far;
					stringFormat_0.Alignment = StringAlignment.Far;
				}
				Rectangle colorBoxRect = default(Rectangle);
				RectangleF textBoxRect = default(RectangleF);
				GetDisplayLayout(cellBounds, ref colorBoxRect, ref textBoxRect);
				Color white = Color.White;
				if (value != null && !(value.GetType() == typeof(DBNull)))
				{
					white = Color.FromKnownColor((KnownColor)(byte)value);
					SolidBrush solidBrush = new SolidBrush(white);
					graphics.FillRectangle(solidBrush, colorBoxRect);
					graphics.DrawRectangle(Pens.Black, colorBoxRect);
					graphics.DrawString(white.Name, cellStyle.Font, Brushes.Black, textBoxRect);
					solidBrush.Dispose();
				}
			}
		}
		catch (Exception)
		{
		}
	}

	public override object ParseFormattedValue(object formattedValue, DataGridViewCellStyle cellStyle, TypeConverter formattedValueTypeConverter, TypeConverter valueTypeConverter)
	{
		return base.ParseFormattedValue(formattedValue, cellStyle, formattedValueTypeConverter, valueTypeConverter);
	}

	protected virtual void GetDisplayLayout(Rectangle CellRect, ref Rectangle colorBoxRect, ref RectangleF textBoxRect)
	{
		colorBoxRect.X = CellRect.X + 2 + 2;
		colorBoxRect.Y = CellRect.Y + 4;
		colorBoxRect.Size = new Size(37, CellRect.Height - 10);
		textBoxRect = RectangleF.FromLTRB(colorBoxRect.X + colorBoxRect.Width + 5, colorBoxRect.Y - 1, CellRect.X + CellRect.Width - 2, colorBoxRect.Y + colorBoxRect.Height);
	}

	private static bool PartPainted(DataGridViewPaintParts paintParts, DataGridViewPaintParts paintPart)
	{
		return (paintParts & paintPart) != 0;
	}

	private static bool IsInStatus(DataGridViewElementStates currentStatus, DataGridViewElementStates checkStatus)
	{
		return (currentStatus & checkStatus) != 0;
	}

	private bool OwnsEditor(int rowIndex)
	{
		if (rowIndex != -1 && base.DataGridView != null)
		{
			if (base.DataGridView.EditingControl is DataGridViewColourPickerEditor dataGridViewColourPickerEditor)
			{
				return rowIndex == dataGridViewColourPickerEditor.EditingControlRowIndex;
			}
			return false;
		}
		return false;
	}

	internal void SetValue(int rowIndex, DateTime value)
	{
	}

	public override void InitializeEditingControl(int rowIndex, object initialFormattedValue, DataGridViewCellStyle dataGridViewCellStyle)
	{
		base.InitializeEditingControl(rowIndex, initialFormattedValue, dataGridViewCellStyle);
		if (base.DataGridView.EditingControl is DataGridViewColourPickerEditor dataGridViewColourPickerEditor)
		{
			dataGridViewColourPickerEditor.RightToLeft = base.DataGridView.RightToLeft;
			initialFormattedValue.ToString();
			if (byte.TryParse(initialFormattedValue.ToString(), out var result))
			{
				dataGridViewColourPickerEditor.SelectedIndex = result - 1;
			}
		}
	}
}
