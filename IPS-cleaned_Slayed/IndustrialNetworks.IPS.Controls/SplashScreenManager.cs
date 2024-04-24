using System.Threading.Tasks;
using System.Windows.Forms;

namespace NetStudio.IPS.Controls;

public class SplashScreenManager
{
	public static SplashScreen? splashScreen;

	public static void Show()
	{
		splashScreen = new SplashScreen
		{
			StartPosition = FormStartPosition.CenterScreen
		};
		Task.Run(delegate
		{
			splashScreen.ShowDialog();
		});
	}

	public static void SetVisible(bool show)
	{
		if (splashScreen != null)
		{
			splashScreen.Opacity = (show ? 100 : 0);
		}
	}

	public static void Close()
	{
		if (splashScreen != null)
		{
			splashScreen.Dispose();
		}
	}
}
