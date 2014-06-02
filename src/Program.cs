using System;
using Gtk;
using Gdk;
using Cairo;

namespace shapetest
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
