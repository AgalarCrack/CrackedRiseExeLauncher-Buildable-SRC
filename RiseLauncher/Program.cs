using System;
using System.Threading;
using System.Windows.Forms;

namespace RiseLauncher
{
	internal static class Program
	{
		[STAThread]
		private static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			FormMain form = new FormMain();
			new Thread(delegate()
			{
				form.start();
			})
			{
				IsBackground = false
			}.Start();
			Application.Run(form);
		}
	}
}
