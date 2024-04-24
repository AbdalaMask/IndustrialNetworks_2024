using System.Diagnostics;

namespace NetStudio.IPS.Entity;

public sealed class SingleInstance
{
	public static bool AlreadyRunning()
	{
		bool result = false;
		try
		{
			Process currentProcess = Process.GetCurrentProcess();
			Process[] processes = Process.GetProcesses();
			foreach (Process process in processes)
			{
				if (process.Id != currentProcess.Id && process.ProcessName.Equals(currentProcess.ProcessName))
				{
					result = true;
					nint mainWindowHandle = process.MainWindowHandle;
					if (User32API.IsIconic(mainWindowHandle))
					{
						User32API.ShowWindow(mainWindowHandle, 9);
					}
					User32API.SetForegroundWindow(mainWindowHandle);
					break;
				}
			}
		}
		catch
		{
		}
		return result;
	}
}
