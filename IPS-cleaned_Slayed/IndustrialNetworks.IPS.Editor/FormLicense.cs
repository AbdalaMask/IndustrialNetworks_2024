using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using NetStudio.Common.Files;
using NetStudio.Common.Security;
using NetStudio.DriverComm;
using NetStudio.DriverComm.Models;
using NetStudio.IPS.Controls;
using NetStudio.IPS.Local;

namespace NetStudio.IPS.Editor;

public class FormLicense : Form
{
	private NetStudio.Common.Security.License _License;

	private JsonObjectManager jsonObject;

	private LocalAppSettingsManager localAppSettingsManager;

	private IContainer components;

	private TextBox txtDrive;

	private Button btnImport;

	private Button btnExport;

	private Button btnSaveAsLicense;

	private GroupBox gbDrive;

	private GroupBox gbCode;

	private RichTextBox txtCode;

	private Label label1;

	public FormLicense()
	{
		InitializeComponent();
	}

	private async void FormLicense_Load(object sender, EventArgs e)
	{
		try
		{
			await WaitFormManager.ShowAsync(this, "Loading...");
			jsonObject = new JsonObjectManager();
			ApiResponse license;
			if (AppHelper.Settings.Mode)
			{
				license = ClientHelper.AppSettings.GetLicense();
			}
			else
			{
				localAppSettingsManager = new LocalAppSettingsManager();
				license = localAppSettingsManager.GetLicense();
			}
			if (license.Success && license.Data != null)
			{
				_License = (NetStudio.Common.Security.License)license.Data;
				if (_License != null && _License != null)
				{
					txtDrive.DataBindings.Add("Text", _License, "SerialNumber", formattingEnabled: true, DataSourceUpdateMode.OnPropertyChanged);
					txtCode.DataBindings.Add("Text", _License, "Code", formattingEnabled: true, DataSourceUpdateMode.OnPropertyChanged);
				}
				await WaitFormManager.CloseAsync();
				return;
			}
			throw new Exception(license.Message);
		}
		catch (Exception ex)
		{
			await WaitFormManager.CloseAsync();
			MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private void btnImport_Click(object sender, EventArgs e)
	{
		try
		{
			OpenFileDialog openFileDialog = new OpenFileDialog
			{
				Multiselect = false,
				Title = "Select",
				CheckFileExists = true,
				CheckPathExists = true,
				DefaultExt = "json",
				Filter = "Json files (*.json)|*.json",
				FilterIndex = 1,
				RestoreDirectory = true
			};
			if (openFileDialog.ShowDialog() == DialogResult.OK)
			{
				NetStudio.Common.Security.License license = jsonObject.Read<NetStudio.Common.Security.License>(openFileDialog.FileName);
				if (license != null)
				{
					txtDrive.Text = license.SerialNumber;
					txtCode.Text = license.Code;
				}
			}
		}
		catch (Exception ex)
		{
			MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private void btnExport_Click(object sender, EventArgs e)
	{
		try
		{
			SaveFileDialog saveFileDialog = new SaveFileDialog
			{
				Title = "Export...",
				CheckPathExists = true,
				DefaultExt = "json",
				Filter = "Json files (*.json)|*.json",
				FilterIndex = 1,
				RestoreDirectory = true,
				FileName = "IPS-License"
			};
			if (saveFileDialog.ShowDialog() == DialogResult.OK)
			{
				jsonObject.Write(saveFileDialog.FileName, _License);
			}
		}
		catch (Exception ex)
		{
			MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private async void btnSaveAsLicense_Click(object sender, EventArgs e)
	{
		try
		{
			try
			{
				if (_License == null)
				{
					goto end_IL_0084;
				}
				await WaitFormManager.ShowAsync(this, "Saving...");
				base.Opacity = 0.0;
				ApiResponse apiResponse = null;
				if (AppHelper.Settings.Mode)
				{
					apiResponse = ClientHelper.AppSettings.SetLicense(_License);
				}
				else if (localAppSettingsManager != null)
				{
					apiResponse = localAppSettingsManager.SetLicense(_License);
				}
				if (apiResponse == null)
				{
					MessageBox.Show(this, "License setup failed!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
					return;
				}
				if (!apiResponse.Success)
				{
					throw new Exception(apiResponse.Message);
				}
				await WaitFormManager.CloseAsync();
				MessageBox.Show(this, "License saved successfully!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
				base.DialogResult = DialogResult.OK;
				goto end_IL_0051;
				end_IL_0084:;
			}
			catch (Exception ex)
			{
				base.Opacity = 1.0;
				await WaitFormManager.CloseAsync();
				MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
				goto end_IL_0051;
			}
			end_IL_0051:;
		}
		finally
		{
			await WaitFormManager.CloseAsync();
		}
	}

	protected override bool ProcessDialogKey(Keys keyData)
	{
		if (Control.ModifierKeys == Keys.None && keyData == Keys.Escape)
		{
			Close();
			return true;
		}
		return base.ProcessDialogKey(keyData);
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	private void InitializeComponent()
	{
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NetStudio.IPS.Editor.FormLicense));
		this.txtDrive = new System.Windows.Forms.TextBox();
		this.btnImport = new System.Windows.Forms.Button();
		this.btnExport = new System.Windows.Forms.Button();
		this.btnSaveAsLicense = new System.Windows.Forms.Button();
		this.gbDrive = new System.Windows.Forms.GroupBox();
		this.gbCode = new System.Windows.Forms.GroupBox();
		this.txtCode = new System.Windows.Forms.RichTextBox();
		this.label1 = new System.Windows.Forms.Label();
		this.gbDrive.SuspendLayout();
		this.gbCode.SuspendLayout();
		base.SuspendLayout();
		this.txtDrive.Font = new System.Drawing.Font("Segoe UI Semibold", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
		this.txtDrive.ForeColor = System.Drawing.Color.MidnightBlue;
		this.txtDrive.Location = new System.Drawing.Point(6, 22);
		this.txtDrive.Name = "txtDrive";
		this.txtDrive.Size = new System.Drawing.Size(438, 23);
		this.txtDrive.TabIndex = 5;
		this.btnImport.Location = new System.Drawing.Point(202, 226);
		this.btnImport.Name = "btnImport";
		this.btnImport.Size = new System.Drawing.Size(70, 28);
		this.btnImport.TabIndex = 2;
		this.btnImport.Text = "Import";
		this.btnImport.UseVisualStyleBackColor = true;
		this.btnImport.Click += new System.EventHandler(btnImport_Click);
		this.btnExport.Location = new System.Drawing.Point(278, 226);
		this.btnExport.Name = "btnExport";
		this.btnExport.Size = new System.Drawing.Size(70, 28);
		this.btnExport.TabIndex = 3;
		this.btnExport.Text = "Export";
		this.btnExport.UseVisualStyleBackColor = true;
		this.btnExport.Click += new System.EventHandler(btnExport_Click);
		this.btnSaveAsLicense.Location = new System.Drawing.Point(354, 226);
		this.btnSaveAsLicense.Name = "btnSaveAsLicense";
		this.btnSaveAsLicense.Size = new System.Drawing.Size(102, 28);
		this.btnSaveAsLicense.TabIndex = 1;
		this.btnSaveAsLicense.Text = "Save as license";
		this.btnSaveAsLicense.UseVisualStyleBackColor = true;
		this.btnSaveAsLicense.Click += new System.EventHandler(btnSaveAsLicense_Click);
		this.gbDrive.Controls.Add(this.txtDrive);
		this.gbDrive.Location = new System.Drawing.Point(5, 3);
		this.gbDrive.Name = "gbDrive";
		this.gbDrive.Size = new System.Drawing.Size(451, 60);
		this.gbDrive.TabIndex = 5;
		this.gbDrive.TabStop = false;
		this.gbDrive.Text = "SerialNumber";
		this.gbDrive.Visible = false;
		this.gbCode.Controls.Add(this.txtCode);
		this.gbCode.ForeColor = System.Drawing.Color.Navy;
		this.gbCode.Location = new System.Drawing.Point(5, 3);
		this.gbCode.Name = "gbCode";
		this.gbCode.Size = new System.Drawing.Size(451, 217);
		this.gbCode.TabIndex = 7;
		this.gbCode.TabStop = false;
		this.gbCode.Text = "Serial Key";
		this.txtCode.Location = new System.Drawing.Point(6, 22);
		this.txtCode.Name = "txtCode";
		this.txtCode.Size = new System.Drawing.Size(438, 185);
		this.txtCode.TabIndex = 0;
		this.txtCode.Text = "";
		this.label1.Font = new System.Drawing.Font("Segoe UI Semibold", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
		this.label1.ForeColor = System.Drawing.Color.MidnightBlue;
		this.label1.Location = new System.Drawing.Point(5, 230);
		this.label1.Margin = new System.Windows.Forms.Padding(7, 0, 4, 0);
		this.label1.MaximumSize = new System.Drawing.Size(0, 20);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(169, 20);
		this.label1.TabIndex = 28;
		this.label1.Text = "MAKE IN VIETNAM";
		this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		base.AutoScaleDimensions = new System.Drawing.SizeF(7f, 15f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(463, 264);
		base.Controls.Add(this.label1);
		base.Controls.Add(this.gbCode);
		base.Controls.Add(this.gbDrive);
		base.Controls.Add(this.btnSaveAsLicense);
		base.Controls.Add(this.btnExport);
		base.Controls.Add(this.btnImport);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
		base.Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.Name = "FormLicense";
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
		this.Text = "License Information";
		base.TopMost = true;
		base.Load += new System.EventHandler(FormLicense_Load);
		this.gbDrive.ResumeLayout(false);
		this.gbDrive.PerformLayout();
		this.gbCode.ResumeLayout(false);
		base.ResumeLayout(false);
	}
}
