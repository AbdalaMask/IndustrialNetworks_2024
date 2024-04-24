using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using NetStudio.Common.DataTypes;
using NetStudio.Common.Files;
using NetStudio.Common.IndusCom;
using NetStudio.Common.Manager;
using NetStudio.DriverComm;
using NetStudio.DriverComm.Models;
using NetStudio.DriverManager;
using NetStudio.IPS.Controls;
using NetStudio.IPS.Entity;
using NetStudio.IPS.Monitor;
using NetStudio.IPS.Properties;

namespace NetStudio.IPS;

public class FormTagMonitor : Form
{
	private Channel? channel;

	private Device? device;

	private Group? group;

	private BindingSource sourceTags;

	private Tag? currentTag;

	private BindingSourceSync sourceLogs;

	private CancellationTokenSource _cts = new CancellationTokenSource();

	private TreeNode? rootNode;

	private TreeNode? channelNode;

	private TreeNode? deviceNode;

	private TreeNode? groupNode;

	private TreeNode? selectedNode;

	private IContainer components;

	private ImageList imageCollection;

	private SplitContainer splitContainer1;

	private TreeView treeViewMain;

	private ToolStripSeparator toolStripSeparator4;

	private ToolStripButton btnClearSearchBox;

	private ToolStripTextBox txtSearchBox;

	private ToolStripLabel lblSearchBox;

	private ToolStripLabel lblInfo;

	private ToolStripButton btnWrite;

	private ToolStripButton btnImportData;

	private ToolStrip toolBar;

	private DataGrid dgvTags;

	private ToolStripLabel lblTotalName;

	private SplitContainer splitContainer2;

	private DataGridLog dgvLogs;

	private ContextMenuStrip contextMenuLogs;

	private ToolStripMenuItem mnClearLog;

	private ToolStripMenuItem mnUnselect;

	private ToolStripButton btnDeviceMonitor;

	private ToolStripSeparator toolStripSeparator1;

	private ContextMenuStrip contextMenuTags;

	private ToolStripMenuItem mnON;

	private ToolStripMenuItem mnOFF;

	private ToolStripMenuItem mnWriteTag;

	private ToolStripButton btnExportData;

	private ToolStripSeparator toolStripSeparator3;

	private ToolStripLabel lblTotalTags;

	private ToolStripLabel lblTotalUnit;

	private ToolStripSeparator GroupLineHorizontal;

	private ToolStripLabel lblGroupName;

	private ToolStripLabel lblGroupTags;

	private ToolStripLabel lblGroupUnit;

	private ToolStripSeparator GroupLineVertical;

	private ToolStripButton btnRealTimeCharting;

	private ToolStripButton btnRestart;

	private ContextMenuStrip contextMenuTreeList;

	private ToolStripMenuItem mnExpandAll;

	private ToolStripMenuItem mnCollapseAll;

	private DataGridViewTextBoxColumn colID;

	private DataGridViewTextBoxColumn colTagName;

	private DataGridViewTextBoxColumn colFullName;

	private DataGridViewTextBoxColumn colAddress;

	private DataGridViewTextBoxColumn colDataType;

	private DataGridViewTextBoxColumn colValue;

	private DataGridViewTextBoxColumn colStatus;

	private DataGridViewTextBoxColumn colTime;

	private DataGridViewTextBoxColumn colMode;

	private DataGridViewTextBoxColumn colResolution;

	private DataGridViewCheckBoxColumn colIsScaling;

	private DataGridViewTextBoxColumn colAImin;

	private DataGridViewTextBoxColumn colAImax;

	private DataGridViewTextBoxColumn colRLmin;

	private DataGridViewTextBoxColumn colRLmax;

	private DataGridViewTextBoxColumn colOffset;

	private DataGridViewTextBoxColumn colDescription;

	private DataGridViewTextBoxColumn colTimeLog;

	private DataGridViewTextBoxColumn colCounter;

	private DataGridViewTextBoxColumn colEventType;

	private DataGridViewTextBoxColumn colSource;

	private DataGridViewTextBoxColumn colMessage;

	private DataGridViewTextBoxColumn LogType;

	public FormTagMonitor()
	{
		InitializeComponent();
		base.FormClosing += FormTagMonitor_FormClosing;
	}

	private async void FormTagMonitor_Load(object sender, EventArgs e)
	{
		try
		{
			await OnInitialize();
			Thread thread = new Thread(async delegate(object? obj)
			{
				await OnPolling((CancellationToken)obj);
			});
			thread.IsBackground = true;
			thread.Start(_cts.Token);
		}
		catch (Exception ex)
		{
			MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private async void FormTagMonitor_FormClosing(object? sender, FormClosingEventArgs e)
	{
		try
		{
			if (AppHelper.Settings.Mode)
			{
				_cts.Cancel();
				if (AppHelper.Driver != null)
				{
					await AppHelper.Driver.Disconnect(ClientHelper.ClientInfo);
				}
			}
			else if (DriverHelper.Protocol != null)
			{
				await DriverHelper.Protocol.StopAsync();
			}
		}
		catch (Exception ex)
		{
			MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private async Task OnInitialize()
	{
		try
		{
			await WaitFormManager.ShowAsync(this, "Loading...");
			sourceTags = new BindingSource();
			AppHelper.DataChanged = false;
			if (await DriverConnector())
			{
				LoadTreeList();
				dgvTags.ReadOnly = true;
				SetGroupTagVisible(visible: false);
				OptionsChanged();
				dgvTags.DataSource = sourceTags;
				dgvTags.DoubleClick += OnDataGridViewDoubleClick;
				dgvTags.SelectionChanged += OnDataGridViewSelectionChanged;
				dgvTags.CellFormatting += OnDataGridViewCellFormatting;
				dgvLogs.CellFormatting += OnDataGridLogCellFormatting;
				ClientDataSource.OnIpsLogChanged = (EventIpsLogChanged)Delegate.Combine(ClientDataSource.OnIpsLogChanged, new EventIpsLogChanged(OnIpsLogChanged));
				sourceLogs = new BindingSourceSync();
				sourceLogs.DataSource = (AppHelper.Settings.Mode ? ClientDataSource.Logs : DriverDataSource.Logs);
				dgvLogs.DataSource = sourceLogs;
			}
			await WaitFormManager.CloseAsync();
		}
		catch (Exception ex)
		{
			await WaitFormManager.CloseAsync();
			MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private async Task OnPolling(CancellationToken cancellationToken)
	{
		while (AppHelper.Settings.Mode && !cancellationToken.IsCancellationRequested)
		{
			if (await ClientHelper.GetConnectionStatusAsync() == CommunicationState.Closed)
			{
				try
				{
					await AppHelper.Driver.Connect(ClientHelper.ClientInfo);
				}
				catch (Exception)
				{
				}
			}
			Thread.Sleep(9000);
		}
	}

	private void OnIpsLogChanged(IpsLog? ipsLog_0)
	{
		sourceLogs.ResetBindings(metadataChanged: true);
	}

	private void btnDeviceMonitor_Click(object sender, EventArgs e)
	{
		try
		{
			FormDeviceMonitor formDeviceMonitor = new FormDeviceMonitor(AppHelper.Driver);
			formDeviceMonitor.StartPosition = FormStartPosition.CenterParent;
			formDeviceMonitor.ShowDialog();
		}
		catch (Exception ex)
		{
			MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private async void btnImportData_Click(object sender, EventArgs e)
	{
		try
		{
			if (device != null && device.Groups.Count > 0)
			{
				OpenFileDialog openFile = new OpenFileDialog
				{
					Multiselect = false,
					Title = "Import data",
					CheckFileExists = true,
					CheckPathExists = true,
					DefaultExt = "data",
					Filter = "Data files (*.Data)|*.data",
					FilterIndex = 1,
					RestoreDirectory = true
				};
				if (openFile.ShowDialog() == DialogResult.OK)
				{
					await WaitFormManager.ShowAsync(this, "Importing...");
					RtDevice rtDevice = new XmlObjectManager().Read<RtDevice>(openFile.FileName);
					StringBuilder builder = new StringBuilder();
					int count = 0;
					if (rtDevice != null && rtDevice.RtGroups.Count > 0 && rtDevice.ChannelId == device.ChannelId && rtDevice.Id == device.Id && rtDevice.Name == device.Name)
					{
						foreach (RtGroup rtGroup in rtDevice.RtGroups)
						{
							foreach (RtTag tag in rtGroup.RtTags)
							{
								IPSResult iPSResult = await AppHelper.Driver.WriteTag(tag.TagName, (object)tag.Value);
								if (iPSResult.Status != CommStatus.Success)
								{
									StringBuilder stringBuilder = builder;
									StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(4, 3, stringBuilder);
									int num = count + 1;
									count = num;
									handler.AppendFormatted(num);
									handler.AppendLiteral(". ");
									handler.AppendFormatted(tag.TagName);
									handler.AppendLiteral(": ");
									handler.AppendFormatted(iPSResult.Message);
									stringBuilder.AppendLine(ref handler);
								}
							}
						}
						await WaitFormManager.CloseAsync();
						((FormMain)base.Parent.Parent).Activate();
						if (builder.Length > 5)
						{
							MessageBox.Show(this, builder.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
						}
						else
						{
							MessageBox.Show(this, "Imported data successfully!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
						}
					}
					else
					{
						MessageBox.Show(this, "The data is not from the device you selected.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
					}
				}
			}
			else
			{
				MessageBox.Show(this, "Please select a device.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
			}
			await Task.CompletedTask;
		}
		catch (Exception ex)
		{
			await WaitFormManager.CloseAsync();
			Activate();
			MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private async void btnExportData_Click(object sender, EventArgs e)
	{
		try
		{
			if (device != null && device.Groups.Count > 0)
			{
				SaveFileDialog saveFileDialog = new SaveFileDialog
				{
					Title = "Export data",
					CheckPathExists = true,
					DefaultExt = "data",
					Filter = "Data files (*.Data)|*.data",
					FilterIndex = 1,
					RestoreDirectory = true
				};
				saveFileDialog.FileName = device.Name ?? "";
				if (saveFileDialog.ShowDialog() != DialogResult.OK)
				{
					return;
				}
				await WaitFormManager.ShowAsync(this, "Exporting...");
				RtDevice rtDevice = new RtDevice
				{
					ChannelId = device.ChannelId,
					Id = device.Id,
					Name = device.Name,
					Active = device.Active,
					StationNo = device.StationNo,
					Description = device.Description,
					Adapter = device.Adapter
				};
				new DataMonitor();
				foreach (Group group in device.Groups)
				{
					RtGroup rtGroup = new RtGroup
					{
						ChannelId = group.ChannelId,
						DeviceId = group.DeviceId,
						Id = group.Id,
						Name = group.Name,
						Description = group.Description
					};
					rtDevice.RtGroups.Add(rtGroup);
					foreach (Tag tag in group.Tags)
					{
						RtTag item = new RtTag
						{
							Id = tag.Id,
							TagName = tag.FullName,
							Address = tag.Address,
							DataType = tag.DataType,
							Value = tag.Value,
							Description = tag.Description
						};
						rtGroup.RtTags.Add(item);
					}
				}
				new XmlObjectManager().Write(saveFileDialog.FileName, rtDevice);
				await WaitFormManager.CloseAsync();
				((FormMain)base.Parent.Parent).Activate();
				MessageBox.Show(this, "Export data successfully!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
				return;
			}
			MessageBox.Show(this, "Please select a device.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
		}
		catch (Exception ex)
		{
			await WaitFormManager.CloseAsync();
			MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private void SetGroupTagVisible(bool visible)
	{
		lblGroupName.Visible = visible;
		lblGroupTags.Visible = visible;
		lblGroupUnit.Visible = visible;
		GroupLineVertical.Visible = visible;
	}

	private void searchBox_TextChanged(object sender, EventArgs e)
	{
		if (group != null)
		{
			List<Tag> list = group.Tags.FindAll((Tag tg) => tg.Name.Contains(txtSearchBox.Text, StringComparison.OrdinalIgnoreCase) || tg.Address.Contains(txtSearchBox.Text, StringComparison.OrdinalIgnoreCase) || tg.DataType.ToString().Contains(txtSearchBox.Text, StringComparison.OrdinalIgnoreCase) || tg.Mode.ToString().Contains(txtSearchBox.Text, StringComparison.OrdinalIgnoreCase) || tg.IsScaling.ToString().Contains(txtSearchBox.Text, StringComparison.OrdinalIgnoreCase) || tg.Id.ToString().Contains(txtSearchBox.Text) || tg.AImin.ToString().Contains(txtSearchBox.Text) || tg.AImax.ToString().Contains(txtSearchBox.Text) || tg.RLmin.ToString().Contains(txtSearchBox.Text) || tg.RLmax.ToString().Contains(txtSearchBox.Text) || tg.Offset.ToString().Contains(txtSearchBox.Text) || (tg.Description ?? "").Contains(txtSearchBox.Text));
			sourceTags.DataSource = new BindingList<Tag>(list);
		}
	}

	private void btnClearSearchBox_Click(object sender, EventArgs e)
	{
		try
		{
			txtSearchBox.Text = string.Empty;
		}
		catch (Exception ex)
		{
			MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private async void treeViewMain_AfterSelect(object sender, TreeViewEventArgs e)
	{
		try
		{
			channelNode = null;
			deviceNode = null;
			groupNode = null;
			TreeView treeView = (TreeView)sender;
			selectedNode = treeView.SelectedNode;
			switch (treeViewMain.SelectedNode.Level)
			{
			case 0:
				rootNode = selectedNode;
				break;
			case 1:
				channelNode = selectedNode;
				rootNode = channelNode.Parent;
				break;
			case 2:
				deviceNode = selectedNode;
				channelNode = deviceNode.Parent;
				rootNode = channelNode.Parent;
				break;
			case 3:
				groupNode = selectedNode;
				deviceNode = groupNode.Parent;
				channelNode = deviceNode.Parent;
				rootNode = channelNode.Parent;
				break;
			}
			await SearchData();
		}
		catch (Exception ex)
		{
			MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private async Task SearchData()
	{
		try
		{
			if (ClientDataSource.Channels == null)
			{
				return;
			}
			channel = null;
			device = null;
			group = null;
			switch (treeViewMain.SelectedNode.Level)
			{
			default:
				lblInfo.Text = string.Empty;
				if (lblGroupTags.Visible)
				{
					SetGroupTagVisible(visible: false);
				}
				break;
			case 0:
				if (lblGroupTags.Visible)
				{
					SetGroupTagVisible(visible: false);
				}
				Invoke(() => lblInfo.Text = "Industrial Protocols");
				break;
			case 1:
				if (lblGroupTags.Visible)
				{
					SetGroupTagVisible(visible: false);
				}
				if (channelNode == null)
				{
					break;
				}
				channel = ClientDataSource.Channels.FirstOrDefault((Channel channel_0) => channel_0.Name.ToLower() == channelNode.Text.ToLower());
				if (channel != null)
				{
					Invoke(delegate
					{
						ToolStripLabel toolStripLabel = lblInfo;
						string obj = channel.GetInfos() ?? "";
						string result = obj;
						toolStripLabel.Text = obj;
						return result;
					});
				}
				break;
			case 2:
				if (lblGroupTags.Visible)
				{
					SetGroupTagVisible(visible: false);
				}
				if (channelNode == null || deviceNode == null)
				{
					break;
				}
				channel = ClientDataSource.Channels.FirstOrDefault((Channel channel_0) => channel_0.Name.ToLower() == channelNode.Text.ToLower());
				if (channel == null)
				{
					break;
				}
				device = channel.Devices.Where((Device device_0) => device_0.Name.Equals(deviceNode.Text)).FirstOrDefault();
				if (device != null)
				{
					Invoke(() => lblInfo.Text = channel.Name + "/" + device.GetInfos());
				}
				break;
			case 3:
				if (channelNode == null || deviceNode == null || groupNode == null)
				{
					break;
				}
				try
				{
					await WaitFormManager.ShowAsync(this, "Loading...");
					channel = ClientDataSource.Channels.FirstOrDefault((Channel channel_0) => channel_0.Name.ToLower() == channelNode.Text.ToLower());
					if (channel != null)
					{
						device = channel.Devices.Where((Device device_0) => device_0.Name.Equals(deviceNode.Text)).FirstOrDefault();
						if (device != null)
						{
							group = device.Groups.Where((Group group_0) => group_0.Name.Equals(groupNode.Text)).FirstOrDefault();
							if (group != null)
							{
								if (group.Tags.Count == 0)
								{
									Invoke(() => lblInfo.Text = $"Group: {channel.Name}/{device.Name}/{group.Name}");
								}
								else if (char.IsLetter(group.Tags[0].Address[0]))
								{
									Invoke(delegate
									{
										colAddress.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
										return DataGridViewContentAlignment.MiddleLeft;
									});
								}
								sourceTags.DataSource = new BindingListSync<Tag>(group.Tags);
								lblGroupTags.Text = $"{group.Tags.Count}";
								if (!lblGroupTags.Visible)
								{
									SetGroupTagVisible(visible: true);
								}
								if (channel.Manufacturer == Manufacturer.PANASONIC)
								{
									colAddress.HeaderText = "FP Address";
								}
								else
								{
									colAddress.HeaderText = "Address";
								}
							}
						}
					}
					await WaitFormManager.CloseAsync();
				}
				catch (Exception ex)
				{
					await WaitFormManager.CloseAsync();
					MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
				}
				break;
			}
			if (treeViewMain.SelectedNode.Level < 3)
			{
				sourceTags.DataSource = new BindingList<Tag>(new List<Tag>());
			}
		}
		catch (Exception ex2)
		{
			MessageBox.Show(this, ex2.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	public void LoadTreeList()
	{
		try
		{
			int num = 0;
			treeViewMain.Nodes.Clear();
			treeViewMain.ImageList = imageCollection;
			treeViewMain.SelectedNode = null;
			TreeNode treeNode = new TreeNode("Industrial Protocols");
			treeViewMain.Nodes.Add(treeNode);
			if (ClientDataSource.Channels != null && ClientDataSource.Channels.Count > 0)
			{
				foreach (Channel channel in ClientDataSource.Channels)
				{
					TreeNode treeNode2 = new TreeNode(channel.Name)
					{
						ImageIndex = 1,
						SelectedImageIndex = 1,
						Tag = channel.Id
					};
					treeNode.Nodes.Add(treeNode2);
					if (channel.Devices == null || !channel.Devices.Any())
					{
						continue;
					}
					foreach (Device device in channel.Devices)
					{
						TreeNode treeNode3 = new TreeNode(device.Name)
						{
							ImageIndex = 2,
							SelectedImageIndex = 2,
							Tag = device.Id
						};
						treeNode2.Nodes.Add(treeNode3);
						foreach (Group group in device.Groups)
						{
							TreeNode node = new TreeNode(group.Name)
							{
								ImageIndex = 3,
								SelectedImageIndex = 3,
								Tag = group.Id
							};
							treeNode3.Nodes.Add(node);
							num += group.Tags.Count;
						}
					}
				}
				treeViewMain.ExpandAll();
			}
			lblTotalTags.Text = $"{num}";
		}
		catch (Exception ex)
		{
			MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private async Task<bool> DriverConnector()
	{
		try
		{
			ClientDataSource.ResetAll();
			if (AppHelper.Settings.Mode)
			{
				ClientDataSource.Channels = AppHelper.Driver.GetChannels() ?? new List<Channel>();
			}
			else
			{
				DriverHelper.Protocol = new ProtocolManager(new AppSettings
				{
					Settings = new NetStudio.Common.Manager.Settings
					{
						Directory = AppHelper.Settings.Directory,
						FileName = AppHelper.Settings.FileName
					}
				});
				await DriverHelper.Protocol.OnLoadProject();
				ClientDataSource.Channels = DriverHelper.Protocol.GetChannels();
			}
			if (ClientDataSource.Channels != null && ClientDataSource.Channels.Any())
			{
				foreach (Channel channel in ClientDataSource.Channels)
				{
					foreach (Device device in channel.Devices)
					{
						ClientDataSource.Devices.Add($"{device.ChannelId}.{device.Name}", device);
						foreach (Group group in device.Groups)
						{
							foreach (Tag tag in group.Tags)
							{
								tag.FullName = $"{channel.Name}.{device.Name}.{group.Name}.{tag.Name}";
								switch (tag.DataType)
								{
								case DataType.BOOL:
									tag.Value = false;
									break;
								case DataType.BYTE:
								case DataType.INT:
								case DataType.UINT:
								case DataType.WORD:
								case DataType.DINT:
								case DataType.UDINT:
								case DataType.DWORD:
								case DataType.REAL:
								case DataType.LINT:
								case DataType.ULINT:
								case DataType.LWORD:
								case DataType.LREAL:
									tag.Value = 0;
									break;
								case DataType.TIME16:
								case DataType.TIME32:
									tag.Value = 0;
									break;
								case DataType.STRING:
									tag.Value = string.Empty;
									break;
								}
								ClientDataSource.Tags.Add(tag.FullName, tag);
							}
						}
					}
				}
			}
			if (!AppHelper.Settings.Mode)
			{
				await DriverHelper.Protocol.StartAsync();
			}
			else
			{
				ClientHelper.Status = CommunicationState.Closed;
				if (!(await AppHelper.Driver.Connect(ClientHelper.ClientInfo)))
				{
					MessageBox.Show(this, "Connection to driver server: failed.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
				}
			}
			return true;
		}
		catch (Exception)
		{
		}
		return false;
	}

	private void OnDataGridViewSelectionChanged(object? sender, EventArgs e)
	{
		try
		{
			currentTag = sourceTags.Current as Tag;
			if (currentTag != null)
			{
				lblInfo.Text = $"Tag: {channel.Name}/{device.Name}/{group.Name}/{currentTag.Name}";
				SetEnableMenuOnOff(currentTag.DataType == DataType.BOOL);
			}
		}
		catch (Exception ex)
		{
			MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private void OnDataGridViewDoubleClick(object? sender, EventArgs e)
	{
		try
		{
			if (group == null || group.Tags.Count <= 0 || currentTag == null)
			{
				return;
			}
			FormWriteTag obj = new FormWriteTag(currentTag)
			{
				StartPosition = FormStartPosition.CenterParent
			};
			obj.OnWriteTagChanged = (EventWriteTagChanged)Delegate.Combine(obj.OnWriteTagChanged, (EventWriteTagChanged)delegate
			{
				try
				{
					dgvTags.RefreshEdit();
				}
				catch (Exception ex2)
				{
					MessageBox.Show(this, ex2.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
				}
			});
			obj.ShowDialog();
		}
		catch (Exception ex)
		{
			MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private void OnDataGridViewCellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
	{
		try
		{
			if (e.ColumnIndex < 0)
			{
				return;
			}
			if (dgvTags.Columns[e.ColumnIndex].Name == "colValue")
			{
				if (e.Value == null)
				{
					return;
				}
				if (group != null && group.Tags[e.RowIndex].DataType == DataType.BOOL)
				{
					if ($"{e.Value}".Equals("True"))
					{
						e.CellStyle.ForeColor = Color.Green;
					}
					else
					{
						e.CellStyle.ForeColor = Color.Red;
					}
				}
				else
				{
					e.CellStyle.ForeColor = Color.Navy;
				}
			}
			else if (dgvTags.Columns[e.ColumnIndex].Name == "colStatus" && e.Value != null && e.CellStyle != null)
			{
				if ((TagStatus)e.Value == TagStatus.Bad)
				{
					e.CellStyle.ForeColor = Color.White;
					e.CellStyle.BackColor = Color.Red;
				}
				else
				{
					e.CellStyle.BackColor = Color.LightGreen;
					e.CellStyle.ForeColor = Color.Blue;
				}
			}
		}
		catch (Exception ex)
		{
			MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private void SetEnableMenuOnOff(bool enabled)
	{
		mnON.Visible = enabled;
		mnOFF.Visible = enabled;
		mnWriteTag.Visible = !enabled;
	}

	private async void mnON_Click(object sender, EventArgs e)
	{
		try
		{
			if (currentTag == null || currentTag.DataType != 0)
			{
				return;
			}
			IPSResult iPSResult = (AppHelper.Settings.Mode ? (await AppHelper.Driver.WriteTag(currentTag.FullName, (object)true)) : (await DriverHelper.Protocol.WriteTagAsync(currentTag.FullName, true)));
			if (iPSResult != null)
			{
				if (iPSResult.Status == CommStatus.Success)
				{
					dgvTags.RefreshEdit();
				}
				else
				{
					MessageBox.Show(this, iPSResult.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
				}
			}
		}
		catch (Exception ex)
		{
			MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private async void mnOFF_Click(object sender, EventArgs e)
	{
		try
		{
			if (currentTag == null || currentTag.DataType != 0)
			{
				return;
			}
			IPSResult iPSResult = (AppHelper.Settings.Mode ? (await AppHelper.Driver.WriteTag(currentTag.FullName, (object)false)) : (await DriverHelper.Protocol.WriteTagAsync(currentTag.FullName, false)));
			if (iPSResult != null)
			{
				if (iPSResult.Status == CommStatus.Success)
				{
					dgvTags.RefreshEdit();
				}
				else
				{
					MessageBox.Show(this, iPSResult.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
				}
			}
		}
		catch (Exception ex)
		{
			MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	public void OptionsChanged()
	{
		bool scalingColumn = NetStudio.IPS.Properties.Settings.Default.ScalingColumn;
		colIsScaling.Visible = scalingColumn;
		colAImax.Visible = scalingColumn;
		colAImin.Visible = scalingColumn;
		colRLmax.Visible = scalingColumn;
		colRLmin.Visible = scalingColumn;
		colIsScaling.Visible = scalingColumn;
		colOffset.Visible = NetStudio.IPS.Properties.Settings.Default.OffsetColumn;
		colResolution.Visible = NetStudio.IPS.Properties.Settings.Default.ResolutionColumn;
	}

	private async void mnClearLog_Click(object sender, EventArgs e)
	{
		try
		{
			if ((await AppHelper.Driver.ClearLog()).Success)
			{
				ClientDataSource.Logs.Clear();
			}
		}
		catch (Exception ex)
		{
			MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private void mnUnselect_Click(object sender, EventArgs e)
	{
		try
		{
			dgvLogs.ClearSelection();
		}
		catch (Exception ex)
		{
			MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private void OnDataGridLogCellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
	{
		try
		{
			if (e.ColumnIndex < 0 || ClientDataSource.Logs.Count == 0 || e.RowIndex >= ClientDataSource.Logs.Count || ClientDataSource.Logs[e.RowIndex] == null)
			{
				return;
			}
			string name = dgvLogs.Columns[e.ColumnIndex].Name;
			if (!(name == "colEventType"))
			{
				if (name == "colTimeLog")
				{
					e.Value = ClientDataSource.Logs[e.RowIndex].Time.ToString("MM/dd/yyyy HH:mm:ss");
				}
				return;
			}
			switch (ClientDataSource.Logs[e.RowIndex].EvenType)
			{
			case EvenType.Information:
				e.CellStyle.ForeColor = Color.Black;
				break;
			case EvenType.Error:
				e.CellStyle.ForeColor = Color.Red;
				break;
			case EvenType.Warning:
				e.CellStyle.ForeColor = Color.DarkOrange;
				break;
			}
		}
		catch (Exception ex)
		{
			MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private void btnRealTimeCharting_Click(object sender, EventArgs e)
	{
		try
		{
			FormRealTimeChart formRealTimeChart = new FormRealTimeChart();
			formRealTimeChart.Name = "RealTimeCharting";
			formRealTimeChart.WindowState = FormWindowState.Maximized;
			formRealTimeChart.ShowDialog();
		}
		catch (Exception ex)
		{
			MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private async void btnRestart_Click(object sender, EventArgs e)
	{
		if (!AppHelper.Settings.Mode)
		{
			MessageBox.Show(this, "Local mode is not supported for this function", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			return;
		}
		try
		{
			await WaitFormManager.ShowAsync(this, "Restarting the driver server...");
			ApiResponse response = await AppHelper.Driver.Restart();
			if (!response.Success)
			{
				throw new Exception(response.Message);
			}
			await WaitFormManager.CloseAsync();
			MessageBox.Show(this, response.Message, "Information", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
			Close();
		}
		catch (Exception ex)
		{
			await WaitFormManager.CloseAsync();
			MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private void mnExpandAll_Click(object sender, EventArgs e)
	{
		try
		{
			treeViewMain.ExpandAll();
		}
		catch (Exception ex)
		{
			MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private void mnCollapseAll_Click(object sender, EventArgs e)
	{
		try
		{
			treeViewMain.CollapseAll();
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
		this.components = new System.ComponentModel.Container();
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NetStudio.IPS.FormTagMonitor));
		System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle = new System.Windows.Forms.DataGridViewCellStyle();
		System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
		System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
		System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
		System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
		System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
		System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle7 = new System.Windows.Forms.DataGridViewCellStyle();
		System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle8 = new System.Windows.Forms.DataGridViewCellStyle();
		System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle9 = new System.Windows.Forms.DataGridViewCellStyle();
		System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle10 = new System.Windows.Forms.DataGridViewCellStyle();
		System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle11 = new System.Windows.Forms.DataGridViewCellStyle();
		System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle12 = new System.Windows.Forms.DataGridViewCellStyle();
		System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle13 = new System.Windows.Forms.DataGridViewCellStyle();
		System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle14 = new System.Windows.Forms.DataGridViewCellStyle();
		System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle15 = new System.Windows.Forms.DataGridViewCellStyle();
		System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle16 = new System.Windows.Forms.DataGridViewCellStyle();
		System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle17 = new System.Windows.Forms.DataGridViewCellStyle();
		System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle18 = new System.Windows.Forms.DataGridViewCellStyle();
		System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle19 = new System.Windows.Forms.DataGridViewCellStyle();
		System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle20 = new System.Windows.Forms.DataGridViewCellStyle();
		System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle21 = new System.Windows.Forms.DataGridViewCellStyle();
		System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle22 = new System.Windows.Forms.DataGridViewCellStyle();
		System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle23 = new System.Windows.Forms.DataGridViewCellStyle();
		this.imageCollection = new System.Windows.Forms.ImageList(this.components);
		this.splitContainer1 = new System.Windows.Forms.SplitContainer();
		this.splitContainer2 = new System.Windows.Forms.SplitContainer();
		this.dgvTags = new NetStudio.IPS.Controls.DataGrid();
		this.colID = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.colTagName = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.colFullName = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.colAddress = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.colDataType = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.colValue = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.colStatus = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.colTime = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.colMode = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.colResolution = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.colIsScaling = new System.Windows.Forms.DataGridViewCheckBoxColumn();
		this.colAImin = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.colAImax = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.colRLmin = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.colRLmax = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.colOffset = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.colDescription = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.contextMenuTags = new System.Windows.Forms.ContextMenuStrip(this.components);
		this.mnON = new System.Windows.Forms.ToolStripMenuItem();
		this.mnOFF = new System.Windows.Forms.ToolStripMenuItem();
		this.mnWriteTag = new System.Windows.Forms.ToolStripMenuItem();
		this.dgvLogs = new NetStudio.IPS.Controls.DataGridLog();
		this.contextMenuLogs = new System.Windows.Forms.ContextMenuStrip(this.components);
		this.mnClearLog = new System.Windows.Forms.ToolStripMenuItem();
		this.mnUnselect = new System.Windows.Forms.ToolStripMenuItem();
		this.treeViewMain = new System.Windows.Forms.TreeView();
		this.contextMenuTreeList = new System.Windows.Forms.ContextMenuStrip(this.components);
		this.mnExpandAll = new System.Windows.Forms.ToolStripMenuItem();
		this.mnCollapseAll = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
		this.btnClearSearchBox = new System.Windows.Forms.ToolStripButton();
		this.txtSearchBox = new System.Windows.Forms.ToolStripTextBox();
		this.lblSearchBox = new System.Windows.Forms.ToolStripLabel();
		this.lblInfo = new System.Windows.Forms.ToolStripLabel();
		this.btnWrite = new System.Windows.Forms.ToolStripButton();
		this.btnImportData = new System.Windows.Forms.ToolStripButton();
		this.toolBar = new System.Windows.Forms.ToolStrip();
		this.btnDeviceMonitor = new System.Windows.Forms.ToolStripButton();
		this.btnRealTimeCharting = new System.Windows.Forms.ToolStripButton();
		this.btnRestart = new System.Windows.Forms.ToolStripButton();
		this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
		this.btnExportData = new System.Windows.Forms.ToolStripButton();
		this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
		this.lblTotalName = new System.Windows.Forms.ToolStripLabel();
		this.lblTotalTags = new System.Windows.Forms.ToolStripLabel();
		this.lblTotalUnit = new System.Windows.Forms.ToolStripLabel();
		this.GroupLineHorizontal = new System.Windows.Forms.ToolStripSeparator();
		this.lblGroupName = new System.Windows.Forms.ToolStripLabel();
		this.lblGroupTags = new System.Windows.Forms.ToolStripLabel();
		this.lblGroupUnit = new System.Windows.Forms.ToolStripLabel();
		this.GroupLineVertical = new System.Windows.Forms.ToolStripSeparator();
		this.colTimeLog = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.colCounter = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.colEventType = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.colSource = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.colMessage = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.LogType = new System.Windows.Forms.DataGridViewTextBoxColumn();
		((System.ComponentModel.ISupportInitialize)this.splitContainer1).BeginInit();
		this.splitContainer1.Panel1.SuspendLayout();
		this.splitContainer1.Panel2.SuspendLayout();
		this.splitContainer1.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.splitContainer2).BeginInit();
		this.splitContainer2.Panel1.SuspendLayout();
		this.splitContainer2.Panel2.SuspendLayout();
		this.splitContainer2.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.dgvTags).BeginInit();
		this.contextMenuTags.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.dgvLogs).BeginInit();
		this.contextMenuLogs.SuspendLayout();
		this.contextMenuTreeList.SuspendLayout();
		this.toolBar.SuspendLayout();
		base.SuspendLayout();
		this.imageCollection.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
		this.imageCollection.ImageStream = (System.Windows.Forms.ImageListStreamer)resources.GetObject("imageCollection.ImageStream");
		this.imageCollection.TransparentColor = System.Drawing.Color.Transparent;
		this.imageCollection.Images.SetKeyName(0, "connection.png");
		this.imageCollection.Images.SetKeyName(1, "channel.png");
		this.imageCollection.Images.SetKeyName(2, "device.png");
		this.imageCollection.Images.SetKeyName(3, "group.png");
		this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
		this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
		this.splitContainer1.Location = new System.Drawing.Point(0, 25);
		this.splitContainer1.Name = "splitContainer1";
		this.splitContainer1.Panel1.Controls.Add(this.splitContainer2);
		this.splitContainer1.Panel2.Controls.Add(this.treeViewMain);
		this.splitContainer1.Size = new System.Drawing.Size(1161, 678);
		this.splitContainer1.SplitterDistance = 904;
		this.splitContainer1.TabIndex = 6;
		this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
		this.splitContainer2.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
		this.splitContainer2.Location = new System.Drawing.Point(0, 0);
		this.splitContainer2.Name = "splitContainer2";
		this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
		this.splitContainer2.Panel1.Controls.Add(this.dgvTags);
		this.splitContainer2.Panel2.Controls.Add(this.dgvLogs);
		this.splitContainer2.Size = new System.Drawing.Size(904, 678);
		this.splitContainer2.SplitterDistance = 508;
		this.splitContainer2.TabIndex = 5;
		this.dgvTags.AllowUserToAddRows = false;
		dataGridViewCellStyle.BackColor = System.Drawing.Color.DarkGray;
		this.dgvTags.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle;
		this.dgvTags.BackgroundColor = System.Drawing.Color.LightGray;
		this.dgvTags.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		dataGridViewCellStyle2.BackColor = System.Drawing.Color.Black;
		dataGridViewCellStyle2.Font = new System.Drawing.Font("Segoe UI", 9f);
		dataGridViewCellStyle2.ForeColor = System.Drawing.Color.White;
		dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
		dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
		dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
		this.dgvTags.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
		this.dgvTags.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
		this.dgvTags.Columns.AddRange(this.colID, this.colTagName, this.colFullName, this.colAddress, this.colDataType, this.colValue, this.colStatus, this.colTime, this.colMode, this.colResolution, this.colIsScaling, this.colAImin, this.colAImax, this.colRLmin, this.colRLmax, this.colOffset, this.colDescription);
		this.dgvTags.ContextMenuStrip = this.contextMenuTags;
		dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
		dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Window;
		dataGridViewCellStyle3.Font = new System.Drawing.Font("Segoe UI", 9f);
		dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.ControlText;
		dataGridViewCellStyle3.SelectionBackColor = System.Drawing.Color.LightGoldenrodYellow;
		dataGridViewCellStyle3.SelectionForeColor = System.Drawing.Color.Black;
		dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
		this.dgvTags.DefaultCellStyle = dataGridViewCellStyle3;
		this.dgvTags.Dock = System.Windows.Forms.DockStyle.Fill;
		this.dgvTags.Location = new System.Drawing.Point(0, 0);
		this.dgvTags.MultiSelect = false;
		this.dgvTags.Name = "dgvTags";
		this.dgvTags.ReadOnly = true;
		dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
		dataGridViewCellStyle4.BackColor = System.Drawing.Color.Black;
		dataGridViewCellStyle4.Font = new System.Drawing.Font("Segoe UI", 9f);
		dataGridViewCellStyle4.ForeColor = System.Drawing.SystemColors.WindowText;
		dataGridViewCellStyle4.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
		dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
		this.dgvTags.RowHeadersDefaultCellStyle = dataGridViewCellStyle4;
		dataGridViewCellStyle5.BackColor = System.Drawing.Color.LightGray;
		this.dgvTags.RowsDefaultCellStyle = dataGridViewCellStyle5;
		this.dgvTags.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
		this.dgvTags.Size = new System.Drawing.Size(904, 508);
		this.dgvTags.TabIndex = 4;
		this.dgvTags.VirtualMode = true;
		this.colID.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
		this.colID.DataPropertyName = "ID";
		dataGridViewCellStyle6.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
		this.colID.DefaultCellStyle = dataGridViewCellStyle6;
		this.colID.HeaderText = "ID";
		this.colID.Name = "colID";
		this.colID.ReadOnly = true;
		this.colID.Visible = false;
		this.colTagName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
		this.colTagName.DataPropertyName = "Name";
		this.colTagName.HeaderText = "Tag name";
		this.colTagName.Name = "colTagName";
		this.colTagName.ReadOnly = true;
		this.colTagName.Width = 83;
		this.colFullName.DataPropertyName = "FullName";
		this.colFullName.HeaderText = "Full Name";
		this.colFullName.Name = "colFullName";
		this.colFullName.ReadOnly = true;
		this.colFullName.Visible = false;
		this.colAddress.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
		this.colAddress.DataPropertyName = "Address";
		dataGridViewCellStyle7.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
		this.colAddress.DefaultCellStyle = dataGridViewCellStyle7;
		this.colAddress.HeaderText = "Address";
		this.colAddress.Name = "colAddress";
		this.colAddress.ReadOnly = true;
		this.colAddress.Width = 74;
		this.colDataType.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
		this.colDataType.DataPropertyName = "DataType";
		dataGridViewCellStyle8.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
		this.colDataType.DefaultCellStyle = dataGridViewCellStyle8;
		this.colDataType.HeaderText = "Data type";
		this.colDataType.Name = "colDataType";
		this.colDataType.ReadOnly = true;
		this.colDataType.Width = 82;
		this.colValue.DataPropertyName = "Value";
		dataGridViewCellStyle9.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
		this.colValue.DefaultCellStyle = dataGridViewCellStyle9;
		this.colValue.HeaderText = "Value";
		this.colValue.MinimumWidth = 120;
		this.colValue.Name = "colValue";
		this.colValue.ReadOnly = true;
		this.colValue.Width = 120;
		this.colStatus.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
		this.colStatus.DataPropertyName = "Status";
		dataGridViewCellStyle10.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
		this.colStatus.DefaultCellStyle = dataGridViewCellStyle10;
		this.colStatus.HeaderText = "Status";
		this.colStatus.Name = "colStatus";
		this.colStatus.ReadOnly = true;
		this.colStatus.Width = 64;
		this.colTime.DataPropertyName = "Time";
		dataGridViewCellStyle11.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
		this.colTime.DefaultCellStyle = dataGridViewCellStyle11;
		this.colTime.HeaderText = "Time";
		this.colTime.MinimumWidth = 100;
		this.colTime.Name = "colTime";
		this.colTime.ReadOnly = true;
		this.colMode.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
		this.colMode.DataPropertyName = "ModeName";
		dataGridViewCellStyle12.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
		this.colMode.DefaultCellStyle = dataGridViewCellStyle12;
		this.colMode.HeaderText = "Access";
		this.colMode.MinimumWidth = 90;
		this.colMode.Name = "colMode";
		this.colMode.ReadOnly = true;
		this.colMode.Resizable = System.Windows.Forms.DataGridViewTriState.True;
		this.colMode.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
		this.colMode.Width = 90;
		this.colResolution.DataPropertyName = "Resolution";
		dataGridViewCellStyle13.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
		this.colResolution.DefaultCellStyle = dataGridViewCellStyle13;
		this.colResolution.HeaderText = "Resolution";
		this.colResolution.MinimumWidth = 100;
		this.colResolution.Name = "colResolution";
		this.colResolution.ReadOnly = true;
		this.colIsScaling.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
		this.colIsScaling.DataPropertyName = "IsScaling";
		this.colIsScaling.HeaderText = "Scaling";
		this.colIsScaling.Name = "colIsScaling";
		this.colIsScaling.ReadOnly = true;
		this.colIsScaling.Resizable = System.Windows.Forms.DataGridViewTriState.True;
		this.colIsScaling.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
		this.colIsScaling.Width = 70;
		this.colAImin.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
		this.colAImin.DataPropertyName = "AImin";
		dataGridViewCellStyle14.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
		this.colAImin.DefaultCellStyle = dataGridViewCellStyle14;
		this.colAImin.HeaderText = "AI min";
		this.colAImin.Name = "colAImin";
		this.colAImin.ReadOnly = true;
		this.colAImin.Width = 67;
		this.colAImax.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
		this.colAImax.DataPropertyName = "AImax";
		dataGridViewCellStyle15.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
		this.colAImax.DefaultCellStyle = dataGridViewCellStyle15;
		this.colAImax.HeaderText = "AI max";
		this.colAImax.Name = "colAImax";
		this.colAImax.ReadOnly = true;
		this.colAImax.Width = 69;
		this.colRLmin.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
		this.colRLmin.DataPropertyName = "RLmin";
		dataGridViewCellStyle16.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
		this.colRLmin.DefaultCellStyle = dataGridViewCellStyle16;
		this.colRLmin.HeaderText = "Real min";
		this.colRLmin.Name = "colRLmin";
		this.colRLmin.ReadOnly = true;
		this.colRLmin.Width = 78;
		this.colRLmax.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
		this.colRLmax.DataPropertyName = "RLmax";
		dataGridViewCellStyle17.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
		this.colRLmax.DefaultCellStyle = dataGridViewCellStyle17;
		this.colRLmax.HeaderText = "Real max";
		this.colRLmax.Name = "colRLmax";
		this.colRLmax.ReadOnly = true;
		this.colRLmax.Width = 80;
		this.colOffset.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
		this.colOffset.DataPropertyName = "Offset";
		dataGridViewCellStyle18.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
		this.colOffset.DefaultCellStyle = dataGridViewCellStyle18;
		this.colOffset.HeaderText = "Offset";
		this.colOffset.Name = "colOffset";
		this.colOffset.ReadOnly = true;
		this.colOffset.Width = 64;
		this.colDescription.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
		this.colDescription.DataPropertyName = "Description";
		this.colDescription.HeaderText = "Description";
		this.colDescription.Name = "colDescription";
		this.colDescription.ReadOnly = true;
		this.contextMenuTags.Items.AddRange(new System.Windows.Forms.ToolStripItem[3] { this.mnON, this.mnOFF, this.mnWriteTag });
		this.contextMenuTags.Name = "contextMenuTags";
		this.contextMenuTags.Size = new System.Drawing.Size(169, 70);
		this.mnON.Image = NetStudio.IPS.Properties.Resources.Resources_512_on_square;
		this.mnON.Name = "mnON";
		this.mnON.ShortcutKeys = System.Windows.Forms.Keys.Z | System.Windows.Forms.Keys.Control;
		this.mnON.Size = new System.Drawing.Size(168, 22);
		this.mnON.Text = "ON";
		this.mnON.Click += new System.EventHandler(mnON_Click);
		this.mnOFF.Image = NetStudio.IPS.Properties.Resources.Resources_512_off_square;
		this.mnOFF.Name = "mnOFF";
		this.mnOFF.ShortcutKeys = System.Windows.Forms.Keys.X | System.Windows.Forms.Keys.Control;
		this.mnOFF.Size = new System.Drawing.Size(168, 22);
		this.mnOFF.Text = "OFF";
		this.mnOFF.Click += new System.EventHandler(mnOFF_Click);
		this.mnWriteTag.Image = NetStudio.IPS.Properties.Resources.Resources_512_write_tag;
		this.mnWriteTag.Name = "mnWriteTag";
		this.mnWriteTag.ShortcutKeys = System.Windows.Forms.Keys.W | System.Windows.Forms.Keys.Control;
		this.mnWriteTag.Size = new System.Drawing.Size(168, 22);
		this.mnWriteTag.Text = "Write Tag";
		this.mnWriteTag.Click += new System.EventHandler(OnDataGridViewDoubleClick);
		this.dgvLogs.AllowUserToAddRows = false;
		this.dgvLogs.AllowUserToDeleteRows = false;
		this.dgvLogs.BackgroundColor = System.Drawing.Color.White;
		this.dgvLogs.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.dgvLogs.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.SingleHorizontal;
		dataGridViewCellStyle19.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
		dataGridViewCellStyle19.BackColor = System.Drawing.SystemColors.Control;
		dataGridViewCellStyle19.Font = new System.Drawing.Font("Segoe UI", 9f);
		dataGridViewCellStyle19.ForeColor = System.Drawing.SystemColors.WindowText;
		dataGridViewCellStyle19.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
		this.dgvLogs.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle19;
		this.dgvLogs.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
		this.dgvLogs.Columns.AddRange(this.colTimeLog, this.colCounter, this.colEventType, this.colSource, this.colMessage, this.LogType);
		this.dgvLogs.ContextMenuStrip = this.contextMenuLogs;
		this.dgvLogs.Dock = System.Windows.Forms.DockStyle.Fill;
		this.dgvLogs.GridColor = System.Drawing.Color.GhostWhite;
		this.dgvLogs.Location = new System.Drawing.Point(0, 0);
		this.dgvLogs.MultiSelect = false;
		this.dgvLogs.Name = "dgvLogs";
		this.dgvLogs.ReadOnly = true;
		this.dgvLogs.RowHeadersVisible = false;
		this.dgvLogs.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
		this.dgvLogs.Size = new System.Drawing.Size(904, 166);
		this.dgvLogs.TabIndex = 1;
		this.dgvLogs.VirtualMode = true;
		this.contextMenuLogs.Items.AddRange(new System.Windows.Forms.ToolStripItem[2] { this.mnClearLog, this.mnUnselect });
		this.contextMenuLogs.Name = "contextMenuLogs";
		this.contextMenuLogs.Size = new System.Drawing.Size(122, 48);
		this.mnClearLog.Image = NetStudio.IPS.Properties.Resources.Resources_512_clear;
		this.mnClearLog.Name = "mnClearLog";
		this.mnClearLog.Size = new System.Drawing.Size(121, 22);
		this.mnClearLog.Text = "Clear log";
		this.mnClearLog.Click += new System.EventHandler(mnClearLog_Click);
		this.mnUnselect.Image = NetStudio.IPS.Properties.Resources.Resources_512_unselect;
		this.mnUnselect.Name = "mnUnselect";
		this.mnUnselect.Size = new System.Drawing.Size(121, 22);
		this.mnUnselect.Text = "Unselect";
		this.mnUnselect.Visible = false;
		this.mnUnselect.Click += new System.EventHandler(mnUnselect_Click);
		this.treeViewMain.ContextMenuStrip = this.contextMenuTreeList;
		this.treeViewMain.Dock = System.Windows.Forms.DockStyle.Fill;
		this.treeViewMain.Location = new System.Drawing.Point(0, 0);
		this.treeViewMain.Name = "treeViewMain";
		this.treeViewMain.Size = new System.Drawing.Size(253, 678);
		this.treeViewMain.TabIndex = 0;
		this.treeViewMain.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(treeViewMain_AfterSelect);
		this.contextMenuTreeList.Items.AddRange(new System.Windows.Forms.ToolStripItem[2] { this.mnExpandAll, this.mnCollapseAll });
		this.contextMenuTreeList.Name = "contextMenuStrip1";
		this.contextMenuTreeList.Size = new System.Drawing.Size(137, 48);
		this.mnExpandAll.Image = NetStudio.IPS.Properties.Resources.Resources_512_minus;
		this.mnExpandAll.Name = "mnExpandAll";
		this.mnExpandAll.Size = new System.Drawing.Size(136, 22);
		this.mnExpandAll.Text = "Expand All";
		this.mnExpandAll.Click += new System.EventHandler(mnExpandAll_Click);
		this.mnCollapseAll.Image = NetStudio.IPS.Properties.Resources.Resources_512_plus;
		this.mnCollapseAll.Name = "mnCollapseAll";
		this.mnCollapseAll.Size = new System.Drawing.Size(136, 22);
		this.mnCollapseAll.Text = "Collapse All";
		this.mnCollapseAll.Click += new System.EventHandler(mnCollapseAll_Click);
		this.toolStripSeparator4.Name = "toolStripSeparator4";
		this.toolStripSeparator4.Size = new System.Drawing.Size(6, 25);
		this.btnClearSearchBox.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.btnClearSearchBox.Image = NetStudio.IPS.Properties.Resources.Resources_512_close;
		this.btnClearSearchBox.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.btnClearSearchBox.Name = "btnClearSearchBox";
		this.btnClearSearchBox.Size = new System.Drawing.Size(23, 22);
		this.btnClearSearchBox.Text = "Clear";
		this.btnClearSearchBox.Click += new System.EventHandler(btnClearSearchBox_Click);
		this.txtSearchBox.BackColor = System.Drawing.Color.White;
		this.txtSearchBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.txtSearchBox.Name = "txtSearchBox";
		this.txtSearchBox.Size = new System.Drawing.Size(350, 25);
		this.txtSearchBox.TextChanged += new System.EventHandler(searchBox_TextChanged);
		this.lblSearchBox.Name = "lblSearchBox";
		this.lblSearchBox.Size = new System.Drawing.Size(45, 22);
		this.lblSearchBox.Text = "Search:";
		this.lblInfo.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
		this.lblInfo.Name = "lblInfo";
		this.lblInfo.Size = new System.Drawing.Size(70, 22);
		this.lblInfo.Text = "Information";
		this.btnWrite.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.btnWrite.Image = NetStudio.IPS.Properties.Resources.Resources_512_write_tag;
		this.btnWrite.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.btnWrite.Name = "btnWrite";
		this.btnWrite.Size = new System.Drawing.Size(23, 22);
		this.btnWrite.Text = "&Write";
		this.btnWrite.Click += new System.EventHandler(OnDataGridViewDoubleClick);
		this.btnImportData.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.btnImportData.Enabled = false;
		this.btnImportData.Image = NetStudio.IPS.Properties.Resources.Resources_512_import;
		this.btnImportData.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.btnImportData.Name = "btnImportData";
		this.btnImportData.Size = new System.Drawing.Size(23, 22);
		this.btnImportData.Text = "Import data";
		this.btnImportData.Click += new System.EventHandler(btnImportData_Click);
		this.toolBar.BackColor = System.Drawing.Color.Snow;
		this.toolBar.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
		this.toolBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[21]
		{
			this.btnDeviceMonitor, this.btnRealTimeCharting, this.btnRestart, this.toolStripSeparator1, this.btnExportData, this.btnImportData, this.btnWrite, this.toolStripSeparator3, this.lblSearchBox, this.lblInfo,
			this.txtSearchBox, this.btnClearSearchBox, this.toolStripSeparator4, this.lblTotalName, this.lblTotalTags, this.lblTotalUnit, this.GroupLineHorizontal, this.lblGroupName, this.lblGroupTags, this.lblGroupUnit,
			this.GroupLineVertical
		});
		this.toolBar.Location = new System.Drawing.Point(0, 0);
		this.toolBar.Name = "toolBar";
		this.toolBar.Size = new System.Drawing.Size(1161, 25);
		this.toolBar.TabIndex = 5;
		this.toolBar.Text = "Tool bar";
		this.btnDeviceMonitor.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.btnDeviceMonitor.Image = NetStudio.IPS.Properties.Resources.Resources_512_device;
		this.btnDeviceMonitor.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.btnDeviceMonitor.Name = "btnDeviceMonitor";
		this.btnDeviceMonitor.Size = new System.Drawing.Size(23, 22);
		this.btnDeviceMonitor.Text = "Device monitoring";
		this.btnDeviceMonitor.Click += new System.EventHandler(btnDeviceMonitor_Click);
		this.btnRealTimeCharting.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.btnRealTimeCharting.Image = NetStudio.IPS.Properties.Resources.Resources_512_line_chart;
		this.btnRealTimeCharting.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.btnRealTimeCharting.Name = "btnRealTimeCharting";
		this.btnRealTimeCharting.Size = new System.Drawing.Size(23, 22);
		this.btnRealTimeCharting.Text = "Real-time charting";
		this.btnRealTimeCharting.Click += new System.EventHandler(btnRealTimeCharting_Click);
		this.btnRestart.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.btnRestart.Image = NetStudio.IPS.Properties.Resources.Resources_512_restart;
		this.btnRestart.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.btnRestart.Name = "btnRestart";
		this.btnRestart.Size = new System.Drawing.Size(23, 22);
		this.btnRestart.Text = "Restart the driver server";
		this.btnRestart.ToolTipText = "Restart the driver server";
		this.btnRestart.Click += new System.EventHandler(btnRestart_Click);
		this.toolStripSeparator1.Name = "toolStripSeparator1";
		this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
		this.btnExportData.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.btnExportData.Enabled = false;
		this.btnExportData.Image = NetStudio.IPS.Properties.Resources.Resources_512_export;
		this.btnExportData.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.btnExportData.Name = "btnExportData";
		this.btnExportData.Size = new System.Drawing.Size(23, 22);
		this.btnExportData.Text = "Export data";
		this.btnExportData.Click += new System.EventHandler(btnExportData_Click);
		this.toolStripSeparator3.Name = "toolStripSeparator3";
		this.toolStripSeparator3.Size = new System.Drawing.Size(6, 25);
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
		this.colTimeLog.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
		this.colTimeLog.DataPropertyName = "Time";
		dataGridViewCellStyle20.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
		this.colTimeLog.DefaultCellStyle = dataGridViewCellStyle20;
		this.colTimeLog.HeaderText = "Date Time";
		this.colTimeLog.MinimumWidth = 130;
		this.colTimeLog.Name = "colTimeLog";
		this.colTimeLog.ReadOnly = true;
		this.colTimeLog.Width = 130;
		this.colCounter.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
		this.colCounter.DataPropertyName = "Counter";
		dataGridViewCellStyle21.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
		this.colCounter.DefaultCellStyle = dataGridViewCellStyle21;
		this.colCounter.HeaderText = "Count";
		this.colCounter.Name = "colCounter";
		this.colCounter.ReadOnly = true;
		this.colCounter.Width = 65;
		this.colEventType.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
		this.colEventType.DataPropertyName = "EvenType";
		dataGridViewCellStyle22.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
		dataGridViewCellStyle22.ForeColor = System.Drawing.SystemColors.ControlText;
		this.colEventType.DefaultCellStyle = dataGridViewCellStyle22;
		this.colEventType.HeaderText = "Event type";
		this.colEventType.Name = "colEventType";
		this.colEventType.ReadOnly = true;
		this.colEventType.Width = 87;
		this.colSource.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
		this.colSource.DataPropertyName = "Source";
		dataGridViewCellStyle23.ForeColor = System.Drawing.Color.MediumBlue;
		this.colSource.DefaultCellStyle = dataGridViewCellStyle23;
		this.colSource.HeaderText = "Source";
		this.colSource.MinimumWidth = 150;
		this.colSource.Name = "colSource";
		this.colSource.ReadOnly = true;
		this.colSource.Width = 150;
		this.colMessage.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
		this.colMessage.DataPropertyName = "Message";
		this.colMessage.HeaderText = "Message";
		this.colMessage.Name = "colMessage";
		this.colMessage.ReadOnly = true;
		this.LogType.DataPropertyName = "LogType";
		this.LogType.HeaderText = "LogType";
		this.LogType.Name = "LogType";
		this.LogType.ReadOnly = true;
		this.LogType.Visible = false;
		base.AutoScaleDimensions = new System.Drawing.SizeF(7f, 15f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(1161, 703);
		base.Controls.Add(this.splitContainer1);
		base.Controls.Add(this.toolBar);
		this.DoubleBuffered = true;
		base.Name = "FormTagMonitor";
		this.Text = "Monitoring";
		base.Load += new System.EventHandler(FormTagMonitor_Load);
		this.splitContainer1.Panel1.ResumeLayout(false);
		this.splitContainer1.Panel2.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.splitContainer1).EndInit();
		this.splitContainer1.ResumeLayout(false);
		this.splitContainer2.Panel1.ResumeLayout(false);
		this.splitContainer2.Panel2.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.splitContainer2).EndInit();
		this.splitContainer2.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.dgvTags).EndInit();
		this.contextMenuTags.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.dgvLogs).EndInit();
		this.contextMenuLogs.ResumeLayout(false);
		this.contextMenuTreeList.ResumeLayout(false);
		this.toolBar.ResumeLayout(false);
		this.toolBar.PerformLayout();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
