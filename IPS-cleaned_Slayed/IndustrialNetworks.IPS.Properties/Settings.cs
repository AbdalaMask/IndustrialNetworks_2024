using System.CodeDom.Compiler;
using System.Configuration;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace NetStudio.IPS.Properties;

[GeneratedCode("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "17.9.0.0")]
[CompilerGenerated]
internal sealed class Settings : ApplicationSettingsBase
{
	private static Settings defaultInstance = (Settings)SettingsBase.Synchronized(new Settings());

	public static Settings Default => defaultInstance;

	[DebuggerNonUserCode]
	[DefaultSettingValue("")]
	[UserScopedSetting]
	public string Directory
	{
		get
		{
			return (string)this["Directory"];
		}
		set
		{
			this["Directory"] = value;
		}
	}

	[DebuggerNonUserCode]
	[DefaultSettingValue("")]
	[UserScopedSetting]
	public string FileName
	{
		get
		{
			return (string)this["FileName"];
		}
		set
		{
			this["FileName"] = value;
		}
	}

	[DefaultSettingValue("False")]
	[UserScopedSetting]
	[DebuggerNonUserCode]
	public bool Mode
	{
		get
		{
			return (bool)this["Mode"];
		}
		set
		{
			this["Mode"] = value;
		}
	}

	[UserScopedSetting]
	[DefaultSettingValue("127.0.0.1")]
	[DebuggerNonUserCode]
	public string IP
	{
		get
		{
			return (string)this["IPAddress"];
		}
		set
		{
			this["IPAddress"] = value;
		}
	}

	[DefaultSettingValue("5012")]
	[UserScopedSetting]
	[DebuggerNonUserCode]
	public int Port
	{
		get
		{
			return (int)this["Port"];
		}
		set
		{
			this["Port"] = value;
		}
	}

	[DebuggerNonUserCode]
	[DefaultSettingValue("")]
	[UserScopedSetting]
	public string UserName
	{
		get
		{
			return (string)this["UserName"];
		}
		set
		{
			this["UserName"] = value;
		}
	}

	[DefaultSettingValue("")]
	[UserScopedSetting]
	[DebuggerNonUserCode]
	public string Password
	{
		get
		{
			return (string)this["Password"];
		}
		set
		{
			this["Password"] = value;
		}
	}

	[DefaultSettingValue("True")]
	[UserScopedSetting]
	[DebuggerNonUserCode]
	public bool ScalingColumn
	{
		get
		{
			return (bool)this["ScalingColumn"];
		}
		set
		{
			this["ScalingColumn"] = value;
		}
	}

	[UserScopedSetting]
	[DebuggerNonUserCode]
	[DefaultSettingValue("True")]
	public bool OffsetColumn
	{
		get
		{
			return (bool)this["OffsetColumn"];
		}
		set
		{
			this["OffsetColumn"] = value;
		}
	}

	[DefaultSettingValue("True")]
	[UserScopedSetting]
	[DebuggerNonUserCode]
	public bool ResolutionColumn
	{
		get
		{
			return (bool)this["ResolutionColumn"];
		}
		set
		{
			this["ResolutionColumn"] = value;
		}
	}
}
