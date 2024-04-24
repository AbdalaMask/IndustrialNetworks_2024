using System;
using System.Drawing;
using System.Windows.Forms;

namespace NetStudio.IPS.Controls;

public class DataGridViewButtonTextBoxCell : DataGridViewTextBoxCell
{
	private readonly Button _button = new Button
	{
		Cursor = Cursors.Default,
		TextAlign = ContentAlignment.MiddleCenter,
		Text = "..."
	};

	public event EventHandler ButtonClick
	{
		add
		{
			_button.Click += value;
		}
		remove
		{
			_button.Click -= value;
		}
	}
}
