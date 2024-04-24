using System.Runtime.InteropServices;

namespace NetStudio.IPS.Entity;

internal class User32API
{
	public const int SW_RESTORE = 9;

	[DllImport("User32.dll")]
	public static extern bool IsIconic(nint hWnd);

	[DllImport("User32.dll")]
	public static extern bool SetForegroundWindow(nint hWnd);

	[DllImport("User32.dll")]
	public static extern bool ShowWindow(nint hWnd, int nCmdShow);
}
