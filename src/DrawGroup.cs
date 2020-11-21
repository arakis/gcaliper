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

using System.Collections.Generic;
using System.IO;
using Gtk;
using Gdk;
using Cairo;
using IO = System.IO;

namespace gcaliper
{
    public class TDrawGroup : Gtk.Window
    {
        public Cairo.Region maskMap;
        public ImageSurface image;
        public TPartList parts = new TPartList();
        protected Menu menu;
        protected Style originalStyle;
        protected bool needRedraw = true;
        //protected StatusIcon statusIcon;
        public TDrawGroup()
            : base(Gtk.WindowType.Toplevel)
        {
            originalStyle = this.Style.Copy();
            Decorated = false;
            Events = EventMask.AllEventsMask;

            //statusIcon = new StatusIcon ("../appicon.ico");
            //statusIcon.Tooltip = "gcaliper";
            SetIconFromFile(IO.Path.Combine(AppConfig.appRootDir, "appicon.ico"));

            menu = new Menu();

            var minItem = new MenuItem("Minimize");
            menu.Add(minItem);
            minItem.ButtonReleaseEvent += (o, e) =>
            {
                if (e.Event.Button == 1)
                {
                    Iconify();
                    //statusIcon.Visible=true;
                    //Hide();
                }
            };

            var quitItem = new MenuItem("Quit");
            menu.Add(quitItem);
            quitItem.ButtonReleaseEvent += (o, e) =>
            {
                if (e.Event.Button == 1)
                    Application.Quit();
            };

            setWindowShape();
        }

        public void invalidateImage()
        {
            if (needRedraw)
                return;

            needRedraw = true;
            QueueDraw();
        }

        protected void OnDeleteEvent(object sender, DeleteEventArgs a)
        {
            Application.Quit();
            a.RetVal = true;
        }

        protected override void OnHidden()
        {
            Application.Quit();
            base.OnHidden();
        }

        public void setWindowShape()
        {
            //this.ShapeCombineMask(maskMap, 0, 0);
            ShapeCombineRegion(maskMap);
        }

        protected void generateMask()
        {
            if (maskMap != null)
                maskMap.Dispose();

            using (var maskImage = new ImageSurface(Format.Argb32, image.Width, image.Height))
            {
                using (var cr = new Context(maskImage))
                {
                    cr.SetSourceRGB(0, 0, 0);
                    cr.Operator = Operator.Clear;
                    cr.Paint();
                    cr.Operator = Operator.Source;

                    cr.SetSource(image, 0, 0);
                    cr.Rectangle(new Cairo.Rectangle(0, 0, image.Width, image.Height));
                    cr.Paint();
                }
                maskMap = Gdk.CairoHelper.RegionCreateFromSurface(maskImage);
            }
        }
    }
}