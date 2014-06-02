using System;
using System.Collections.Generic;
using Gtk;
using Gdk;
using Cairo;

namespace gcaliper
{
	public class TDrawGroup : Gtk.Window
	{
		//public Pixbuf maskBuf;
		public Pixmap maskMap;
		public ImageSurface image;
		public TPartList parts = new TPartList ();

		public TDrawGroup () : base (Gtk.WindowType.Toplevel)
		{
			//current = this;

			//Build ();
			Decorated = false;

			/*			draw = new TCaliperGroup ();
			draw.Show ();
			Add (draw);*/

			Events = EventMask.AllEventsMask;

			setWindowShape ();
		}
		/*public TDrawGroup ()
		{
			Events = EventMask.AllEventsMask;
		}*/
		protected void OnDeleteEvent (object sender, DeleteEventArgs a)
		{
			Application.Quit ();
			a.RetVal = true;
		}

		public void setWindowShape ()
		{
			this.ShapeCombineMask (maskMap, 0, 0);
		}

		protected void generateMask ()
		{
			if (maskMap != null)
				maskMap.Dispose ();

			maskMap = new Gdk.Pixmap (null, image.Width, image.Height, 1);
			using (var cr = CairoHelper.Create (maskMap)) {
				cr.SetSourceRGB (0, 0, 0);
				cr.Operator = Operator.Clear;
				cr.Paint ();
				cr.Operator = Operator.Source;

				cr.SetSource (image, 0, 0);
				cr.Rectangle (new Cairo.Rectangle (0, 0, image.Width, image.Height));
				cr.Paint ();
			}
		}
	}

	public class TCaliperGroup : TDrawGroup
	{
		public TCaliperPart1 part1;
		public TCaliperPart2 part2;

		public TCaliperGroup ()
		{
			parts.Add (part1 = new TCaliperPart1 ());
			parts.Add (part2 = new TCaliperPart2 ());

			//part2.rect.Y = 20;
			part2.rect.X = 100;

			generateImage ();
			generateMask ();
		}

		double angle = 0.0174532925 * 22;
		public System.Drawing.Rectangle rotationRect;

		public void generateImage ()
		{
			rotationRect = parts.getRotationRect ();

			using (var surf = new Cairo.ImageSurface (Format.ARGB32, rotationRect.Width, rotationRect.Height)) {
				using (var cr = new Context (surf)) {

					//Clear
					cr.Operator = Operator.Clear;
					cr.Paint ();
					cr.Operator = Operator.Over;

					foreach (var part in parts) {
						if (part.rotate) {
							//Draw image

							var r = part.rect;

							using (var pat = new SurfacePattern (part.image)) {
								pat.Matrix = new Matrix (){ X0 = -r.X, Y0 = 0 };
								//pat.Matrix = pat.Matrix;

								cr.SetSource (pat);
								cr.Rectangle (new Cairo.Rectangle (r.X, r.Y, r.Width, r.Height));
								cr.Fill ();

							}
						}
					}
				}

				//surf.WriteToPng ("test.png");


				var center = new System.Drawing.Point (0, 0);
				//var angle = 0;
				var rr = funcs.rotateRect (rotationRect, center, angle);

				//Rotate
				var surf2 = new Cairo.ImageSurface (Format.ARGB32, rr.Width, rr.Height);
				using (var cr = new Context (surf2)) {
					cr.Operator = Operator.Clear;
					cr.Paint ();
					cr.Operator = Operator.Over;

					cr.Translate (-rr.X, -rr.Y);
					cr.Rotate (angle);
					//var pp = funcs.rotatePoint (rotationRect.Location, new System.Drawing.Point (0, 0), angle);
					using (var pat2 = new SurfacePattern (surf)) {
						//pat2.Matrix = new Matrix (){ X0 =  -rr.X, Y0 = -rr.Y };

						cr.SetSource (pat2);
						//cr.Translate (100, 100);
						cr.Paint ();
					}
				}

				//surf2.WriteToPng ("test2.png");

				//Surface to pixbuf
				/*
			image = new Pixbuf (Colorspace.Rgb, true, 8, surf2.Width, surf2.Height);
			for (var y = 0; y < surf2.Height; y++) {
				for (var x = 0; x < surf2.Width; x++) {
					image.setPixel (x, y, surf2.getPixelUInt (x, y));
				}
			}*/
				image = surf2;
			}
		}

		private System.Drawing.Point mousePos;
		private System.Drawing.Point startMousePos;
		private System.Drawing.Point startRectPos;
		private System.Drawing.Point startWinPos;

		private bool resizing = false;
		private bool moving = false;

		protected override bool OnMotionNotifyEvent (EventMotion evnt)
		{
			mousePos = new System.Drawing.Point ((int)evnt.XRoot, (int)evnt.YRoot);

			var relMousePos = new System.Drawing.Point (mousePos.X - startMousePos.X, mousePos.Y - startMousePos.Y);

			if (resizing) {
				part2.rect.X = (startRectPos.X + relMousePos.X);

				var rotatedCenter = new System.Drawing.PointF (0, 0); //TODO


				//angle = funcs.GetAngleOfLineBetweenTwoPoints (rotatedCenter, relMousePos);

				needRedraw = true;
				QueueDraw ();
			}

			if (moving) {
				var x = (startWinPos.X + (mousePos.X - startMousePos.X));
				var y = (startWinPos.Y + (mousePos.Y - startMousePos.Y));
				Move (x, y);
			}

			return base.OnMotionNotifyEvent (evnt);
		}

		protected override bool OnButtonPressEvent (EventButton evnt)
		{
			if (evnt.Button == 1) {
				int x;
				int y;
				GetPosition (out x, out y);

				startWinPos = new System.Drawing.Point (x, y);
				startMousePos = new System.Drawing.Point ((int)evnt.XRoot, (int)evnt.YRoot);
				startRectPos = part2.rect.Location;

				if (part2.rect.Contains ((int)evnt.X, (int)evnt.Y)) {
					resizing = true;

				} else if (part1.rect.Contains ((int)evnt.X, (int)evnt.Y)) {
					moving = true;
				}
			}

			return base.OnButtonPressEvent (evnt);
		}

		protected override bool OnButtonReleaseEvent (EventButton evnt)
		{
			if (evnt.Button == 1) {
				resizing = false;
				moving = false;
			}
			return base.OnButtonReleaseEvent (evnt);
		}

		private Pixmap bgPixMap;

		public void redraw ()
		{
			needRedraw = false;
			try {

				generateImage ();

				var pixmap = new Pixmap (null, image.Width, image.Height, 24);
				pixmap.Colormap = Colormap.System;
				using (var cr = CairoHelper.Create (pixmap)) {
					cr.SetSource (image);
					cr.Paint ();
				}

				generateMask ();

				this.Style.SetBgPixmap (StateType.Normal, pixmap);
				SetSizeRequest (image.Width, image.Height);

				if (this.bgPixMap != null)
					this.bgPixMap.Dispose ();
				this.bgPixMap = pixmap;
				setWindowShape ();

				//GdkWindow.DrawPixbuf (null, image, 0, 0, 0, 0, -1, -1, RgbDither.Normal, 0, 0);

				//s.SetBaseGC(StateType.Normal, 
				/*
				var cr = CairoHelper.Create (GdkWindow);

				cr.Rotate (Math.PI / 4);

				cr.SetSourceRGB (0, 0, 0);
				cr.Operator = Operator.Source;
				cr.SelectFontFace ("Arial", FontSlant.Normal, FontWeight.Normal);
				cr.SetFontSize(20);
				cr.MoveTo (20, 20);
				cr.ShowText ("text " + pos.ToString ());
				*/

			} catch (Exception ex) {
				new MessageDialog (null, DialogFlags.Modal, MessageType.Error, ButtonsType.Ok, ex.ToString ());
			}

		}

		bool needRedraw = true;

		protected override bool OnExposeEvent (EventExpose evnt)
		{
			if (needRedraw)
				redraw ();
			return base.OnExposeEvent (evnt);
		}
	}
}