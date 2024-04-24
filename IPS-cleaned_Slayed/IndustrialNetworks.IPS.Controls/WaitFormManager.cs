using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NetStudio.IPS.Controls;

internal class WaitFormManager
{
	private static CancellationTokenSource? _cancellationTokenSource;

	public static async Task ShowAsync(Form parent, string message)
	{
	
		if (_cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested)
		{
			await _cancellationTokenSource.CancelAsync();
		}
		_cancellationTokenSource = new CancellationTokenSource(9000);
		Thread thread = new Thread(delegate(object? cancellationToken)
		{
			WaitForm waitForm = new WaitForm(message, (CancellationToken)cancellationToken);
			waitForm.ShowInTaskbar = true;
			waitForm.TopMost = true;
			waitForm.TopLevel = true;
			waitForm.StartPosition = FormStartPosition.CenterScreen;
			waitForm.ShowDialog();
		});
		thread.IsBackground = true;
		thread.Start(_cancellationTokenSource.Token);
	}

	public static async Task CloseAsync()
	{
		if (_cancellationTokenSource != null)
		{
			await _cancellationTokenSource.CancelAsync();
			_cancellationTokenSource = null;
		}
	}
}
