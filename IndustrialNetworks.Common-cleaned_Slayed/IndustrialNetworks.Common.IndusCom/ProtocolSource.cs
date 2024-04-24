using System.Collections.Generic;

namespace NetStudio.Common.IndusCom;

public static class ProtocolSource
{
	public static Dictionary<IpsProtocolType, string> IPC_TCP = new Dictionary<IpsProtocolType, string>
	{
		{
			IpsProtocolType.MODBUS_RTU,
			IpsProtocolType.MODBUS_RTU.GetDescription()
		},
		{
			IpsProtocolType.MODBUS_ASCII,
			IpsProtocolType.MODBUS_ASCII.GetDescription()
		},
		{
			IpsProtocolType.MODBUS_TCP,
			IpsProtocolType.MODBUS_TCP.GetDescription()
		}
	};

	public static Dictionary<IpsProtocolType, string> IPC_SERIAL = new Dictionary<IpsProtocolType, string>
	{
		{
			IpsProtocolType.MODBUS_RTU,
			IpsProtocolType.MODBUS_RTU.GetDescription()
		},
		{
			IpsProtocolType.MODBUS_ASCII,
			IpsProtocolType.MODBUS_ASCII.GetDescription()
		}
	};

	public static Dictionary<IpsProtocolType, string> SIEMENS_TCP = new Dictionary<IpsProtocolType, string> { 
	{
		IpsProtocolType.S7_TCP,
		IpsProtocolType.S7_TCP.GetDescription()
	} };

	public static Dictionary<IpsProtocolType, string> SIEMENS_SERIAL = new Dictionary<IpsProtocolType, string>
	{
		{
			IpsProtocolType.S7_MPI,
			IpsProtocolType.S7_MPI.GetDescription()
		},
		{
			IpsProtocolType.S7_PPI,
			IpsProtocolType.S7_PPI.GetDescription()
		}
	};

	public static Dictionary<IpsProtocolType, string> PANASONIC = new Dictionary<IpsProtocolType, string> { 
	{
		IpsProtocolType.MEWTOCOL_PROTOCOL,
		IpsProtocolType.MEWTOCOL_PROTOCOL.GetDescription()
	} };

	public static Dictionary<IpsProtocolType, string> MISUBISHI_TCP = new Dictionary<IpsProtocolType, string>
	{
		{
			IpsProtocolType.MISUBISHI_MC_PROTOCOL,
			IpsProtocolType.MISUBISHI_MC_PROTOCOL.GetDescription()
		},
		{
			IpsProtocolType.MISUBISHI_SLMP_PROTOCOL,
			IpsProtocolType.MISUBISHI_SLMP_PROTOCOL.GetDescription()
		}
	};

	public static Dictionary<IpsProtocolType, string> MISUBISHI_SERIAL = new Dictionary<IpsProtocolType, string>
	{
		{
			IpsProtocolType.DEDICATED1_PROTOCOL,
			IpsProtocolType.DEDICATED1_PROTOCOL.GetDescription()
		},
		{
			IpsProtocolType.DEDICATED4_PROTOCOL,
			IpsProtocolType.DEDICATED4_PROTOCOL.GetDescription()
		},
		{
			IpsProtocolType.FX_SERIAL_PROTOCOL,
			IpsProtocolType.FX_SERIAL_PROTOCOL.GetDescription()
		}
	};

	public static Dictionary<IpsProtocolType, string> OMRON_ETHERNET = new Dictionary<IpsProtocolType, string>
	{
		{
			IpsProtocolType.FINS_TCP_PROTOCOL,
			IpsProtocolType.FINS_TCP_PROTOCOL.GetDescription()
		},
		{
			IpsProtocolType.FINS_UDP_PROTOCOL,
			IpsProtocolType.FINS_UDP_PROTOCOL.GetDescription()
		}
	};

	public static Dictionary<IpsProtocolType, string> OMRON_SERIAL = new Dictionary<IpsProtocolType, string> { 
	{
		IpsProtocolType.HOSTLINK_FINS_PROTOCOL,
		IpsProtocolType.HOSTLINK_FINS_PROTOCOL.GetDescription()
	} };

	public static Dictionary<IpsProtocolType, string> LSIS = new Dictionary<IpsProtocolType, string>
	{
		{
			IpsProtocolType.CNET_XGT_PROTOCOL,
			IpsProtocolType.CNET_XGT_PROTOCOL.GetDescription()
		},
		{
			IpsProtocolType.FENET_XGT_PROTOCOL,
			IpsProtocolType.FENET_XGT_PROTOCOL.GetDescription()
		}
	};

	public static Dictionary<IpsProtocolType, string> VIGOR = new Dictionary<IpsProtocolType, string>
	{
		{
			IpsProtocolType.VS_PROTOCOL,
			IpsProtocolType.VS_PROTOCOL.GetDescription()
		},
		{
			IpsProtocolType.VB_PROTOCOL,
			IpsProtocolType.VB_PROTOCOL.GetDescription()
		}
	};

	public static Dictionary<IpsProtocolType, string> FATEK = new Dictionary<IpsProtocolType, string> { 
	{
		IpsProtocolType.FATEK_PROTOCOL,
		IpsProtocolType.FATEK_PROTOCOL.GetDescription()
	} };

	public static Dictionary<IpsProtocolType, string> DELTA_ETHERNET = new Dictionary<IpsProtocolType, string>
	{
		{
			IpsProtocolType.DELTA_TCP,
			IpsProtocolType.DELTA_TCP.GetDescription()
		},
		{
			IpsProtocolType.DELTA_ASCII,
			IpsProtocolType.DELTA_ASCII.GetDescription()
		},
		{
			IpsProtocolType.DELTA_RTU,
			IpsProtocolType.DELTA_RTU.GetDescription()
		}
	};

	public static Dictionary<IpsProtocolType, string> DELTA_SERIAL = new Dictionary<IpsProtocolType, string>
	{
		{
			IpsProtocolType.DELTA_ASCII,
			IpsProtocolType.DELTA_ASCII.GetDescription()
		},
		{
			IpsProtocolType.DELTA_RTU,
			IpsProtocolType.DELTA_RTU.GetDescription()
		}
	};

	public static Dictionary<IpsProtocolType, string> KEYENCE_ETHERNET = new Dictionary<IpsProtocolType, string> { 
	{
		IpsProtocolType.KEYENCE_MC_PROTOCOL,
		IpsProtocolType.KEYENCE_MC_PROTOCOL.GetDescription()
	} };

	public static Dictionary<IpsProtocolType, string> KEYENCE_SERIAL = new Dictionary<IpsProtocolType, string> { 
	{
		IpsProtocolType.KEYENCE_MC_PROTOCOL,
		IpsProtocolType.KEYENCE_MC_PROTOCOL.GetDescription()
	} };
}
