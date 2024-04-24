using System;
using System.Diagnostics;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;

namespace NetStudio.Common.Hardware;

public static class HardwareInfo
{
	[DllImport("kernel32.dll")]
	private static extern long GetVolumeInformation(string PathName, StringBuilder VolumeNameBuffer, uint VolumeNameSize, ref uint VolumeSerialNumber, ref uint MaximumComponentLength, ref uint FileSystemFlags, StringBuilder FileSystemNameBuffer, uint FileSystemNameSize);

	public static string GetDriveInfo(string driveLetter)
	{
        uint VolumeSerialNumber = 0u;
		uint MaximumComponentLength = 0u;
		StringBuilder stringBuilder = new StringBuilder(256);
		uint FileSystemFlags = 0u;
		StringBuilder stringBuilder2 = new StringBuilder(256);
		if (GetVolumeInformation(driveLetter, stringBuilder, (uint)stringBuilder.Capacity, ref VolumeSerialNumber, ref MaximumComponentLength, ref FileSystemFlags, stringBuilder2, (uint)stringBuilder2.Capacity) == 0L)
		{
			throw new Exception("Error getting volume information.");
		}
		return VolumeSerialNumber.ToString();
	}

	public static DriveInfo GetDriveInfo()
	{
		ManagementObjectSearcher managementObjectSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive");
		DriveInfo driveInfo = new DriveInfo();
		foreach (ManagementObject item in managementObjectSearcher.Get())
		{
			driveInfo.Model = item["Model"].ToString().Trim();
			driveInfo.Type = item["InterfaceType"].ToString().Trim();
			driveInfo.SerialNo = item["SerialNumber"].ToString().Trim();
		}
		return driveInfo;
	}

	public static string ExecuteCommandSync(object command)
	{
		try
		{
			if (command == null)
			{
				command = "vol";
			}
			ProcessStartInfo processStartInfo = new ProcessStartInfo("cmd", "/c " + command);
			processStartInfo.RedirectStandardOutput = true;
			processStartInfo.UseShellExecute = false;
			processStartInfo.CreateNoWindow = true;
			Process process = new Process();
			process.StartInfo = processStartInfo;
			process.Start();
			return process.StandardOutput.ReadToEnd();
		}
		catch (Exception)
		{
			return string.Empty;
		}
	}
}
