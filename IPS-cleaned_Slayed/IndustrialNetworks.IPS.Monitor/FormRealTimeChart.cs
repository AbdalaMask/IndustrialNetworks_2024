using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using NetStudio.IPS.Properties;

namespace NetStudio.IPS.Monitor;

public class FormRealTimeChart : Form
{
	private IContainer components;

	private ToolStrip toolBar;

	private ToolStripLabel lblSearchBox;

	private ToolStripLabel lblInfo;

	private ToolStripTextBox txtSearchBox;

	private ToolStripButton btnClearSearchBox;

	private ToolStripSeparator toolStripSeparator4;

	private ToolStripLabel lblTotalName;

	private ToolStripLabel lblTotalTags;

	private ToolStripLabel lblTotalUnit;

	private ToolStripSeparator GroupLineHorizontal;

	private ToolStripLabel lblGroupName;

	private ToolStripLabel lblGroupTags;

	private ToolStripLabel lblGroupUnit;

	private ToolStripSeparator GroupLineVertical;

	private SplitContainer splitContainer1;

	private TreeView treeViewMain;

	public FormRealTimeChart()
	{
		InitializeComponent();
	}

	private void dgvTags_CellContentClick(object sender, DataGridViewCellEventArgs e)
	{
	}

	private void toolBar_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
	{
	}

	private void treeViewMain_AfterSelect(object sender, TreeViewEventArgs e)
	{
	}

	private void splitContainer1_SplitterMoved(object sender, SplitterEventArgs e)
	{
	}

	private void splitContainer2_SplitterMoved(object sender, SplitterEventArgs e)
	{
	}

	private void contextMenuLogs_Opening(object sender, CancelEventArgs e)
	{
	}

	private void dgvLogs_CellContentClick(object sender, DataGridViewCellEventArgs e)
	{
	}

	private void contextMenuTags_Opening(object sender, CancelEventArgs e)
	{
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
		this.toolBar = new System.Windows.Forms.ToolStrip();
		this.lblSearchBox = new System.Windows.Forms.ToolStripLabel();
		this.lblInfo = new System.Windows.Forms.ToolStripLabel();
		this.txtSearchBox = new System.Windows.Forms.ToolStripTextBox();
		this.btnClearSearchBox = new System.Windows.Forms.ToolStripButton();
		this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
		this.lblTotalName = new System.Windows.Forms.ToolStripLabel();
		this.lblTotalTags = new System.Windows.Forms.ToolStripLabel();
		this.lblTotalUnit = new System.Windows.Forms.ToolStripLabel();
		this.GroupLineHorizontal = new System.Windows.Forms.ToolStripSeparator();
		this.lblGroupName = new System.Windows.Forms.ToolStripLabel();
		this.lblGroupTags = new System.Windows.Forms.ToolStripLabel();
		this.lblGroupUnit = new System.Windows.Forms.ToolStripLabel();
		this.GroupLineVertical = new System.Windows.Forms.ToolStripSeparator();
		this.splitContainer1 = new System.Windows.Forms.SplitContainer();
		this.treeViewMain = new System.Windows.Forms.TreeView();
		this.toolBar.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.splitContainer1).BeginInit();
		this.splitContainer1.Panel2.SuspendLayout();
		this.splitContainer1.SuspendLayout();
		base.SuspendLayout();
		this.toolBar.BackColor = System.Drawing.Color.Snow;
		this.toolBar.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
		this.toolBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[13]
		{
			this.lblSearchBox, this.lblInfo, this.txtSearchBox, this.btnClearSearchBox, this.toolStripSeparator4, this.lblTotalName, this.lblTotalTags, this.lblTotalUnit, this.GroupLineHorizontal, this.lblGroupName,
			this.lblGroupTags, this.lblGroupUnit, this.GroupLineVertical
		});
		this.toolBar.Location = new System.Drawing.Point(0, 0);
		this.toolBar.Name = "toolBar";
		this.toolBar.Size = new System.Drawing.Size(1136, 25);
		this.toolBar.TabIndex = 6;
		this.toolBar.Text = "Tool bar";
		this.lblSearchBox.Name = "lblSearchBox";
		this.lblSearchBox.Size = new System.Drawing.Size(45, 22);
		this.lblSearchBox.Text = "Search:";
		this.lblInfo.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
		this.lblInfo.Name = "lblInfo";
		this.lblInfo.Size = new System.Drawing.Size(70, 22);
		this.lblInfo.Text = "Information";
		this.txtSearchBox.BackColor = System.Drawing.Color.White;
		this.txtSearchBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.txtSearchBox.Name = "txtSearchBox";
		this.txtSearchBox.Size = new System.Drawing.Size(350, 25);
		this.btnClearSearchBox.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.btnClearSearchBox.Image = NetStudio.IPS.Properties.Resources.Resources_512_close;
		this.btnClearSearchBox.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.btnClearSearchBox.Name = "btnClearSearchBox";
		this.btnClearSearchBox.Size = new System.Drawing.Size(23, 22);
		this.btnClearSearchBox.Text = "Clear";
		this.toolStripSeparator4.Name = "toolStripSeparator4";
		this.toolStripSeparator4.Size = new System.Drawing.Size(6, 25);
		this.lblTotalName.Name = "lblTotalName";
		this.lblTotalName.Size = new System.Drawing.Size(35, 22);
		this.lblTotalName.Text = "Total:";
		this.lblTotalTags.ForeColor = System.Drawing.Color.Blue;
		this.lblTotalTags.Name = "lblTotalTags";
		this.lblTotalTags.Size = new System.Drawing.Size(13, 22);
		this.lblTotalTags.Text = "0";
		this.lblTotalUnit.ForeColor = System.Drawing.Color.DarkSlateGray;
		this.lblTotalUnit.Name = "lblTotalUnit";
		this.lblTotalUnit.Size = new System.Drawing.Size(29, 22);
		this.lblTotalUnit.Text = "tags";
		this.GroupLineHorizontal.Name = "GroupLineHorizontal";
		this.GroupLineHorizontal.Size = new System.Drawing.Size(6, 25);
		this.lblGroupName.Name = "lblGroupName";
		this.lblGroupName.Size = new System.Drawing.Size(43, 22);
		this.lblGroupName.Text = "Group:";
		this.lblGroupTags.ForeColor = System.Drawing.Color.Crimson;
		this.lblGroupTags.Name = "lblGroupTags";
		this.lblGroupTags.Size = new System.Drawing.Size(13, 22);
		this.lblGroupTags.Text = "0";
		this.lblGroupUnit.ForeColor = System.Drawing.Color.DarkSlateGray;
		this.lblGroupUnit.Name = "lblGroupUnit";
		this.lblGroupUnit.Size = new System.Drawing.Size(29, 22);
		this.lblGroupUnit.Text = "tags";
		this.GroupLineVertical.Name = "GroupLineVertical";
		this.GroupLineVertical.Size = new System.Drawing.Size(6, 25);
		this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
		this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
		this.splitContainer1.Location = new System.Drawing.Point(0, 25);
		this.splitContainer1.Name = "splitContainer1";
		this.splitContainer1.Panel2.Controls.Add(this.treeViewMain);
		this.splitContainer1.Size = new System.Drawing.Size(1136, 665);
		this.splitContainer1.SplitterDistance = 879;
		this.splitContainer1.TabIndex = 7;
		this.treeViewMain.Dock = System.Windows.Forms.DockStyle.Fill;
		this.treeViewMain.Location = new System.Drawing.Point(0, 0);
		this.treeViewMain.Name = "treeViewMain";
		this.treeViewMain.Size = new System.Drawing.Size(253, 665);
		this.treeViewMain.TabIndex = 0;
		base.AutoScaleDimensions = new System.Drawing.SizeF(7f, 15f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(1136, 690);
		base.Controls.Add(this.splitContainer1);
		base.Controls.Add(this.toolBar);
		base.Name = "FormRealTimeChart";
		this.Text = "Real-time charting";
		this.toolBar.ResumeLayout(false);
		this.toolBar.PerformLayout();
		this.splitContainer1.Panel2.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.splitContainer1).EndInit();
		this.splitContainer1.ResumeLayout(false);
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
