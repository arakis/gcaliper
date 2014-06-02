using System;
using Gtk;
using Gdk;
using Cairo;

namespace shapetest
{
	public partial class MainWindow: Gtk.Window
	{
		TDrawGroup draw;

		public static MainWindow current;

		public MainWindow () : base (Gtk.WindowType.Toplevel)
		{
			current = this;

			Build ();
			Decorated = false;

			draw = new TCaliperGroup ();
			draw.Show ();
			Add (draw);

			setWindowShape ();
		}

		public void setWindowShape ()
		{
			this.ShapeCombineMask (draw.maskMap, 0, 0);
		}

		protected void OnDeleteEvent (object sender, DeleteEventArgs a)
		{
			Application.Quit ();
			a.RetVal = true;
		}
	}
}