using System;
using System.IO;
using Gtk;
using Gdk;
using Cairo;
using IO = System.IO;

namespace gcaliper
{
	class Program
	{

		public static void Main (string[] args)
		{
			Environment.CurrentDirectory = "/";

			try {

				GLib.ExceptionManager.UnhandledException += (e) => {
					IO.File.AppendAllText ("gcaliper.error.log", e.ToString ());
				};

				Application.Init ();
				AppConfig.init();

				var win = new TCaliperGroup ();
				win.Show ();

				Application.Run ();

			} catch (Exception e) {
				IO.File.AppendAllText ("gcaliper.error.log", e.ToString ());
			}
		}

	}
}
