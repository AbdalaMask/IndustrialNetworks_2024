using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using NetStudio.Common.DataTypes;
using NetStudio.Common.Manager;

namespace NetStudio.IPS.Monitor;

public class FormWriteTag : Form
{
	private BOOL toggle = false;

	private Tag currentTag;

	public EventWriteTagChanged OnWriteTagChanged;

	private IContainer components;

	private TextBox txtValue;

	private Button btnWrite;

	private Button btnON;

	private Button btnOFF;

	private GroupBox groupBox1;

	private GroupBox groupBox2;

	private GroupBox groupBox3;

	private Label lblMessage;

	private Label lblCharCount;

	private TextBox lblTagName;

	public FormWriteTag(Tag current)
	{
		InitializeComponent();
		currentTag = current;
		try
		{
			currentTag = currentTag ?? new Tag();
			lblTagName.DataBindings.Add("Text", currentTag, "FullName");
			txtValue.Text = (((object)currentTag.Value == null) ? string.Empty : currentTag.Value.ToString());
			txtValue.KeyDown += txtValue_KeyDown;
			if (currentTag.DataType == DataType.BOOL)
			{
				txtValue.ReadOnly = true;
				btnWrite.Text = "Write: ON";
				base.ActiveControl = btnWrite;
				btnON.Visible = true;
				btnOFF.Visible = true;
				btnWrite.Visible = false;
			}
			else
			{
				base.ActiveControl = txtValue;
				btnON.Visible = false;
				btnOFF.Visible = false;
				btnWrite.Visible = true;
			}
		}
		catch (Exception ex)
		{
			MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private string GetValueString()
	{
		string result = txtValue.Text;
		if (!currentTag.IsScaling && currentTag.Offset != 0f)
		{
			switch (currentTag.DataType)
			{
			case DataType.BYTE:
				result = $"{(byte)(double.Parse(txtValue.Text) * (double)currentTag.Offset)}";
				break;
			case DataType.INT:
				result = $"{(short)(double.Parse(txtValue.Text) * (double)currentTag.Offset)}";
				break;
			case DataType.UINT:
				result = $"{(ushort)(double.Parse(txtValue.Text) * (double)currentTag.Offset)}";
				break;
			case DataType.DINT:
				result = $"{(int)(double.Parse(txtValue.Text) * (double)currentTag.Offset)}";
				break;
			case DataType.UDINT:
				result = $"{(uint)(double.Parse(txtValue.Text) * (double)currentTag.Offset)}";
				break;
			case DataType.LINT:
				result = $"{(long)(double.Parse(txtValue.Text) * (double)currentTag.Offset)}";
				break;
			case DataType.ULINT:
				result = $"{(ulong)(double.Parse(txtValue.Text) * (double)currentTag.Offset)}";
				break;
			}
		}
		return result;
	}

	private async void btnWrite_Click(object? sender, EventArgs e)
	{
		try
		{
			lblMessage.Text = string.Empty;
			if (currentTag == null)
			{
				return;
			}
			string valueString = GetValueString();
			object value;
			IPSResult iPSResult;
			if (currentTag.DataType == DataType.BOOL)
			{
				toggle = !toggle;
				value = toggle;
				iPSResult = await AppHelper.Driver.WriteTag(currentTag.FullName, (object)toggle);
				if ((bool)toggle)
				{
					btnWrite.Text = "Write: OFF";
					txtValue.Text = "ON";
					txtValue.ForeColor = Color.Green;
				}
				else
				{
					btnWrite.Text = "Write: ON";
					txtValue.Text = "OFF";
					txtValue.ForeColor = Color.Red;
				}
			}
			else
			{
				value = currentTag.DataType switch
				{
					DataType.INT => new INT(valueString), 
					DataType.UINT => new UINT(valueString), 
					DataType.WORD => new WORD(valueString.ToUpper()), 
					DataType.DINT => new DINT(valueString), 
					DataType.UDINT => new UDINT(valueString), 
					DataType.DWORD => new DWORD(valueString.ToUpper()), 
					DataType.REAL => new REAL(valueString), 
					DataType.LINT => new LINT(valueString), 
					DataType.ULINT => new ULINT(valueString), 
					DataType.LWORD => new LWORD(valueString.ToUpper()), 
					DataType.LREAL => new LREAL(valueString), 
					DataType.TIME16 => new TIME16(txtValue.Text, currentTag.Resolution), 
					DataType.TIME32 => new TIME32(txtValue.Text, currentTag.Resolution), 
					DataType.STRING => new STRING(valueString), 
					_ => new INT(valueString), 
				};
				iPSResult = await AppHelper.Driver.WriteTag(currentTag.FullName, value);
			}
			if (iPSResult != null)
			{
				SetMessage(iPSResult.Message, iPSResult.Status != CommStatus.Success);
				if (iPSResult.Status == CommStatus.Success && OnWriteTagChanged != null)
				{
					currentTag.Value = value;
					OnWriteTagChanged(currentTag, value);
				}
			}
		}
		catch (Exception ex)
		{
			SetMessage("Write data: failure.");
			MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private async void btnON_Click(object sender, EventArgs e)
	{
		try
		{
			if (currentTag == null || currentTag.DataType != 0)
			{
				return;
			}
			lblMessage.Text = string.Empty;
			txtValue.Text = "true";
			IPSResult iPSResult = await AppHelper.Driver.WriteTag(currentTag.FullName, (object)true);
			if (iPSResult != null)
			{
				SetMessage(iPSResult.Message, iPSResult.Status != CommStatus.Success);
				if (iPSResult.Status == CommStatus.Success && OnWriteTagChanged != null)
				{
					currentTag.Value = true;
					OnWriteTagChanged(currentTag, true);
				}
			}
		}
		catch (Exception ex)
		{
			SetMessage("Write data: failure.");
			MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private async void btnOFF_Click(object sender, EventArgs e)
	{
		try
		{
			if (currentTag == null || currentTag.DataType != 0)
			{
				return;
			}
			lblMessage.Text = string.Empty;
			txtValue.Text = "false";
			IPSResult iPSResult = await AppHelper.Driver.WriteTag(currentTag.FullName, (object)false);
			if (iPSResult != null)
			{
				SetMessage(iPSResult.Message, iPSResult.Status != CommStatus.Success);
				if (iPSResult.Status == CommStatus.Success && OnWriteTagChanged != null)
				{
					currentTag.Value = false;
					OnWriteTagChanged(currentTag, false);
				}
			}
		}
		catch (Exception ex)
		{
			SetMessage("Write data: failure.");
			MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	public void SetMessage(string message, bool isError = true)
	{
		lblMessage.Text = message;
		if (isError)
		{
			lblMessage.ForeColor = Color.Red;
		}
		else
		{
			lblMessage.ForeColor = Color.Green;
		}
	}

	private void txtValue_KeyDown(object? sender, KeyEventArgs e)
	{
		if (e.KeyCode == Keys.Return)
		{
			btnWrite_Click(sender, e);
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

	private void txtValue_TextChanged(object sender, EventArgs e)
	{
		try
		{
			if (string.IsNullOrEmpty(txtValue.Text))
			{
				lblCharCount.Text = "0";
				return;
			}
			lblCharCount.Text = $"Number of characters: {txtValue.Text.Length}";
		}
		catch (Exception ex)
		{
			lblMessage.Text = ex.Message;
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NetStudio.IPS.Monitor.FormWriteTag));
		this.txtValue = new System.Windows.Forms.TextBox();
		this.btnWrite = new System.Windows.Forms.Button();
		this.btnON = new System.Windows.Forms.Button();
		this.btnOFF = new System.Windows.Forms.Button();
		this.groupBox1 = new System.Windows.Forms.GroupBox();
		this.lblTagName = new System.Windows.Forms.TextBox();
		this.groupBox2 = new System.Windows.Forms.GroupBox();
		this.groupBox3 = new System.Windows.Forms.GroupBox();
		this.lblMessage = new System.Windows.Forms.Label();
		this.lblCharCount = new System.Windows.Forms.Label();
		this.groupBox1.SuspendLayout();
		this.groupBox2.SuspendLayout();
		this.groupBox3.SuspendLayout();
		base.SuspendLayout();
		this.txtValue.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.txtValue.Location = new System.Drawing.Point(10, 20);
		this.txtValue.Name = "txtValue";
		this.txtValue.Size = new System.Drawing.Size(482, 23);
		this.txtValue.TabIndex = 6;
		this.txtValue.Text = "0";
		this.txtValue.TextChanged += new System.EventHandler(txtValue_TextChanged);
		this.btnWrite.Location = new System.Drawing.Point(430, 201);
		this.btnWrite.Name = "btnWrite";
		this.btnWrite.Size = new System.Drawing.Size(76, 30);
		this.btnWrite.TabIndex = 1;
		this.btnWrite.Text = "Write";
		this.btnWrite.UseVisualStyleBackColor = true;
		this.btnWrite.Click += new System.EventHandler(btnWrite_Click);
		this.btnON.ForeColor = System.Drawing.Color.Green;
		this.btnON.Location = new System.Drawing.Point(170, 201);
		this.btnON.Name = "btnON";
		this.btnON.Size = new System.Drawing.Size(76, 30);
		this.btnON.TabIndex = 2;
		this.btnON.Text = "ON";
		this.btnON.UseVisualStyleBackColor = true;
		this.btnON.Click += new System.EventHandler(btnON_Click);
		this.btnOFF.ForeColor = System.Drawing.Color.Red;
		this.btnOFF.Location = new System.Drawing.Point(252, 201);
		this.btnOFF.Name = "btnOFF";
		this.btnOFF.Size = new System.Drawing.Size(76, 30);
		this.btnOFF.TabIndex = 3;
		this.btnOFF.Text = "OFF";
		this.btnOFF.UseVisualStyleBackColor = true;
		this.btnOFF.Click += new System.EventHandler(btnOFF_Click);
		this.groupBox1.Controls.Add(this.lblTagName);
		this.groupBox1.ForeColor = System.Drawing.Color.Navy;
		this.groupBox1.Location = new System.Drawing.Point(6, 83);
		this.groupBox1.Name = "groupBox1";
		this.groupBox1.Size = new System.Drawing.Size(500, 54);
		this.groupBox1.TabIndex = 19;
		this.groupBox1.TabStop = false;
		this.groupBox1.Text = "Tag name";
		this.lblTagName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.lblTagName.Location = new System.Drawing.Point(9, 19);
		this.lblTagName.Name = "lblTagName";
		this.lblTagName.ReadOnly = true;
		this.lblTagName.Size = new System.Drawing.Size(482, 23);
		this.lblTagName.TabIndex = 5;
		this.groupBox2.Controls.Add(this.txtValue);
		this.groupBox2.ForeColor = System.Drawing.Color.Navy;
		this.groupBox2.Location = new System.Drawing.Point(6, 142);
		this.groupBox2.Name = "groupBox2";
		this.groupBox2.Size = new System.Drawing.Size(500, 54);
		this.groupBox2.TabIndex = 20;
		this.groupBox2.TabStop = false;
		this.groupBox2.Text = "Set Value";
		this.groupBox3.Controls.Add(this.lblMessage);
		this.groupBox3.ForeColor = System.Drawing.Color.Navy;
		this.groupBox3.Location = new System.Drawing.Point(6, 5);
		this.groupBox3.Name = "groupBox3";
		this.groupBox3.Size = new System.Drawing.Size(500, 73);
		this.groupBox3.TabIndex = 22;
		this.groupBox3.TabStop = false;
		this.groupBox3.Text = "Message";
		this.lblMessage.ForeColor = System.Drawing.SystemColors.ControlText;
		this.lblMessage.Location = new System.Drawing.Point(10, 20);
		this.lblMessage.Name = "lblMessage";
		this.lblMessage.Size = new System.Drawing.Size(482, 40);
		this.lblMessage.TabIndex = 21;
		this.lblCharCount.AutoSize = true;
		this.lblCharCount.ForeColor = System.Drawing.Color.DarkSlateGray;
		this.lblCharCount.Location = new System.Drawing.Point(6, 209);
		this.lblCharCount.Name = "lblCharCount";
		this.lblCharCount.Size = new System.Drawing.Size(122, 15);
		this.lblCharCount.TabIndex = 23;
		this.lblCharCount.Text = "Number of characters";
		base.AutoScaleDimensions = new System.Drawing.SizeF(7f, 15f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(513, 241);
		base.Controls.Add(this.lblCharCount);
		base.Controls.Add(this.groupBox3);
		base.Controls.Add(this.groupBox1);
		base.Controls.Add(this.groupBox2);
		base.Controls.Add(this.btnOFF);
		base.Controls.Add(this.btnON);
		base.Controls.Add(this.btnWrite);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
		base.Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.Name = "FormWriteTag";
		this.Text = "Write tag";
		this.groupBox1.ResumeLayout(false);
		this.groupBox1.PerformLayout();
		this.groupBox2.ResumeLayout(false);
		this.groupBox2.PerformLayout();
		this.groupBox3.ResumeLayout(false);
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
