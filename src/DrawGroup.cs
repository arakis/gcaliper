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
		public Pixmap maskMap;
		public ImageSurface image;
		public TPartList parts = new TPartList ();
		protected Menu menu;
		private Style originalStyle;
		protected bool needRedraw = true;

		public TDrawGroup () : base (Gtk.WindowType.Toplevel)
		{
			originalStyle = this.Style.Copy ();
			Decorated = false;
			Events = EventMask.AllEventsMask;

			menu = new Menu ();
			var quitItem = new MenuItem ("Quit");
			menu.Add(quitItem);
			quitItem.ButtonReleaseEvent += (o, e) => {
				if(e.Event.Button==1)
					Application.Quit();
			};

			var color1 = new MenuItem ("Color");
			menu.Add(color1);
			color1.ButtonReleaseEvent += (o, e) => {
				if(e.Event.Button==1)
				{
					using(var chooser=new ColorSelectionDialog("change color")){
						//chooser.TransientFor=this;
						chooser.Style=originalStyle;

						if(chooser.Run()==(int)ResponseType.Ok){
							foreach(var part in parts){
								part.applyContrast(new TColor(chooser.ColorSelection.CurrentColor));
							}
							invalidateImage();
						}
						chooser.Hide();
					}
				}
			};

			setWindowShape ();
		}

		public void invalidateImage ()
		{
			if (needRedraw)
				return;

			needRedraw = true;
			QueueDraw ();
		}

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
		public TCaliperPart3 part3;

		public TCaliperGroup ()
		{
			parts.Add (part1 = new TCaliperPart1 ());
			parts.Add (part2 = new TCaliperPart2 ());
			parts.Add (part3 = new TCaliperPart3 ());

			//part2.rect.Y = 20;
			part2.rect.X = 100;

			//generateImage ();
			//generateMask ();
		}
		// *** configuration ***
		public POINT rotationCenterImage = new POINT (20, 65);
		public POINT displayCenterOffset = new POINT (45, 68);
		public int minX=15;
		public int minDistanceForRotation = 50;
		public double snapAngle = 0.5;
		// ***
		double angle = 0.0174532925 * 0;
		double tmpAngle = 0;
		public RECT unrotatedRect;
		public RECT rotatedRect;
		public POINT rotationCenterRoot = new POINT (1920 + 1920 / 2, 1200 / 2);
		public POINT rotationCenterZero = new POINT (0, 0);

		protected override bool OnConfigureEvent (EventConfigure evnt)
		{
			updateRotationCenter ();
			if (!positioned) {
				needRedraw = false;
				positioned = true;
				invalidateImage ();
			}
			return base.OnConfigureEvent (evnt);
		}

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
								pat.Matrix = new Matrix (){ X0 = -r.X, Y0 = -r.Y };
								//pat.Matrix = pat.Matrix;

								cr.SetSource (pat);
								cr.Rectangle (new Cairo.Rectangle (r.X, r.Y, r.Width, r.Height));
								cr.Fill ();

							}
						}
					}

					if (debug) {
						cr.LineWidth = 5;
						cr.SetSourceRGBA (1, 0, 0, 1);
						cr.Translate (debugPoint.X, debugPoint.Y);
						cr.Arc (0, 0, 2, 0, Math.PI * 2);
						cr.StrokePreserve ();
					}

				}

				//surf.WriteToPng ("test.png");

				//var angle = 0;
				var oldRotatedRect = rotatedRect;
				rotatedRect = funcs.rotateRect (unrotatedRect, rotationCenterZero, angle);

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

					foreach (var part in parts) {
						if (!part.rotate) {
							cr.Matrix = new Matrix ();

							var c = new POINT (part2.rect.Location.X + displayCenterOffset.X, part2.rect.Location.Y + displayCenterOffset.Y);

							part.rect.X = c.X;
							part.rect.Y = c.Y;

							var p = ImagePosToRotatedPos (part.rect.Location);

							p.X -= part.rect.Width / 2;
							p.Y -= part.rect.Height / 2;

							//Draw image

							using (var pat = new SurfacePattern (part.image)) {
								pat.Matrix = new Matrix (){ X0 = -p.X, Y0 = -p.Y };
								//pat.Matrix = pat.Matrix;

								cr.SetSource (pat);
								cr.Rectangle (new Cairo.Rectangle (p.X, p.Y, part.rect.Width, part.rect.Height));
								cr.Fill ();

								cr.SetSourceRGBA (0, 1, 0, 1);
								cr.SelectFontFace ("Arial", FontSlant.Normal, FontWeight.Normal);
								cr.SetFontSize (10);
								cr.MoveTo (p.X + 10, p.Y + 25);
								cr.ShowText (distance.ToString ()+" "+Math.Round(angle,1).ToString());
								cr.Fill ();
							}
						}
					}
/*
					cr.Matrix = new Matrix ();
					var pos = ImagePosToRotatedPos (part2.rect.Location);
					//cr.Translate (pos.X, pos.Y);

					cr.SetSourceRGBA (0, 1, 0, 1);
					cr.SelectFontFace ("Arial", FontSlant.Normal, FontWeight.Normal);
					cr.SetFontSize (10);
					cr.MoveTo (pos.X, pos.Y);
					cr.ShowText ("aaaa");
					cr.Fill ();
*/
				}

				//surf2.WriteToPng ("test2.png");

				if (image != null)
					image.Dispose ();
				image = surf2;
			}
		}

		public int distance {
			get{
				return part2.rect.X - minX;
			 }
			set{
				part2.rect.X =value+ minX;
			 }
		}

		public bool debug = false;
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
		private POINT startRootMousePos;
		private POINT startRectPos;
		private POINT startWinPos;
		private POINT mouseImagePos;
		private bool resizing = false;
		private bool moving = false;
		private POINT debugPoint = new POINT (10, 10);
		private double moveMouseAngleOffset;
		private int moveMouseXOffset;

		private POINT AbsPosToUnrotatedPos (POINT pos)
		{
			return funcs.rotatePoint (new POINT (mousePos.X + rotatedRect.X, mousePos.Y + rotatedRect.Y), new POINT (0, 0), -angle);
		}

		public int getDistanceToRotationCenter (POINT rootPos)
		{
			return (int)Math.Round (Math.Abs (Math.Sqrt (Math.Pow (rootPos.X - rotationCenterRoot.X, 2) + Math.Pow (rootPos.Y - rotationCenterRoot.Y, 2))));
		}

		protected override bool OnMotionNotifyEvent (EventMotion evnt)
		{
			rootMousePos = new POINT ((int)evnt.XRoot, (int)evnt.YRoot);
			mousePos = new POINT ((int)evnt.X, (int)evnt.Y);

			mouseImagePos = AbsPosToUnrotatedPos (mousePos);

			if (debug) {
				debugText = part2.rect.Contains (mouseImagePos).ToString ();
				debugPoint = mouseImagePos;
				invalidateImage ();
			}

			var relMousePos = new POINT (rootMousePos.X - startRootMousePos.X, rootMousePos.Y - startRootMousePos.Y);

			if (resizing) {
				if (Math.Abs (relMousePos.X) > 10 || Math.Abs (relMousePos.Y) > 10) {

					part2.rect.X = getDistanceToRotationCenter (rootMousePos);
					part2.rect.X -= moveMouseXOffset;
					part2.rect.X = Math.Max (part2.rect.X, minX);

					if (distance > minDistanceForRotation) {
						tmpAngle = funcs.GetAngleOfLineBetweenTwoPoints (rotationCenterRoot, rootMousePos);
						tmpAngle -= moveMouseAngleOffset;

						var angleMarkers = new double[]{ 0, Math.PI / 2, Math.PI, -Math.PI, -(Math.PI / 2) };

						for (var i = 0; i < angleMarkers.Length; i++) {
							var a = angleMarkers [i];
							if (tmpAngle < a + snapAngle && tmpAngle > a - snapAngle) {
								angle = a;
							}
						}
					}

					invalidateImage ();
				}
			}

			if (moving) {
				var x = (startWinPos.X + (rootMousePos.X - startRootMousePos.X));
				var y = (startWinPos.Y + (rootMousePos.Y - startRootMousePos.Y));
				Move (x, y);
				updateRotationCenter ();
			}

			return base.OnMotionNotifyEvent (evnt);
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
				startRootMousePos = new POINT ((int)evnt.XRoot, (int)evnt.YRoot);
				startRectPos = part2.rect.Location;

				if (part2.rect.Contains (mouseImagePos)) {
					resizing = true;

					moveMouseXOffset = getDistanceToRotationCenter (startRootMousePos) - part2.rect.X;
					moveMouseAngleOffset = funcs.GetAngleOfLineBetweenTwoPoints (rotationCenterRoot, startRootMousePos) - angle;

				} else if (part1.rect.Contains (mouseImagePos)) {
					moving = true;
				}
			}
			if (evnt.Button == 3) {
				this.menu.ShowAll ();
				this.menu.Popup ();
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

		public void updatePosition ()
		{
			var p = rotationCenterRoot;

			var r = funcs.rotatePoint (rotationCenterImage, rotationCenterZero, angle);

			p.X -= r.X;
			p.Y -= r.Y;

			p.X += rotatedRect.X;
			p.Y += rotatedRect.Y;

			Move (p.X, p.Y);
		}

		public void updateRotationCenter ()
		{
			//return;
			int x, y;
			GetPosition (out x, out y);

			var p = new POINT (x, y);
			var r = funcs.rotatePoint (rotationCenterImage, rotationCenterZero, angle);

			p.X -= rotatedRect.X;
			p.Y -= rotatedRect.Y;

			p.X += r.X;
			p.Y += r.Y;

			rotationCenterRoot = p;
		}

		public POINT ImagePosToRotatedPos (POINT imgPos)
		{
			var p = new POINT (0, 0);
			var r = funcs.rotatePoint (imgPos, rotationCenterZero, angle);

			p.X -= rotatedRect.X;
			p.Y -= rotatedRect.Y;

			p.X += r.X;
			p.Y += r.Y;

			return p;
		}

		public bool positioned = false;

		public void redraw ()
		{
			if (!positioned)
				return;

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

				updatePosition ();
			} catch (Exception ex) {
				new MessageDialog (null, DialogFlags.Modal, MessageType.Error, ButtonsType.Ok, ex.ToString ());
			}

		}

		protected override bool OnExposeEvent (EventExpose evnt)
		{
			if (needRedraw)
				redraw ();

			return base.OnExposeEvent (evnt);
		}
	}
}