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
using Cairo;
using Gdk;

namespace gcaliper
{
    public static class Extensions
    {
        public static Color GetPixel(this Pixbuf buf, int x, int y)
        {
            if (buf.NChannels == 4)
                return Color.FromArgbPointer(buf.Pixels + (y * buf.Rowstride) + (x * buf.NChannels));
            if (buf.NChannels == 3)
                return Color.FromRgbPointer(buf.Pixels + (y * buf.Rowstride) + (x * buf.NChannels));

            throw new NotSupportedException();
        }

        public static unsafe uint GetPixelUInt(this Pixbuf buf, int x, int y)
        {
            if (buf.NChannels == 4)
                return *(uint*)(buf.Pixels + (y * buf.Rowstride) + (x * buf.NChannels));

            throw new NotSupportedException();
        }

        public static unsafe uint GetPixelUInt(this ImageSurface buf, int x, int y)
        {
            var color = GetBgraPixelUInt(buf, x, y);

            byte a = (byte)(color >> 24);
            byte r = (byte)(color >> 16);
            byte g = (byte)(color >> 8);
            byte b = (byte)(color >> 0);

            return (uint)((a << 24) | (b << 16) | (g << 8) | (r << 0));

        }

        public static unsafe uint GetBgraPixelUInt(this ImageSurface buf, int x, int y)
        {
            if (buf.Format == Format.Argb32)
                return *(uint*)(buf.DataPtr + (y * buf.Stride) + (x * 4));
            if (buf.Format == Format.Rgb24)
                return *(uint*)(buf.DataPtr + (y * buf.Stride) + (x * 3));
            throw new NotSupportedException();
        }

        public static unsafe Color GetPixel(this ImageSurface buf, int x, int y)
        {

            if (buf.Format == Format.Argb32)
            {
                var ptr = buf.DataPtr + (y * buf.Stride) + (x * 4);
                return Color.FromArgbPointer2(ptr);
            }

            throw new NotSupportedException();
        }

        public static unsafe void SetPixel(this ImageSurface buf, int x, int y, Color c)
        {

            if (buf.Format == Format.Argb32)
            {
                byte* pix = (byte*)(buf.DataPtr + (y * buf.Stride) + (x * 4));
                *pix = c.B;
                *(pix + 1) = c.G;
                *(pix + 2) = c.R;
                *(pix + 3) = c.A;
                return;
            }

            throw new NotSupportedException();
        }

        public static unsafe void SetPixel(this Pixbuf buf, int x, int y, Color color)
        {
            byte* pix = (byte*)(buf.Pixels + (y * buf.Rowstride) + (x * buf.NChannels));
            *pix = color.R;
            *(pix + 1) = color.G;
            *(pix + 2) = color.B;
            if (buf.NChannels == 4)
                *(pix + 3) = color.A;
        }

        public static unsafe void SetPixel(this Pixbuf buf, int x, int y, uint argbcolor)
        {
            uint* pix = (uint*)(buf.Pixels + (y * buf.Rowstride) + (x * buf.NChannels));
            *pix = argbcolor;
        }
    }

}
