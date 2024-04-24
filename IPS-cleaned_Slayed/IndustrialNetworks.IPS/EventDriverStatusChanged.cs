using System.ServiceProcess;

namespace NetStudio.IPS;

public delegate void EventDriverStatusChanged(ServiceControllerStatus status, bool notAvailable = false);
