using System;
using Gtk;
using Gdk;
using Cairo;

namespace gcaliper
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			try {
				Application.Init ();
				var win = new TCaliperGroup ();
				win.Show ();
				GLib.ExceptionManager.UnhandledException += (e) => {
					System.IO.File.AppendAllText ("gcaliper.error.log", e.ToString ());
				};
				Application.Run ();
			} catch (Exception e) {
				System.IO.File.AppendAllText ("gcaliper.error.log", e.ToString ());
			}
		}
	}
}
