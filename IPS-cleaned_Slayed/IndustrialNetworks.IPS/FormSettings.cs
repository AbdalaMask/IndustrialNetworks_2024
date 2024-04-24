using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using NetStudio.Common;
using NetStudio.Common.Security;
using NetStudio.IPS.Properties;

namespace NetStudio.IPS;

public class FormSettings : Form
{
	private IContainer components;

	private TabControl tabControl1;

	private TabPage tabPageMode;

	private TabPage tabPageGridView;

	private GroupBox gbEthernet;

	private NumericUpDown txtPort;

	private Label lblPort;

	private TextBox txtIPAddress;

	private Label lblIPAddress;

	private CheckBox chkMode;

	private GroupBox gbOptions;

	private Button btnSave;

	private CheckBox chkShowScaling;

	private CheckBox chkShowDivideBy;

	private GroupBox groupBox1;

	private TextBox txtPassword;

	private Label label1;

	private TextBox txtUserName;

	private Label label2;

	private ErrorProvider errorProvider1;

	private CheckBox chkShowResolution;

	public FormSettings()
	{
		InitializeComponent();
	}

	private void FormSettings_Load(object sender, EventArgs e)
	{
		try
		{
			if (!errorProvider1.HasErrors)
			{
				AppHelper.Settings.Mode = false;
				txtIPAddress.DataBindings.Add("Enabled", chkMode, "Checked");
				txtIPAddress.DataBindings.Add("Text", AppHelper.Settings, "IPAddress");
				txtIPAddress.Validating += txtIPAddress_Validating;
				txtPort.DataBindings.Add("Enabled", chkMode, "Checked");
				txtPort.DataBindings.Add("Value", AppHelper.Settings, "Port");
				txtUserName.DataBindings.Add("Enabled", chkMode, "Checked");
				txtUserName.DataBindings.Add("Text", AppHelper.Settings, "UserName");
				txtUserName.Validating += txtUserName_Validating;
				txtPassword.DataBindings.Add("Enabled", chkMode, "Checked");
				txtPassword.DataBindings.Add("Text", AppHelper.Settings, "Password");
				txtPassword.Validating += txtPassword_Validating;
				chkMode.DataBindings.Add("Checked", AppHelper.Settings, "Mode", formattingEnabled: true, DataSourceUpdateMode.OnPropertyChanged);
				chkShowScaling.DataBindings.Add("Checked", AppHelper.Settings, "Scaling", formattingEnabled: true, DataSourceUpdateMode.OnPropertyChanged);
				chkShowDivideBy.DataBindings.Add("Checked", AppHelper.Settings, "Offset", formattingEnabled: true, DataSourceUpdateMode.OnPropertyChanged);
				chkShowResolution.DataBindings.Add("Checked", AppHelper.Settings, "Resolution", formattingEnabled: true, DataSourceUpdateMode.OnPropertyChanged);
				base.ActiveControl = btnSave;
			}
		}
		catch (Exception ex)
		{
			MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private void btnSave_Click(object sender, EventArgs e)
	{
		try
		{
			if (AppHelper.Settings.Mode && (AppHelper.Settings.IP == "127.0.0.1" || AppHelper.Settings.IP == "localhost") && new DriverController(this).DriverService == null)
			{
				MessageBox.Show(this, "You cannot install remote mode because your computer does not have the driver installed.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
				AppHelper.Settings.Mode = false;
			}
			else if (ValidateInputs())
			{
				AppHelper.Settings.Mode = false;
				Settings.Default.IP = AppHelper.Settings.IP;
				Settings.Default.Port = AppHelper.Settings.Port;
				Settings.Default.UserName = AppHelper.Settings.Username;
				Settings.Default.Password = RSACryptoUtility.EncryptString("u14ca0598a4e4133bbat2ea2165a1812", AppHelper.Settings.Password);
				Settings.Default.Mode = AppHelper.Settings.Mode;
				Settings.Default.ScalingColumn = AppHelper.Settings.Scaling;
				Settings.Default.OffsetColumn = AppHelper.Settings.Offset;
				Settings.Default.ResolutionColumn = AppHelper.Settings.Resolution;
				Settings.Default.Save();
				base.DialogResult = DialogResult.OK;
			}
		}
		catch (Exception ex)
		{
			MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private void txtIPAddress_Validating(object? sender, CancelEventArgs e)
	{
		if (!chkMode.Checked)
		{
			return;
		}
		TextBox textBox = (TextBox)sender;
		if (!string.IsNullOrEmpty(textBox.Text) && !string.IsNullOrWhiteSpace(textBox.Text))
		{
			if (Utility.IsIpV4AddressValid(textBox.Text))
			{
				e.Cancel = false;
				errorProvider1.SetError(textBox, null);
				errorProvider1.Clear();
			}
			else
			{
				errorProvider1.SetIconAlignment(textBox, ErrorIconAlignment.MiddleLeft);
				errorProvider1.SetIconPadding(textBox, 4);
				errorProvider1.SetError(textBox, "IP Address: Invalid.");
			}
		}
		else
		{
			e.Cancel = true;
			textBox.Focus();
			errorProvider1.SetIconAlignment(textBox, ErrorIconAlignment.MiddleLeft);
			errorProvider1.SetIconPadding(textBox, 4);
			errorProvider1.SetError(textBox, "Please enter your IP address");
		}
	}

	private void txtUserName_Validating(object? sender, CancelEventArgs e)
	{
		if (chkMode.Checked)
		{
			TextBox textBox = (TextBox)sender;
			if (!string.IsNullOrEmpty(textBox.Text) && !string.IsNullOrWhiteSpace(textBox.Text))
			{
				e.Cancel = false;
				errorProvider1.SetError(textBox, null);
				errorProvider1.Clear();
			}
			else
			{
				e.Cancel = true;
				textBox.Focus();
				errorProvider1.SetIconAlignment(textBox, ErrorIconAlignment.MiddleLeft);
				errorProvider1.SetIconPadding(textBox, 4);
				errorProvider1.SetError(textBox, "Please enter your user name");
			}
		}
	}

	private void txtPassword_Validating(object? sender, CancelEventArgs e)
	{
		if (chkMode.Checked)
		{
			TextBox textBox = (TextBox)sender;
			if (!string.IsNullOrEmpty(textBox.Text) && !string.IsNullOrWhiteSpace(textBox.Text))
			{
				e.Cancel = false;
				errorProvider1.SetError(textBox, null);
				errorProvider1.Clear();
			}
			else
			{
				e.Cancel = true;
				textBox.Focus();
				errorProvider1.SetIconAlignment(textBox, ErrorIconAlignment.MiddleLeft);
				errorProvider1.SetIconPadding(textBox, 4);
				errorProvider1.SetError(textBox, "Please enter your password");
			}
		}
	}

	private bool ValidateInputs()
	{
		if (!chkMode.Checked)
		{
			return true;
		}
		if (string.IsNullOrEmpty(txtIPAddress.Text) || string.IsNullOrWhiteSpace(txtIPAddress.Text))
		{
			errorProvider1.SetIconAlignment(txtIPAddress, ErrorIconAlignment.MiddleLeft);
			errorProvider1.SetIconPadding(txtIPAddress, 4);
			errorProvider1.SetError(txtIPAddress, "Please enter your IP Address");
		}
		if (!Utility.IsIpV4AddressValid(txtIPAddress.Text))
		{
			errorProvider1.SetIconAlignment(txtIPAddress, ErrorIconAlignment.MiddleLeft);
			errorProvider1.SetIconPadding(txtIPAddress, 4);
			errorProvider1.SetError(txtIPAddress, "IP Address: Invalid.");
		}
		if (string.IsNullOrEmpty(txtUserName.Text) || string.IsNullOrWhiteSpace(txtUserName.Text))
		{
			errorProvider1.SetIconAlignment(txtUserName, ErrorIconAlignment.MiddleLeft);
			errorProvider1.SetIconPadding(txtUserName, 4);
			errorProvider1.SetError(txtUserName, "Please enter your user name");
		}
		if (string.IsNullOrEmpty(txtPassword.Text) || string.IsNullOrWhiteSpace(txtPassword.Text))
		{
			errorProvider1.SetIconAlignment(txtPassword, ErrorIconAlignment.MiddleLeft);
			errorProvider1.SetIconPadding(txtPassword, 4);
			errorProvider1.SetError(txtPassword, "Please enter your password");
		}
		return !errorProvider1.HasErrors;
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
		this.components = new System.ComponentModel.Container();
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NetStudio.IPS.FormSettings));
		this.tabControl1 = new System.Windows.Forms.TabControl();
		this.tabPageMode = new System.Windows.Forms.TabPage();
		this.groupBox1 = new System.Windows.Forms.GroupBox();
		this.txtPassword = new System.Windows.Forms.TextBox();
		this.label1 = new System.Windows.Forms.Label();
		this.txtUserName = new System.Windows.Forms.TextBox();
		this.label2 = new System.Windows.Forms.Label();
		this.gbEthernet = new System.Windows.Forms.GroupBox();
		this.txtPort = new System.Windows.Forms.NumericUpDown();
		this.lblPort = new System.Windows.Forms.Label();
		this.txtIPAddress = new System.Windows.Forms.TextBox();
		this.lblIPAddress = new System.Windows.Forms.Label();
		this.tabPageGridView = new System.Windows.Forms.TabPage();
		this.gbOptions = new System.Windows.Forms.GroupBox();
		this.chkShowResolution = new System.Windows.Forms.CheckBox();
		this.chkShowDivideBy = new System.Windows.Forms.CheckBox();
		this.chkShowScaling = new System.Windows.Forms.CheckBox();
		this.chkMode = new System.Windows.Forms.CheckBox();
		this.btnSave = new System.Windows.Forms.Button();
		this.errorProvider1 = new System.Windows.Forms.ErrorProvider(this.components);
		this.tabControl1.SuspendLayout();
		this.tabPageMode.SuspendLayout();
		this.groupBox1.SuspendLayout();
		this.gbEthernet.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.txtPort).BeginInit();
		this.tabPageGridView.SuspendLayout();
		this.gbOptions.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.errorProvider1).BeginInit();
		base.SuspendLayout();
		this.tabControl1.Controls.Add(this.tabPageMode);
		this.tabControl1.Controls.Add(this.tabPageGridView);
		this.tabControl1.Dock = System.Windows.Forms.DockStyle.Top;
		this.tabControl1.Location = new System.Drawing.Point(0, 0);
		this.tabControl1.Name = "tabControl1";
		this.tabControl1.SelectedIndex = 0;
		this.tabControl1.Size = new System.Drawing.Size(380, 228);
		this.tabControl1.TabIndex = 0;
		this.tabPageMode.Controls.Add(this.groupBox1);
		this.tabPageMode.Controls.Add(this.gbEthernet);
		this.tabPageMode.Location = new System.Drawing.Point(4, 24);
		this.tabPageMode.Name = "tabPageMode";
		this.tabPageMode.Padding = new System.Windows.Forms.Padding(3);
		this.tabPageMode.Size = new System.Drawing.Size(372, 200);
		this.tabPageMode.TabIndex = 0;
		this.tabPageMode.Text = "Mode";
		this.tabPageMode.UseVisualStyleBackColor = true;
		this.groupBox1.Controls.Add(this.txtPassword);
		this.groupBox1.Controls.Add(this.label1);
		this.groupBox1.Controls.Add(this.txtUserName);
		this.groupBox1.Controls.Add(this.label2);
		this.groupBox1.ForeColor = System.Drawing.Color.Navy;
		this.groupBox1.Location = new System.Drawing.Point(9, 102);
		this.groupBox1.Name = "groupBox1";
		this.groupBox1.Size = new System.Drawing.Size(355, 90);
		this.groupBox1.TabIndex = 86;
		this.groupBox1.TabStop = false;
		this.groupBox1.Text = "Authentication";
		this.txtPassword.Location = new System.Drawing.Point(96, 52);
		this.txtPassword.Name = "txtPassword";
		this.txtPassword.PasswordChar = '*';
		this.txtPassword.Size = new System.Drawing.Size(253, 23);
		this.txtPassword.TabIndex = 4;
		this.txtPassword.UseSystemPasswordChar = true;
		this.label1.AutoSize = true;
		this.label1.ForeColor = System.Drawing.SystemColors.ControlText;
		this.label1.Location = new System.Drawing.Point(11, 56);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(60, 15);
		this.label1.TabIndex = 16;
		this.label1.Text = "Password:";
		this.txtUserName.Location = new System.Drawing.Point(96, 20);
		this.txtUserName.Name = "txtUserName";
		this.txtUserName.Size = new System.Drawing.Size(253, 23);
		this.txtUserName.TabIndex = 3;
		this.label2.AutoSize = true;
		this.label2.ForeColor = System.Drawing.SystemColors.ControlText;
		this.label2.Location = new System.Drawing.Point(5, 24);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(66, 15);
		this.label2.TabIndex = 14;
		this.label2.Text = "User name:";
		this.gbEthernet.Controls.Add(this.txtPort);
		this.gbEthernet.Controls.Add(this.lblPort);
		this.gbEthernet.Controls.Add(this.txtIPAddress);
		this.gbEthernet.Controls.Add(this.lblIPAddress);
		this.gbEthernet.ForeColor = System.Drawing.Color.Navy;
		this.gbEthernet.Location = new System.Drawing.Point(9, 6);
		this.gbEthernet.Name = "gbEthernet";
		this.gbEthernet.Size = new System.Drawing.Size(355, 90);
		this.gbEthernet.TabIndex = 0;
		this.gbEthernet.TabStop = false;
		this.gbEthernet.Text = "Driver Server";
		this.txtPort.Location = new System.Drawing.Point(96, 55);
		this.txtPort.Maximum = new decimal(new int[4] { 100000, 0, 0, 0 });
		this.txtPort.Name = "txtPort";
		this.txtPort.Size = new System.Drawing.Size(253, 23);
		this.txtPort.TabIndex = 2;
		this.txtPort.Value = new decimal(new int[4] { 102, 0, 0, 0 });
		this.lblPort.AutoSize = true;
		this.lblPort.ForeColor = System.Drawing.SystemColors.ControlText;
		this.lblPort.Location = new System.Drawing.Point(38, 57);
		this.lblPort.Name = "lblPort";
		this.lblPort.Size = new System.Drawing.Size(32, 15);
		this.lblPort.TabIndex = 16;
		this.lblPort.Text = "Port:";
		this.txtIPAddress.Location = new System.Drawing.Point(96, 22);
		this.txtIPAddress.Name = "txtIPAddress";
		this.txtIPAddress.Size = new System.Drawing.Size(253, 23);
		this.txtIPAddress.TabIndex = 1;
		this.txtIPAddress.Text = "127.0.0.1";
		this.lblIPAddress.AutoSize = true;
		this.lblIPAddress.ForeColor = System.Drawing.SystemColors.ControlText;
		this.lblIPAddress.Location = new System.Drawing.Point(5, 24);
		this.lblIPAddress.Name = "lblIPAddress";
		this.lblIPAddress.Size = new System.Drawing.Size(65, 15);
		this.lblIPAddress.TabIndex = 14;
		this.lblIPAddress.Text = "IP Address:";
		this.tabPageGridView.Controls.Add(this.gbOptions);
		this.tabPageGridView.Location = new System.Drawing.Point(4, 24);
		this.tabPageGridView.Name = "tabPageGridView";
		this.tabPageGridView.Padding = new System.Windows.Forms.Padding(3);
		this.tabPageGridView.Size = new System.Drawing.Size(372, 200);
		this.tabPageGridView.TabIndex = 1;
		this.tabPageGridView.Text = "Grid view";
		this.tabPageGridView.UseVisualStyleBackColor = true;
		this.gbOptions.Controls.Add(this.chkShowResolution);
		this.gbOptions.Controls.Add(this.chkShowDivideBy);
		this.gbOptions.Controls.Add(this.chkShowScaling);
		this.gbOptions.ForeColor = System.Drawing.Color.Navy;
		this.gbOptions.Location = new System.Drawing.Point(10, 6);
		this.gbOptions.Name = "gbOptions";
		this.gbOptions.Size = new System.Drawing.Size(354, 180);
		this.gbOptions.TabIndex = 87;
		this.gbOptions.TabStop = false;
		this.gbOptions.Text = "Options";
		this.chkShowResolution.AutoSize = true;
		this.chkShowResolution.ForeColor = System.Drawing.SystemColors.ControlText;
		this.chkShowResolution.Location = new System.Drawing.Point(10, 100);
		this.chkShowResolution.Name = "chkShowResolution";
		this.chkShowResolution.Size = new System.Drawing.Size(155, 19);
		this.chkShowResolution.TabIndex = 2;
		this.chkShowResolution.Text = "Show resolution column";
		this.chkShowResolution.UseVisualStyleBackColor = true;
		this.chkShowDivideBy.AutoSize = true;
		this.chkShowDivideBy.ForeColor = System.Drawing.SystemColors.ControlText;
		this.chkShowDivideBy.Location = new System.Drawing.Point(10, 61);
		this.chkShowDivideBy.Name = "chkShowDivideBy";
		this.chkShowDivideBy.Size = new System.Drawing.Size(150, 19);
		this.chkShowDivideBy.TabIndex = 1;
		this.chkShowDivideBy.Text = "Show divide by column";
		this.chkShowDivideBy.UseVisualStyleBackColor = true;
		this.chkShowScaling.AutoSize = true;
		this.chkShowScaling.ForeColor = System.Drawing.SystemColors.ControlText;
		this.chkShowScaling.Location = new System.Drawing.Point(10, 25);
		this.chkShowScaling.Name = "chkShowScaling";
		this.chkShowScaling.Size = new System.Drawing.Size(144, 19);
		this.chkShowScaling.TabIndex = 0;
		this.chkShowScaling.Text = "Show scaling columns";
		this.chkShowScaling.UseVisualStyleBackColor = true;
		this.chkMode.AutoSize = true;
		this.chkMode.ForeColor = System.Drawing.SystemColors.ControlText;
		this.chkMode.Location = new System.Drawing.Point(14, 239);
		this.chkMode.Name = "chkMode";
		this.chkMode.Size = new System.Drawing.Size(104, 19);
		this.chkMode.TabIndex = 5;
		this.chkMode.Text = "Mode: Remote";
		this.chkMode.UseVisualStyleBackColor = true;
		this.chkMode.Visible = false;
		this.btnSave.Location = new System.Drawing.Point(292, 233);
		this.btnSave.Name = "btnSave";
		this.btnSave.Size = new System.Drawing.Size(76, 30);
		this.btnSave.TabIndex = 6;
		this.btnSave.Text = "Save";
		this.btnSave.UseVisualStyleBackColor = true;
		this.btnSave.Click += new System.EventHandler(btnSave_Click);
		this.errorProvider1.ContainerControl = this;
		base.AutoScaleDimensions = new System.Drawing.SizeF(7f, 15f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(380, 275);
		base.Controls.Add(this.chkMode);
		base.Controls.Add(this.btnSave);
		base.Controls.Add(this.tabControl1);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
		base.Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.Name = "FormSettings";
		this.Text = "Settings";
		base.Load += new System.EventHandler(FormSettings_Load);
		this.tabControl1.ResumeLayout(false);
		this.tabPageMode.ResumeLayout(false);
		this.groupBox1.ResumeLayout(false);
		this.groupBox1.PerformLayout();
		this.gbEthernet.ResumeLayout(false);
		this.gbEthernet.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.txtPort).EndInit();
		this.tabPageGridView.ResumeLayout(false);
		this.gbOptions.ResumeLayout(false);
		this.gbOptions.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.errorProvider1).EndInit();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
