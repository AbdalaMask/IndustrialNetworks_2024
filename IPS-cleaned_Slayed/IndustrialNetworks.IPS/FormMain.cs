using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.ServiceProcess;
using System.Threading;
using System.Windows.Forms;
using NetStudio.Common.Manager;
using NetStudio.DriverComm.Models;
using NetStudio.IPS.Controls;
using NetStudio.IPS.Editor;
using NetStudio.IPS.Editor.Alarms;
using NetStudio.IPS.Editor.AsrsLink;
using NetStudio.IPS.Editor.Historiant;
using NetStudio.IPS.Local;
using NetStudio.IPS.Properties;

namespace NetStudio.IPS;

public class FormMain : Form
{
	private DriverController driverController;

	private IContainer components;

	private ToolStrip toolBar;

	private StatusStrip statusBar;

	private ToolStripStatusLabel lblYoutube;

	private ToolStripButton btnNewProject;

	private ToolStripButton btnTagEdit;

	private ToolStripButton btnOpenProject;

	private MenuStrip menu;

	private ToolStripMenuItem fileToolStripMenuItem;

	private ToolStripMenuItem mnNewProject;

	private ToolStripMenuItem mnOpenProject;

	private ToolStripSeparator toolStripSeparator5;

	private ToolStripMenuItem mnImport;

	private ToolStripMenuItem mnExport;

	private ToolStripMenuItem mnQuit;

	private ToolStripMenuItem editToolStripMenuItem;

	private ToolStripMenuItem mnTagEdit;

	private ToolStripStatusLabel lblMode;

	private ToolStripStatusLabel toolStripStatusLabel3;

	private ToolStripButton btnAbout;

	private ToolStripMenuItem helpToolStripMenuItem;

	private ToolStripMenuItem mnAbout;

	private ToolStripButton btnMonitoring;

	private ToolStripSeparator toolStripSeparator3;

	private ToolStripSeparator toolStripSeparator6;

	private ToolStripLabel toolStripLabel1;

	private ToolStripSeparator toolStripSeparator7;

	private ToolStripButton btnQuit;

	private ToolStripButton btnSettings;

	private ToolStripStatusLabel lblIPAddress;

	private ToolStripStatusLabel lblPort;

	private ToolStripStatusLabel lblUserName;

	private ToolStripButton btnRun;

	private ToolStripButton btnStop;

	private ToolStripSeparator toolStripSeparator1;

	private ToolStripButton btnRestart;

	private ToolStripMenuItem mnLicensermation;

	private ToolStripButton btnAlarms;

	private ToolStripButton btnHistoricalData;

	private ToolStripSeparator toolStripSeparator2;

	private ToolStripButton btnDataLink;

	private ToolStripButton btnASRSLink;

	private ToolStripMenuItem mnSaveAs;

	private ToolStripButton btnSave;

	private ToolStripMenuItem mnSave;

	private ToolStripStatusLabel lblStatus;

	public FormMain()
	{
		InitializeComponent();
		lblStatus.Visible = false;
		base.FormClosing += FormMain_FormClosing;
		ClientDataSource.OnCommunicationStateChanged = (EventCommunicationStateChanged)Delegate.Combine(ClientDataSource.OnCommunicationStateChanged, new EventCommunicationStateChanged(OnCommunicationStatusChanged));
		SplashScreenManager.Show();
		try
		{
		//	OnCheckDriverServer();
			//OnLoadSettings();
			Thread.Sleep(3000);
		}
		catch (Exception ex)
		{
			MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
		finally
		{
			SplashScreenManager.Close();
		}
	}

	private void FormMain_FormClosing(object? sender, FormClosingEventArgs e)
	{
		try
		{
			Form[] mdiChildren;
			try
			{
				if (AppHelper.DataChanged && MessageBox.Show(this, "Do you want to save the changes?", "Question", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
				{
					mdiChildren = base.MdiChildren;
					foreach (Form form in mdiChildren)
					{
						if (form is FormHistoricalData)
						{
							((FormHistoricalData)form).ValidateDataToSave();
						}
					}
					AppHelper.WriteProject();
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
			}
			AppHelper.CloseApp = true;
			mdiChildren = base.MdiChildren;
			for (int i = 0; i < mdiChildren.Length; i++)
			{
				mdiChildren[i].Close();
			}
		}
		catch (Exception ex2)
		{
			MessageBox.Show(this, ex2.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private void OnCommunicationStatusChanged(CommunicationState state)
	{
		try
		{
			if (state == CommunicationState.Closed)
			{
				lblStatus.Text = "Disconnect";
				lblStatus.BackColor = Color.Red;
			}
			else
			{
				lblStatus.Text = "Connected";
				lblStatus.BackColor = Color.Green;
			}
		}
		catch (Exception)
		{
		}
	}

	private async void OnCheckDriverServer()
	{
		try
		{
			driverController = new DriverController(this);
			DriverController obj = driverController;
			obj.OnDriverStatusChanged = (EventDriverStatusChanged)Delegate.Combine(obj.OnDriverStatusChanged, new EventDriverStatusChanged(OnDriverStatusChanged));
			if (!NetStudio.IPS.Properties.Settings.Default.Mode || driverController.DriverService == null || driverController.DriverStatus != ServiceControllerStatus.Stopped)
			{
				return;
			}
			SplashScreenManager.Close();
			if (MessageBox.Show(this, "Do you want to start the Driver server?", "Question", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
			{
				int num = 0;
				object obj2 = default(object);
				try
				{
					driverController.Start();
				}
				catch (Exception ex2)
				{
					obj2 = ex2;
					num = 1;
				}
				if (num == 1)
				{
					Exception ex = (Exception)obj2;
					await WaitFormManager.CloseAsync();
					MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
				}
			}
		}
		catch (Exception ex3)
		{
			MessageBox.Show(this, ex3.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private void OnDriverStatusChanged(ServiceControllerStatus status, bool notAvailable)
	{
		if (notAvailable)
		{
			btnRun.Enabled = false;
			btnStop.Enabled = btnRun.Enabled;
			btnRestart.Enabled = btnRun.Enabled;
		}
		else
		{
			SetEnableButtons(status);
		}
	}

	private async void SetEnableButtons(ServiceControllerStatus status)
	{
		if (NetStudio.IPS.Properties.Settings.Default.Mode)
		{
			switch (status)
			{
			case ServiceControllerStatus.Stopped:
				btnRun.Enabled = true;
				btnStop.Enabled = !btnRun.Enabled;
				btnRestart.Enabled = !btnRun.Enabled;
				btnTagEdit.Enabled = !btnRun.Enabled;
				btnMonitoring.Enabled = !btnRun.Enabled;
				btnNewProject.Enabled = !btnRun.Enabled;
				btnOpenProject.Enabled = !btnRun.Enabled;
				mnNewProject.Enabled = !btnRun.Enabled;
				mnOpenProject.Enabled = !btnRun.Enabled;
				mnTagEdit.Enabled = !btnRun.Enabled;
				await WaitFormManager.CloseAsync();
				break;
			case ServiceControllerStatus.Running:
				btnRun.Enabled = false;
				btnStop.Enabled = !btnRun.Enabled;
				btnRestart.Enabled = !btnRun.Enabled;
				btnTagEdit.Enabled = !btnRun.Enabled;
				btnMonitoring.Enabled = !btnRun.Enabled;
				btnNewProject.Enabled = !btnRun.Enabled;
				btnOpenProject.Enabled = !btnRun.Enabled;
				mnNewProject.Enabled = !btnRun.Enabled;
				mnOpenProject.Enabled = !btnRun.Enabled;
				mnTagEdit.Enabled = !btnRun.Enabled;
				await WaitFormManager.CloseAsync();
				break;
			case ServiceControllerStatus.StartPending:
			case ServiceControllerStatus.StopPending:
			case ServiceControllerStatus.ContinuePending:
				btnRun.Enabled = false;
				btnStop.Enabled = false;
				btnRestart.Enabled = false;
				btnNewProject.Enabled = false;
				btnOpenProject.Enabled = false;
				mnNewProject.Enabled = false;
				mnOpenProject.Enabled = false;
				mnTagEdit.Enabled = false;
				break;
			}
			btnRun.Enabled = false;
			btnStop.Enabled = btnRun.Enabled;
			btnRestart.Enabled = btnRun.Enabled;
		}
	}

	private async void btnRun_Click(object sender, EventArgs e)
	{
		try
		{
			Form[] mdiChildren = base.MdiChildren;
			foreach (Form form in mdiChildren)
			{
				if (form.IsHandleCreated)
				{
					form.Close();
				}
			}
			driverController.Start();
		}
		catch (Exception ex)
		{
			await WaitFormManager.CloseAsync();
			MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private async void btnStop_Click(object sender, EventArgs e)
	{
		try
		{
			Form[] mdiChildren = base.MdiChildren;
			foreach (Form form in mdiChildren)
			{
				if (form.IsHandleCreated)
				{
					form.Close();
				}
			}
			driverController.Stop();
		}
		catch (Exception ex)
		{
			await WaitFormManager.CloseAsync();
			MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private async void btnRestart_Click(object sender, EventArgs e)
	{
		try
		{
			Form[] mdiChildren = base.MdiChildren;
			foreach (Form form in mdiChildren)
			{
				if (form.IsHandleCreated)
				{
					form.Close();
				}
			}
			driverController.Stop();
			driverController.Restart();
		}
		catch (Exception ex)
		{
			await WaitFormManager.CloseAsync();
			MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private void btnSettings_Click(object sender, EventArgs e)
	{
		try
		{
			AppHelper.Settings.Mode = false;
			bool mode = AppHelper.Settings.Mode;
			if (new FormSettings
			{
				StartPosition = FormStartPosition.CenterParent,
				ShowInTaskbar = false
			}.ShowDialog() != DialogResult.OK)
			{
				return;
			}
			UpdateIES();
			if (mode != AppHelper.Settings.Mode)
			{
				if (MessageBox.Show(this, "The application will restart to update some changes", "Information", MessageBoxButtons.OK, MessageBoxIcon.Asterisk) == DialogResult.OK)
				{
					base.MdiChildren.FirstOrDefault((Form form_0) => form_0.Name == "TagEditor")?.Close();
					base.MdiChildren.FirstOrDefault((Form form_0) => form_0.Name == "Monitoring")?.Close();
					base.MdiChildren.FirstOrDefault((Form form_0) => form_0.Name == "HistoricalData")?.Close();
					base.MdiChildren.FirstOrDefault((Form form_0) => form_0.Name == "Alarms")?.Close();
					base.MdiChildren.FirstOrDefault((Form form_0) => form_0.Name == "AsrsLink")?.Close();
					Application.Restart();
				}
			}
			else
			{
				Form form = base.MdiChildren.FirstOrDefault((Form form_0) => form_0.Name == "TagEditor");
				if (form != null)
				{
					((FormTagEditor)form).OptionsChanged();
				}
				Form form2 = base.MdiChildren.FirstOrDefault((Form form_0) => form_0.Name == "Monitoring");
				if (form2 != null)
				{
					((FormTagMonitor)form2).OptionsChanged();
				}
			}
		}
		catch (Exception ex)
		{
			MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private void SetEnableOpenProject(bool enable)
	{
		mnNewProject.Enabled = enable;
		mnOpenProject.Enabled = enable;
		btnNewProject.Enabled = enable;
		btnOpenProject.Enabled = enable;
	}

	private void OnLoadSettings()
	{
		try
		{
			AppHelper.ReadSettings();
			AppHelper.Settings.Mode = false;
			SetEnableOpenProject(!AppHelper.Settings.Mode);
			string empty = string.Empty;
			string text = (AppHelper.Settings.Mode ? "Remote" : "Local");
			UpdateIES();
			if (string.IsNullOrEmpty(AppHelper.Settings.FileName) || string.IsNullOrWhiteSpace(AppHelper.Settings.FileName))
			{
				CreateFileProjectDefault();
			}
			if (AppHelper.Settings.Mode)
			{
				Text = "Industrial Protocols";
			}
			else
			{
				empty = ParserProjectName(AppHelper.Settings.FileName);
				if (!string.IsNullOrEmpty(empty) && !string.IsNullOrWhiteSpace(empty))
				{
					Text = "Industrial Protocols - " + empty;
				}
				else
				{
					Text = "Industrial Protocols";
				}
			}
			lblMode.Text = "Mode: " + text;
			lblIPAddress.Text = "IP Address: " + AppHelper.Settings.IP;
			lblPort.Text = $"Port: {AppHelper.Settings.Port}";
			lblUserName.Text = "User: " + AppHelper.Settings.Username;
			lblStatus.Visible = AppHelper.Settings.Mode;
		}
		catch (Exception ex)
		{
			MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private void UpdateIES()
	{
		mnImport.Visible = AppHelper.Settings.Mode;
		mnExport.Visible = AppHelper.Settings.Mode;
		mnSaveAs.Visible = !AppHelper.Settings.Mode;
		mnNewProject.Visible = !AppHelper.Settings.Mode;
	}

	private string CreateFileProjectDefault()
	{
		AppHelper.Settings.Directory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
		if (!Directory.Exists(AppHelper.Settings.Directory))
		{
			Directory.CreateDirectory(AppHelper.Settings.Directory);
		}
		AppHelper.Settings.FileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "IPS.json");
		if (!File.Exists(AppHelper.Settings.FileName))
		{
			EditHelper.Editor.New("IPS");
		}
		AppHelper.WriteSettings(AppHelper.Settings);
		return AppHelper.Settings.FileName;
	}

	private string ParserProjectName(string fileName)
	{
		string result = string.Empty;
		string[] array = fileName.Split('\\');
		if (array != null && array.Length > 1)
		{
			result = array[^1];
			result = result.Substring(0, result.Length - 5);
		}
		return result;
	}

	private void FormMain_Load(object sender, EventArgs e)
	{
		try
		{
			Activate();
		}
		catch (Exception ex)
		{
			MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private async void btnNewProject_Click(object sender, EventArgs e)
	{
		try
		{
			FormNewProject formNewProject = new FormNewProject
			{
				StartPosition = FormStartPosition.CenterParent,
				ShowInTaskbar = false
			};
			if (formNewProject.ShowDialog() == DialogResult.OK)
			{
				Form[] mdiChildren = base.MdiChildren;
				for (int i = 0; i < mdiChildren.Length; i++)
				{
					mdiChildren[i].Close();
				}
				string text = (string)formNewProject.Tag;
				Text = "Industrial Protocols - " + text;
				btnTagEdit_Click(sender, e);
			}
		}
		catch (Exception ex)
		{
			MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
		finally
		{
			await WaitFormManager.CloseAsync();
		}
	}

	private async void btnOpenProject_Click(object sender, EventArgs e)
	{
		try
		{
			OpenFileDialog openFile = new OpenFileDialog
			{
				Multiselect = false,
				Title = "Select project",
				CheckFileExists = true,
				CheckPathExists = true,
				DefaultExt = "json",
				Filter = "json files (*.json)|*.json",
				FilterIndex = 1,
				RestoreDirectory = true
			};
			if (openFile.ShowDialog() != DialogResult.OK)
			{
				return;
			}
			await WaitFormManager.ShowAsync(this, "Loading...");
			if ((await EditHelper.Editor.OpenAsync(openFile.FileName)).Success)
			{
				AppHelper.Settings.FileName = openFile.FileName;
				AppHelper.WriteSettings(AppHelper.Settings);
				Text = "Industrial Protocols - " + ParserProjectName(AppHelper.Settings.FileName);
				Form[] mdiChildren = base.MdiChildren;
				for (int i = 0; i < mdiChildren.Length; i++)
				{
					mdiChildren[i].Close();
				}
				btnTagEdit_Click(sender, e);
			}
		}
		catch (Exception ex)
		{
			MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
		finally
		{
			await WaitFormManager.CloseAsync();
		}
	}

	private async void mnSaveAs_Click(object sender, EventArgs e)
	{
		try
		{
			if (EditHelper.IndusProtocol != null)
			{
				SaveFileDialog saveFileDialog = new SaveFileDialog
				{
					Title = "Save as",
					CheckPathExists = true,
					InitialDirectory = NetStudio.IPS.Properties.Settings.Default.Directory,
					DefaultExt = "json",
					Filter = "Json files (*.Json)|*.json",
					FilterIndex = 1,
					RestoreDirectory = true
				};
				if (saveFileDialog.ShowDialog() == DialogResult.OK)
				{
					ApiResponse apiResponse = await EditHelper.Editor.WriteAsync(saveFileDialog.FileName, EditHelper.IndusProtocol);
					if (!apiResponse.Success)
					{
						throw new Exception(apiResponse.Message);
					}
				}
			}
			else
			{
				MessageBox.Show(this, "Please open the editor before performing this task.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			}
		}
		catch (Exception ex)
		{
			MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private async void mnImport_Click(object sender, EventArgs e)
	{
		try
		{
			OpenFileDialog openFile = new OpenFileDialog
			{
				Multiselect = false,
				Title = "Driver Server: Import",
				CheckFileExists = true,
				CheckPathExists = true,
				DefaultExt = "json",
				Filter = "json files (*.json)|*.json",
				FilterIndex = 1,
				RestoreDirectory = true
			};
			if (openFile.ShowDialog() != DialogResult.OK)
			{
				return;
			}
			await WaitFormManager.ShowAsync(this, "Loading...");
			ApiResponse apiResponse = await EditHelper.Editor.OpenAsync(openFile.FileName);
			if (apiResponse.Success)
			{
				EditHelper.IndusProtocol = (IndustrialProtocol)apiResponse.Data;
				if (await AppHelper.WriteProjectAsync())
				{
					Text = "Industrial Protocols";
					Form[] mdiChildren = base.MdiChildren;
					for (int i = 0; i < mdiChildren.Length; i++)
					{
						mdiChildren[i].Close();
					}
					btnTagEdit_Click(sender, e);
				}
			}
			else
			{
				MessageBox.Show(this, apiResponse.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
			}
		}
		catch (Exception ex)
		{
			MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
		finally
		{
			await WaitFormManager.CloseAsync();
		}
	}

	private async void mnExport_Click(object sender, EventArgs e)
	{
		try
		{
			SaveFileDialog saveFileDialog = new SaveFileDialog
			{
				Title = "Driver Server: Export",
				CheckPathExists = true,
				InitialDirectory = NetStudio.IPS.Properties.Settings.Default.Directory,
				DefaultExt = "json",
				Filter = "Json files (*.Json)|*.json",
				FilterIndex = 1,
				RestoreDirectory = true
			};
			if (saveFileDialog.ShowDialog() == DialogResult.OK && EditHelper.IndusProtocol != null)
			{
				ApiResponse apiResponse = await EditHelper.Editor.WriteAsync(saveFileDialog.FileName, EditHelper.IndusProtocol);
				if (!apiResponse.Success)
				{
					throw new Exception(apiResponse.Message);
				}
			}
		}
		catch (Exception ex)
		{
			MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private void btnTagEdit_Click(object sender, EventArgs e)
	{
		try
		{
			Form form = base.MdiChildren.FirstOrDefault((Form form_0) => form_0.Name == "TagEditor");
			if (form != null)
			{
				((FormTagEditor)form).Activate();
				return;
			}
			FormTagEditor formTagEditor = new FormTagEditor();
			formTagEditor.Name = "TagEditor";
			formTagEditor.MdiParent = this;
			formTagEditor.WindowState = FormWindowState.Maximized;
			formTagEditor.Show();
		}
		catch (Exception ex)
		{
			MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private async void btnMonitoring_Click(object sender, EventArgs e)
	{
		try
		{
			if (AppHelper.DataChanged && EditHelper.IndusProtocol != null)
			{
				await EditHelper.Editor.WriteAsync(AppHelper.Settings.FileName, EditHelper.IndusProtocol);
			}
			Form form = base.MdiChildren.FirstOrDefault((Form form_0) => form_0.Name == "Monitoring");
			if (form != null)
			{
				FormTagMonitor formTagMonitor = (FormTagMonitor)form;
				if (!AppHelper.DataChanged)
				{
					formTagMonitor.Activate();
					return;
				}
				formTagMonitor.Close();
			}
			FormTagMonitor formTagMonitor2 = new FormTagMonitor();
			formTagMonitor2.Name = "Monitoring";
			formTagMonitor2.MdiParent = this;
			formTagMonitor2.WindowState = FormWindowState.Maximized;
			formTagMonitor2.VisibleChanged += delegate(object? sender, EventArgs e)
			{
				if (sender != null)
				{
					lblStatus.Visible = AppHelper.Settings.Mode;
				}
			};
			formTagMonitor2.Show();
		}
		catch (Exception ex)
		{
			MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private void btnAbout_Click(object sender, EventArgs e)
	{
		try
		{
			FormAbout formAbout = new FormAbout();
			formAbout.StartPosition = FormStartPosition.CenterParent;
			formAbout.ShowInTaskbar = false;
			formAbout.ShowDialog();
		}
		catch (Exception ex)
		{
			MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private void lblYoutube_Click(object sender, EventArgs e)
	{
		try
		{
			Process.Start(new ProcessStartInfo("https://www.youtube.com/NetStudio")
			{
				UseShellExecute = true
			});
		}
		catch (Exception ex)
		{
			MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private void mnQuit_Click(object sender, EventArgs e)
	{
		Close();
	}

	private async void mnLicensermation_Click(object sender, EventArgs e)
	{
		try
		{
			FormLicense formLicense = new FormLicense();
			formLicense.StartPosition = FormStartPosition.CenterScreen;
			formLicense.ShowInTaskbar = false;
			formLicense.ShowDialog();
		}
		catch (Exception ex)
		{
			await WaitFormManager.CloseAsync();
			MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private void btnSave_Click(object sender, EventArgs e)
	{
		try
		{
			if (!AppHelper.DataChanged)
			{
				return;
			}
			Form[] mdiChildren = base.MdiChildren;
			foreach (Form form in mdiChildren)
			{
				if (form is FormHistoricalData)
				{
					((FormHistoricalData)form).ValidateDataToSave();
				}
			}
			if (AppHelper.WriteProject())
			{
				MessageBox.Show(this, "Data saved successfully.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
			}
		}
		catch (Exception ex)
		{
			MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private void btnAlarms_Click(object sender, EventArgs e)
	{
		try
		{
			Form form = base.MdiChildren.FirstOrDefault((Form form_0) => form_0.Name == "Alarms");
			if (form != null)
			{
				((FormAlarms)form).Activate();
				return;
			}
			FormAlarms formAlarms = new FormAlarms();
			formAlarms.Name = "Alarms";
			formAlarms.MdiParent = this;
			formAlarms.WindowState = FormWindowState.Maximized;
			formAlarms.Show();
		}
		catch (Exception ex)
		{
			MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private void btnHistoricalData_Click(object sender, EventArgs e)
	{
		try
		{
			Form form = base.MdiChildren.FirstOrDefault((Form form_0) => form_0.Name == "HistoricalData");
			if (form != null)
			{
				((FormHistoricalData)form).Activate();
				return;
			}
			FormHistoricalData formHistoricalData = new FormHistoricalData();
			formHistoricalData.Name = "HistoricalData";
			formHistoricalData.MdiParent = this;
			formHistoricalData.WindowState = FormWindowState.Maximized;
			formHistoricalData.Show();
		}
		catch (Exception ex)
		{
			MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private void btnDataLink_Click(object sender, EventArgs e)
	{
	}

	private void btnASRSLink_Click(object sender, EventArgs e)
	{
		try
		{
			Form form = base.MdiChildren.FirstOrDefault((Form form_0) => form_0.Name == "AsrsLink");
			if (form != null)
			{
				((FormAsrsLink)form).Activate();
				return;
			}
			FormAsrsLink formAsrsLink = new FormAsrsLink();
			formAsrsLink.Name = "AsrsLink";
			formAsrsLink.MdiParent = this;
			formAsrsLink.WindowState = FormWindowState.Maximized;
			formAsrsLink.Show();
		}
		catch (Exception ex)
		{
			MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	protected override async void Dispose(bool disposing)
	{
		try
		{
			if (AppHelper.DataChanged)
			{
				await EditHelper.Editor.WriteAsync(AppHelper.Settings.FileName, EditHelper.IndusProtocol);
			}
		}
		catch (Exception)
		{
		}
		DriverController obj = driverController;
		obj.OnDriverStatusChanged = (EventDriverStatusChanged)Delegate.Remove(obj.OnDriverStatusChanged, new EventDriverStatusChanged(OnDriverStatusChanged));
		if (disposing && components != null)
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	private void InitializeComponent()
	{
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NetStudio.IPS.FormMain));
		this.toolBar = new System.Windows.Forms.ToolStrip();
		this.btnNewProject = new System.Windows.Forms.ToolStripButton();
		this.btnOpenProject = new System.Windows.Forms.ToolStripButton();
		this.btnTagEdit = new System.Windows.Forms.ToolStripButton();
		this.btnAbout = new System.Windows.Forms.ToolStripButton();
		this.btnSave = new System.Windows.Forms.ToolStripButton();
		this.btnMonitoring = new System.Windows.Forms.ToolStripButton();
		this.btnAlarms = new System.Windows.Forms.ToolStripButton();
		this.btnHistoricalData = new System.Windows.Forms.ToolStripButton();
		this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
		this.btnDataLink = new System.Windows.Forms.ToolStripButton();
		this.btnASRSLink = new System.Windows.Forms.ToolStripButton();
		this.btnSettings = new System.Windows.Forms.ToolStripButton();
		this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
		this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
		this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
		this.btnRun = new System.Windows.Forms.ToolStripButton();
		this.btnStop = new System.Windows.Forms.ToolStripButton();
		this.btnRestart = new System.Windows.Forms.ToolStripButton();
		this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
		this.btnQuit = new System.Windows.Forms.ToolStripButton();
		this.statusBar = new System.Windows.Forms.StatusStrip();
		this.lblStatus = new System.Windows.Forms.ToolStripStatusLabel();
		this.lblMode = new System.Windows.Forms.ToolStripStatusLabel();
		this.lblIPAddress = new System.Windows.Forms.ToolStripStatusLabel();
		this.lblPort = new System.Windows.Forms.ToolStripStatusLabel();
		this.lblUserName = new System.Windows.Forms.ToolStripStatusLabel();
		this.toolStripStatusLabel3 = new System.Windows.Forms.ToolStripStatusLabel();
		this.lblYoutube = new System.Windows.Forms.ToolStripStatusLabel();
		this.menu = new System.Windows.Forms.MenuStrip();
		this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.mnNewProject = new System.Windows.Forms.ToolStripMenuItem();
		this.mnOpenProject = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
		this.mnSave = new System.Windows.Forms.ToolStripMenuItem();
		this.mnSaveAs = new System.Windows.Forms.ToolStripMenuItem();
		this.mnImport = new System.Windows.Forms.ToolStripMenuItem();
		this.mnExport = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
		this.mnQuit = new System.Windows.Forms.ToolStripMenuItem();
		this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.mnTagEdit = new System.Windows.Forms.ToolStripMenuItem();
		this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
		this.mnAbout = new System.Windows.Forms.ToolStripMenuItem();
		this.mnLicensermation = new System.Windows.Forms.ToolStripMenuItem();
		this.toolBar.SuspendLayout();
		this.statusBar.SuspendLayout();
		this.menu.SuspendLayout();
		base.SuspendLayout();
		this.toolBar.BackColor = System.Drawing.SystemColors.ScrollBar;
		this.toolBar.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
		this.toolBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[20]
		{
			this.btnNewProject, this.btnOpenProject, this.btnTagEdit, this.btnAbout, this.btnSave, this.btnMonitoring, this.btnAlarms, this.btnHistoricalData, this.toolStripSeparator2, this.btnDataLink,
			this.btnASRSLink, this.btnSettings, this.toolStripSeparator6, this.toolStripLabel1, this.toolStripSeparator7, this.btnRun, this.btnStop, this.btnRestart, this.toolStripSeparator1, this.btnQuit
		});
		this.toolBar.Location = new System.Drawing.Point(0, 24);
		this.toolBar.Name = "toolBar";
		this.toolBar.Size = new System.Drawing.Size(1213, 25);
		this.toolBar.TabIndex = 0;
		this.toolBar.Text = "Tool bar";
		this.btnNewProject.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.btnNewProject.Image = (System.Drawing.Image)resources.GetObject("btnNewProject.Image");
		this.btnNewProject.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.btnNewProject.Name = "btnNewProject";
		this.btnNewProject.Size = new System.Drawing.Size(23, 22);
		this.btnNewProject.Text = "&New project";
		this.btnNewProject.Click += new System.EventHandler(btnNewProject_Click);
		this.btnOpenProject.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.btnOpenProject.Image = (System.Drawing.Image)resources.GetObject("btnOpenProject.Image");
		this.btnOpenProject.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.btnOpenProject.Name = "btnOpenProject";
		this.btnOpenProject.Size = new System.Drawing.Size(23, 22);
		this.btnOpenProject.Text = "Open project";
		this.btnOpenProject.Click += new System.EventHandler(btnOpenProject_Click);
		this.btnTagEdit.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.btnTagEdit.Image = NetStudio.IPS.Properties.Resources.Resources_512_tag_editor;
		this.btnTagEdit.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.btnTagEdit.Name = "btnTagEdit";
		this.btnTagEdit.Size = new System.Drawing.Size(23, 22);
		this.btnTagEdit.Text = "&Tag editor";
		this.btnTagEdit.Click += new System.EventHandler(btnTagEdit_Click);
		this.btnAbout.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
		this.btnAbout.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.btnAbout.Image = NetStudio.IPS.Properties.Resources.Resources_512_info;
		this.btnAbout.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.btnAbout.Name = "btnAbout";
		this.btnAbout.Size = new System.Drawing.Size(23, 22);
		this.btnAbout.Text = "About";
		this.btnAbout.Click += new System.EventHandler(btnAbout_Click);
		this.btnSave.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.btnSave.Image = NetStudio.IPS.Properties.Resources.Resources_512_save;
		this.btnSave.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.btnSave.Name = "btnSave";
		this.btnSave.Size = new System.Drawing.Size(23, 22);
		this.btnSave.Text = "Save";
		this.btnSave.Click += new System.EventHandler(btnSave_Click);
		this.btnMonitoring.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.btnMonitoring.Image = NetStudio.IPS.Properties.Resources.Resources_512_monitoring;
		this.btnMonitoring.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.btnMonitoring.Name = "btnMonitoring";
		this.btnMonitoring.Size = new System.Drawing.Size(23, 22);
		this.btnMonitoring.Text = "Monitoring";
		this.btnMonitoring.Click += new System.EventHandler(btnMonitoring_Click);
		this.btnAlarms.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.btnAlarms.Image = NetStudio.IPS.Properties.Resources.Resources_512_alarm;
		this.btnAlarms.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.btnAlarms.Name = "btnAlarms";
		this.btnAlarms.Size = new System.Drawing.Size(23, 22);
		this.btnAlarms.Text = "Alarms";
		this.btnAlarms.Click += new System.EventHandler(btnAlarms_Click);
		this.btnHistoricalData.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.btnHistoricalData.Image = NetStudio.IPS.Properties.Resources.Resources_512_historiant;
		this.btnHistoricalData.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.btnHistoricalData.Name = "btnHistoricalData";
		this.btnHistoricalData.Size = new System.Drawing.Size(23, 22);
		this.btnHistoricalData.Text = "Historical data";
		this.btnHistoricalData.Click += new System.EventHandler(btnHistoricalData_Click);
		this.toolStripSeparator2.Name = "toolStripSeparator2";
		this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
		this.btnDataLink.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.btnDataLink.Image = NetStudio.IPS.Properties.Resources.Resources_512_data_link;
		this.btnDataLink.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.btnDataLink.Name = "btnDataLink";
		this.btnDataLink.Size = new System.Drawing.Size(23, 22);
		this.btnDataLink.Text = "Data-Link";
		this.btnDataLink.Click += new System.EventHandler(btnDataLink_Click);
		this.btnASRSLink.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.btnASRSLink.Image = NetStudio.IPS.Properties.Resources.Resources_512_asrs_link;
		this.btnASRSLink.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.btnASRSLink.Name = "btnASRSLink";
		this.btnASRSLink.Size = new System.Drawing.Size(23, 22);
		this.btnASRSLink.Text = "ASRS-Link";
		this.btnASRSLink.Visible = false;
		this.btnASRSLink.Click += new System.EventHandler(btnASRSLink_Click);
		this.btnSettings.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.btnSettings.Image = NetStudio.IPS.Properties.Resources.Resources_512_settings;
		this.btnSettings.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.btnSettings.Name = "btnSettings";
		this.btnSettings.Size = new System.Drawing.Size(23, 22);
		this.btnSettings.Text = "Settings";
		this.btnSettings.ToolTipText = "Settings";
		this.btnSettings.Click += new System.EventHandler(btnSettings_Click);
		this.toolStripSeparator6.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
		this.toolStripSeparator6.Name = "toolStripSeparator6";
		this.toolStripSeparator6.Size = new System.Drawing.Size(6, 25);
		this.toolStripLabel1.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
		this.toolStripLabel1.ForeColor = System.Drawing.Color.Navy;
		this.toolStripLabel1.Name = "toolStripLabel1";
		this.toolStripLabel1.Size = new System.Drawing.Size(153, 22);
		this.toolStripLabel1.Text = "Hotline: (+84) 0909-886-483";
		this.toolStripLabel1.Click += new System.EventHandler(lblYoutube_Click);
		this.toolStripSeparator7.Name = "toolStripSeparator7";
		this.toolStripSeparator7.Size = new System.Drawing.Size(6, 25);
		this.btnRun.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.btnRun.Enabled = false;
		this.btnRun.Image = NetStudio.IPS.Properties.Resources.Resources_512_play;
		this.btnRun.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.btnRun.Name = "btnRun";
		this.btnRun.Size = new System.Drawing.Size(23, 22);
		this.btnRun.Text = "Run";
		this.btnRun.Click += new System.EventHandler(btnRun_Click);
		this.btnStop.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.btnStop.Enabled = false;
		this.btnStop.Image = NetStudio.IPS.Properties.Resources.Resources_512_stop;
		this.btnStop.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.btnStop.Name = "btnStop";
		this.btnStop.Size = new System.Drawing.Size(23, 22);
		this.btnStop.Text = "Stop";
		this.btnStop.Click += new System.EventHandler(btnStop_Click);
		this.btnRestart.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.btnRestart.Enabled = false;
		this.btnRestart.Image = NetStudio.IPS.Properties.Resources.Resources_512_restart;
		this.btnRestart.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.btnRestart.Name = "btnRestart";
		this.btnRestart.Size = new System.Drawing.Size(23, 22);
		this.btnRestart.Text = "Restart";
		this.btnRestart.Click += new System.EventHandler(btnRestart_Click);
		this.toolStripSeparator1.Name = "toolStripSeparator1";
		this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
		this.btnQuit.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.btnQuit.Image = NetStudio.IPS.Properties.Resources.Resources_512_close;
		this.btnQuit.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.btnQuit.Name = "btnQuit";
		this.btnQuit.Size = new System.Drawing.Size(23, 22);
		this.btnQuit.Text = "Quit";
		this.btnQuit.Visible = false;
		this.btnQuit.Click += new System.EventHandler(mnQuit_Click);
		this.statusBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[7] { this.lblStatus, this.lblMode, this.lblIPAddress, this.lblPort, this.lblUserName, this.toolStripStatusLabel3, this.lblYoutube });
		this.statusBar.Location = new System.Drawing.Point(0, 668);
		this.statusBar.Name = "statusBar";
		this.statusBar.Size = new System.Drawing.Size(1213, 24);
		this.statusBar.TabIndex = 1;
		this.statusBar.Text = "Status bar";
		this.lblStatus.BackColor = System.Drawing.Color.Red;
		this.lblStatus.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.All;
		this.lblStatus.BorderStyle = System.Windows.Forms.Border3DStyle.SunkenOuter;
		this.lblStatus.ForeColor = System.Drawing.Color.White;
		this.lblStatus.Name = "lblStatus";
		this.lblStatus.Size = new System.Drawing.Size(70, 19);
		this.lblStatus.Text = "Disconnect";
		this.lblMode.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.All;
		this.lblMode.BorderStyle = System.Windows.Forms.Border3DStyle.SunkenOuter;
		this.lblMode.Name = "lblMode";
		this.lblMode.Size = new System.Drawing.Size(76, 19);
		this.lblMode.Text = "Mode: Local";
		this.lblIPAddress.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.All;
		this.lblIPAddress.BorderStyle = System.Windows.Forms.Border3DStyle.SunkenOuter;
		this.lblIPAddress.Name = "lblIPAddress";
		this.lblIPAddress.Size = new System.Drawing.Size(117, 19);
		this.lblIPAddress.Text = "IP Address: 127.0.0.1";
		this.lblPort.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.All;
		this.lblPort.BorderStyle = System.Windows.Forms.Border3DStyle.SunkenOuter;
		this.lblPort.Name = "lblPort";
		this.lblPort.Size = new System.Drawing.Size(57, 19);
		this.lblPort.Text = "Port: 502";
		this.lblUserName.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.All;
		this.lblUserName.BorderStyle = System.Windows.Forms.Border3DStyle.SunkenOuter;
		this.lblUserName.Name = "lblUserName";
		this.lblUserName.Size = new System.Drawing.Size(76, 19);
		this.lblUserName.Text = "User: Admin";
		this.toolStripStatusLabel3.Name = "toolStripStatusLabel3";
		this.toolStripStatusLabel3.Size = new System.Drawing.Size(682, 19);
		this.toolStripStatusLabel3.Spring = true;
		this.toolStripStatusLabel3.Text = "     ";
		this.lblYoutube.ForeColor = System.Drawing.Color.DarkSlateGray;
		this.lblYoutube.Name = "lblYoutube";
		this.lblYoutube.Size = new System.Drawing.Size(120, 19);
		this.lblYoutube.Text = "©Industrial Networks";
		this.lblYoutube.Click += new System.EventHandler(lblYoutube_Click);
		this.menu.Items.AddRange(new System.Windows.Forms.ToolStripItem[3] { this.fileToolStripMenuItem, this.editToolStripMenuItem, this.helpToolStripMenuItem });
		this.menu.Location = new System.Drawing.Point(0, 0);
		this.menu.Name = "menu";
		this.menu.Size = new System.Drawing.Size(1213, 24);
		this.menu.TabIndex = 3;
		this.menu.Text = "menuStrip";
		this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[9] { this.mnNewProject, this.mnOpenProject, this.toolStripSeparator5, this.mnSave, this.mnSaveAs, this.mnImport, this.mnExport, this.toolStripSeparator3, this.mnQuit });
		this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
		this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
		this.fileToolStripMenuItem.Text = "&File";
		this.mnNewProject.Image = NetStudio.IPS.Properties.Resources.Resources_512_new_project;
		this.mnNewProject.Name = "mnNewProject";
		this.mnNewProject.Size = new System.Drawing.Size(143, 22);
		this.mnNewProject.Text = "&New project";
		this.mnNewProject.Click += new System.EventHandler(btnNewProject_Click);
		this.mnOpenProject.Image = NetStudio.IPS.Properties.Resources.Resources_512_open_folder;
		this.mnOpenProject.Name = "mnOpenProject";
		this.mnOpenProject.Size = new System.Drawing.Size(143, 22);
		this.mnOpenProject.Text = "&Open project";
		this.mnOpenProject.Click += new System.EventHandler(btnOpenProject_Click);
		this.toolStripSeparator5.Name = "toolStripSeparator5";
		this.toolStripSeparator5.Size = new System.Drawing.Size(140, 6);
		this.mnSave.Image = NetStudio.IPS.Properties.Resources.Resources_512_save;
		this.mnSave.Name = "mnSave";
		this.mnSave.ShortcutKeys = System.Windows.Forms.Keys.S | System.Windows.Forms.Keys.Control;
		this.mnSave.Size = new System.Drawing.Size(143, 22);
		this.mnSave.Text = "Save";
		this.mnSave.Click += new System.EventHandler(btnSave_Click);
		this.mnSaveAs.Image = NetStudio.IPS.Properties.Resources.Resources_512_save;
		this.mnSaveAs.Name = "mnSaveAs";
		this.mnSaveAs.Size = new System.Drawing.Size(143, 22);
		this.mnSaveAs.Text = "Save as";
		this.mnSaveAs.Click += new System.EventHandler(mnSaveAs_Click);
		this.mnImport.Image = NetStudio.IPS.Properties.Resources.Resources_512_import;
		this.mnImport.Name = "mnImport";
		this.mnImport.Size = new System.Drawing.Size(143, 22);
		this.mnImport.Text = "&Import";
		this.mnImport.Visible = false;
		this.mnImport.Click += new System.EventHandler(mnImport_Click);
		this.mnExport.Image = NetStudio.IPS.Properties.Resources.Resources_512_export;
		this.mnExport.Name = "mnExport";
		this.mnExport.Size = new System.Drawing.Size(143, 22);
		this.mnExport.Text = "&Export";
		this.mnExport.Visible = false;
		this.mnExport.Click += new System.EventHandler(mnExport_Click);
		this.toolStripSeparator3.Name = "toolStripSeparator3";
		this.toolStripSeparator3.Size = new System.Drawing.Size(140, 6);
		this.mnQuit.Image = NetStudio.IPS.Properties.Resources.Resources_512_close;
		this.mnQuit.Name = "mnQuit";
		this.mnQuit.Size = new System.Drawing.Size(143, 22);
		this.mnQuit.Text = "&Quit";
		this.mnQuit.Click += new System.EventHandler(mnQuit_Click);
		this.editToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[1] { this.mnTagEdit });
		this.editToolStripMenuItem.Name = "editToolStripMenuItem";
		this.editToolStripMenuItem.Size = new System.Drawing.Size(39, 20);
		this.editToolStripMenuItem.Text = "&Edit";
		this.mnTagEdit.Image = NetStudio.IPS.Properties.Resources.Resources_512_tag_editor;
		this.mnTagEdit.Name = "mnTagEdit";
		this.mnTagEdit.Size = new System.Drawing.Size(115, 22);
		this.mnTagEdit.Text = "&Tag Edit";
		this.mnTagEdit.Click += new System.EventHandler(btnTagEdit_Click);
		this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[2] { this.mnAbout, this.mnLicensermation });
		this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
		this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
		this.helpToolStripMenuItem.Text = "&Help";
		this.mnAbout.Image = NetStudio.IPS.Properties.Resources.Resources_512_info;
		this.mnAbout.Name = "mnAbout";
		this.mnAbout.Size = new System.Drawing.Size(179, 22);
		this.mnAbout.Text = "&About";
		this.mnAbout.Click += new System.EventHandler(btnAbout_Click);
		this.mnLicensermation.Image = NetStudio.IPS.Properties.Resources.Resources_512_device;
		this.mnLicensermation.Name = "mnLicensermation";
		this.mnLicensermation.Size = new System.Drawing.Size(179, 22);
		this.mnLicensermation.Text = "&License Information";
		this.mnLicensermation.Click += new System.EventHandler(mnLicensermation_Click);
		base.AutoScaleDimensions = new System.Drawing.SizeF(7f, 15f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		this.BackgroundImage = NetStudio.IPS.Properties.Resources.Resources_530_Digital_Transformation;
		this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
		base.ClientSize = new System.Drawing.Size(1213, 692);
		base.Controls.Add(this.statusBar);
		base.Controls.Add(this.toolBar);
		base.Controls.Add(this.menu);
		base.Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
		base.IsMdiContainer = true;
		base.MainMenuStrip = this.menu;
		base.Name = "FormMain";
		this.Text = "Industrial Protocols";
		base.WindowState = System.Windows.Forms.FormWindowState.Maximized;
		base.Load += new System.EventHandler(FormMain_Load);
		this.toolBar.ResumeLayout(false);
		this.toolBar.PerformLayout();
		this.statusBar.ResumeLayout(false);
		this.statusBar.PerformLayout();
		this.menu.ResumeLayout(false);
		this.menu.PerformLayout();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
