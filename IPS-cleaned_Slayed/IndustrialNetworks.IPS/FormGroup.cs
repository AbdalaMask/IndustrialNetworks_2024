using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using NetStudio.Common.Manager;
using NetStudio.IPS.Local;

namespace NetStudio.IPS;

public class FormGroup : Form
{
	private Device device;

	private Group? group;

	public EventGroupChanged? OnGroupChanged;

	private IContainer components;

	private Label label1;

	private TextBox txtChannel;

	private Label label2;

	private TextBox txtDevice;

	private Button btnCancel;

	private Button btnSave;

	private ErrorProvider errorProvider1;

	private Label label3;

	private TextBox txtGroupName;

	private GroupBox groupBox2;

	private GroupBox groupBox1;

	private TextBox txtDescription;

	public FormGroup(Channel channel, Device device, Group? group = null)
	{
		InitializeComponent();
		try
		{
			txtChannel.Text = channel.Name;
			txtDevice.Text = device.Name;
			this.device = device;
			if (group != null)
			{
				Text = "Edit: Group";
				this.group = (Group)group.Clone();
			}
			else
			{
				Text = "Add new: Group";
				this.group = new Group();
			}
			this.group.ChannelId = channel.Id;
			this.group.DeviceId = device.Id;
			txtGroupName.KeyDown += txtGroupName_KeyDown;
			txtGroupName.DataBindings.Add("Text", this.group, "Name", formattingEnabled: true, DataSourceUpdateMode.OnPropertyChanged);
			txtDescription.DataBindings.Add("Text", this.group, "Description", formattingEnabled: true, DataSourceUpdateMode.OnPropertyChanged);
		}
		catch (Exception ex)
		{
			MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private void FormGroup_Load(object sender, EventArgs e)
	{
		try
		{
			base.ActiveControl = txtGroupName;
		}
		catch (Exception ex)
		{
			MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private void btnSave_Click(object? sender, EventArgs e)
	{
		try
		{
			if (errorProvider1.HasErrors)
			{
				return;
			}
			if (group == null)
			{
				throw new Exception("Group is null");
			}
			if (Text.StartsWith("add", StringComparison.OrdinalIgnoreCase))
			{
				EditHelper.AddGroup(group);
				if (OnGroupChanged != null)
				{
					OnGroupChanged(group, isAddnew: true);
				}
			}
			else
			{
				EditHelper.EditGroup(group);
				if (OnGroupChanged != null)
				{
					OnGroupChanged(group, isAddnew: false);
				}
			}
			base.DialogResult = DialogResult.OK;
		}
		catch (Exception ex)
		{
			MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private void txtGroupName_KeyDown(object? sender, KeyEventArgs e)
	{
		if (e.KeyCode == Keys.Return)
		{
			btnSave_Click(sender, e);
		}
	}

	private void txtGroupName_Validating(object sender, CancelEventArgs e)
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
			errorProvider1.SetIconAlignment(txtGroupName, ErrorIconAlignment.MiddleLeft);
			errorProvider1.SetIconPadding(txtGroupName, 2);
			errorProvider1.SetError(textBox, "Please enter your Group name");
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NetStudio.IPS.FormGroup));
		this.label1 = new System.Windows.Forms.Label();
		this.txtChannel = new System.Windows.Forms.TextBox();
		this.label2 = new System.Windows.Forms.Label();
		this.txtDevice = new System.Windows.Forms.TextBox();
		this.btnCancel = new System.Windows.Forms.Button();
		this.btnSave = new System.Windows.Forms.Button();
		this.errorProvider1 = new System.Windows.Forms.ErrorProvider(this.components);
		this.label3 = new System.Windows.Forms.Label();
		this.txtGroupName = new System.Windows.Forms.TextBox();
		this.groupBox1 = new System.Windows.Forms.GroupBox();
		this.txtDescription = new System.Windows.Forms.TextBox();
		this.groupBox2 = new System.Windows.Forms.GroupBox();
		((System.ComponentModel.ISupportInitialize)this.errorProvider1).BeginInit();
		this.groupBox1.SuspendLayout();
		this.groupBox2.SuspendLayout();
		base.SuspendLayout();
		this.label1.AutoSize = true;
		this.label1.Location = new System.Drawing.Point(10, 24);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(87, 15);
		this.label1.TabIndex = 0;
		this.label1.Text = "Channel name:";
		this.txtChannel.Location = new System.Drawing.Point(124, 20);
		this.txtChannel.Name = "txtChannel";
		this.txtChannel.ReadOnly = true;
		this.txtChannel.Size = new System.Drawing.Size(255, 23);
		this.txtChannel.TabIndex = 5;
		this.label2.AutoSize = true;
		this.label2.Location = new System.Drawing.Point(10, 56);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(78, 15);
		this.label2.TabIndex = 2;
		this.label2.Text = "Device name:";
		this.txtDevice.Location = new System.Drawing.Point(124, 52);
		this.txtDevice.Name = "txtDevice";
		this.txtDevice.ReadOnly = true;
		this.txtDevice.Size = new System.Drawing.Size(255, 23);
		this.txtDevice.TabIndex = 6;
		this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		this.btnCancel.Location = new System.Drawing.Point(322, 284);
		this.btnCancel.Name = "btnCancel";
		this.btnCancel.Size = new System.Drawing.Size(76, 30);
		this.btnCancel.TabIndex = 4;
		this.btnCancel.Text = "Cancel";
		this.btnCancel.UseVisualStyleBackColor = true;
		this.btnSave.Location = new System.Drawing.Point(240, 284);
		this.btnSave.Name = "btnSave";
		this.btnSave.Size = new System.Drawing.Size(76, 30);
		this.btnSave.TabIndex = 3;
		this.btnSave.Text = "Save";
		this.btnSave.UseVisualStyleBackColor = true;
		this.btnSave.Click += new System.EventHandler(btnSave_Click);
		this.errorProvider1.ContainerControl = this;
		this.label3.AutoSize = true;
		this.label3.Location = new System.Drawing.Point(10, 88);
		this.label3.Name = "label3";
		this.label3.Size = new System.Drawing.Size(76, 15);
		this.label3.TabIndex = 15;
		this.label3.Text = "Group name:";
		this.txtGroupName.Location = new System.Drawing.Point(124, 85);
		this.txtGroupName.Name = "txtGroupName";
		this.txtGroupName.Size = new System.Drawing.Size(255, 23);
		this.txtGroupName.TabIndex = 1;
		this.txtGroupName.Validating += new System.ComponentModel.CancelEventHandler(txtGroupName_Validating);
		this.groupBox1.Controls.Add(this.txtDescription);
		this.groupBox1.ForeColor = System.Drawing.Color.Navy;
		this.groupBox1.Location = new System.Drawing.Point(9, 133);
		this.groupBox1.Name = "groupBox1";
		this.groupBox1.Size = new System.Drawing.Size(389, 145);
		this.groupBox1.TabIndex = 49;
		this.groupBox1.TabStop = false;
		this.groupBox1.Text = "Description";
		this.txtDescription.Location = new System.Drawing.Point(10, 22);
		this.txtDescription.Multiline = true;
		this.txtDescription.Name = "txtDescription";
		this.txtDescription.Size = new System.Drawing.Size(369, 110);
		this.txtDescription.TabIndex = 6;
		this.groupBox2.Controls.Add(this.txtChannel);
		this.groupBox2.Controls.Add(this.label1);
		this.groupBox2.Controls.Add(this.txtGroupName);
		this.groupBox2.Controls.Add(this.label2);
		this.groupBox2.Controls.Add(this.label3);
		this.groupBox2.Controls.Add(this.txtDevice);
		this.groupBox2.ForeColor = System.Drawing.Color.Navy;
		this.groupBox2.Location = new System.Drawing.Point(9, 5);
		this.groupBox2.Name = "groupBox2";
		this.groupBox2.Size = new System.Drawing.Size(389, 122);
		this.groupBox2.TabIndex = 50;
		this.groupBox2.TabStop = false;
		this.groupBox2.Text = "General";
		base.AutoScaleDimensions = new System.Drawing.SizeF(7f, 15f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(408, 326);
		base.Controls.Add(this.groupBox2);
		base.Controls.Add(this.groupBox1);
		base.Controls.Add(this.btnCancel);
		base.Controls.Add(this.btnSave);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
		base.Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.Name = "FormGroup";
		this.Text = "Group";
		base.Load += new System.EventHandler(FormGroup_Load);
		((System.ComponentModel.ISupportInitialize)this.errorProvider1).EndInit();
		this.groupBox1.ResumeLayout(false);
		this.groupBox1.PerformLayout();
		this.groupBox2.ResumeLayout(false);
		this.groupBox2.PerformLayout();
		base.ResumeLayout(false);
	}
}
