using System;
using System.Windows.Forms;

namespace NetStudio.IPS;

internal static class Program
{
	[STAThread]
	private static void Main()
	{
		ApplicationConfiguration.Initialize();
		Application.Run(new FormMain());
	}
}
