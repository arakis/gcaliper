/*******************************************************************************************************

  Copyright (C) Sebastian Loncar, Web: http://loncar.de
  Project: https://github.com/Arakis/gcaliper

  MIT License:

  Permission is hereby granted, free of charge, to any person obtaining a copy of this software and
  associated documentation files (the "Software"), to deal in the Software without restriction,
  including without limitation the rights to use, copy, modify, merge, publish, distribute,
  sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is
  furnished to do so, subject to the following conditions:

  The above copyright notice and this permission notice shall be included in all copies or substantial
  portions of the Software.

  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT
  NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
  NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES
  OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
  CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

*******************************************************************************************************/

using System;
using System.Drawing;
using Gtk;
using POINT = System.Drawing.Point;
using RECT = System.Drawing.Rectangle;

namespace gcaliper
{
    public static class Helper
    {

        public static double RadToDeg(double rad)
        {
            return 180 / Math.PI * rad;
        }

        public static double GetAngleOfLineBetweenTwoPoints(PointF p1, PointF p2)
        {
            double xDiff = p2.X - p1.X;
            double yDiff = p2.Y - p1.Y;
            return Math.Atan2(yDiff, xDiff);
        }

        public static POINT Add(this POINT p, int x, int y)
        {
            return new POINT(p.X + x, p.Y + y);
        }

        public static void ShowMessage(string txt)
        {
            new MessageDialog(null, DialogFlags.Modal, MessageType.Error, ButtonsType.Ok, txt).ShowAll();
        }

        public static PointF RotatePoint(PointF p, PointF center, double angle)
        {
            var x = (Math.Cos(angle) * (p.X - center.X)) - (Math.Sin(angle) * (p.Y - center.Y)) + center.X;
            var y = (Math.Sin(angle) * (p.X - center.X)) + (Math.Cos(angle) * (p.Y - center.Y)) + center.Y;
            return new PointF((float)x, (float)y);
        }

        public static POINT RotatePoint(POINT p, POINT center, double angle)
        {
            double s = Math.Sin(angle);
            double c = Math.Cos(angle);

            // translate point back to origin:
            p.X -= center.X;
            p.Y -= center.Y;

            // rotate point
            double xnew = (p.X * c) - (p.Y * s);
            double ynew = (p.X * s) + (p.Y * c);

            // translate point back:
            p.X = (int)Math.Round(xnew + center.X);
            p.Y = (int)Math.Round(ynew + center.Y);

            return p;
        }

        public static RECT RotateRect(RECT r, POINT center, double angle)
        {
            var p1 = new POINT(r.Left, r.Top);
            var p2 = new POINT(r.Right, r.Top);
            var p3 = new POINT(r.Right, r.Bottom);
            var p4 = new POINT(r.Left, r.Bottom);

            var r1 = RotatePoint(p1, center, angle);
            var r2 = RotatePoint(p2, center, angle);
            var r3 = RotatePoint(p3, center, angle);
            var r4 = RotatePoint(p4, center, angle);

            var left = Math.Min(r1.X, Math.Min(r2.X, Math.Min(r3.X, r4.X)));
            var right = Math.Max(r1.X, Math.Max(r2.X, Math.Max(r3.X, r4.X)));

            var top = Math.Min(r1.Y, Math.Min(r2.Y, Math.Min(r3.Y, r4.Y)));
            var bottom = Math.Max(r1.Y, Math.Max(r2.Y, Math.Max(r3.Y, r4.Y)));

            return new RECT(left, top, right - left, bottom - top);
        }
    }

}
