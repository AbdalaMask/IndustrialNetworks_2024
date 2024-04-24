using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using NetStudio.Common.AsrsLink;
using NetStudio.DriverComm;
using NetStudio.DriverComm.Models;
using NetStudio.IPS.Local;

namespace NetStudio.IPS.Editor.AsrsLink;

public class FormAsrsTable : Form
{
	private AsrsTable _Current;

	private IContainer components;

	private GroupBox groupBox1;

	private Button btnSave;

	private ComboBox cboxTable;

	public FormAsrsTable(AsrsTable table)
	{
		InitializeComponent();
		_Current = table;
	}

	private async void FormAsrsTable_Load(object sender, EventArgs e)
	{
		try
		{
			AsrsServer asrsServer = null;
			if (EditHelper.IndusProtocol != null && EditHelper.IndusProtocol.AsrsServer != null)
			{
				asrsServer = EditHelper.IndusProtocol.AsrsServer;
			}
			if (asrsServer != null)
			{
				SqlRequestInfo rstInfo = new SqlRequestInfo
				{
					ServerName = asrsServer.ServerName,
					DatabaseName = asrsServer.DatabaseName,
					Login = asrsServer.Login,
					Password = asrsServer.Password
				};
				ApiResponse apiResponse = await ClientHelper.Editor.GetTableNamesAsync(rstInfo);
				if (apiResponse.Success)
				{
					cboxTable.DataSource = (List<AsrsTable>)apiResponse.Data;
					cboxTable.DisplayMember = "Name";
					cboxTable.ValueMember = "Id";
					cboxTable.SelectedValue = _Current.Id;
				}
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
			if (string.IsNullOrEmpty(cboxTable.Text) || string.IsNullOrWhiteSpace(cboxTable.Text))
			{
				throw new Exception("Please enter the table name");
			}
			EditHelper.Invalidate(_Current);
			base.Tag = _Current;
			base.DialogResult = DialogResult.OK;
		}
		catch (Exception ex)
		{
			MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private void cboxTable_SelectedIndexChanged(object sender, EventArgs e)
	{
		try
		{
			AsrsTable asrsTable = (AsrsTable)((ComboBox)sender).SelectedItem;
			if (_Current.Id == 0)
			{
				_Current = asrsTable;
			}
			else
			{
				_Current.Name = asrsTable.Name;
			}
		}
		catch (Exception ex)
		{
			MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NetStudio.IPS.Editor.AsrsLink.FormAsrsTable));
		this.groupBox1 = new System.Windows.Forms.GroupBox();
		this.cboxTable = new System.Windows.Forms.ComboBox();
		this.btnSave = new System.Windows.Forms.Button();
		this.groupBox1.SuspendLayout();
		base.SuspendLayout();
		this.groupBox1.Controls.Add(this.cboxTable);
		this.groupBox1.Location = new System.Drawing.Point(9, 3);
		this.groupBox1.Name = "groupBox1";
		this.groupBox1.Size = new System.Drawing.Size(267, 53);
		this.groupBox1.TabIndex = 0;
		this.groupBox1.TabStop = false;
		this.cboxTable.FormattingEnabled = true;
		this.cboxTable.Location = new System.Drawing.Point(10, 18);
		this.cboxTable.Name = "cboxTable";
		this.cboxTable.Size = new System.Drawing.Size(247, 23);
		this.cboxTable.TabIndex = 0;
		this.cboxTable.SelectedIndexChanged += new System.EventHandler(cboxTable_SelectedIndexChanged);
		this.btnSave.Location = new System.Drawing.Point(195, 62);
		this.btnSave.Name = "btnSave";
		this.btnSave.Size = new System.Drawing.Size(81, 30);
		this.btnSave.TabIndex = 2;
		this.btnSave.Text = "Save";
		this.btnSave.UseVisualStyleBackColor = true;
		this.btnSave.Click += new System.EventHandler(btnSave_Click);
		base.AutoScaleDimensions = new System.Drawing.SizeF(7f, 15f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(286, 100);
		base.Controls.Add(this.btnSave);
		base.Controls.Add(this.groupBox1);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
		base.Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.Name = "FormAsrsTable";
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
		this.Text = "AS/RS Table";
		base.Load += new System.EventHandler(FormAsrsTable_Load);
		this.groupBox1.ResumeLayout(false);
		base.ResumeLayout(false);
	}
}
