using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using NetStudio.Common.AsrsLink;
using NetStudio.Database.SqlServer;
using NetStudio.DriverComm;
using NetStudio.DriverComm.Models;
using NetStudio.IPS.Controls;
using NetStudio.IPS.Local;

namespace NetStudio.IPS.Editor.AsrsLink;

public class FormAsrsServer : Form
{
	private AsrsServer? _Current;

	private IContainer components;

	private GroupBox groupBox1;

	private TextBox txtPassword;

	private Label label3;

	private TextBox txtLogin;

	private Label label2;

	private TextBox txtServerName;

	private Label label1;

	private Button btnSave;

	private TextBox txtDatabase;

	private Label label4;

	private ErrorProvider errorProvider1;

	private Button btnTestConnection;

	private CheckBox chkActive;

	private CheckBox chkSynchronized;

	public FormAsrsServer(AsrsServer asrsConnector)
	{
		InitializeComponent();
		_Current = asrsConnector;
	}

	private void FormSqlServer_Load(object sender, EventArgs e)
	{
		try
		{
			txtServerName.DataBindings.Add("Text", _Current, "ServerName", formattingEnabled: true, DataSourceUpdateMode.OnPropertyChanged);
			txtDatabase.DataBindings.Add("Text", _Current, "DatabaseName", formattingEnabled: true, DataSourceUpdateMode.OnPropertyChanged);
			txtLogin.DataBindings.Add("Text", _Current, "Login", formattingEnabled: true, DataSourceUpdateMode.OnPropertyChanged);
			txtPassword.DataBindings.Add("Text", _Current, "Password", formattingEnabled: true, DataSourceUpdateMode.OnPropertyChanged);
			chkActive.DataBindings.Add("Checked", _Current, "Active", formattingEnabled: true, DataSourceUpdateMode.OnPropertyChanged);
			chkSynchronized.DataBindings.Add("Checked", _Current, "Synchronized", formattingEnabled: true, DataSourceUpdateMode.OnPropertyChanged);
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
			errorProvider1.SetError(txtServerName, null);
			errorProvider1.SetError(txtDatabase, null);
			errorProvider1.SetError(txtLogin, null);
			errorProvider1.SetError(txtPassword, null);
			errorProvider1.Clear();
			if (string.IsNullOrEmpty(txtServerName.Text) || string.IsNullOrWhiteSpace(txtServerName.Text))
			{
				base.ActiveControl = txtServerName;
				errorProvider1.SetIconAlignment(txtServerName, ErrorIconAlignment.MiddleLeft);
				errorProvider1.SetIconPadding(txtServerName, 2);
				errorProvider1.SetError(txtServerName, "Please enter the server name");
			}
			if (string.IsNullOrEmpty(txtDatabase.Text) || string.IsNullOrWhiteSpace(txtDatabase.Text))
			{
				base.ActiveControl = txtDatabase;
				errorProvider1.SetIconAlignment(txtDatabase, ErrorIconAlignment.MiddleLeft);
				errorProvider1.SetIconPadding(txtDatabase, 2);
				errorProvider1.SetError(txtDatabase, "Please enter the database name");
			}
			if (string.IsNullOrEmpty(txtLogin.Text) || string.IsNullOrWhiteSpace(txtLogin.Text))
			{
				base.ActiveControl = txtLogin;
				errorProvider1.SetIconAlignment(txtLogin, ErrorIconAlignment.MiddleLeft);
				errorProvider1.SetIconPadding(txtLogin, 2);
				errorProvider1.SetError(txtLogin, "Please enter the user name");
			}
			if (string.IsNullOrEmpty(txtPassword.Text) || string.IsNullOrWhiteSpace(txtPassword.Text))
			{
				base.ActiveControl = txtPassword;
				errorProvider1.SetIconAlignment(txtPassword, ErrorIconAlignment.MiddleLeft);
				errorProvider1.SetIconPadding(txtPassword, 2);
				errorProvider1.SetError(txtPassword, "Please enter the password");
			}
			if (!errorProvider1.HasErrors)
			{
				base.Tag = _Current;
				base.DialogResult = DialogResult.OK;
			}
		}
		catch (Exception ex)
		{
			MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private async void btnTestConnection_Click(object sender, EventArgs e)
	{
		try
		{
			if (_Current == null)
			{
				return;
			}
			await WaitFormManager.ShowAsync(this, "Checking...");
			base.Opacity = 0.0;
			string connectionString = string.Format(SqlServerBase.FormatConnectionString, _Current.ServerName, _Current.DatabaseName, _Current.Login, _Current.Password);
			if (AppHelper.Settings.Mode)
			{
				ApiResponse response = await ClientHelper.Editor.TestConnectionAsync(connectionString);
				if (!response.Success)
				{
					throw new Exception(response.Message);
				}
				await WaitFormManager.CloseAsync();
				MessageBox.Show(this, response.Message, "Information", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
			}
			else
			{
				ApiResponse response = await EditHelper.Editor.TestConnectionAsync(connectionString);
				if (!response.Success)
				{
					throw new Exception(response.Message);
				}
				await WaitFormManager.CloseAsync();
				MessageBox.Show(this, response.Message, "Information", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
			}
		}
		catch (Exception ex)
		{
			await WaitFormManager.CloseAsync();
			MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
		finally
		{
			base.Opacity = 100.0;
			await WaitFormManager.CloseAsync();
		}
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NetStudio.IPS.Editor.AsrsLink.FormAsrsServer));
		this.groupBox1 = new System.Windows.Forms.GroupBox();
		this.chkActive = new System.Windows.Forms.CheckBox();
		this.txtDatabase = new System.Windows.Forms.TextBox();
		this.label4 = new System.Windows.Forms.Label();
		this.txtPassword = new System.Windows.Forms.TextBox();
		this.label3 = new System.Windows.Forms.Label();
		this.txtLogin = new System.Windows.Forms.TextBox();
		this.label2 = new System.Windows.Forms.Label();
		this.txtServerName = new System.Windows.Forms.TextBox();
		this.label1 = new System.Windows.Forms.Label();
		this.btnSave = new System.Windows.Forms.Button();
		this.errorProvider1 = new System.Windows.Forms.ErrorProvider(this.components);
		this.btnTestConnection = new System.Windows.Forms.Button();
		this.chkSynchronized = new System.Windows.Forms.CheckBox();
		this.groupBox1.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.errorProvider1).BeginInit();
		base.SuspendLayout();
		this.groupBox1.Controls.Add(this.chkSynchronized);
		this.groupBox1.Controls.Add(this.chkActive);
		this.groupBox1.Controls.Add(this.txtDatabase);
		this.groupBox1.Controls.Add(this.label4);
		this.groupBox1.Controls.Add(this.txtPassword);
		this.groupBox1.Controls.Add(this.label3);
		this.groupBox1.Controls.Add(this.txtLogin);
		this.groupBox1.Controls.Add(this.label2);
		this.groupBox1.Controls.Add(this.txtServerName);
		this.groupBox1.Controls.Add(this.label1);
		this.groupBox1.Location = new System.Drawing.Point(9, 3);
		this.groupBox1.Name = "groupBox1";
		this.groupBox1.Size = new System.Drawing.Size(342, 187);
		this.groupBox1.TabIndex = 0;
		this.groupBox1.TabStop = false;
		this.chkActive.AutoSize = true;
		this.chkActive.Location = new System.Drawing.Point(122, 160);
		this.chkActive.Name = "chkActive";
		this.chkActive.Size = new System.Drawing.Size(59, 19);
		this.chkActive.TabIndex = 5;
		this.chkActive.Text = "Active";
		this.chkActive.UseVisualStyleBackColor = true;
		this.txtDatabase.Location = new System.Drawing.Point(122, 53);
		this.txtDatabase.Name = "txtDatabase";
		this.txtDatabase.Size = new System.Drawing.Size(208, 23);
		this.txtDatabase.TabIndex = 2;
		this.label4.AutoSize = true;
		this.label4.Location = new System.Drawing.Point(12, 57);
		this.label4.Name = "label4";
		this.label4.Size = new System.Drawing.Size(61, 15);
		this.label4.TabIndex = 4;
		this.label4.Text = "Database: ";
		this.txtPassword.Location = new System.Drawing.Point(122, 123);
		this.txtPassword.Name = "txtPassword";
		this.txtPassword.PasswordChar = '*';
		this.txtPassword.Size = new System.Drawing.Size(208, 23);
		this.txtPassword.TabIndex = 4;
		this.label3.AutoSize = true;
		this.label3.Location = new System.Drawing.Point(12, 127);
		this.label3.Name = "label3";
		this.label3.Size = new System.Drawing.Size(63, 15);
		this.label3.TabIndex = 0;
		this.label3.Text = "Password: ";
		this.txtLogin.Location = new System.Drawing.Point(122, 88);
		this.txtLogin.Name = "txtLogin";
		this.txtLogin.Size = new System.Drawing.Size(208, 23);
		this.txtLogin.TabIndex = 3;
		this.label2.AutoSize = true;
		this.label2.Location = new System.Drawing.Point(12, 92);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(69, 15);
		this.label2.TabIndex = 0;
		this.label2.Text = "User name: ";
		this.txtServerName.Location = new System.Drawing.Point(122, 18);
		this.txtServerName.Name = "txtServerName";
		this.txtServerName.Size = new System.Drawing.Size(208, 23);
		this.txtServerName.TabIndex = 1;
		this.label1.AutoSize = true;
		this.label1.Location = new System.Drawing.Point(12, 22);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(78, 15);
		this.label1.TabIndex = 0;
		this.label1.Text = "Server name: ";
		this.btnSave.Location = new System.Drawing.Point(270, 196);
		this.btnSave.Name = "btnSave";
		this.btnSave.Size = new System.Drawing.Size(81, 30);
		this.btnSave.TabIndex = 5;
		this.btnSave.Text = "Save";
		this.btnSave.UseVisualStyleBackColor = true;
		this.btnSave.Click += new System.EventHandler(btnSave_Click);
		this.errorProvider1.ContainerControl = this;
		this.btnTestConnection.Location = new System.Drawing.Point(9, 196);
		this.btnTestConnection.Name = "btnTestConnection";
		this.btnTestConnection.Size = new System.Drawing.Size(107, 30);
		this.btnTestConnection.TabIndex = 6;
		this.btnTestConnection.Text = "Test connection";
		this.btnTestConnection.UseVisualStyleBackColor = true;
		this.btnTestConnection.Click += new System.EventHandler(btnTestConnection_Click);
		this.chkSynchronized.AutoSize = true;
		this.chkSynchronized.Location = new System.Drawing.Point(220, 160);
		this.chkSynchronized.Name = "chkSynchronized";
		this.chkSynchronized.Size = new System.Drawing.Size(97, 19);
		this.chkSynchronized.TabIndex = 6;
		this.chkSynchronized.Text = "Synchronized";
		this.chkSynchronized.UseVisualStyleBackColor = true;
		base.AutoScaleDimensions = new System.Drawing.SizeF(7f, 15f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(363, 238);
		base.Controls.Add(this.btnTestConnection);
		base.Controls.Add(this.btnSave);
		base.Controls.Add(this.groupBox1);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
		base.Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.Name = "FormAsrsServer";
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
		this.Text = "AS/RS Server";
		base.Load += new System.EventHandler(FormSqlServer_Load);
		this.groupBox1.ResumeLayout(false);
		this.groupBox1.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.errorProvider1).EndInit();
		base.ResumeLayout(false);
	}
}
