using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using NetStudio.IPS.Controls;
using NetStudio.IPS.Local;
using NetStudio.IPS.Properties;

namespace NetStudio.IPS;

public class FormNewProject : Form
{
	private IContainer components;

	private Label label1;

	private TextBox txtProjectName;

	private Label label2;

	private Button btnCancel;

	private Button btnSave;

	private ErrorProvider errorProvider1;

	private ButtonTextBox txtLocation;

	private GroupBox groupBox1;

	public FormNewProject()
	{
		InitializeComponent();
	}

	private void FormNewProject_Load(object sender, EventArgs e)
	{
		try
		{
			if (AppHelper.Settings.Mode)
			{
				AppHelper.Settings.Directory = "Data";
				txtLocation.ReadOnly = true;
			}
			txtLocation.DataBindings.Add("Text", AppHelper.Settings, "Directory", formattingEnabled: true, DataSourceUpdateMode.OnPropertyChanged);
			txtLocation.ButtonClick += btnSelectLocation_Click;
		}
		catch (Exception ex)
		{
			MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private void btnSelectLocation_Click(object? sender, EventArgs e)
	{
		try
		{
			FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog
			{
				ShowNewFolderButton = true
			};
			if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
			{
				AppHelper.Settings.Directory = folderBrowserDialog.SelectedPath;
				Settings.Default.Directory = AppHelper.Settings.Directory;
				Settings.Default.Save();
			}
		}
		catch (Exception ex)
		{
			MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private async void btnSave_Click(object? sender, EventArgs e)
	{
		try
		{
			if (errorProvider1.HasErrors)
			{
				return;
			}
			if (!Directory.Exists(AppHelper.Settings.Directory))
			{
				Directory.CreateDirectory(AppHelper.Settings.Directory);
			}
			string fileName = Path.Combine(AppHelper.Settings.Directory, txtProjectName.Text + ".json");
			if (string.IsNullOrEmpty(txtProjectName.Text) || string.IsNullOrWhiteSpace(txtProjectName.Text))
			{
				txtProjectName.Focus();
				errorProvider1.SetIconPadding(txtLocation, 1);
				errorProvider1.SetIconAlignment(txtProjectName, ErrorIconAlignment.MiddleLeft);
				errorProvider1.SetError(txtProjectName, "Please enter your location.");
			}
			if (File.Exists(fileName))
			{
				txtProjectName.Focus();
				errorProvider1.SetIconPadding(txtLocation, 1);
				errorProvider1.SetIconAlignment(txtProjectName, ErrorIconAlignment.MiddleLeft);
				errorProvider1.SetError(txtProjectName, "Project name already exists.");
			}
			if (string.IsNullOrEmpty(txtLocation.Text) || string.IsNullOrWhiteSpace(txtLocation.Text))
			{
				txtProjectName.Focus();
				errorProvider1.SetIconPadding(txtLocation, 1);
				errorProvider1.SetIconAlignment(txtLocation, ErrorIconAlignment.MiddleLeft);
				errorProvider1.SetError(txtLocation, "Please enter your location.");
			}
			if (!errorProvider1.HasErrors)
			{
				if ((await EditHelper.Editor.NewAsync(fileName)).Success && !AppHelper.Settings.Mode)
				{
					AppHelper.Settings.FileName = fileName;
					AppHelper.WriteSettings(AppHelper.Settings);
				}
				base.Tag = txtProjectName.Text;
				base.DialogResult = DialogResult.OK;
			}
		}
		catch (Exception ex)
		{
			MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private void txtProjectName_Validating(object sender, CancelEventArgs e)
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
			errorProvider1.SetIconPadding(textBox, 1);
			errorProvider1.SetIconAlignment(textBox, ErrorIconAlignment.MiddleLeft);
			errorProvider1.SetError(textBox, "Please enter your project name.");
		}
	}

	private void txtLocation_Validating(object sender, CancelEventArgs e)
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
			errorProvider1.SetIconPadding(textBox, 1);
			errorProvider1.SetIconAlignment(textBox, ErrorIconAlignment.MiddleLeft);
			errorProvider1.SetError(textBox, "Please enter your location.");
		}
	}

	private void OnKeyEnter_KeyDown(object? sender, KeyEventArgs e)
	{
		if (e.KeyCode == Keys.Return)
		{
			btnSave_Click(sender, e);
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
		this.components = new System.ComponentModel.Container();
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NetStudio.IPS.FormNewProject));
		this.label1 = new System.Windows.Forms.Label();
		this.txtProjectName = new System.Windows.Forms.TextBox();
		this.label2 = new System.Windows.Forms.Label();
		this.btnCancel = new System.Windows.Forms.Button();
		this.btnSave = new System.Windows.Forms.Button();
		this.errorProvider1 = new System.Windows.Forms.ErrorProvider(this.components);
		this.txtLocation = new NetStudio.IPS.Controls.ButtonTextBox();
		this.groupBox1 = new System.Windows.Forms.GroupBox();
		((System.ComponentModel.ISupportInitialize)this.errorProvider1).BeginInit();
		this.groupBox1.SuspendLayout();
		base.SuspendLayout();
		this.label1.AutoSize = true;
		this.label1.Location = new System.Drawing.Point(20, 15);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(90, 15);
		this.label1.TabIndex = 0;
		this.label1.Text = "Project name(*)";
		this.txtProjectName.Location = new System.Drawing.Point(20, 33);
		this.txtProjectName.Name = "txtProjectName";
		this.txtProjectName.Size = new System.Drawing.Size(471, 23);
		this.txtProjectName.TabIndex = 1;
		this.txtProjectName.KeyDown += new System.Windows.Forms.KeyEventHandler(OnKeyEnter_KeyDown);
		this.txtProjectName.Validating += new System.ComponentModel.CancelEventHandler(txtProjectName_Validating);
		this.label2.AutoSize = true;
		this.label2.Location = new System.Drawing.Point(20, 66);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(66, 15);
		this.label2.TabIndex = 2;
		this.label2.Text = "Location(*)";
		this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		this.btnCancel.Location = new System.Drawing.Point(437, 129);
		this.btnCancel.Name = "btnCancel";
		this.btnCancel.Size = new System.Drawing.Size(76, 30);
		this.btnCancel.TabIndex = 14;
		this.btnCancel.Text = "Cancel";
		this.btnCancel.UseVisualStyleBackColor = true;
		this.btnSave.Location = new System.Drawing.Point(355, 129);
		this.btnSave.Name = "btnSave";
		this.btnSave.Size = new System.Drawing.Size(76, 30);
		this.btnSave.TabIndex = 13;
		this.btnSave.Text = "Save";
		this.btnSave.UseVisualStyleBackColor = true;
		this.btnSave.Click += new System.EventHandler(btnSave_Click);
		this.errorProvider1.ContainerControl = this;
		this.txtLocation.ButtonText = "...";
		this.txtLocation.ButtonWidth = 30;
		this.txtLocation.Location = new System.Drawing.Point(20, 85);
		this.txtLocation.Name = "txtLocation";
		this.txtLocation.Size = new System.Drawing.Size(471, 23);
		this.txtLocation.TabIndex = 15;
		this.groupBox1.Controls.Add(this.label1);
		this.groupBox1.Controls.Add(this.txtLocation);
		this.groupBox1.Controls.Add(this.txtProjectName);
		this.groupBox1.Controls.Add(this.label2);
		this.groupBox1.Location = new System.Drawing.Point(10, 3);
		this.groupBox1.Name = "groupBox1";
		this.groupBox1.Size = new System.Drawing.Size(503, 120);
		this.groupBox1.TabIndex = 16;
		this.groupBox1.TabStop = false;
		base.AutoScaleDimensions = new System.Drawing.SizeF(7f, 15f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(523, 166);
		base.Controls.Add(this.groupBox1);
		base.Controls.Add(this.btnCancel);
		base.Controls.Add(this.btnSave);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
		base.Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.Name = "FormNewProject";
		this.Text = "New project";
		base.Load += new System.EventHandler(FormNewProject_Load);
		((System.ComponentModel.ISupportInitialize)this.errorProvider1).EndInit();
		this.groupBox1.ResumeLayout(false);
		this.groupBox1.PerformLayout();
		base.ResumeLayout(false);
	}
}
