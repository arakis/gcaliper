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

using Gtk;
using Gdk;
using Cairo;
using IO = System.IO;

namespace gcaliper
{
    public class DrawGroup : Gtk.Window
    {
        public Cairo.Region MaskMap;
        public ImageSurface Image;
        public PartList Parts = new PartList();
        protected Menu Menu;
        protected Style OriginalStyle;
        protected bool NeedRedraw = true;
        //protected StatusIcon statusIcon;

        public DrawGroup()
            : base(Gtk.WindowType.Toplevel)
        {
            OriginalStyle = Style.Copy();
            Decorated = false;
            Events = EventMask.AllEventsMask;

            //statusIcon = new StatusIcon ("../appicon.ico");
            //statusIcon.Tooltip = "gcaliper";
            SetIconFromFile(IO.Path.Combine(AppConfig.AppRootDir, "appicon.ico"));

            Menu = new Menu();

            var minItem = new MenuItem("Minimize");
            Menu.Add(minItem);
            minItem.ButtonReleaseEvent += (o, e) =>
            {
                if (e.Event.Button == 1)
                {
                    Iconify();
                    //statusIcon.Visible=true;
                    //Hide();
                }
            };

            var aboutItem = new MenuItem("About");
            Menu.Add(aboutItem);
            aboutItem.ButtonReleaseEvent += (o, e) =>
            {
                Helper.ShowMessage("Author: Sebastian Loncar.\nProject Url: https://github.com/Arakis/gcaliper\nEmail: sebastian.loncar@gmail.com");
            };

            var quitItem = new MenuItem("Quit");
            Menu.Add(quitItem);
            quitItem.ButtonReleaseEvent += (o, e) =>
            {
                if (e.Event.Button == 1)
                    Application.Quit();
            };

            SetWindowShape();
        }

        public void InvalidateImage()
        {
            if (NeedRedraw)
                return;

            NeedRedraw = true;
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

        public void SetWindowShape()
        {
            //this.ShapeCombineMask(maskMap, 0, 0);
            ShapeCombineRegion(MaskMap);
        }

        protected void GenerateMask()
        {
            if (MaskMap != null)
                MaskMap.Dispose();

            using (var maskImage = new ImageSurface(Format.Argb32, Image.Width, Image.Height))
            {
                using (var cr = new Context(maskImage))
                {
                    cr.SetSourceRGB(0, 0, 0);
                    cr.Operator = Operator.Clear;
                    cr.Paint();
                    cr.Operator = Operator.Source;

                    cr.SetSource(Image, 0, 0);
                    cr.Rectangle(new Cairo.Rectangle(0, 0, Image.Width, Image.Height));
                    cr.Paint();
                }
                MaskMap = Gdk.CairoHelper.RegionCreateFromSurface(maskImage);
            }
        }
    }
}