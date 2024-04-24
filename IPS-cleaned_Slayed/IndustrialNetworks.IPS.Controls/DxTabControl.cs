using System.Drawing;
using System.Windows.Forms;

namespace NetStudio.IPS.Controls;

public class DxTabControl : TabControl
{
	protected override void OnCreateControl()
	{
		base.OnCreateControl();
		base.Appearance = TabAppearance.FlatButtons;
		base.ItemSize = new Size(0, 1);
		base.SizeMode = TabSizeMode.Fixed;
	}
}
