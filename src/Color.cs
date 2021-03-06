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

namespace gcaliper
{

    public struct Color
    {
        public byte R;
        public byte G;
        public byte B;
        public byte A;

        public Color(byte r, byte g, byte b)
        {
            R = r;
            G = g;
            B = b;
            A = 255;
        }

        public Color(byte r, byte g, byte b, byte a)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        public Color(Gdk.Color c)
        {
            R = (byte)Math.Round(byte.MaxValue / (double)ushort.MaxValue * c.Red, 0);
            G = (byte)Math.Round(byte.MaxValue / (double)ushort.MaxValue * c.Green, 0);
            B = (byte)Math.Round(byte.MaxValue / (double)ushort.MaxValue * c.Blue, 0);
            A = 255;
        }

        public static unsafe Color FromArgbPointer(IntPtr ptr)
        {
            var c = new Color();
            byte* pix = (byte*)ptr;
            c.R = *pix;
            c.G = *(pix + 1);
            c.B = *(pix + 2);
            c.A = *(pix + 3);
            return c;
        }

        public static unsafe Color FromArgbPointer2(IntPtr ptr)
        {
            var c = new Color();
            byte* pix = (byte*)ptr;
            c.R = *(pix + 2);
            c.G = *(pix + 1);
            c.B = *(pix + 0);
            c.A = *(pix + 3);
            return c;
        }

        public static unsafe Color FromRgbPointer(IntPtr ptr)
        {
            var c = new Color();
            byte* pix = (byte*)ptr;
            c.R = *pix;
            c.G = *(pix + 1);
            c.B = *(pix + 2);
            return c;
        }
    }

}
