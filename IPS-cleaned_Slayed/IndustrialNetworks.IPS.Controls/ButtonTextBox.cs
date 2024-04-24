using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace NetStudio.IPS.Controls;

public class ButtonTextBox : TextBox
{
	private readonly Button _button = new Button
	{
		Cursor = Cursors.Default,
		TextAlign = ContentAlignment.MiddleCenter,
		Text = "..."
	};

	public int ButtonWidth { get; set; } = 30;


	public string ButtonText
	{
		get
		{
			return _button.Text;
		}
		set
		{
			_button.Text = value;
		}
	}

	public Button Button => _button;

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

	public ButtonTextBox()
	{
		_button.SizeChanged += delegate(object? sender, EventArgs e)
		{
			OnResize(e);
		};
		base.Controls.Add(_button);
	}

	protected override void OnResize(EventArgs eventArgs_0)
	{
		base.OnResize(eventArgs_0);
		_button.Size = new Size(ButtonWidth, base.ClientSize.Height + 2);
		_button.Location = new Point(base.ClientSize.Width - (ButtonWidth - 1), -1);
		SendMessage(base.Handle, 211, 2, _button.Width << 16);
	}

	[DllImport("user32.dll")]
	private static extern nint SendMessage(nint hWnd, int y, nint intptr_0, nint intptr_1);
}
