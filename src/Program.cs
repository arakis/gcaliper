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
			try {

				Application.Init ();
				AppConfig.init();

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
