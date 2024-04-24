using System;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using NetStudio.IPS.Controls;

namespace NetStudio.IPS;

public class DriverController
{
	private const string ServiceName = "DriverServer";

	private bool servicePoll = true;

	private ServiceController? serviceController;

	public EventDriverStatusChanged OnDriverStatusChanged;

	public EventLoadProjectChanged OnLoadProjectChanged;

	private Form? formParent;

	public ServiceControllerStatus DriverStatus
	{
		get
		{
			serviceController = ServiceController.GetServices().FirstOrDefault((ServiceController serviceController_0) => serviceController_0.ServiceName == "DriverServer");
			if (serviceController == null)
			{
				return ServiceControllerStatus.Stopped;
			}
			return serviceController.Status;
		}
	}

	public ServiceController? DriverService
	{
		get
		{
			serviceController = ServiceController.GetServices().FirstOrDefault((ServiceController serviceController_0) => serviceController_0.ServiceName == "DriverServer");
			return serviceController;
		}
	}

	public DriverController(Form parent)
	{
		formParent = parent;
		ThreadPool.QueueUserWorkItem(OnDriverPolling);
	}

	private async void OnDriverPolling(object? obj)
	{
		while (servicePoll)
		{
			 
			try
			{
				try
				{
					serviceController = ServiceController.GetServices().FirstOrDefault((ServiceController serviceController_0) => serviceController_0.ServiceName == "DriverServer");
					if (serviceController == null)
					{
						if (OnDriverStatusChanged != null)
						{
							OnDriverStatusChanged(ServiceControllerStatus.Stopped, notAvailable: true);
						}
					}
					else if (OnDriverStatusChanged != null)
					{
						OnDriverStatusChanged(serviceController.Status);
					}
				}
				catch (Exception)
				{
				}
			}
			catch (Exception obj2)
			{
				obj = obj2;
			}
			await Task.Delay(1000);
			 
		}
	}

	public async void Start()
	{
		serviceController = ServiceController.GetServices().FirstOrDefault((ServiceController serviceController_0) => serviceController_0.ServiceName == "DriverServer");
		if (serviceController != null && serviceController.Status == ServiceControllerStatus.Stopped)
		{
			await WaitFormManager.ShowAsync(formParent, "Starting...");
			if (OnDriverStatusChanged != null)
			{
				OnDriverStatusChanged(ServiceControllerStatus.StartPending);
			}
			serviceController.Start();
			await WaitFormManager.CloseAsync();
		}
	}

	public async void Stop()
	{
		serviceController = ServiceController.GetServices().FirstOrDefault((ServiceController serviceController_0) => serviceController_0.ServiceName == "DriverServer");
		if (serviceController != null && serviceController.Status == ServiceControllerStatus.Running)
		{
			await WaitFormManager.ShowAsync(formParent, "Stopping...");
			if (OnDriverStatusChanged != null)
			{
				OnDriverStatusChanged(ServiceControllerStatus.StopPending);
			}
			serviceController.Stop();
			await WaitFormManager.CloseAsync();
		}
	}

	public async void Restart()
	{
		serviceController = ServiceController.GetServices().FirstOrDefault((ServiceController serviceController_0) => serviceController_0.ServiceName == "DriverServer");
		if (serviceController != null && serviceController.Status == ServiceControllerStatus.Running)
		{
			await WaitFormManager.ShowAsync(formParent, "Stopping...");
			if (OnDriverStatusChanged != null)
			{
				OnDriverStatusChanged(ServiceControllerStatus.StopPending);
			}
			serviceController.Stop();
			await WaitFormManager.CloseAsync();
		}
		Thread.Sleep(1000);
		serviceController = ServiceController.GetServices().FirstOrDefault((ServiceController serviceController_0) => serviceController_0.ServiceName == "DriverServer");
		if (serviceController != null && serviceController.Status == ServiceControllerStatus.Stopped)
		{
			await WaitFormManager.ShowAsync(formParent, "Starting...");
			if (OnDriverStatusChanged != null)
			{
				OnDriverStatusChanged(ServiceControllerStatus.StartPending);
			}
			serviceController.Start();
			await WaitFormManager.CloseAsync();
		}
	}

	public async Task StartAsync()
	{
		await Task.Run(delegate
		{
			Start();
		});
	}
}
