using System;
using System.Drawing;
using System.Windows.Forms;

namespace NetStudio.IPS.Controls;

public class ColorPicker : ComboBox
{
	private const int RECTCOLOR_LEFT = 4;

	private const int RECTCOLOR_TOP = 3;

	private const int RECTCOLOR_WIDTH = 40;

	private const int RECTTEXT_MARGIN = 3;

	private const int RECTTEXT_LEFT = 47;

	public ColorPicker()
	{
		base.DrawMode = DrawMode.OwnerDrawFixed;
		base.DropDownStyle = ComboBoxStyle.DropDownList;
		for (byte b = 1; b < 174; b++)
		{
			MyColour item = new MyColour
			{
				Colour = Color.FromKnownColor((KnownColor)b),
				Colourid = b
			};
			base.Items.Add(item);
		}
		base.DisplayMember = "Name";
		base.ValueMember = "Colourid";
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
	}

	protected override void OnDrawItem(DrawItemEventArgs drawItemEventArgs_0)
	{
		if (drawItemEventArgs_0.State == DrawItemState.Selected || drawItemEventArgs_0.State == DrawItemState.None)
		{
			drawItemEventArgs_0.DrawBackground();
		}
		Graphics graphics = drawItemEventArgs_0.Graphics;
        int x = 4;
        Color empty = drawItemEventArgs_0.Index != -1 ? ((MyColour)Items[drawItemEventArgs_0.Index]).Colour : SelectedIndex < 0 ? BackColor : Color.FromName(SelectedText);
        graphics.FillRectangle(new SolidBrush(empty), x, drawItemEventArgs_0.Bounds.Top + 3, 40, base.ItemHeight - 6);
		graphics.DrawRectangle(Pens.Black, x, drawItemEventArgs_0.Bounds.Top + 3, 40, base.ItemHeight - 6);
		graphics.DrawString(empty.Name, drawItemEventArgs_0.Font, new SolidBrush(ForeColor), new Rectangle(47, drawItemEventArgs_0.Bounds.Top, drawItemEventArgs_0.Bounds.Width - 47, base.ItemHeight));
		base.OnDrawItem(drawItemEventArgs_0);
	}

	protected override void OnDropDownStyleChanged(EventArgs eventArgs_0)
	{
		if (base.DropDownStyle != ComboBoxStyle.DropDownList)
		{
			base.DropDownStyle = ComboBoxStyle.DropDownList;
		}
	}
}
