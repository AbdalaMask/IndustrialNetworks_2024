using System.Drawing;

namespace NetStudio.IPS.Controls;

public class MyColour
{
	private byte _colourid;

	private Color _colour;

	public byte Colourid
	{
		get
		{
			return _colourid;
		}
		set
		{
			_colourid = value;
		}
	}

	public Color Colour
	{
		get
		{
			return _colour;
		}
		set
		{
			_colour = value;
		}
	}
}
