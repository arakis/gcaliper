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

using Cairo;

namespace gcaliper
{
    public class Part
    {
        public virtual void ApplyContrast(Color c)
        {
        }

        public ImageSurface Image;
        public bool Rotate;
        //public bool drawNonrotated=false;
        //public PointD rotationCenter;
        //public double rotationAngle;
        public System.Drawing.Rectangle Rect;
        //public System.Drawing.Rectangle rotatedRect;

        public virtual void draw(Context cr)
        {
            var r = Rect;
            using (var pat = new SurfacePattern(Image))
            {
                pat.Matrix = new Matrix() { X0 = -r.X, Y0 = -r.Y };
                pat.Extend = Extend.Repeat;
                //pat.Matrix = pat.Matrix;

                cr.SetSource(pat);
                cr.Rectangle(new Cairo.Rectangle(r.X, r.Y, r.Width, r.Height));
                cr.Fill();

            }
        }

    }

}


