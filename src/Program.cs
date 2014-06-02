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
			Application.Init ();
			var win = new TCaliperGroup ();
			win.Show ();

			Application.Run ();
		}

	}
}
