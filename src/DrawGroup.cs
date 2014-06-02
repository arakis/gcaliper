using System;
using System.Collections.Generic;
using Gtk;
using Gdk;
using Cairo;
using POINT = System.Drawing.Point;
using RECT = System.Drawing.Rectangle;

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
		public RECT unrotatedRect;
		public RECT rotatedRect;
		public POINT rotationCenter = new POINT (0, 0);
		//public POINT rotatedCorner = new POINT (0, 0);
		public void generateImage ()
		{
			unrotatedRect = parts.getRotationRect ();

			using (var surf = new Cairo.ImageSurface (Format.ARGB32, unrotatedRect.Width, unrotatedRect.Height)) {
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

					if (debug) {
						cr.LineWidth = 2;
						cr.SetSourceRGBA (1, 0, 0, 1);
						cr.Translate (debugPoint.X, debugPoint.Y);
						cr.Arc (0, 0, 2, 0, Math.PI * 2);
						cr.StrokePreserve ();
					}

				}

				//surf.WriteToPng ("test.png");


				//var angle = 0;
				rotatedRect = funcs.rotateRect (unrotatedRect, rotationCenter, angle);

				//Rotate
				var surf2 = new Cairo.ImageSurface (Format.ARGB32, rotatedRect.Width, rotatedRect.Height);
				using (var cr = new Context (surf2)) {
					cr.Operator = Operator.Clear;
					cr.Paint ();
					cr.Operator = Operator.Over;

					cr.Translate (-rotatedRect.X, -rotatedRect.Y);
					cr.Rotate (angle);
					//var pp = funcs.rotatePoint (rotationRect.Location, new POINT (0, 0), angle);
					using (var pat2 = new SurfacePattern (surf)) {
						//pat2.Matrix = new Matrix (){ X0 =  -rr.X, Y0 = -rr.Y };

						cr.SetSource (pat2);
						//cr.Translate (100, 100);
						cr.Paint ();
					}

					//Debug
					if (true) {
						cr.Matrix = new Matrix ();
						if (debugText != null) {
							//cr.Operator=Operator.Source;
							cr.SetSourceRGBA (0, 1, 0, 1);
							cr.SelectFontFace ("Arial", FontSlant.Normal, FontWeight.Normal);
							cr.SetFontSize (20);
							cr.MoveTo (20, 20);
							cr.ShowText (debugText);
							cr.Fill ();
						}
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

		public bool debug = true;
		private string _debugText;

		public string debugText {
			get {
				return _debugText;

			}set { 
				if (value == _debugText)
					return;
				_debugText = value;
				invalidateImage ();
			}
		}

		private POINT rootMousePos;
		private POINT mousePos;
		private POINT startMousePos;
		private POINT startRectPos;
		private POINT startWinPos;
		private POINT mouseImagePos;
		private bool resizing = false;
		private bool moving = false;
		private POINT debugPoint = new POINT (10, 10);

		private POINT AbsPosToUnrotatedPos (POINT pos)
		{
			return funcs.rotatePoint (new POINT (mousePos.X + rotatedRect.X, mousePos.Y + rotatedRect.Y), new POINT (0, 0), -angle);
		}

		protected override bool OnMotionNotifyEvent (EventMotion evnt)
		{
			rootMousePos = new POINT ((int)evnt.XRoot, (int)evnt.YRoot);
			mousePos = new POINT ((int)evnt.X, (int)evnt.Y);

			mouseImagePos = AbsPosToUnrotatedPos (mousePos);

			debugText = part2.rect.Contains (mouseImagePos).ToString ();
			debugPoint = mouseImagePos;
			invalidateImage ();


			var relMousePos = new POINT (rootMousePos.X - startMousePos.X, rootMousePos.Y - startMousePos.Y);

			if (resizing) {
				part2.rect.X = (startRectPos.X + relMousePos.X);

				angle = funcs.GetAngleOfLineBetweenTwoPoints (rotationCenter, relMousePos);

				invalidateImage ();
			}

			if (moving) {
				var x = (startWinPos.X + (rootMousePos.X - startMousePos.X));
				var y = (startWinPos.Y + (rootMousePos.Y - startMousePos.Y));
				Move (x, y);
			}

			return base.OnMotionNotifyEvent (evnt);
		}

		public void invalidateImage ()
		{
			if (needRedraw)
				return;
			needRedraw = true;
			QueueDraw ();
		}

		protected override bool OnButtonPressEvent (EventButton evnt)
		{
			mousePos = new POINT ((int)evnt.X, (int)evnt.Y);
			mouseImagePos = AbsPosToUnrotatedPos (mousePos);
			if (evnt.Button == 1) {
				int x;
				int y;
				GetPosition (out x, out y);

				startWinPos = new POINT (x, y);
				startMousePos = new POINT ((int)evnt.XRoot, (int)evnt.YRoot);
				startRectPos = part2.rect.Location;

				if (part2.rect.Contains (mouseImagePos)) {
					resizing = true;

				} else if (part2.rect.Contains (mouseImagePos)) {
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