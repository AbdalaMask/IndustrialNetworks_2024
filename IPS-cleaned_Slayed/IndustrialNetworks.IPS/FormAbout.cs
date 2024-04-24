using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using NetStudio.IPS.Properties;

namespace NetStudio.IPS;

internal class FormAbout : Form
{
	private IContainer components;

	private Label labelProductName;

	private Label labelVersion;

	private Label labelCopyright;

	private TextBox textBoxDescription;

	private Button okButton;

	private PictureBox pictureBox1;

	private Label label1;

	public string AssemblyTitle
	{
		get
		{
			object[] customAttributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), inherit: false);
			if (customAttributes.Length != 0)
			{
				AssemblyTitleAttribute assemblyTitleAttribute = (AssemblyTitleAttribute)customAttributes[0];
				if (assemblyTitleAttribute.Title != "")
				{
					return assemblyTitleAttribute.Title;
				}
			}
			return Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().CodeBase);
		}
	}

	public string AssemblyVersion => Assembly.GetExecutingAssembly().GetName().Version.ToString();

	public string AssemblyDescription
	{
		get
		{
			object[] customAttributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyDescriptionAttribute), inherit: false);
			if (customAttributes.Length == 0)
			{
				return "";
			}
			return ((AssemblyDescriptionAttribute)customAttributes[0]).Description;
		}
	}

	public string AssemblyProduct
	{
		get
		{
			object[] customAttributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyProductAttribute), inherit: false);
			if (customAttributes.Length == 0)
			{
				return "";
			}
			return ((AssemblyProductAttribute)customAttributes[0]).Product;
		}
	}

	public string AssemblyCopyright
	{
		get
		{
			object[] customAttributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), inherit: false);
			if (customAttributes.Length == 0)
			{
				return "";
			}
			return ((AssemblyCopyrightAttribute)customAttributes[0]).Copyright;
		}
	}

	public string AssemblyCompany
	{
		get
		{
			object[] customAttributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCompanyAttribute), inherit: false);
			if (customAttributes.Length == 0)
			{
				return "";
			}
			return ((AssemblyCompanyAttribute)customAttributes[0]).Company;
		}
	}

	public FormAbout()
	{
		InitializeComponent();
		Text = $"About {AssemblyTitle} - Demo version";
		labelProductName.Text = "Product: " + AssemblyProduct;
		labelVersion.Text = $"Version: {AssemblyVersion}";
		labelCopyright.Text = "Copyright: Industrial Networks";
		textBoxDescription.Text = AssemblyDescription;
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NetStudio.IPS.FormAbout));
		this.labelProductName = new System.Windows.Forms.Label();
		this.labelVersion = new System.Windows.Forms.Label();
		this.labelCopyright = new System.Windows.Forms.Label();
		this.textBoxDescription = new System.Windows.Forms.TextBox();
		this.okButton = new System.Windows.Forms.Button();
		this.pictureBox1 = new System.Windows.Forms.PictureBox();
		this.label1 = new System.Windows.Forms.Label();
		((System.ComponentModel.ISupportInitialize)this.pictureBox1).BeginInit();
		base.SuspendLayout();
		this.labelProductName.Location = new System.Drawing.Point(166, 6);
		this.labelProductName.Margin = new System.Windows.Forms.Padding(7, 0, 4, 0);
		this.labelProductName.MaximumSize = new System.Drawing.Size(0, 20);
		this.labelProductName.Name = "labelProductName";
		this.labelProductName.Size = new System.Drawing.Size(285, 20);
		this.labelProductName.TabIndex = 19;
		this.labelProductName.Text = "Product Name";
		this.labelProductName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.labelVersion.Location = new System.Drawing.Point(166, 37);
		this.labelVersion.Margin = new System.Windows.Forms.Padding(7, 0, 4, 0);
		this.labelVersion.MaximumSize = new System.Drawing.Size(0, 20);
		this.labelVersion.Name = "labelVersion";
		this.labelVersion.Size = new System.Drawing.Size(245, 20);
		this.labelVersion.TabIndex = 0;
		this.labelVersion.Text = "Version";
		this.labelVersion.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.labelCopyright.Location = new System.Drawing.Point(166, 69);
		this.labelCopyright.Margin = new System.Windows.Forms.Padding(7, 0, 4, 0);
		this.labelCopyright.MaximumSize = new System.Drawing.Size(0, 20);
		this.labelCopyright.Name = "labelCopyright";
		this.labelCopyright.Size = new System.Drawing.Size(285, 20);
		this.labelCopyright.TabIndex = 21;
		this.labelCopyright.Text = "Copyright";
		this.labelCopyright.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.textBoxDescription.Location = new System.Drawing.Point(7, 100);
		this.textBoxDescription.Margin = new System.Windows.Forms.Padding(7, 3, 4, 3);
		this.textBoxDescription.Multiline = true;
		this.textBoxDescription.Name = "textBoxDescription";
		this.textBoxDescription.ReadOnly = true;
		this.textBoxDescription.ScrollBars = System.Windows.Forms.ScrollBars.Both;
		this.textBoxDescription.Size = new System.Drawing.Size(444, 172);
		this.textBoxDescription.TabIndex = 23;
		this.textBoxDescription.TabStop = false;
		this.textBoxDescription.Text = "Description";
		this.okButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.okButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		this.okButton.Location = new System.Drawing.Point(381, 277);
		this.okButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
		this.okButton.Name = "okButton";
		this.okButton.Size = new System.Drawing.Size(70, 30);
		this.okButton.TabIndex = 25;
		this.okButton.Text = "&OK";
		this.pictureBox1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.pictureBox1.Image = NetStudio.IPS.Properties.Resources.Resources_512_tag;
		this.pictureBox1.Location = new System.Drawing.Point(7, 6);
		this.pictureBox1.Name = "pictureBox1";
		this.pictureBox1.Size = new System.Drawing.Size(149, 88);
		this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
		this.pictureBox1.TabIndex = 26;
		this.pictureBox1.TabStop = false;
		this.label1.Font = new System.Drawing.Font("Segoe UI Semibold", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
		this.label1.ForeColor = System.Drawing.Color.MidnightBlue;
		this.label1.Location = new System.Drawing.Point(7, 282);
		this.label1.Margin = new System.Windows.Forms.Padding(7, 0, 4, 0);
		this.label1.MaximumSize = new System.Drawing.Size(0, 20);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(280, 20);
		this.label1.TabIndex = 27;
		this.label1.Text = "MAKE IN VIETNAM";
		this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		base.AutoScaleDimensions = new System.Drawing.SizeF(7f, 15f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(458, 314);
		base.Controls.Add(this.label1);
		base.Controls.Add(this.pictureBox1);
		base.Controls.Add(this.labelProductName);
		base.Controls.Add(this.okButton);
		base.Controls.Add(this.labelVersion);
		base.Controls.Add(this.labelCopyright);
		base.Controls.Add(this.textBoxDescription);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
		base.Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
		base.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.Name = "FormAbout";
		base.Padding = new System.Windows.Forms.Padding(10);
		base.ShowInTaskbar = false;
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
		this.Text = "FormAbout";
		((System.ComponentModel.ISupportInitialize)this.pictureBox1).EndInit();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
