using System.ComponentModel;

namespace NetStudio.Keyence.MC;

public enum PLCModel
{
	[Description("KV-8000")]
	KV_8000,
	[Description("KV-7500")]
	KV_7500,
	[Description("KV-7300")]
	KV_7300,
	[Description("KV-5500")]
	KV_5500,
	[Description("KV-5000")]
	KV_5000,
	[Description("KV-3000")]
	KV_3000,
	[Description("KV-1000")]
	KV_1000,
	[Description("KV-700+M")]
	KV_700M,
	[Description("KV-700")]
	KV_700,
	[Description("KV-NC32")]
	KV_NC32,
	[Description("KV-N60")]
	KV_N60,
	[Description("KV-N40")]
	KV_N40,
	[Description("KV-N24")]
	KV_N24,
	[Description("KV-N14")]
	KV_N14,
	[Description("KV-24(40)")]
	KV_24_40,
	[Description("KV-10(16)")]
	KV_10_16,
	[Description("KV-P16")]
	KV_P16
}
