using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using NetStudio.Common.DataTypes;
using NetStudio.Common.IndusCom;
using NetStudio.Common.Manager;
using NetStudio.DriverComm.Models;
using NetStudio.IPS.Controls;
using NetStudio.IPS.Entity;
using NetStudio.IPS.Local;
using NetStudio.IPS.Properties;

namespace NetStudio.IPS;

public class FormTagEditor : Form
{
	private Channel? channel;

	private Device? device;

	private Group? group;

	private BindingSource source;

	private Tag? currentTag;

	private bool _IsSelectMode;

	public EventSelectTagChanged? OnSelectTagChanged;

	private TreeNode? rootNode;

	private TreeNode? channelNode;

	private TreeNode? deviceNode;

	private TreeNode? groupNode;

	private TreeNode? selectedNode;

	private IContainer components;

	private ToolStrip toolBar;

	private ToolStripButton btnAddTag;

	private ToolStripButton btnEdit;

	private ToolStripButton btnDelete;

	private ToolStripLabel lblInfo;

	private ToolStripTextBox txtSearchBox;

	private ToolStripSeparator toolStripSeparator2;

	private ToolStripLabel lblSearchBox;

	private SplitContainer splitContainer1;

	private TreeView treeViewMain;

	private ContextMenuStrip contextMenuTreeList;

	private ToolStripMenuItem mnChannel;

	private ToolStripMenuItem mnDevice;

	private ToolStripMenuItem mnGroup;

	private ToolStripMenuItem mnDelete;

	private ImageList imageCollection;

	private ToolStripButton btnClearSearchBox;

	private ToolStripSeparator toolStripSeparator4;

	private ToolStripMenuItem mnCopy;

	private ToolStripButton btnCopy;

	private DataGrid dataGrid_0;

	private ToolStripLabel lblTotalName;

	private ToolStripButton btnAddRange;

	private ContextMenuStrip contextMenuGrid;

	private ToolStripMenuItem mnGridEdit;

	private ToolStripMenuItem mnGridDelete;

	private ToolStripMenuItem mnGridCopy;

	private ToolStripSeparator toolStripSeparator1;

	private ToolStripSeparator toolStripSeparator3;

	private ToolStripLabel lblTotalTags;

	private ToolStripLabel toolStripLabel1;

	private ToolStripSeparator GroupLineHorizontal;

	private ToolStripLabel lblGroupName;

	private ToolStripLabel lblGroupTags;

	private ToolStripLabel lblGroupUnit;

	private ToolStripSeparator GroupLineVertical;

	private DataGridViewTextBoxColumn colID;

	private DataGridViewTextBoxColumn colTagName;

	private DataGridViewTextBoxColumn colFullName;

	private DataGridViewTextBoxColumn colAddress;

	private DataGridViewTextBoxColumn colDataType;

	private DataGridViewTextBoxColumn colMode;

	private DataGridViewTextBoxColumn colResolution;

	private DataGridViewCheckBoxColumn colIsScaling;

	private DataGridViewTextBoxColumn colAImin;

	private DataGridViewTextBoxColumn colAImax;

	private DataGridViewTextBoxColumn colRLmin;

	private DataGridViewTextBoxColumn colRLmax;

	private DataGridViewTextBoxColumn colOffset;

	private DataGridViewTextBoxColumn colDescription;

	private ToolStripSeparator toolStripSeparator5;

	private ToolStripMenuItem MnExpandAll;

	private ToolStripMenuItem mnCollapseAll;

	public FormTagEditor(bool isSelectMode = false)
	{
		InitializeComponent();
		try
		{
			_IsSelectMode = isSelectMode;
			dataGrid_0.ReadOnly = true;
			source = new BindingSource();
			dataGrid_0.DataSource = source;
			OptionsChanged();
		}
		catch (Exception ex)
		{
			MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private async void FormTagEdit_Load(object sender, EventArgs e)
	{
		try
		{
			ApiResponse apiResponse = await LoadTreeList();
			if (!apiResponse.Success)
			{
				throw new Exception(apiResponse.Message);
			}
			dataGrid_0.DoubleClick += OnDataGridViewDoubleClick;
			dataGrid_0.SelectionChanged += OnDataGridViewSelectionChanged;
			SetEnabled(flag: false);
			SetGroupTagVisible(visible: false);
		}
		catch (Exception ex)
		{
			MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private void SetGroupTagVisible(bool visible)
	{
		Invoke(() => lblGroupName.Visible = visible);
		Invoke(() => lblGroupTags.Visible = visible);
		Invoke(() => lblGroupUnit.Visible = visible);
		Invoke(() => GroupLineVertical.Visible = visible);
	}

	private void OnDataGridViewSelectionChanged(object? sender, EventArgs e)
	{
		try
		{
			if (channel != null && device != null && group != null)
			{
				currentTag = source.Current as Tag;
				if (currentTag != null)
				{
					lblInfo.Text = $"Tag: {channel.Name}/{device.Name}/{group.Name}/{currentTag.Name}";
				}
			}
		}
		catch (Exception ex)
		{
			MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private void OnDataGridViewUserDeletingRow(object? sender, DataGridViewRowCancelEventArgs e)
	{
		try
		{
			if (currentTag != null)
			{
				if (DialogResult.Yes == MessageBox.Show(this, "Do you want to remove the tag(" + currentTag.Name + ")?", "Question", MessageBoxButtons.YesNo, MessageBoxIcon.Question))
				{
					EditHelper.RemoveTag(currentTag);
					AppHelper.DataChanged = true;
					source.Remove(currentTag);
				}
				else
				{
					e.Cancel = true;
				}
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
			if (_IsSelectMode)
			{
				Tag tag = (Tag)source.Current;
				if (channel == null || device == null || group == null || tag == null)
				{
					return;
				}
				switch (tag.DataType)
				{
				default:
					throw new NotSupportedException($"This data type({tag.DataType}) is not supported.");
				case DataType.BOOL:
				case DataType.BYTE:
				case DataType.INT:
				case DataType.UINT:
				case DataType.DINT:
				case DataType.UDINT:
				case DataType.REAL:
				case DataType.LINT:
				case DataType.ULINT:
				case DataType.LREAL:
					tag.FullName = $"{channel.Name}.{device.Name}.{group.Name}.{tag.Name}";
					if (OnSelectTagChanged != null)
					{
						if (!OnSelectTagChanged(tag))
						{
							throw new Exception("The tag name already exists.");
						}
						base.DialogResult = DialogResult.OK;
					}
					else
					{
						base.DialogResult = DialogResult.Cancel;
					}
					break;
				}
			}
			else
			{
				btnEdit_Click(sender, e);
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

	private void btnAddRangeTag_Click(object sender, EventArgs e)
	{
		try
		{
			if (channel != null && device != null && group != null)
			{
				FormRangeTag obj = new FormRangeTag(channel, device, group)
				{
					StartPosition = FormStartPosition.CenterParent,
					ShowInTaskbar = false
				};
				obj.OnRangeTagChanged = (EventRangeTagChanged)Delegate.Combine(obj.OnRangeTagChanged, (EventRangeTagChanged)async delegate
				{
					AppHelper.DataChanged = true;
					source.ResetBindings(metadataChanged: true);
					await OnTagCountChanged();
				});
				obj.ShowDialog();
			}
		}
		catch (Exception ex)
		{
			MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private void btnAddTag_Click(object sender, EventArgs e)
	{
		try
		{
			if (channel == null || device == null || group == null)
			{
				return;
			}
			FormTag form = new FormTag(channel, device, group, null, EditMode.AddNew)
			{
				StartPosition = FormStartPosition.CenterParent,
				ShowInTaskbar = false
			};
			FormTag formTag = form;
			formTag.OnTagChanged = (EventTagChanged)Delegate.Combine(formTag.OnTagChanged, (EventTagChanged)async delegate(Tag tg, bool isAddNew, bool hasAddNew)
			{
				AppHelper.DataChanged = true;
				source.ResetBindings(metadataChanged: true);
				if (hasAddNew)
				{
					await OnTagCountChanged();
					form.OnInitialize(channel, device, group, (Tag)tg.Clone(), EditMode.Continuous);
				}
				int num = dataGrid_0.Rows.Count - 1;
				dataGrid_0.Rows[num].Selected = true;
				dataGrid_0.FirstDisplayedScrollingRowIndex = num;
			});
			form.ShowDialog();
		}
		catch (Exception ex)
		{
			MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private void btnEdit_Click(object sender, EventArgs e)
	{
		try
		{
			currentTag = source.Current as Tag;
			if (channel != null && device != null && group != null && currentTag != null)
			{
				FormTag obj = new FormTag(channel, device, group, currentTag, EditMode.Edit)
				{
					StartPosition = FormStartPosition.CenterParent,
					ShowInTaskbar = false
				};
				obj.OnTagChanged = (EventTagChanged)Delegate.Combine(obj.OnTagChanged, (EventTagChanged)delegate
				{
					AppHelper.DataChanged = true;
					source.ResetCurrentItem();
				});
				obj.ShowDialog();
			}
		}
		catch (Exception ex)
		{
			MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private void btnCopy_Click(object sender, EventArgs e)
	{
		try
		{
			currentTag = source.Current as Tag;
			if (channel == null || device == null || group == null || currentTag == null)
			{
				return;
			}
			Tag tag_ = (Tag)currentTag.Clone();
			FormTag form = new FormTag(channel, device, group, tag_, EditMode.Copy)
			{
				StartPosition = FormStartPosition.CenterParent,
				ShowInTaskbar = false
			};
			FormTag formTag = form;
			formTag.OnTagChanged = (EventTagChanged)Delegate.Combine(formTag.OnTagChanged, (EventTagChanged)async delegate(Tag tg, bool isAddNew, bool hasAddNew)
			{
				AppHelper.DataChanged = true;
				source.ResetBindings(metadataChanged: true);
				await OnTagCountChanged();
				if (hasAddNew)
				{
					form.OnInitialize(channel, device, group, (Tag)tg.Clone(), EditMode.Copy);
				}
				int num = dataGrid_0.Rows.Count - 1;
				dataGrid_0.Rows[num].Selected = true;
				dataGrid_0.FirstDisplayedScrollingRowIndex = num;
			});
			form.ShowDialog();
		}
		catch (Exception ex)
		{
			MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private async void btnDelete_Click(object sender, EventArgs e)
	{
		try
		{
			Tag tag = (Tag)source.Current;
			if (tag != null && DialogResult.Yes == MessageBox.Show(this, "Do you want to remove the tag(" + tag.Name + ")?", "Question", MessageBoxButtons.YesNo, MessageBoxIcon.Question))
			{
				EditHelper.RemoveTag(tag);
				AppHelper.DataChanged = true;
				source.ResetBindings(metadataChanged: true);
				await OnTagCountChanged();
			}
		}
		catch (Exception ex)
		{
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

	private void RefreshData()
	{
		try
		{
			if (group != null)
			{
				source.ResetBindings(metadataChanged: true);
			}
		}
		catch (Exception ex)
		{
			MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private void SetEnabled(bool flag)
	{
		btnAddTag.Enabled = flag;
		btnAddRange.Enabled = flag;
		btnEdit.Enabled = flag;
		btnDelete.Enabled = flag;
		btnCopy.Enabled = flag;
		lblSearchBox.Enabled = flag;
		txtSearchBox.ReadOnly = !flag;
		if (txtSearchBox.ReadOnly)
		{
			txtSearchBox.Text = string.Empty;
		}
		if (treeViewMain.SelectedNode != null)
		{
			mnCopy.Visible = treeViewMain.SelectedNode.Level > 0;
		}
	}

	private async Task OnTagCountChanged()
	{
		await Task.Run(delegate
		{
			try
			{
				if (device != null)
				{
					int num = 0;
					foreach (Group group in device.Groups)
					{
						num += group.Tags.Count;
					}
					lblTotalTags.Text = $"{num}";
					if (source != null)
					{
						lblGroupTags.Text = $"{source.Count}";
					}
				}
			}
			catch (Exception)
			{
			}
		});
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

	private void searchBox_TextChanged(object sender, EventArgs e)
	{
		if (group != null)
		{
			source.DataSource = group.Tags.FindAll((Tag tg) => tg.Name.Contains(txtSearchBox.Text, StringComparison.OrdinalIgnoreCase) || tg.Address.Contains(txtSearchBox.Text, StringComparison.OrdinalIgnoreCase) || tg.DataType.ToString().Contains(txtSearchBox.Text, StringComparison.OrdinalIgnoreCase) || tg.Mode.ToString().Contains(txtSearchBox.Text, StringComparison.OrdinalIgnoreCase) || tg.IsScaling.ToString().Contains(txtSearchBox.Text, StringComparison.OrdinalIgnoreCase) || tg.Id.ToString().Contains(txtSearchBox.Text) || tg.AImin.ToString().Contains(txtSearchBox.Text) || tg.AImax.ToString().Contains(txtSearchBox.Text) || tg.RLmin.ToString().Contains(txtSearchBox.Text) || tg.RLmax.ToString().Contains(txtSearchBox.Text) || tg.Offset.ToString().Contains(txtSearchBox.Text) || (tg.Description ?? "").Contains(txtSearchBox.Text));
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
			default:
				rootNode = selectedNode;
				mnChannel.Text = "Add new channel";
				mnChannel.Visible = true;
				mnDevice.Visible = false;
				mnGroup.Visible = false;
				SetEnabled(flag: false);
				break;
			case 1:
				channelNode = selectedNode;
				rootNode = channelNode.Parent;
				mnChannel.Text = "Edit channel";
				mnDevice.Text = "Add new device";
				mnChannel.Visible = true;
				mnDevice.Visible = true;
				mnGroup.Visible = false;
				SetEnabled(flag: false);
				break;
			case 2:
				deviceNode = selectedNode;
				channelNode = deviceNode.Parent;
				rootNode = channelNode.Parent;
				mnDevice.Text = "Edit device";
				mnGroup.Text = "Add new group";
				mnChannel.Visible = false;
				mnDevice.Visible = true;
				mnGroup.Visible = true;
				SetEnabled(flag: false);
				break;
			case 3:
				groupNode = selectedNode;
				deviceNode = groupNode.Parent;
				channelNode = deviceNode.Parent;
				rootNode = channelNode.Parent;
				mnChannel.Visible = false;
				mnDevice.Visible = false;
				mnGroup.Text = "Edit group";
				mnGroup.Visible = true;
				SetEnabled(flag: true);
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
			channel = null;
			device = null;
			group = null;
			switch (treeViewMain.SelectedNode.Level)
			{
			default:
				if (lblGroupTags.Visible)
				{
					SetGroupTagVisible(visible: false);
				}
				lblInfo.Text = string.Empty;
				break;
			case 0:
				if (lblGroupTags.Visible)
				{
					SetGroupTagVisible(visible: false);
				}
				lblInfo.Text = "Industrial Protocols";
				break;
			case 1:
				if (lblGroupTags.Visible)
				{
					SetGroupTagVisible(visible: false);
				}
				if (channelNode != null)
				{
					channel = EditHelper.GetChannelByName(channelNode.Text);
					if (channel != null)
					{
						lblInfo.Text = channel.GetInfos() ?? "";
					}
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
				channel = EditHelper.GetChannelByName(channelNode.Text);
				if (channel != null)
				{
					device = channel.Devices.Where((Device device_0) => device_0.Name.Equals(deviceNode.Text)).FirstOrDefault();
					if (device != null)
					{
						lblInfo.Text = channel.Name + "/" + device.GetInfos();
					}
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
					channel = EditHelper.GetChannelByName(channelNode.Text);
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
									lblInfo.Text = $"Group: {channel.Name}/{device.Name}/{group.Name}";
								}
								else if (char.IsLetter(group.Tags[0].Address[0]))
								{
									colAddress.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
								}
								source.DataSource = group.Tags;
								lblGroupTags.Text = $"{group.Tags.Count}";
								if (!lblGroupTags.Visible)
								{
									SetGroupTagVisible(visible: true);
								}
								SetEnabled(flag: true);
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
				source.DataSource = Array.Empty<Tag>();
			}
		}
		catch (Exception ex2)
		{
			MessageBox.Show(this, ex2.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	public async Task<ApiResponse> LoadTreeList()
	{
		ApiResponse response = null;
		try
		{
			await WaitFormManager.ShowAsync(this, "Loading...");
			int totalTags = 0;
			channelNode = null;
			deviceNode = null;
			groupNode = null;
			selectedNode = null;
			treeViewMain.Nodes.Clear();
			treeViewMain.ImageList = imageCollection;
			TreeNode root = new TreeNode("Industrial Protocols");
			treeViewMain.Nodes.Add(root);
			response = await EditHelper.OnLoadProject();
			if ((response == null || (response != null && response.Success)) && EditHelper.IndusProtocol != null)
			{
				foreach (Channel channel in EditHelper.IndusProtocol.Channels)
				{
					TreeNode treeNode = new TreeNode(channel.Name)
					{
						ImageIndex = 1,
						SelectedImageIndex = 1,
						Tag = channel.Id
					};
					root.Nodes.Add(treeNode);
					if (channel.Devices == null || !channel.Devices.Any())
					{
						continue;
					}
					foreach (Device device in channel.Devices)
					{
						TreeNode treeNode2 = new TreeNode(device.Name)
						{
							ImageIndex = 2,
							SelectedImageIndex = 2,
							Tag = device.Id
						};
						treeNode.Nodes.Add(treeNode2);
						foreach (Group group in device.Groups)
						{
							TreeNode node = new TreeNode(group.Name)
							{
								ImageIndex = 3,
								SelectedImageIndex = 3,
								Tag = group.Id
							};
							treeNode2.Nodes.Add(node);
							totalTags += group.Tags.Count;
						}
					}
				}
				if (source.DataSource == null)
				{
					source.DataSource = Array.Empty<Tag>();
				}
				treeViewMain.ExpandAll();
				lblTotalTags.Text = $"{totalTags}";
			}
			await WaitFormManager.CloseAsync();
		}
		catch (Exception ex)
		{
			await WaitFormManager.CloseAsync();
			MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
		return response ?? new ApiResponse
		{
			Message = "Read request successfully.",
			Success = true
		};
	}

	private void mnChannel_Click(object sender, EventArgs e)
	{
		try
		{
			ToolStripMenuItem toolStripMenuItem = (ToolStripMenuItem)sender;
			if (treeViewMain.SelectedNode.Level != 0 && treeViewMain.SelectedNode.Level != 1)
			{
				return;
			}
			if (treeViewMain.SelectedNode.Level == 1)
			{
				channel = EditHelper.GetChannelByName(channelNode.Text);
			}
			FormChannel obj = new FormChannel(channel)
			{
				Text = toolStripMenuItem.Text,
				StartPosition = FormStartPosition.CenterParent,
				ShowInTaskbar = false
			};
			obj.OnChannelChanged = (EventChannelChanged)Delegate.Combine(obj.OnChannelChanged, (EventChannelChanged)delegate(Channel channel_0, bool isAddNew)
			{
				try
				{
					if (isAddNew)
					{
						if (rootNode != null)
						{
							TreeNode node = new TreeNode(channel_0.Name)
							{
								ImageIndex = 1,
								SelectedImageIndex = 1
							};
							rootNode.Nodes.Add(node);
							rootNode.ExpandAll();
							treeViewMain.SelectedNode = node;
						}
					}
					else if (channelNode != null)
					{
						channelNode.Text = channel_0.Name;
					}
					if (channel_0 != null)
					{
						AppHelper.DataChanged = true;
						lblInfo.Text = channel_0.GetInfos() ?? "";
					}
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

	private void mnDevice_Click(object sender, EventArgs e)
	{
		try
		{
			ToolStripMenuItem menu = (ToolStripMenuItem)sender;
			if (treeViewMain.SelectedNode.Level != 1 && treeViewMain.SelectedNode.Level != 2)
			{
				throw new InvalidOperationException("Operation is not valid.");
			}
			channel = EditHelper.GetChannelByName(channelNode.Text);
			if (treeViewMain.SelectedNode.Level == 2 && channel != null && channel.Devices != null)
			{
				device = channel.Devices.Where((Device device_0) => device_0.Name.Equals(deviceNode.Text)).FirstOrDefault();
			}
			if (channel == null)
			{
				throw new InvalidOperationException("Operation is not valid.");
			}
			switch (channel.Manufacturer)
			{
			case Manufacturer.IPC:
				OnLoadModiconDevice(menu);
				break;
			case Manufacturer.SIEMENS:
				OnLoadSiemensDevice(menu);
				break;
			case Manufacturer.MITSUBISHI:
				OnLoadMitsubishiDevice(menu);
				break;
			case Manufacturer.OMRON:
				OnLoadOmronDevice(menu);
				break;
			case Manufacturer.PANASONIC:
				OnLoadPanasonicDevice(menu);
				break;
			case Manufacturer.LS:
				OnLoadLsDevice(menu);
				break;
			case Manufacturer.DELTA:
				OnLoadDeltaDevice(menu);
				break;
			case Manufacturer.FATEK:
				OnLoadDevice(menu);
				break;
			case Manufacturer.VIGOR:
				OnLoadDevice(menu);
				break;
			case Manufacturer.KEYENCE:
				OnLoadKeyenceDevice(menu);
				break;
			case Manufacturer.SCHNEIDER:
				break;
			}
		}
		catch (Exception ex)
		{
			MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private void mnGroup_Click(object sender, EventArgs e)
	{
		try
		{
			ToolStripMenuItem toolStripMenuItem = (ToolStripMenuItem)sender;
			if (treeViewMain.SelectedNode.Level != 2 && treeViewMain.SelectedNode.Level != 3)
			{
				throw new InvalidOperationException("Operation is not valid.");
			}
			channel = EditHelper.GetChannelByName(channelNode.Text);
			if (channel != null && channel.Devices != null)
			{
				device = channel.Devices.Where((Device device_0) => device_0.Name.Equals(deviceNode.Text)).FirstOrDefault();
			}
			if (treeViewMain.SelectedNode.Level == 3 && channel != null && device != null)
			{
				group = device.Groups.Where((Group group_0) => group_0.Name.Equals(groupNode.Text)).FirstOrDefault();
			}
			if (channel == null || device == null)
			{
				throw new InvalidOperationException("Operation is not valid.");
			}
			FormGroup obj = new FormGroup(channel, device, group)
			{
				Text = toolStripMenuItem.Text,
				StartPosition = FormStartPosition.CenterParent,
				ShowInTaskbar = false
			};
			obj.OnGroupChanged = (EventGroupChanged)Delegate.Combine(obj.OnGroupChanged, (EventGroupChanged)delegate(Group group_0, bool isAddNew)
			{
				try
				{
					if (isAddNew)
					{
						if (deviceNode != null)
						{
							TreeNode node = new TreeNode(group_0.Name)
							{
								ImageIndex = 3,
								SelectedImageIndex = 3
							};
							deviceNode.Nodes.Add(node);
							deviceNode.ExpandAll();
							treeViewMain.SelectedNode = node;
						}
					}
					else if (groupNode != null)
					{
						groupNode.Text = group_0.Name;
					}
					AppHelper.DataChanged = true;
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

	private void mnCopy_Click(object sender, EventArgs e)
	{
		try
		{
			if (selectedNode == null)
			{
				return;
			}
			switch (treeViewMain.SelectedNode.Level)
			{
			default:
				throw new NotSupportedException();
			case 1:
			{
				if (channelNode == null)
				{
					break;
				}
				channel = EditHelper.GetChannelByName(channelNode.Text);
				if (channel == null || DialogResult.Yes != MessageBox.Show(this, "Do you want to copy the channel(" + channel.Name + ")?", "Question", MessageBoxButtons.YesNo, MessageBoxIcon.Question))
				{
					break;
				}
				channel = EditHelper.CopyChannel(channel);
				if (channel == null || rootNode == null)
				{
					break;
				}
				TreeNode treeNode2 = new TreeNode(channel.Name)
				{
					ImageIndex = 1,
					SelectedImageIndex = 1
				};
				rootNode.Nodes.Add(treeNode2);
				if (channel.Devices.Count > 0)
				{
					foreach (Device device in channel.Devices)
					{
						TreeNode treeNode3 = new TreeNode(device.Name)
						{
							ImageIndex = 2,
							SelectedImageIndex = 2
						};
						treeNode2.Nodes.Add(treeNode3);
						if (device.Groups.Count <= 0)
						{
							continue;
						}
						using List<Group>.Enumerator enumerator = device.Groups.GetEnumerator();
						while (enumerator.MoveNext())
						{
							TreeNode node3 = new TreeNode(enumerator.Current.Name)
							{
								ImageIndex = 3,
								SelectedImageIndex = 3
							};
							treeNode3.Nodes.Add(node3);
						}
					}
				}
				rootNode.ExpandAll();
				AppHelper.DataChanged = true;
				break;
			}
			case 2:
			{
				if (rootNode == null || channelNode == null || deviceNode == null)
				{
					break;
				}
				channel = EditHelper.GetChannelByName(channelNode.Text);
				if (channel == null)
				{
					break;
				}
				this.device = channel.Devices.Where((Device device_0) => device_0.Name.Equals(deviceNode.Text)).FirstOrDefault();
				if (this.device == null || DialogResult.Yes != MessageBox.Show(this, $"Do you want to copy the device({this.device.Name}) of channel({channel.Name})?", "Question", MessageBoxButtons.YesNo, MessageBoxIcon.Question))
				{
					break;
				}
				this.device = EditHelper.CopyDevice(this.device);
				if (this.device == null)
				{
					break;
				}
				TreeNode treeNode = new TreeNode(this.device.Name)
				{
					ImageIndex = 2,
					SelectedImageIndex = 2
				};
				channelNode.Nodes.Add(treeNode);
				if (this.device.Groups.Count > 0)
				{
					using List<Group>.Enumerator enumerator = this.device.Groups.GetEnumerator();
					while (enumerator.MoveNext())
					{
						TreeNode node2 = new TreeNode(enumerator.Current.Name)
						{
							ImageIndex = 3,
							SelectedImageIndex = 3
						};
						treeNode.Nodes.Add(node2);
					}
				}
				rootNode.ExpandAll();
				AppHelper.DataChanged = true;
				break;
			}
			case 3:
				if (channelNode == null || deviceNode == null || groupNode == null)
				{
					break;
				}
				channel = EditHelper.GetChannelByName(channelNode.Text);
				if (channel == null)
				{
					break;
				}
				this.device = channel.Devices.Where((Device device_0) => device_0.Name.Equals(deviceNode.Text)).FirstOrDefault();
				if (this.device == null)
				{
					break;
				}
				group = this.device.Groups.Where((Group group_0) => group_0.Name.Equals(groupNode.Text)).FirstOrDefault();
				if (group != null && DialogResult.Yes == MessageBox.Show(this, $"Do you want to copy the Group({group.Name}) of device({this.device.Name}), channel({channel.Name})?", "Question", MessageBoxButtons.YesNo, MessageBoxIcon.Question))
				{
					group = EditHelper.CopyGroup(group);
					if (group != null && rootNode != null)
					{
						TreeNode node = new TreeNode(group.Name)
						{
							ImageIndex = 3,
							SelectedImageIndex = 3
						};
						deviceNode.Nodes.Add(node);
						rootNode.ExpandAll();
						AppHelper.DataChanged = true;
					}
				}
				break;
			}
		}
		catch (Exception ex)
		{
			MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private async void mnDelete_Click(object sender, EventArgs e)
	{
		try
		{
			if (selectedNode == null)
			{
				return;
			}
			switch (treeViewMain.SelectedNode.Level)
			{
			default:
				throw new InvalidOperationException();
			case 0:
				if (DialogResult.Yes == MessageBox.Show(this, "Do you want to remove them all?", "Question", MessageBoxButtons.YesNo, MessageBoxIcon.Question))
				{
					EditHelper.RemoveAll();
					selectedNode.Nodes.Clear();
					AppHelper.DataChanged = true;
					await OnTagCountChanged();
				}
				break;
			case 1:
				if (channelNode != null)
				{
					channel = EditHelper.GetChannelByName(channelNode.Text);
					if (channel != null && DialogResult.Yes == MessageBox.Show(this, "Do you want to remove the channel(" + channel.Name + ")?", "Question", MessageBoxButtons.YesNo, MessageBoxIcon.Question))
					{
						EditHelper.RemoveChannel(channel);
						selectedNode.Nodes.Remove(channelNode);
						AppHelper.DataChanged = true;
						await OnTagCountChanged();
					}
				}
				break;
			case 2:
				if (channelNode == null || deviceNode == null)
				{
					break;
				}
				channel = EditHelper.GetChannelByName(channelNode.Text);
				if (channel != null)
				{
					device = channel.Devices.Where((Device device_0) => device_0.Name.Equals(deviceNode.Text)).FirstOrDefault();
					if (device != null && DialogResult.Yes == MessageBox.Show(this, $"Do you want to remove the device({device.Name}) of channel({channel.Name})?", "Question", MessageBoxButtons.YesNo, MessageBoxIcon.Question))
					{
						EditHelper.RemoveDevice(device);
						channelNode.Nodes.Remove(deviceNode);
						AppHelper.DataChanged = true;
						await OnTagCountChanged();
					}
				}
				break;
			case 3:
				if (channelNode == null || deviceNode == null || groupNode == null)
				{
					break;
				}
				channel = EditHelper.GetChannelByName(channelNode.Text);
				if (channel == null)
				{
					break;
				}
				device = channel.Devices.Where((Device device_0) => device_0.Name.Equals(deviceNode.Text)).FirstOrDefault();
				if (device != null)
				{
					group = device.Groups.Where((Group group_0) => group_0.Name.Equals(groupNode.Text)).FirstOrDefault();
					if (group != null && DialogResult.Yes == MessageBox.Show(this, $"Do you want to remove the Group({group.Name}) of device({device.Name}), channel({channel.Name})?", "Question", MessageBoxButtons.YesNo, MessageBoxIcon.Question))
					{
						EditHelper.RemoveGroup(group);
						deviceNode.Nodes.Remove(groupNode);
						AppHelper.DataChanged = true;
						await OnTagCountChanged();
					}
				}
				break;
			}
		}
		catch (Exception ex)
		{
			MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private void OnLoadCommDevice(ToolStripMenuItem menu)
	{
		if (channel == null)
		{
			throw new InvalidOperationException("Operation is not valid.");
		}
		if (channel.ConnectionType == ConnectionType.Serial)
		{
			CommSerialDevice obj = new CommSerialDevice(channel, device)
			{
				Text = menu.Text,
				StartPosition = FormStartPosition.CenterParent,
				ShowInTaskbar = false
			};
			obj.OnDeviceChanged = (EventDeviceChanged)Delegate.Combine(obj.OnDeviceChanged, (EventDeviceChanged)delegate(Device device_0, bool isAddNew)
			{
				try
				{
					if (isAddNew)
					{
						if (channelNode != null)
						{
							TreeNode treeNode = new TreeNode(device_0.Name)
							{
								ImageIndex = 2,
								SelectedImageIndex = 2
							};
							channelNode.Nodes.Add(treeNode);
							if (device_0.Groups.Count > 0)
							{
								foreach (Group group in device_0.Groups)
								{
									treeNode.Nodes.Add(new TreeNode(group.Name)
									{
										ImageIndex = 3,
										SelectedImageIndex = 3
									});
								}
							}
							channelNode.ExpandAll();
						}
					}
					else if (deviceNode != null)
					{
						deviceNode.Text = device_0.Name;
					}
					AppHelper.DataChanged = true;
				}
				catch (Exception ex2)
				{
					MessageBox.Show(this, ex2.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
				}
			});
			obj.ShowDialog();
			return;
		}
		CommTcpDevice obj2 = new CommTcpDevice(channel, device)
		{
			Text = menu.Text,
			StartPosition = FormStartPosition.CenterParent,
			ShowInTaskbar = false
		};
		obj2.OnDeviceChanged = (EventDeviceChanged)Delegate.Combine(obj2.OnDeviceChanged, (EventDeviceChanged)delegate(Device device_0, bool isAddNew)
		{
			try
			{
				if (isAddNew)
				{
					if (channelNode != null)
					{
						channelNode.Nodes.Add(new TreeNode(device_0.Name)
						{
							ImageIndex = 2,
							SelectedImageIndex = 2
						});
						channelNode.ExpandAll();
					}
				}
				else if (deviceNode != null)
				{
					deviceNode.Text = device_0.Name;
				}
				AppHelper.DataChanged = true;
			}
			catch (Exception ex)
			{
				MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
			}
		});
		obj2.ShowDialog();
	}

	private void OnLoadModiconDevice(ToolStripMenuItem menu)
	{
		if (channel == null)
		{
			throw new InvalidOperationException("Operation is not valid.");
		}
		if (channel.ConnectionType == ConnectionType.Serial)
		{
			ModiconSerialDevice obj = new ModiconSerialDevice(channel, device)
			{
				Text = menu.Text,
				StartPosition = FormStartPosition.CenterParent,
				ShowInTaskbar = false
			};
			obj.OnDeviceChanged = (EventDeviceChanged)Delegate.Combine(obj.OnDeviceChanged, (EventDeviceChanged)delegate(Device device_0, bool isAddNew)
			{
				try
				{
					if (isAddNew)
					{
						if (channelNode != null)
						{
							TreeNode node2 = new TreeNode(device_0.Name)
							{
								ImageIndex = 2,
								SelectedImageIndex = 2
							};
							channelNode.Nodes.Add(node2);
							channelNode.ExpandAll();
							treeViewMain.SelectedNode = node2;
						}
					}
					else if (deviceNode != null)
					{
						deviceNode.Text = device_0.Name;
					}
					AppHelper.DataChanged = true;
				}
				catch (Exception ex2)
				{
					MessageBox.Show(this, ex2.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
				}
			});
			obj.ShowDialog();
			return;
		}
		ModiconTcpDevice obj2 = new ModiconTcpDevice(channel, device)
		{
			Text = menu.Text,
			StartPosition = FormStartPosition.CenterParent,
			ShowInTaskbar = false
		};
		obj2.OnDeviceChanged = (EventDeviceChanged)Delegate.Combine(obj2.OnDeviceChanged, (EventDeviceChanged)delegate(Device device_0, bool isAddNew)
		{
			try
			{
				if (isAddNew)
				{
					if (channelNode != null)
					{
						TreeNode node = new TreeNode(device_0.Name)
						{
							ImageIndex = 2,
							SelectedImageIndex = 2
						};
						channelNode.Nodes.Add(node);
						channelNode.ExpandAll();
						treeViewMain.SelectedNode = node;
					}
				}
				else if (deviceNode != null)
				{
					deviceNode.Text = device_0.Name;
				}
				AppHelper.DataChanged = true;
			}
			catch (Exception ex)
			{
				MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
			}
		});
		obj2.ShowDialog();
	}

	private void OnLoadSiemensDevice(ToolStripMenuItem menu)
	{
		if (channel == null)
		{
			throw new InvalidOperationException("Operation is not valid.");
		}
		if (channel.ConnectionType == ConnectionType.Serial)
		{
			SiemensTcpDevice obj = new SiemensTcpDevice(channel, device)
			{
				Text = menu.Text,
				StartPosition = FormStartPosition.CenterParent,
				ShowInTaskbar = false
			};
			obj.OnDeviceChanged = (EventDeviceChanged)Delegate.Combine(obj.OnDeviceChanged, (EventDeviceChanged)delegate(Device device_0, bool isAddNew)
			{
				try
				{
					if (isAddNew)
					{
						if (channelNode != null)
						{
							TreeNode node2 = new TreeNode(device_0.Name)
							{
								ImageIndex = 2,
								SelectedImageIndex = 2
							};
							channelNode.Nodes.Add(node2);
							channelNode.ExpandAll();
							treeViewMain.SelectedNode = node2;
						}
					}
					else if (deviceNode != null)
					{
						deviceNode.Text = device_0.Name;
					}
					AppHelper.DataChanged = true;
				}
				catch (Exception ex2)
				{
					MessageBox.Show(this, ex2.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
				}
			});
			obj.ShowDialog();
			return;
		}
		SiemensTcpDevice obj2 = new SiemensTcpDevice(channel, device)
		{
			Text = menu.Text,
			StartPosition = FormStartPosition.CenterParent,
			ShowInTaskbar = false
		};
		obj2.OnDeviceChanged = (EventDeviceChanged)Delegate.Combine(obj2.OnDeviceChanged, (EventDeviceChanged)delegate(Device device_0, bool isAddNew)
		{
			try
			{
				if (isAddNew)
				{
					if (channelNode != null)
					{
						TreeNode node = new TreeNode(device_0.Name)
						{
							ImageIndex = 2,
							SelectedImageIndex = 2
						};
						channelNode.Nodes.Add(node);
						treeViewMain.SelectedNode = node;
					}
				}
				else if (deviceNode != null)
				{
					deviceNode.Text = device_0.Name;
				}
				AppHelper.DataChanged = true;
			}
			catch (Exception ex)
			{
				MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
			}
		});
		obj2.ShowDialog();
	}

	private void OnLoadKeyenceDevice(ToolStripMenuItem menu)
	{
		if (channel == null)
		{
			throw new InvalidOperationException("Operation is not valid.");
		}
		if (channel.ConnectionType == ConnectionType.Serial)
		{
			CommSerialDevice obj = new CommSerialDevice(channel, device)
			{
				Text = menu.Text,
				StartPosition = FormStartPosition.CenterParent,
				ShowInTaskbar = false
			};
			obj.OnDeviceChanged = (EventDeviceChanged)Delegate.Combine(obj.OnDeviceChanged, (EventDeviceChanged)delegate(Device device_0, bool isAddNew)
			{
				try
				{
					if (isAddNew)
					{
						if (channelNode != null)
						{
							TreeNode treeNode = new TreeNode(device_0.Name)
							{
								ImageIndex = 2,
								SelectedImageIndex = 2
							};
							channelNode.Nodes.Add(treeNode);
							if (device_0.Groups.Count > 0)
							{
								foreach (Group group in device_0.Groups)
								{
									treeNode.Nodes.Add(new TreeNode(group.Name)
									{
										ImageIndex = 3,
										SelectedImageIndex = 3
									});
								}
							}
							channelNode.ExpandAll();
						}
					}
					else if (deviceNode != null)
					{
						deviceNode.Text = device_0.Name;
					}
					AppHelper.DataChanged = true;
				}
				catch (Exception ex2)
				{
					MessageBox.Show(this, ex2.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
				}
			});
			obj.ShowDialog();
			return;
		}
		KeyenceTcpDevice obj2 = new KeyenceTcpDevice(channel, device)
		{
			Text = menu.Text,
			StartPosition = FormStartPosition.CenterParent,
			ShowInTaskbar = false
		};
		obj2.OnDeviceChanged = (EventDeviceChanged)Delegate.Combine(obj2.OnDeviceChanged, (EventDeviceChanged)delegate(Device device_0, bool isAddNew)
		{
			try
			{
				if (isAddNew)
				{
					if (channelNode != null)
					{
						channelNode.Nodes.Add(new TreeNode(device_0.Name)
						{
							ImageIndex = 2,
							SelectedImageIndex = 2
						});
						channelNode.ExpandAll();
					}
				}
				else if (deviceNode != null)
				{
					deviceNode.Text = device_0.Name;
				}
				AppHelper.DataChanged = true;
			}
			catch (Exception ex)
			{
				MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
			}
		});
		obj2.ShowDialog();
	}

	private void OnLoadMitsubishiDevice(ToolStripMenuItem menu)
	{
		if (channel == null)
		{
			throw new InvalidOperationException("Operation is not valid.");
		}
		if (channel.ConnectionType == ConnectionType.Serial)
		{
			MitsubishiSerialDevice obj = new MitsubishiSerialDevice(channel, device)
			{
				Text = menu.Text,
				StartPosition = FormStartPosition.CenterParent,
				ShowInTaskbar = false
			};
			obj.OnDeviceChanged = (EventDeviceChanged)Delegate.Combine(obj.OnDeviceChanged, (EventDeviceChanged)delegate(Device device_0, bool isAddNew)
			{
				try
				{
					if (isAddNew)
					{
						if (channelNode != null)
						{
							TreeNode node2 = new TreeNode(device_0.Name)
							{
								ImageIndex = 2,
								SelectedImageIndex = 2
							};
							channelNode.Nodes.Add(node2);
							treeViewMain.SelectedNode = node2;
						}
					}
					else if (deviceNode != null)
					{
						deviceNode.Text = device_0.Name;
					}
					AppHelper.DataChanged = true;
				}
				catch (Exception ex2)
				{
					MessageBox.Show(this, ex2.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
				}
			});
			obj.ShowDialog();
			return;
		}
		MitsubishiTcpDevice obj2 = new MitsubishiTcpDevice(channel, device)
		{
			Text = menu.Text,
			StartPosition = FormStartPosition.CenterParent,
			ShowInTaskbar = false
		};
		obj2.OnDeviceChanged = (EventDeviceChanged)Delegate.Combine(obj2.OnDeviceChanged, (EventDeviceChanged)delegate(Device device_0, bool isAddNew)
		{
			try
			{
				if (isAddNew)
				{
					if (channelNode != null)
					{
						TreeNode node = new TreeNode(device_0.Name)
						{
							ImageIndex = 2,
							SelectedImageIndex = 2
						};
						channelNode.Nodes.Add(node);
						treeViewMain.SelectedNode = node;
					}
				}
				else if (deviceNode != null)
				{
					deviceNode.Text = device_0.Name;
				}
				AppHelper.DataChanged = true;
			}
			catch (Exception ex)
			{
				MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
			}
		});
		obj2.ShowDialog();
	}

	private void OnLoadOmronDevice(ToolStripMenuItem menu)
	{
		if (channel == null)
		{
			throw new InvalidOperationException("Operation is not valid.");
		}
		if (channel.ConnectionType == ConnectionType.Serial)
		{
			OmronSerialDevice obj = new OmronSerialDevice(channel, device)
			{
				Text = menu.Text,
				StartPosition = FormStartPosition.CenterParent,
				ShowInTaskbar = false
			};
			obj.OnDeviceChanged = (EventDeviceChanged)Delegate.Combine(obj.OnDeviceChanged, (EventDeviceChanged)delegate(Device device_0, bool isAddNew)
			{
				try
				{
					if (isAddNew)
					{
						if (channelNode != null)
						{
							TreeNode treeNode = new TreeNode(device_0.Name)
							{
								ImageIndex = 2,
								SelectedImageIndex = 2
							};
							channelNode.Nodes.Add(treeNode);
							if (device_0.Groups.Count > 0)
							{
								foreach (Group group in device_0.Groups)
								{
									treeNode.Nodes.Add(new TreeNode(group.Name)
									{
										ImageIndex = 3,
										SelectedImageIndex = 3
									});
								}
							}
							channelNode.ExpandAll();
							treeViewMain.SelectedNode = treeNode;
						}
					}
					else if (deviceNode != null)
					{
						deviceNode.Text = device_0.Name;
					}
					AppHelper.DataChanged = true;
				}
				catch (Exception ex2)
				{
					MessageBox.Show(this, ex2.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
				}
			});
			obj.ShowDialog();
			return;
		}
		OmronTcpDevice obj2 = new OmronTcpDevice(channel, device)
		{
			Text = menu.Text,
			StartPosition = FormStartPosition.CenterParent,
			ShowInTaskbar = false
		};
		obj2.OnDeviceChanged = (EventDeviceChanged)Delegate.Combine(obj2.OnDeviceChanged, (EventDeviceChanged)delegate(Device device_0, bool isAddNew)
		{
			try
			{
				if (isAddNew)
				{
					if (channelNode != null)
					{
						TreeNode node = new TreeNode(device_0.Name)
						{
							ImageIndex = 2,
							SelectedImageIndex = 2
						};
						channelNode.Nodes.Add(node);
						channelNode.ExpandAll();
						treeViewMain.SelectedNode = node;
					}
				}
				else if (deviceNode != null)
				{
					deviceNode.Text = device_0.Name;
				}
				AppHelper.DataChanged = true;
			}
			catch (Exception ex)
			{
				MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
			}
		});
		obj2.ShowDialog();
	}

	private void OnLoadPanasonicDevice(ToolStripMenuItem menu)
	{
		if (channel == null)
		{
			throw new InvalidOperationException("Operation is not valid.");
		}
		if (channel.ConnectionType == ConnectionType.Serial)
		{
			PanasonicSerialDevice obj = new PanasonicSerialDevice(channel, device)
			{
				Text = menu.Text,
				StartPosition = FormStartPosition.CenterParent,
				ShowInTaskbar = false
			};
			obj.OnDeviceChanged = (EventDeviceChanged)Delegate.Combine(obj.OnDeviceChanged, (EventDeviceChanged)delegate(Device device_0, bool isAddNew)
			{
				try
				{
					if (isAddNew)
					{
						if (channelNode != null)
						{
							TreeNode node2 = new TreeNode(device_0.Name)
							{
								ImageIndex = 2,
								SelectedImageIndex = 2
							};
							channelNode.Nodes.Add(node2);
							treeViewMain.SelectedNode = node2;
						}
					}
					else if (deviceNode != null)
					{
						deviceNode.Text = device_0.Name;
					}
					AppHelper.DataChanged = true;
				}
				catch (Exception ex2)
				{
					MessageBox.Show(this, ex2.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
				}
			});
			obj.ShowDialog();
			return;
		}
		CommTcpDevice obj2 = new CommTcpDevice(channel, device)
		{
			Text = menu.Text,
			StartPosition = FormStartPosition.CenterParent,
			ShowInTaskbar = false
		};
		obj2.OnDeviceChanged = (EventDeviceChanged)Delegate.Combine(obj2.OnDeviceChanged, (EventDeviceChanged)delegate(Device device_0, bool isAddNew)
		{
			try
			{
				if (isAddNew)
				{
					if (channelNode != null)
					{
						TreeNode node = new TreeNode(device_0.Name)
						{
							ImageIndex = 2,
							SelectedImageIndex = 2
						};
						channelNode.Nodes.Add(node);
						treeViewMain.SelectedNode = node;
					}
				}
				else if (deviceNode != null)
				{
					deviceNode.Text = device_0.Name;
				}
				AppHelper.DataChanged = true;
			}
			catch (Exception ex)
			{
				MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
			}
		});
		obj2.ShowDialog();
	}

	private void OnLoadLsDevice(ToolStripMenuItem menu)
	{
		if (channel == null)
		{
			throw new InvalidOperationException("Operation is not valid.");
		}
		if (channel.ConnectionType == ConnectionType.Serial)
		{
			LsSerialDevice obj = new LsSerialDevice(channel, device)
			{
				Text = menu.Text,
				StartPosition = FormStartPosition.CenterParent,
				ShowInTaskbar = false
			};
			obj.OnDeviceChanged = (EventDeviceChanged)Delegate.Combine(obj.OnDeviceChanged, (EventDeviceChanged)delegate(Device device_0, bool isAddNew)
			{
				try
				{
					if (isAddNew)
					{
						if (channelNode != null)
						{
							TreeNode node2 = new TreeNode(device_0.Name)
							{
								ImageIndex = 2,
								SelectedImageIndex = 2
							};
							channelNode.Nodes.Add(node2);
							treeViewMain.SelectedNode = node2;
						}
					}
					else if (deviceNode != null)
					{
						deviceNode.Text = device_0.Name;
					}
					AppHelper.DataChanged = true;
				}
				catch (Exception ex2)
				{
					MessageBox.Show(this, ex2.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
				}
			});
			obj.ShowDialog();
			return;
		}
		LsTcpDevice obj2 = new LsTcpDevice(channel, device)
		{
			Text = menu.Text,
			StartPosition = FormStartPosition.CenterParent,
			ShowInTaskbar = false
		};
		obj2.OnDeviceChanged = (EventDeviceChanged)Delegate.Combine(obj2.OnDeviceChanged, (EventDeviceChanged)delegate(Device device_0, bool isAddNew)
		{
			try
			{
				if (isAddNew)
				{
					if (channelNode != null)
					{
						TreeNode node = new TreeNode(device_0.Name)
						{
							ImageIndex = 2,
							SelectedImageIndex = 2
						};
						channelNode.Nodes.Add(node);
						treeViewMain.SelectedNode = node;
					}
				}
				else if (deviceNode != null)
				{
					deviceNode.Text = device_0.Name;
				}
				AppHelper.DataChanged = true;
			}
			catch (Exception ex)
			{
				MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
			}
		});
		obj2.ShowDialog();
	}

	private void OnLoadDeltaDevice(ToolStripMenuItem menu)
	{
		if (channel == null)
		{
			throw new InvalidOperationException("Operation is not valid.");
		}
		if (channel.ConnectionType == ConnectionType.Serial)
		{
			DeltaSerialDevice obj = new DeltaSerialDevice(channel, device)
			{
				Text = menu.Text,
				StartPosition = FormStartPosition.CenterParent,
				ShowInTaskbar = false
			};
			obj.OnDeviceChanged = (EventDeviceChanged)Delegate.Combine(obj.OnDeviceChanged, (EventDeviceChanged)delegate(Device device_0, bool isAddNew)
			{
				try
				{
					if (isAddNew)
					{
						if (channelNode != null)
						{
							TreeNode treeNode = new TreeNode(device_0.Name)
							{
								ImageIndex = 2,
								SelectedImageIndex = 2
							};
							channelNode.Nodes.Add(treeNode);
							if (device_0.Groups.Count > 0)
							{
								foreach (Group group in device_0.Groups)
								{
									treeNode.Nodes.Add(new TreeNode(group.Name)
									{
										ImageIndex = 3,
										SelectedImageIndex = 3
									});
								}
							}
							channelNode.ExpandAll();
							treeViewMain.SelectedNode = treeNode;
						}
					}
					else if (deviceNode != null)
					{
						deviceNode.Text = device_0.Name;
					}
					AppHelper.DataChanged = true;
				}
				catch (Exception ex2)
				{
					MessageBox.Show(this, ex2.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
				}
			});
			obj.ShowDialog();
			return;
		}
		DeltaTcpDevice obj2 = new DeltaTcpDevice(channel, device)
		{
			Text = menu.Text,
			StartPosition = FormStartPosition.CenterParent,
			ShowInTaskbar = false
		};
		obj2.OnDeviceChanged = (EventDeviceChanged)Delegate.Combine(obj2.OnDeviceChanged, (EventDeviceChanged)delegate(Device device_0, bool isAddNew)
		{
			try
			{
				if (isAddNew)
				{
					if (channelNode != null)
					{
						TreeNode node = new TreeNode(device_0.Name)
						{
							ImageIndex = 2,
							SelectedImageIndex = 2
						};
						channelNode.Nodes.Add(node);
						treeViewMain.SelectedNode = node;
					}
				}
				else if (deviceNode != null)
				{
					deviceNode.Text = device_0.Name;
				}
				AppHelper.DataChanged = true;
			}
			catch (Exception ex)
			{
				MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
			}
		});
		obj2.ShowDialog();
	}

	private void OnLoadDevice(ToolStripMenuItem menu)
	{
		if (channel == null)
		{
			throw new InvalidOperationException("Operation is not valid.");
		}
		if (channel.ConnectionType == ConnectionType.Serial)
		{
			CommSerialDevice obj = new CommSerialDevice(channel, device)
			{
				Text = menu.Text,
				StartPosition = FormStartPosition.CenterParent,
				ShowInTaskbar = false
			};
			obj.OnDeviceChanged = (EventDeviceChanged)Delegate.Combine(obj.OnDeviceChanged, (EventDeviceChanged)delegate(Device device_0, bool isAddNew)
			{
				try
				{
					if (isAddNew)
					{
						if (channelNode != null)
						{
							TreeNode treeNode = new TreeNode(device_0.Name)
							{
								ImageIndex = 2,
								SelectedImageIndex = 2
							};
							channelNode.Nodes.Add(treeNode);
							if (device_0.Groups.Count > 0)
							{
								foreach (Group group in device_0.Groups)
								{
									treeNode.Nodes.Add(new TreeNode(group.Name)
									{
										ImageIndex = 3,
										SelectedImageIndex = 3
									});
								}
							}
							channelNode.ExpandAll();
							treeViewMain.SelectedNode = treeNode;
						}
					}
					else if (deviceNode != null)
					{
						deviceNode.Text = device_0.Name;
					}
					AppHelper.DataChanged = true;
				}
				catch (Exception ex2)
				{
					MessageBox.Show(this, ex2.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
				}
			});
			obj.ShowDialog();
			return;
		}
		CommTcpDevice obj2 = new CommTcpDevice(channel, device)
		{
			Text = menu.Text,
			StartPosition = FormStartPosition.CenterParent,
			ShowInTaskbar = false
		};
		obj2.OnDeviceChanged = (EventDeviceChanged)Delegate.Combine(obj2.OnDeviceChanged, (EventDeviceChanged)delegate(Device device_0, bool isAddNew)
		{
			try
			{
				if (isAddNew)
				{
					if (channelNode != null)
					{
						TreeNode node = new TreeNode(device_0.Name)
						{
							ImageIndex = 2,
							SelectedImageIndex = 2
						};
						channelNode.Nodes.Add(node);
						treeViewMain.SelectedNode = node;
					}
				}
				else if (deviceNode != null)
				{
					deviceNode.Text = device_0.Name;
				}
				AppHelper.DataChanged = true;
			}
			catch (Exception ex)
			{
				MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
			}
		});
		obj2.ShowDialog();
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	private async void InitializeComponent()
	{
		this.components = new System.ComponentModel.Container();
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NetStudio.IPS.FormTagEditor));
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
		this.toolBar = new System.Windows.Forms.ToolStrip();
		this.btnAddTag = new System.Windows.Forms.ToolStripButton();
		this.btnAddRange = new System.Windows.Forms.ToolStripButton();
		this.btnEdit = new System.Windows.Forms.ToolStripButton();
		this.btnCopy = new System.Windows.Forms.ToolStripButton();
		this.btnDelete = new System.Windows.Forms.ToolStripButton();
		this.lblInfo = new System.Windows.Forms.ToolStripLabel();
		this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
		this.lblSearchBox = new System.Windows.Forms.ToolStripLabel();
		this.txtSearchBox = new System.Windows.Forms.ToolStripTextBox();
		this.btnClearSearchBox = new System.Windows.Forms.ToolStripButton();
		this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
		this.lblTotalName = new System.Windows.Forms.ToolStripLabel();
		this.lblTotalTags = new System.Windows.Forms.ToolStripLabel();
		this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
		this.GroupLineHorizontal = new System.Windows.Forms.ToolStripSeparator();
		this.lblGroupName = new System.Windows.Forms.ToolStripLabel();
		this.lblGroupTags = new System.Windows.Forms.ToolStripLabel();
		this.lblGroupUnit = new System.Windows.Forms.ToolStripLabel();
		this.GroupLineVertical = new System.Windows.Forms.ToolStripSeparator();
		this.splitContainer1 = new System.Windows.Forms.SplitContainer();
		this.dataGrid_0 = new NetStudio.IPS.Controls.DataGrid();
		this.colID = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.colTagName = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.colFullName = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.colAddress = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.colDataType = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.colMode = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.colResolution = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.colIsScaling = new System.Windows.Forms.DataGridViewCheckBoxColumn();
		this.colAImin = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.colAImax = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.colRLmin = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.colRLmax = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.colOffset = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.colDescription = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.contextMenuGrid = new System.Windows.Forms.ContextMenuStrip(this.components);
		this.mnGridCopy = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
		this.mnGridEdit = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
		this.mnGridDelete = new System.Windows.Forms.ToolStripMenuItem();
		this.treeViewMain = new System.Windows.Forms.TreeView();
		this.contextMenuTreeList = new System.Windows.Forms.ContextMenuStrip(this.components);
		this.mnChannel = new System.Windows.Forms.ToolStripMenuItem();
		this.mnDevice = new System.Windows.Forms.ToolStripMenuItem();
		this.mnGroup = new System.Windows.Forms.ToolStripMenuItem();
		this.mnCopy = new System.Windows.Forms.ToolStripMenuItem();
		this.mnDelete = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
		this.MnExpandAll = new System.Windows.Forms.ToolStripMenuItem();
		this.mnCollapseAll = new System.Windows.Forms.ToolStripMenuItem();
		this.imageCollection = new System.Windows.Forms.ImageList(this.components);
		this.toolBar.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.splitContainer1).BeginInit();
		this.splitContainer1.Panel1.SuspendLayout();
		this.splitContainer1.Panel2.SuspendLayout();
		this.splitContainer1.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.dataGrid_0).BeginInit();
		this.contextMenuGrid.SuspendLayout();
		this.contextMenuTreeList.SuspendLayout();
		base.SuspendLayout();
		this.toolBar.BackColor = System.Drawing.Color.Snow;
		this.toolBar.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
		this.toolBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[19]
		{
			this.btnAddTag, this.btnAddRange, this.btnEdit, this.btnCopy, this.btnDelete, this.lblInfo, this.toolStripSeparator2, this.lblSearchBox, this.txtSearchBox, this.btnClearSearchBox,
			this.toolStripSeparator4, this.lblTotalName, this.lblTotalTags, this.toolStripLabel1, this.GroupLineHorizontal, this.lblGroupName, this.lblGroupTags, this.lblGroupUnit, this.GroupLineVertical
		});
		this.toolBar.Location = new System.Drawing.Point(0, 0);
		this.toolBar.Name = "toolBar";
		this.toolBar.Size = new System.Drawing.Size(1098, 25);
		this.toolBar.TabIndex = 2;
		this.toolBar.Text = "Tool bar";
		this.btnAddTag.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.btnAddTag.Enabled = false;
		this.btnAddTag.Image = (System.Drawing.Image)resources.GetObject("btnAddTag.Image");
		this.btnAddTag.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.btnAddTag.Name = "btnAddTag";
		this.btnAddTag.Size = new System.Drawing.Size(23, 22);
		this.btnAddTag.Text = "&Add Tag";
		this.btnAddTag.Click += new System.EventHandler(btnAddTag_Click);
		this.btnAddRange.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.btnAddRange.Enabled = false;
		this.btnAddRange.Image = NetStudio.IPS.Properties.Resources.Resources_512_add_range;
		this.btnAddRange.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.btnAddRange.Name = "btnAddRange";
		this.btnAddRange.Size = new System.Drawing.Size(23, 22);
		this.btnAddRange.Text = "Add an array of Tags";
		this.btnAddRange.Click += new System.EventHandler(btnAddRangeTag_Click);
		this.btnEdit.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.btnEdit.Enabled = false;
		this.btnEdit.Image = NetStudio.IPS.Properties.Resources.Resources_512_edit_tag;
		this.btnEdit.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.btnEdit.Name = "btnEdit";
		this.btnEdit.Size = new System.Drawing.Size(23, 22);
		this.btnEdit.Text = "&Edit";
		this.btnEdit.Click += new System.EventHandler(btnEdit_Click);
		this.btnCopy.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.btnCopy.Enabled = false;
		this.btnCopy.Image = NetStudio.IPS.Properties.Resources.Resources_512_copy;
		this.btnCopy.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.btnCopy.Name = "btnCopy";
		this.btnCopy.Size = new System.Drawing.Size(23, 22);
		this.btnCopy.Text = "Copy";
		this.btnCopy.Click += new System.EventHandler(btnCopy_Click);
		this.btnDelete.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.btnDelete.Enabled = false;
		this.btnDelete.Image = NetStudio.IPS.Properties.Resources.Resources_512_remove;
		this.btnDelete.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.btnDelete.Name = "btnDelete";
		this.btnDelete.Size = new System.Drawing.Size(23, 22);
		this.btnDelete.Text = "&Delete";
		this.btnDelete.Click += new System.EventHandler(btnDelete_Click);
		this.lblInfo.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
		this.lblInfo.Name = "lblInfo";
		this.lblInfo.Size = new System.Drawing.Size(70, 22);
		this.lblInfo.Text = "Information";
		this.toolStripSeparator2.Name = "toolStripSeparator2";
		this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
		this.lblSearchBox.Name = "lblSearchBox";
		this.lblSearchBox.Size = new System.Drawing.Size(45, 22);
		this.lblSearchBox.Text = "Search:";
		this.txtSearchBox.BackColor = System.Drawing.Color.White;
		this.txtSearchBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.txtSearchBox.Name = "txtSearchBox";
		this.txtSearchBox.Size = new System.Drawing.Size(350, 25);
		this.txtSearchBox.TextChanged += new System.EventHandler(searchBox_TextChanged);
		this.btnClearSearchBox.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.btnClearSearchBox.Image = NetStudio.IPS.Properties.Resources.Resources_512_close;
		this.btnClearSearchBox.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.btnClearSearchBox.Name = "btnClearSearchBox";
		this.btnClearSearchBox.Size = new System.Drawing.Size(23, 22);
		this.btnClearSearchBox.Text = "Clear";
		this.btnClearSearchBox.Click += new System.EventHandler(btnClearSearchBox_Click);
		this.toolStripSeparator4.Name = "toolStripSeparator4";
		this.toolStripSeparator4.Size = new System.Drawing.Size(6, 25);
		this.lblTotalName.Name = "lblTotalName";
		this.lblTotalName.Size = new System.Drawing.Size(35, 22);
		this.lblTotalName.Text = "Total:";
		this.lblTotalTags.ForeColor = System.Drawing.Color.Blue;
		this.lblTotalTags.Name = "lblTotalTags";
		this.lblTotalTags.Size = new System.Drawing.Size(13, 22);
		this.lblTotalTags.Text = "0";
		this.toolStripLabel1.ForeColor = System.Drawing.Color.DarkSlateGray;
		this.toolStripLabel1.Name = "toolStripLabel1";
		this.toolStripLabel1.Size = new System.Drawing.Size(29, 22);
		this.toolStripLabel1.Text = "tags";
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
		this.splitContainer1.IsSplitterFixed = true;
		this.splitContainer1.Location = new System.Drawing.Point(0, 25);
		this.splitContainer1.Name = "splitContainer1";
		this.splitContainer1.Panel1.Controls.Add(this.dataGrid_0);
		this.splitContainer1.Panel2.Controls.Add(this.treeViewMain);
		this.splitContainer1.Size = new System.Drawing.Size(1098, 654);
		this.splitContainer1.SplitterDistance = 841;
		this.splitContainer1.TabIndex = 4;
		this.dataGrid_0.AllowUserToAddRows = false;
		dataGridViewCellStyle.BackColor = System.Drawing.Color.DarkGray;
		this.dataGrid_0.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle;
		this.dataGrid_0.BackgroundColor = System.Drawing.Color.LightGray;
		this.dataGrid_0.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		dataGridViewCellStyle2.BackColor = System.Drawing.Color.Black;
		dataGridViewCellStyle2.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
		dataGridViewCellStyle2.ForeColor = System.Drawing.Color.White;
		dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
		dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
		dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
		this.dataGrid_0.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
		this.dataGrid_0.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
		this.dataGrid_0.Columns.AddRange(this.colID, this.colTagName, this.colFullName, this.colAddress, this.colDataType, this.colMode, this.colResolution, this.colIsScaling, this.colAImin, this.colAImax, this.colRLmin, this.colRLmax, this.colOffset, this.colDescription);
		this.dataGrid_0.ContextMenuStrip = this.contextMenuGrid;
		dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
		dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Window;
		dataGridViewCellStyle3.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
		dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.ControlText;
		dataGridViewCellStyle3.SelectionBackColor = System.Drawing.Color.LightGoldenrodYellow;
		dataGridViewCellStyle3.SelectionForeColor = System.Drawing.Color.Black;
		dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
		this.dataGrid_0.DefaultCellStyle = dataGridViewCellStyle3;
		this.dataGrid_0.Dock = System.Windows.Forms.DockStyle.Fill;
		this.dataGrid_0.Location = new System.Drawing.Point(0, 0);
		this.dataGrid_0.MultiSelect = false;
		this.dataGrid_0.Name = "dgv";
		this.dataGrid_0.ReadOnly = true;
		dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
		dataGridViewCellStyle4.BackColor = System.Drawing.Color.Black;
		dataGridViewCellStyle4.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
		dataGridViewCellStyle4.ForeColor = System.Drawing.SystemColors.WindowText;
		dataGridViewCellStyle4.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
		dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
		this.dataGrid_0.RowHeadersDefaultCellStyle = dataGridViewCellStyle4;
		dataGridViewCellStyle5.BackColor = System.Drawing.Color.LightGray;
		this.dataGrid_0.RowsDefaultCellStyle = dataGridViewCellStyle5;
		this.dataGrid_0.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
		this.dataGrid_0.Size = new System.Drawing.Size(841, 654);
		this.dataGrid_0.TabIndex = 3;
		this.dataGrid_0.VirtualMode = true;
		this.dataGrid_0.UserDeletingRow += new System.Windows.Forms.DataGridViewRowCancelEventHandler(OnDataGridViewUserDeletingRow);
		this.colID.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
		this.colID.DataPropertyName = "ID";
		dataGridViewCellStyle6.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
		this.colID.DefaultCellStyle = dataGridViewCellStyle6;
		this.colID.HeaderText = "ID";
		this.colID.MinimumWidth = 70;
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
		this.colMode.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
		this.colMode.DataPropertyName = "ModeName";
		dataGridViewCellStyle9.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
		this.colMode.DefaultCellStyle = dataGridViewCellStyle9;
		this.colMode.HeaderText = "Access";
		this.colMode.MinimumWidth = 90;
		this.colMode.Name = "colMode";
		this.colMode.ReadOnly = true;
		this.colMode.Resizable = System.Windows.Forms.DataGridViewTriState.True;
		this.colMode.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
		this.colMode.Width = 90;
		this.colResolution.DataPropertyName = "Resolution";
		dataGridViewCellStyle10.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
		this.colResolution.DefaultCellStyle = dataGridViewCellStyle10;
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
		dataGridViewCellStyle11.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
		this.colAImin.DefaultCellStyle = dataGridViewCellStyle11;
		this.colAImin.HeaderText = "AI min";
		this.colAImin.Name = "colAImin";
		this.colAImin.ReadOnly = true;
		this.colAImin.Width = 67;
		this.colAImax.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
		this.colAImax.DataPropertyName = "AImax";
		dataGridViewCellStyle12.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
		this.colAImax.DefaultCellStyle = dataGridViewCellStyle12;
		this.colAImax.HeaderText = "AI max";
		this.colAImax.Name = "colAImax";
		this.colAImax.ReadOnly = true;
		this.colAImax.Width = 69;
		this.colRLmin.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
		this.colRLmin.DataPropertyName = "RLmin";
		dataGridViewCellStyle13.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
		this.colRLmin.DefaultCellStyle = dataGridViewCellStyle13;
		this.colRLmin.HeaderText = "Real min";
		this.colRLmin.Name = "colRLmin";
		this.colRLmin.ReadOnly = true;
		this.colRLmin.Width = 78;
		this.colRLmax.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
		this.colRLmax.DataPropertyName = "RLmax";
		dataGridViewCellStyle14.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
		this.colRLmax.DefaultCellStyle = dataGridViewCellStyle14;
		this.colRLmax.HeaderText = "Real max";
		this.colRLmax.Name = "colRLmax";
		this.colRLmax.ReadOnly = true;
		this.colRLmax.Width = 80;
		this.colOffset.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
		this.colOffset.DataPropertyName = "Offset";
		dataGridViewCellStyle15.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
		this.colOffset.DefaultCellStyle = dataGridViewCellStyle15;
		this.colOffset.HeaderText = "Offset";
		this.colOffset.Name = "colOffset";
		this.colOffset.ReadOnly = true;
		this.colOffset.Width = 64;
		this.colDescription.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
		this.colDescription.DataPropertyName = "Description";
		this.colDescription.HeaderText = "Description";
		this.colDescription.Name = "colDescription";
		this.colDescription.ReadOnly = true;
		this.contextMenuGrid.Items.AddRange(new System.Windows.Forms.ToolStripItem[5] { this.mnGridCopy, this.toolStripSeparator1, this.mnGridEdit, this.toolStripSeparator3, this.mnGridDelete });
		this.contextMenuGrid.Name = "contextMenuGrid";
		this.contextMenuGrid.Size = new System.Drawing.Size(145, 82);
		this.mnGridCopy.Image = NetStudio.IPS.Properties.Resources.Resources_512_copy;
		this.mnGridCopy.Name = "mnGridCopy";
		this.mnGridCopy.ShortcutKeys = System.Windows.Forms.Keys.C | System.Windows.Forms.Keys.Control;
		this.mnGridCopy.Size = new System.Drawing.Size(144, 22);
		this.mnGridCopy.Text = "Copy";
		this.mnGridCopy.Click += new System.EventHandler(btnCopy_Click);
		this.toolStripSeparator1.Name = "toolStripSeparator1";
		this.toolStripSeparator1.Size = new System.Drawing.Size(141, 6);
		this.mnGridEdit.Image = NetStudio.IPS.Properties.Resources.Resources_512_edit_tag;
		this.mnGridEdit.Name = "mnGridEdit";
		this.mnGridEdit.ShortcutKeys = System.Windows.Forms.Keys.E | System.Windows.Forms.Keys.Control;
		this.mnGridEdit.Size = new System.Drawing.Size(144, 22);
		this.mnGridEdit.Text = "Edit";
		this.mnGridEdit.Click += new System.EventHandler(btnEdit_Click);
		this.toolStripSeparator3.Name = "toolStripSeparator3";
		this.toolStripSeparator3.Size = new System.Drawing.Size(141, 6);
		this.mnGridDelete.Image = NetStudio.IPS.Properties.Resources.Resources_512_remove;
		this.mnGridDelete.Name = "mnGridDelete";
		this.mnGridDelete.ShortcutKeys = System.Windows.Forms.Keys.Delete;
		this.mnGridDelete.Size = new System.Drawing.Size(144, 22);
		this.mnGridDelete.Text = "Delete";
		this.mnGridDelete.Click += new System.EventHandler(btnDelete_Click);
		this.treeViewMain.ContextMenuStrip = this.contextMenuTreeList;
		this.treeViewMain.Dock = System.Windows.Forms.DockStyle.Fill;
		this.treeViewMain.Location = new System.Drawing.Point(0, 0);
		this.treeViewMain.Name = "treeViewMain";
		this.treeViewMain.Size = new System.Drawing.Size(253, 654);
		this.treeViewMain.TabIndex = 0;
		this.treeViewMain.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(treeViewMain_AfterSelect);
		this.contextMenuTreeList.Items.AddRange(new System.Windows.Forms.ToolStripItem[8] { this.mnChannel, this.mnDevice, this.mnGroup, this.mnCopy, this.mnDelete, this.toolStripSeparator5, this.MnExpandAll, this.mnCollapseAll });
		this.contextMenuTreeList.Name = "contextMenu";
		this.contextMenuTreeList.Size = new System.Drawing.Size(181, 186);
		this.mnChannel.Image = NetStudio.IPS.Properties.Resources.Resources_512_channel;
		this.mnChannel.Name = "mnChannel";
		this.mnChannel.Size = new System.Drawing.Size(180, 22);
		this.mnChannel.Text = "&Channel";
		this.mnChannel.Click += new System.EventHandler(mnChannel_Click);
		this.mnDevice.Image = NetStudio.IPS.Properties.Resources.Resources_512_device;
		this.mnDevice.Name = "mnDevice";
		this.mnDevice.Size = new System.Drawing.Size(180, 22);
		this.mnDevice.Text = "&Device";
		this.mnDevice.Click += new System.EventHandler(mnDevice_Click);
		this.mnGroup.Image = NetStudio.IPS.Properties.Resources.Resources_512_group;
		this.mnGroup.Name = "mnGroup";
		this.mnGroup.Size = new System.Drawing.Size(180, 22);
		this.mnGroup.Text = "&Group";
		this.mnGroup.Click += new System.EventHandler(mnGroup_Click);
		this.mnCopy.Image = NetStudio.IPS.Properties.Resources.Resources_512_copy;
		this.mnCopy.Name = "mnCopy";
		this.mnCopy.ShortcutKeys = System.Windows.Forms.Keys.C | System.Windows.Forms.Keys.Control;
		this.mnCopy.Size = new System.Drawing.Size(180, 22);
		this.mnCopy.Text = "&Copy";
		this.mnCopy.Click += new System.EventHandler(mnCopy_Click);
		this.mnDelete.Image = NetStudio.IPS.Properties.Resources.Resources_512_copy;
		this.mnDelete.Name = "mnDelete";
		this.mnDelete.ShortcutKeys = System.Windows.Forms.Keys.Delete;
		this.mnDelete.Size = new System.Drawing.Size(180, 22);
		this.mnDelete.Text = "&Delete";
		this.mnDelete.Click += new System.EventHandler(mnDelete_Click);
		this.toolStripSeparator5.Name = "toolStripSeparator5";
		this.toolStripSeparator5.Size = new System.Drawing.Size(177, 6);
		this.MnExpandAll.Image = NetStudio.IPS.Properties.Resources.Resources_512_minus;
		this.MnExpandAll.Name = "MnExpandAll";
		this.MnExpandAll.Size = new System.Drawing.Size(180, 22);
		this.MnExpandAll.Text = "&Expand All ";
		this.MnExpandAll.Click += new System.EventHandler(mnExpandAll_Click);
		this.mnCollapseAll.Image = NetStudio.IPS.Properties.Resources.Resources_512_plus;
		this.mnCollapseAll.Name = "mnCollapseAll";
		this.mnCollapseAll.Size = new System.Drawing.Size(180, 22);
		this.mnCollapseAll.Text = "&Collapse All";
		this.mnCollapseAll.Click += new System.EventHandler(mnCollapseAll_Click);
		this.imageCollection.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
		this.imageCollection.ImageStream = (System.Windows.Forms.ImageListStreamer)resources.GetObject("imageCollection.ImageStream");
		this.imageCollection.TransparentColor = System.Drawing.Color.Transparent;
        imageCollection.Images.SetKeyName(0, "Resources_512_connection.png");
        imageCollection.Images.SetKeyName(1, "Resources_512_channel.png");
        imageCollection.Images.SetKeyName(2, "Resources_512_device.png");
        imageCollection.Images.SetKeyName(3, "Resources_512_group.png");
        base.AutoScaleDimensions = new System.Drawing.SizeF(7f, 15f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(1098, 679);
		base.Controls.Add(this.splitContainer1);
		base.Controls.Add(this.toolBar);
		this.DoubleBuffered = true;
		base.Name = "FormTagEditor";
		this.Text = "Tag Editor";
		base.Load += new System.EventHandler(FormTagEdit_Load);
		this.toolBar.ResumeLayout(false);
		this.toolBar.PerformLayout();
		this.splitContainer1.Panel1.ResumeLayout(false);
		this.splitContainer1.Panel2.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.splitContainer1).EndInit();
		this.splitContainer1.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.dataGrid_0).EndInit();
		this.contextMenuGrid.ResumeLayout(false);
		this.contextMenuTreeList.ResumeLayout(false);
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
