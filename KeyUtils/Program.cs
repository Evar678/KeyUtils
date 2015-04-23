using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace KeyUtils
{
	static class Program
	{
		public static readonly float Version = 0.6F;
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new MainForm());
		}
	}
}
