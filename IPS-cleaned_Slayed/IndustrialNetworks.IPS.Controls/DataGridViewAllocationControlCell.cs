using System.Drawing;
using System.Windows.Forms;

namespace NetStudio.IPS.Controls;

public class DataGridViewAllocationControlCell : DataGridViewButtonCell
{
	protected override void Paint(Graphics graphics, Rectangle clipBounds, Rectangle cellBounds, int rowIndex, DataGridViewElementStates elementStatus, object value, object formattedValue, string errorText, DataGridViewCellStyle cellStyle, DataGridViewAdvancedBorderStyle advancedBorderStyle, DataGridViewPaintParts paintParts)
	{
		DataGridView? dataGridView = base.DataGridView;
		DataGridViewSelectionColumn dataGridViewSelectionColumn = (DataGridViewSelectionColumn)base.OwningColumn;
		base.Paint(graphics, clipBounds, cellBounds, rowIndex, elementStatus, value, formattedValue, errorText, cellStyle, advancedBorderStyle, DataGridViewPaintParts.Background | DataGridViewPaintParts.Border | DataGridViewPaintParts.ErrorIcon | DataGridViewPaintParts.Focus | DataGridViewPaintParts.SelectionBackground);
		Rectangle cellDisplayRectangle = dataGridView.GetCellDisplayRectangle(dataGridViewSelectionColumn.Index, rowIndex, cutOverflow: false);
		Rectangle contentBounds = GetContentBounds(rowIndex);
		Rectangle bounds = new Rectangle(cellDisplayRectangle.Location, new Size(GetLabelWidth(), cellDisplayRectangle.Height));
		contentBounds.Offset(cellDisplayRectangle.Location);
		base.Paint(graphics, clipBounds, contentBounds, rowIndex, elementStatus, value, (object?)dataGridViewSelectionColumn.ButtonText, errorText, cellStyle, advancedBorderStyle, DataGridViewPaintParts.All);
		TextRenderer.DrawText(graphics, dataGridViewSelectionColumn.LabelText, cellStyle.Font, bounds, cellStyle.ForeColor);
	}

	protected override Rectangle GetContentBounds(Graphics graphics, DataGridViewCellStyle cellStyle, int rowIndex)
	{
		int labelWidth = GetLabelWidth();
		Rectangle contentBounds = base.GetContentBounds(graphics, cellStyle, rowIndex);
		return new Rectangle(contentBounds.Left + labelWidth, contentBounds.Top, contentBounds.Width - labelWidth, contentBounds.Height);
	}

	protected override void OnContentClick(DataGridViewCellEventArgs dataGridViewCellEventArgs_0)
	{
		base.OnContentClick(dataGridViewCellEventArgs_0);
		DataGridView dataGridView = base.DataGridView;
		DataGridViewSelectionColumn dataGridViewSelectionColumn = (DataGridViewSelectionColumn)base.OwningColumn;
		Rectangle contentBounds = GetContentBounds(dataGridViewCellEventArgs_0.RowIndex);
		Rectangle cellDisplayRectangle = dataGridView.GetCellDisplayRectangle(dataGridViewCellEventArgs_0.ColumnIndex, dataGridViewCellEventArgs_0.RowIndex, cutOverflow: false);
		Point position = new Point(cellDisplayRectangle.Left + contentBounds.Left, cellDisplayRectangle.Top + contentBounds.Bottom);
		if (dataGridViewSelectionColumn.ContextMenuStrip != null)
		{
			dataGridViewSelectionColumn.ContextMenuStrip.Show(dataGridView, position);
		}
	}

	private int GetLabelWidth()
	{
		DataGridViewSelectionColumn dataGridViewSelectionColumn = (DataGridViewSelectionColumn)base.OwningColumn;
		return TextRenderer.MeasureText(dataGridViewSelectionColumn.LabelText, dataGridViewSelectionColumn.DefaultCellStyle.Font).Width;
	}
}
