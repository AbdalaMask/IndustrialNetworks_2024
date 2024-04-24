using System;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace NetStudio.IPS.Controls;

internal class WaitForm : Form
{
	private CancellationToken _cancellationToken;

	private System.Windows.Forms.Timer timer;

	private IContainer components;

	private GroupBox groupBox1;

	private ProgressBar progressBar1;

	private Label lblMessage;

	public WaitForm(string message, CancellationToken cancellationToken)
	{
		InitializeComponent();
		lblMessage.Text = message;
		_cancellationToken = cancellationToken;
		base.Load += WaitForm_Load;
		timer = new System.Windows.Forms.Timer();
	}

	private void WaitForm_Load(object? sender, EventArgs e)
	{
		progressBar1.Minimum = 0;
		progressBar1.Maximum = 100;
		progressBar1.Style = ProgressBarStyle.Marquee;
		timer.Interval = 250;
		timer.Tick += Timer_Tick;
		timer.Start();
	}

	private void Timer_Tick(object? sender, EventArgs e)
	{
		try
		{
			if (_cancellationToken.IsCancellationRequested)
			{
				Close();
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
		this.groupBox1 = new System.Windows.Forms.GroupBox();
		this.progressBar1 = new System.Windows.Forms.ProgressBar();
		this.lblMessage = new System.Windows.Forms.Label();
		this.groupBox1.SuspendLayout();
		base.SuspendLayout();
		this.groupBox1.Controls.Add(this.progressBar1);
		this.groupBox1.Controls.Add(this.lblMessage);
		this.groupBox1.Location = new System.Drawing.Point(12, 10);
		this.groupBox1.Name = "groupBox1";
		this.groupBox1.Size = new System.Drawing.Size(275, 68);
		this.groupBox1.TabIndex = 3;
		this.groupBox1.TabStop = false;
		this.groupBox1.Text = "Please Wait";
		this.progressBar1.Location = new System.Drawing.Point(7, 21);
		this.progressBar1.MarqueeAnimationSpeed = 20;
		this.progressBar1.Name = "progressBar1";
		this.progressBar1.Size = new System.Drawing.Size(260, 19);
		this.progressBar1.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
		this.progressBar1.TabIndex = 0;
		this.progressBar1.UseWaitCursor = true;
		this.lblMessage.AutoSize = true;
		this.lblMessage.Location = new System.Drawing.Point(3, 44);
		this.lblMessage.Name = "lblMessage";
		this.lblMessage.Size = new System.Drawing.Size(59, 15);
		this.lblMessage.TabIndex = 1;
		this.lblMessage.Text = "Loading...";
		base.AutoScaleDimensions = new System.Drawing.SizeF(7f, 15f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(300, 91);
		base.Controls.Add(this.groupBox1);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
		base.Name = "WaitForm";
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
		this.Text = "WaitForm";
		this.groupBox1.ResumeLayout(false);
		this.groupBox1.PerformLayout();
		base.ResumeLayout(false);
	}
}
