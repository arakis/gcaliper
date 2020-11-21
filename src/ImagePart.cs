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
    public class ImagePart : Part
    {
        public override void ApplyContrast(Color color)
        {
            ReloadImage();

            for (var y = 0; y < Image.Height; y++)
            {
                for (var x = 0; x < Image.Width; x++)
                {
                    var c = Image.GetPixel(x, y);
                    if (c.R == 255 && c.G == 0 && c.B == 255 && c.A == 255)
                    {
                        Image.SetPixel(x, y, color);
                    }
                }
            }
        }

        public string FileName;

        public ImagePart(string file)
        {
            FileName = file;
            ReloadImage();
            Rect = new System.Drawing.Rectangle(0, 0, Image.Width, Image.Height);
        }

        public void ReloadImage()
        {
            if (Image != null)
                Image.Dispose();

            Image = new ImageSurface(FileName);
        }
    }

}


