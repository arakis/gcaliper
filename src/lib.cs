using System;
using System.Collections.Generic;
using Gtk;
using Gdk;
using Cairo;
using System.Drawing;
using POINT = System.Drawing.Point;
using RECT = System.Drawing.Rectangle;

namespace gcaliper
{
	public static class funcs
	{
		public static double GetAngleOfLineBetweenTwoPoints (PointF p1, PointF p2)
		{
			double xDiff = p2.X - p1.X;
			double yDiff = p2.Y - p1.Y;
			return Math.Atan2 (yDiff, xDiff); 
		}

		public static POINT add(this POINT p, int x, int y) {
			return new POINT (p.X+x, p.Y+y);
		}

		public static void showMessage (string txt)
		{
			new MessageDialog (null, DialogFlags.Modal, MessageType.Error, ButtonsType.Ok, txt).ShowAll();
		}

		public static PointF rotatePoint (PointF p, PointF center, double angle)
		{
			var x = Math.Cos (angle) * (p.X - center.X) - Math.Sin (angle) * (p.Y - center.Y) + center.X;
			var y = Math.Sin (angle) * (p.X - center.X) + Math.Cos (angle) * (p.Y - center.Y) + center.Y;
			return new PointF ((float)x, (float)y);
		}

		public static POINT _rotatePoint (POINT p, POINT center, double angle)
		{
			var x = Math.Cos (angle) * (p.X - center.X) - Math.Sin (angle) * (p.Y - center.Y) + center.X;
			var y = Math.Sin (angle) * (p.X - center.X) + Math.Cos (angle) * (p.Y - center.Y) + center.Y;
			return new POINT ((int)Math.Round (x), (int)Math.Round (y));
		}

		public static POINT  rotatePoint (POINT p, POINT center, double angle)
		{
			double s = Math.Sin (angle);
			double c = Math.Cos (angle);

			// translate point back to origin:
			p.X -= center.X;
			p.Y -= center.Y;

			// rotate point
			double xnew = p.X * c - p.Y * s;
			double ynew = p.X * s + p.Y * c;

			// translate point back:
			p.X = (int)Math.Round (xnew + center.X);
			p.Y = (int)Math.Round (ynew + center.Y);

			return p;
		}

		public static RECT rotateRect (RECT r, POINT center, double angle)
		{
			var p1 = new POINT (r.Left, r.Top);
			var p2 = new POINT (r.Right, r.Top);
			var p3 = new POINT (r.Right, r.Bottom);
			var p4 = new POINT (r.Left, r.Bottom);

			var r1 = rotatePoint (p1, center, angle);
			var r2 = rotatePoint (p2, center, angle);
			var r3 = rotatePoint (p3, center, angle);
			var r4 = rotatePoint (p4, center, angle);

			var left = Math.Min (r1.X, Math.Min (r2.X, Math.Min (r3.X, r4.X)));
			var right = Math.Max (r1.X, Math.Max (r2.X, Math.Max (r3.X, r4.X)));

			var top = Math.Min (r1.Y, Math.Min (r2.Y, Math.Min (r3.Y, r4.Y)));
			var bottom = Math.Max (r1.Y, Math.Max (r2.Y, Math.Max (r3.Y, r4.Y)));

			return new RECT (left, top, right - left, bottom - top);
		}
	}

	public static class Extensions
	{
		public static TColor getPixel (this Pixbuf buf, int x, int y)
		{
			if (buf.NChannels == 4)
				return TColor.fromArgbPointer (buf.Pixels + y * buf.Rowstride + x * buf.NChannels);
			if (buf.NChannels == 3)
				return TColor.fromRgbPointer (buf.Pixels + y * buf.Rowstride + x * buf.NChannels);

			throw new NotSupportedException ();
		}

		public unsafe static uint getPixelUInt (this Pixbuf buf, int x, int y)
		{
			if (buf.NChannels == 4)
				return *((uint*)(buf.Pixels + y * buf.Rowstride + x * buf.NChannels));

			throw new NotSupportedException ();
		}

		public unsafe static uint getPixelUInt (this ImageSurface buf, int x, int y)
		{
			var color = getBgraPixelUInt (buf, x, y);
			/*
			byte a = (byte)(color >> 24);
			byte r = (byte)(color >> 16);
			byte g = (byte)(color >> 8);
			byte b = (byte)(color >> 0);
*/

			byte a = (byte)(color >> 24);
			byte r = (byte)(color >> 16);
			byte g = (byte)(color >> 8);
			byte b = (byte)(color >> 0);

			//var gg = (a << 16);

			return (uint)((a << 24) | (b << 16) | (g << 8) | (r << 0));

		}

		public unsafe static uint getBgraPixelUInt (this ImageSurface buf, int x, int y)
		{
			if (buf.Format == Format.Argb32)
				return *((uint*)(buf.DataPtr + y * buf.Stride + x * 4));
			if (buf.Format == Format.Rgb24)
				return *((uint*)(buf.DataPtr + y * buf.Stride + x * 3));
			throw new NotSupportedException ();
		}

		public unsafe static TColor getPixel (this ImageSurface buf, int x, int y)
		{

			if (buf.Format == Format.Argb32) {
				var ptr = buf.DataPtr + y * buf.Stride + x * 4;


				return TColor.fromArgbPointer2 (ptr);
			}

			throw new NotSupportedException ();
		}

		public unsafe static void setPixel (this ImageSurface buf, int x, int y, TColor c)
		{

			if (buf.Format == Format.Argb32) {
				byte* pix = (byte*)(buf.DataPtr + y * buf.Stride + x * 4);
				*(pix) = c.b;
				*(pix + 1) = c.g;
				*(pix + 2) = c.r;
				*(pix + 3) = c.a;
				return;
			}

			throw new NotSupportedException ();
		}

		public unsafe static void setPixel (this Pixbuf buf, int x, int y, TColor color)
		{
			byte* pix = (byte*)(buf.Pixels + y * buf.Rowstride + x * buf.NChannels);
			*(pix) = color.r;
			*(pix + 1) = color.g;
			*(pix + 2) = color.b;
			if (buf.NChannels == 4)
				*(pix + 3) = color.a;
		}

		public unsafe static void setPixel (this Pixbuf buf, int x, int y, uint argbcolor)
		{
			uint* pix = (uint*)(buf.Pixels + y * buf.Rowstride + x * buf.NChannels);
			*(pix) = argbcolor;
		}
	}

	public struct TColor
	{
		public byte r;
		public byte g;
		public byte b;
		public byte a;

		public TColor (byte r, byte g, byte b)
		{
			this.r = r;
			this.g = g;
			this.b = b;
			this.a = 255;
		}

		public TColor (byte r, byte g, byte b, byte a)
		{
			this.r = r;
			this.g = g;
			this.b = b;
			this.a = a;
		}

		public TColor (Gdk.Color c)
		{
			this.r = (byte)Math.Round (((double)byte.MaxValue / (double)ushort.MaxValue * (double)c.Red), 0);
			this.g = (byte)Math.Round (((double)byte.MaxValue / (double)ushort.MaxValue * (double)c.Green), 0);
			this.b = (byte)Math.Round (((double)byte.MaxValue / (double)ushort.MaxValue * (double)c.Blue), 0);
			this.a = 255;
		}

		public unsafe static TColor fromArgbPointer (IntPtr ptr)
		{
			var c = new TColor ();
			byte* pix = (byte*)ptr;
			c.r = *pix;
			c.g = *(pix + 1);
			c.b = *(pix + 2);
			c.a = *(pix + 3);
			return c;
		}

		public unsafe static TColor fromArgbPointer2 (IntPtr ptr)
		{
			var c = new TColor ();
			byte* pix = (byte*)ptr;
			c.r = *(pix + 2);
			c.g = *(pix + 1);
			c.b = *(pix + 0);
			c.a = *(pix + 3);
			return c;
		}

		public unsafe static TColor fromRgbPointer (IntPtr ptr)
		{
			var c = new TColor ();
			byte* pix = (byte*)ptr;
			c.r = *pix;
			c.g = *(pix + 1);
			c.b = *(pix + 2);
			return c;
		}
	}

	public class TPartList: List<TPart>
	{
		public System.Drawing.Rectangle getRotationRect ()
		{
			var rect = new System.Drawing.Rectangle ();
			foreach (var p in this) {
				if (p.rotate) {
					rect = System.Drawing.Rectangle.Union (rect, p.rect);
				}
			}
			return rect;
		}
	}

	public class TPart
	{
		public virtual void applyContrast (TColor c)
		{
		}

		public ImageSurface image;
		public bool rotate;
		//public bool drawNonrotated=false;
		//public PointD rotationCenter;
		//public double rotationAngle;
		public System.Drawing.Rectangle rect;
		//public System.Drawing.Rectangle rotatedRect;

		public virtual void draw(Context cr){
			var r = rect;
			using (var pat = new SurfacePattern (image)) {
				pat.Matrix = new Matrix (){ X0 = -r.X, Y0 = -r.Y };
				pat.Extend = Extend.Repeat;
				//pat.Matrix = pat.Matrix;

				cr.SetSource (pat);
				cr.Rectangle (new Cairo.Rectangle (r.X, r.Y, r.Width, r.Height));
				cr.Fill ();

			}
		}

	}

	public class TImagePart : TPart
	{
		public override void applyContrast (TColor color)
		{
			reloadImage ();

			for (var y = 0; y < image.Height; y++) {
				for (var x = 0; x < image.Width; x++) {
					var c = image.getPixel (x, y);
					if (c.r == 255 && c.g == 0 && c.b == 255 && c.a == 255) {
						image.setPixel (x, y, color);
					}
				}
			}
		}

		public string fileName;

		public TImagePart (string file)
		{
			this.fileName = file;
			reloadImage ();
			rect = new System.Drawing.Rectangle (0, 0, image.Width, image.Height);
		}

		public void reloadImage ()
		{
			if (image != null)
				image.Dispose ();

			image = new ImageSurface (fileName);
		}
	}

	public class TCaliperPartHead : TImagePart
	{
		public TCaliperPartHead () : base ("../themes/caliper/head.png")
		{
			rotate = true;
		}
	}

	public class TCaliperPartBottom : TImagePart
	{
		public TCaliperPartBottom () : base ("../themes/caliper/bottom.png")
		{
			rotate = true;
		}
	}

	public class TCaliperPartDisplay : TImagePart
	{
		public TCaliperPartDisplay () : base ("../themes/caliper/display.png")
		{
			rotate = false;
			//drawNonrotated = true;
		}
	}

	public class TCaliperPartScale : TImagePart
	{
		public TCaliperPartScale () : base ("../themes/caliper/scale.png")
		{
			rotate = true;
			//drawNonrotated = true;
		}

	}

}


