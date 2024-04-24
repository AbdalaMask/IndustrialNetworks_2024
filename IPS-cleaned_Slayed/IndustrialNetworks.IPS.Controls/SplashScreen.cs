using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using NetStudio.IPS.Properties;

namespace NetStudio.IPS.Controls;

public class SplashScreen : Form
{
	private IContainer components;

	private ProgressBar progressBar1;

	private PictureBox pictureBox1;

	private Label lblMessage;

	private Label label1;

	public SplashScreen()
	{
		InitializeComponent();
		Control.CheckForIllegalCrossThreadCalls = false;
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NetStudio.IPS.Controls.SplashScreen));
		this.progressBar1 = new System.Windows.Forms.ProgressBar();
		this.pictureBox1 = new System.Windows.Forms.PictureBox();
		this.lblMessage = new System.Windows.Forms.Label();
		this.label1 = new System.Windows.Forms.Label();
		((System.ComponentModel.ISupportInitialize)this.pictureBox1).BeginInit();
		base.SuspendLayout();
		this.progressBar1.Location = new System.Drawing.Point(8, 200);
		this.progressBar1.MarqueeAnimationSpeed = 50;
		this.progressBar1.Name = "progressBar1";
		this.progressBar1.Size = new System.Drawing.Size(357, 17);
		this.progressBar1.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
		this.progressBar1.TabIndex = 1;
		this.pictureBox1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.pictureBox1.Image = NetStudio.IPS.Properties.Resources.Resources_530_Digital_Transformation;
		this.pictureBox1.Location = new System.Drawing.Point(8, 9);
		this.pictureBox1.Name = "pictureBox1";
		this.pictureBox1.Size = new System.Drawing.Size(357, 185);
		this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
		this.pictureBox1.TabIndex = 2;
		this.pictureBox1.TabStop = false;
		this.lblMessage.AutoSize = true;
		this.lblMessage.Location = new System.Drawing.Point(5, 221);
		this.lblMessage.Name = "lblMessage";
		this.lblMessage.Size = new System.Drawing.Size(59, 15);
		this.lblMessage.TabIndex = 3;
		this.lblMessage.Text = "Loading...";
		this.label1.AutoSize = true;
		this.label1.ForeColor = System.Drawing.Color.Crimson;
		this.label1.Location = new System.Drawing.Point(242, 221);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(120, 15);
		this.label1.TabIndex = 4;
		this.label1.Text = "©Industrial Networks";
		base.AutoScaleDimensions = new System.Drawing.SizeF(7f, 15f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(373, 240);
		base.Controls.Add(this.label1);
		base.Controls.Add(this.lblMessage);
		base.Controls.Add(this.pictureBox1);
		base.Controls.Add(this.progressBar1);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
		base.Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.Name = "SplashScreen";
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
		this.Text = "SplashScreen";
		((System.ComponentModel.ISupportInitialize)this.pictureBox1).EndInit();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
