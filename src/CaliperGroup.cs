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
using Gtk;
using IO = System.IO;
using POINT = System.Drawing.Point;
using RECT = System.Drawing.Rectangle;

namespace gcaliper
{
    public class CaliperGroup : DrawGroup
    {
        public CaliperPartHead PartHead;
        public CaliperPartBottom PartBottom;
        public CaliperPartDisplay PartDisplay;
        public CaliperPartScale PartScale;

        public CaliperGroup()
        {
            LoadTheme(System.IO.Path.Combine(ThemesRootDirectory, AppConfig.ThemeName));

            Parts.Add(PartBottom = new CaliperPartBottom());
            Parts.Add(PartHead = new CaliperPartHead());
            Parts.Add(PartDisplay = new CaliperPartDisplay());
            Parts.Add(PartScale = new CaliperPartScale());

            //part2.rect.Y = 20;
            Distance = 100;
            PartScale.Rect.Location = ScaleOffset;

            SetContrastColor(JawColor);

            var color1 = new MenuItem("Color");
            Menu.Insert(color1, 0);
            color1.ButtonReleaseEvent += (o, e) =>
            {
                if (e.Event.Button == 1)
                    ShowColorChooser();
            };

            // TODO: Place in visible area
            // TODO: respect DPI
            //setWindowPosition(new POINT(3000, 0));
        }

        public void ShowColorChooser()
        {
            using (var chooser = new ColorSelectionDialog("change color"))
            {
                //chooser.TransientFor=this;
                chooser.Style = OriginalStyle;

                if (chooser.Run() == (int)ResponseType.Ok)
                {
                    AppConfig.JawColor = new Color(chooser.ColorSelection.CurrentColor);
                    AppConfig.Save();
                    SetContrastColor(AppConfig.JawColor);
                }
                chooser.Hide();
            }
        }

        public string ThemesRootDirectory
        {
            get
            {
                return IO.Path.Combine(AppConfig.AppRootDir, "themes");
            }
        }

        public void LoadTheme(string themeDir)
        {
            var themeFile = IO.Path.Combine(themeDir, "theme.conf");
            var ini = new INIFile(themeFile);
            RotationCenterImage = new POINT(ini.GetValue("theme", "rotationCenterX", 0), ini.GetValue("theme", "rotationCenterY", 0));
            DisplayCenterOffset = new POINT(ini.GetValue("theme", "displayCenterX", 0), ini.GetValue("theme", "displayCenterY", 0));
            ScaleOffset = new POINT(ini.GetValue("theme", "scaleOffsetX", 0), ini.GetValue("theme", "scaleOffsetY", 0));
            ZeroDistanceOffset = ini.GetValue("theme", "zeroDistanceOffset", 0);
        }
        // *** configuration ***
        public POINT RotationCenterImage;
        // = new POINT (20, 65);
        public POINT ScaleOffset;
        public POINT DisplayCenterOffset;
        // = new POINT (45, 68);
        public int ZeroDistanceOffset;
        // = 15;
        // ***
        public int MinDistanceForRotation = 10;
        public double SnapAngle = 0.5;
        private Color JawColor = AppConfig.JawColor;
        private double Angle = 0.0174532925 * 0;
        private static double DEG1 = 0.0174532925;
        // ***
        private double TmpAngle;
        public RECT UnrotatedRect;
        public RECT RotatedRect;
        public POINT RotationCenterRoot;
        // = new POINT (1920 + 1920 / 2, 1200 / 2);
        public POINT RotationCenterZero = new POINT(0, 0);

        public void SetContrastColor(Color color)
        {
            JawColor = color;
            foreach (var part in Parts)
            {
                part.ApplyContrast(color);
            }
            InvalidateImage();
        }

        protected override bool OnConfigureEvent(EventConfigure evnt)
        {
            UpdateRotationCenter();
            EnsureInitialDrawn();
            return base.OnConfigureEvent(evnt);
        }

        private void EnsureInitialDrawn()
        {
            if (!Positioned)
            {
                NeedRedraw = false;
                Positioned = true;
                InvalidateImage();
            }
        }

        protected override void OnShown()
        {
            UpdateRotationCenter();
            EnsureInitialDrawn();
            base.OnShown();
        }

        public void GenerateImage()
        {
            UnrotatedRect = Parts.GetRotationRect();

            using (var surf = new ImageSurface(Format.ARGB32, UnrotatedRect.Width, UnrotatedRect.Height))
            {
                using (var cr = new Context(surf))
                {

                    //Clear
                    if (Debug)
                    {
                        cr.SetSourceColor(new Cairo.Color(0, 0.9, 0));
                        cr.Rectangle(0, 0, UnrotatedRect.Width, UnrotatedRect.Height);
                        cr.Fill();
                    }
                    else
                    {
                        cr.Operator = Operator.Clear;
                        cr.Paint();
                        cr.Operator = Operator.Over;
                    }

                    foreach (var part in Parts)
                    {
                        if (part.Rotate)
                        {
                            //Draw image

                            part.Draw(cr);
                        }
                    }

                    if (Debug)
                    {
                        cr.LineWidth = 5;
                        cr.SetSourceRGBA(1, 0, 0, 1);
                        cr.Translate(DebugPoint.X, DebugPoint.Y);
                        cr.Arc(0, 0, 2, 0, Math.PI * 2);
                        cr.StrokePreserve();
                    }

                }

                //surf.WriteToPng ("test.png");

                //var angle = 0;
                var oldRotatedRect = RotatedRect;
                RotatedRect = Helper.RotateRect(UnrotatedRect, RotationCenterZero, Angle);

                //Rotate
                var surf2 = new ImageSurface(Format.ARGB32, RotatedRect.Width, RotatedRect.Height);
                using (var cr = new Context(surf2))
                {
                    cr.Operator = Operator.Clear;
                    cr.Paint();
                    cr.Operator = Operator.Over;

                    cr.Translate(-RotatedRect.X, -RotatedRect.Y);
                    cr.Rotate(Angle);
                    //var pp = funcs.rotatePoint (rotationRect.Location, new POINT (0, 0), angle);
                    using (var pat2 = new SurfacePattern(surf))
                    {
                        //pat2.Matrix = new Matrix (){ X0 =  -rr.X, Y0 = -rr.Y };

                        cr.SetSource(pat2);
                        //cr.Translate (100, 100);
                        cr.Paint();
                    }

                    //Debug
                    if (true)
                    {
                        cr.Matrix = new Matrix();
                        if (DebugText != null)
                        {
                            //cr.Operator=Operator.Source;
                            cr.SetSourceRGBA(0, 1, 0, 1);
                            cr.SelectFontFace("Arial", FontSlant.Normal, FontWeight.Normal);
                            cr.SetFontSize(20);
                            cr.MoveTo(20, 20);
                            cr.ShowText(DebugText);
                            cr.Fill();
                        }
                    }

                    foreach (var part in Parts)
                    {
                        if (!part.Rotate)
                        {
                            cr.Matrix = new Matrix();

                            var c = new POINT(PartBottom.Rect.Location.X + DisplayCenterOffset.X, PartBottom.Rect.Location.Y + DisplayCenterOffset.Y);

                            part.Rect.X = c.X;
                            part.Rect.Y = c.Y;

                            var p = ImagePosToRotatedPos(part.Rect.Location);

                            p.X -= part.Rect.Width / 2;
                            p.Y -= part.Rect.Height / 2;

                            //Draw image

                            using (var pat = new SurfacePattern(part.Image))
                            {
                                pat.Matrix = new Matrix() { X0 = -p.X, Y0 = -p.Y };
                                //pat.Matrix = pat.Matrix;

                                cr.SetSource(pat);
                                cr.Rectangle(new Cairo.Rectangle(p.X, p.Y, part.Rect.Width, part.Rect.Height));
                                cr.Fill();

                                cr.SetSourceRGBA(0, 0, 0, 1);
                                cr.SelectFontFace("Arial", FontSlant.Normal, FontWeight.Normal);
                                cr.SetFontSize(10);
                                cr.MoveTo(p.X + 12, p.Y + 27.2);
                                var text = Distance.ToString();

                                cr.ShowText(text);

                                var deg = Math.Round(Helper.RadToDeg(Angle));

                                if (deg % 45 != 0)
                                {
                                    cr.MoveTo(p.X + 14, p.Y + 40.2);
                                    text = deg.ToString() + "°";
                                    cr.ShowText(text);
                                }

                                cr.Fill();
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

                if (Image != null)
                    Image.Dispose();
                Image = surf2;
            }
        }

        public int Distance
        {
            get
            {
                return PartBottom.Rect.X - ZeroDistanceOffset;
            }
            set
            {
                if (Distance == value)
                    return;
                value = Math.Max(value, 0);
                PartBottom.Rect.X = value + ZeroDistanceOffset;
                UpdatePartScale();
                InvalidateImage();
            }
        }

        public bool Debug;
        private string _DebugText;

        public string DebugText
        {
            get
            {
                return _DebugText;

            }
            set
            {
                if (value == _DebugText)
                    return;
                _DebugText = value;
                InvalidateImage();
            }
        }

        private POINT RootMousePos;
        private POINT MousePos;
        private POINT StartRootMousePos;
        private POINT StartRectPos;
        private POINT StartWinPos;
        private POINT MouseImagePos;
        private bool Resizing;
        private bool Moving;
        private POINT DebugPoint = new POINT(10, 10);
        private double MoveMouseAngleOffset;
        private int MoveMouseXOffset;

        private POINT AbsPosToUnrotatedPos(POINT pos)
        {
            return Helper.RotatePoint(new POINT(MousePos.X + RotatedRect.X, MousePos.Y + RotatedRect.Y), new POINT(0, 0), -Angle);
        }

        public int GetDistanceToRotationCenter(POINT rootPos)
        {
            //return (int)Math.Round (Math.Abs (Math.Sqrt (Math.Pow (rootPos.X - rotationCenterRoot.X, 2) + Math.Pow (rootPos.Y - rotationCenterRoot.Y, 2))));
            //rotationCenterImage.X

            int x, y;
            GetPosition(out x, out y);

            var p = AbsPosToUnrotatedPos(new POINT(rootPos.X - x, rootPos.Y - y));
            return p.X - RotationCenterImage.X;
        }

        protected override bool OnMotionNotifyEvent(EventMotion evnt)
        {
            RootMousePos = new POINT((int)evnt.XRoot, (int)evnt.YRoot);
            MousePos = new POINT((int)evnt.X, (int)evnt.Y);

            MouseImagePos = AbsPosToUnrotatedPos(MousePos);

            if (Debug)
            {
                DebugText = PartBottom.Rect.Contains(MouseImagePos).ToString();
                DebugPoint = MouseImagePos;
                InvalidateImage();
            }

            var relMousePos = new POINT(RootMousePos.X - StartRootMousePos.X, RootMousePos.Y - StartRootMousePos.Y);

            if (Resizing)
            {
                if (Math.Abs(relMousePos.X) > 10 || Math.Abs(relMousePos.Y) > 10)
                {

                    PartBottom.Rect.X = GetDistanceToRotationCenter(RootMousePos);
                    PartBottom.Rect.X -= MoveMouseXOffset;
                    PartBottom.Rect.X = Math.Max(PartBottom.Rect.X, ZeroDistanceOffset);
                    UpdatePartScale();

                    if (Distance > MinDistanceForRotation)
                    {
                        TmpAngle = Helper.GetAngleOfLineBetweenTwoPoints(RotationCenterRoot, RootMousePos);
                        TmpAngle -= MoveMouseAngleOffset;
                        TmpAngle = NormalizeAngle(TmpAngle);

                        if ((evnt.State & ModifierType.ControlMask) == ModifierType.ControlMask && (evnt.State & ModifierType.ShiftMask) != ModifierType.ShiftMask)
                        {
                            Angle = TmpAngle;
                        }
                        else
                        {
                            var snapAngle = SnapAngle;
                            double[] angleMarkers;
                            if ((evnt.State & ModifierType.ShiftMask) == ModifierType.ShiftMask)
                            {
                                angleMarkers = new double[]
                                {
                                    0,
                                    Math.PI / 4,
                                    Math.PI / 2,
                                    Math.PI,
                                    Math.PI - (Math.PI / 4),
                                    -(Math.PI - (Math.PI / 4)),
                                    -(Math.PI / 2),
                                    -(Math.PI / 4),
                                };
                                snapAngle = snapAngle / 2;
                            }
                            else
                            {
                                angleMarkers = new double[] { 0, Math.PI / 2, Math.PI, -Math.PI, -(Math.PI / 2) };
                            }

                            for (var i = 0; i < angleMarkers.Length; i++)
                            {
                                var a = angleMarkers[i];
                                if (TmpAngle <= a + snapAngle && TmpAngle >= a - snapAngle)
                                {
                                    SetAngle(a);
                                    break;
                                }
                            }
                        }
                    }

                    InvalidateImage();
                }
            }

            if (Moving)
            {
                var x = StartWinPos.X + (RootMousePos.X - StartRootMousePos.X);
                var y = StartWinPos.Y + (RootMousePos.Y - StartRootMousePos.Y);
                Move(x, y);
                UpdateRotationCenter();
            }

            return base.OnMotionNotifyEvent(evnt);
        }

        public double NormalizeAngle(double angle)
        {
            if (angle >= Math.PI)
                angle -= Math.PI * 2;
            if (angle <= -Math.PI)
                angle += Math.PI * 2;
            return angle;
        }

        public void SetAngle(double angle)
        {
            Angle = NormalizeAngle(angle);
            InvalidateImage();
        }

        private void UpdatePartScale()
        {
            PartScale.Rect.Width = Distance;
        }

        protected override bool OnButtonPressEvent(EventButton evnt)
        {
            MousePos = new POINT((int)evnt.X, (int)evnt.Y);
            MouseImagePos = AbsPosToUnrotatedPos(MousePos);
            if (evnt.Button == 1)
            {
                int x;
                int y;
                GetPosition(out x, out y);

                StartWinPos = new POINT(x, y);
                StartRootMousePos = new POINT((int)evnt.XRoot, (int)evnt.YRoot);
                StartRectPos = PartBottom.Rect.Location;

                if (PartBottom.Rect.Contains(MouseImagePos))
                {
                    Resizing = true;

                    MoveMouseXOffset = GetDistanceToRotationCenter(StartRootMousePos) - PartBottom.Rect.X;
                    MoveMouseAngleOffset = Helper.GetAngleOfLineBetweenTwoPoints(RotationCenterRoot, StartRootMousePos) - Angle;

                }
                else if (PartHead.Rect.Contains(MouseImagePos) || PartScale.Rect.Contains(MouseImagePos))
                {
                    Moving = true;
                }
            }
            if (evnt.Button == 3)
            {
                Menu.ShowAll();
                Menu.Popup();
            }

            return base.OnButtonPressEvent(evnt);
        }

        protected override bool OnKeyPressEvent(EventKey e)
        {

            if (e.Key == Gdk.Key.Left || e.Key == Gdk.Key.Right || e.Key == Gdk.Key.Up || e.Key == Gdk.Key.Down)
            {
                var step = 1;
                var stepY = 0;

                if ((e.State & ModifierType.ShiftMask) == ModifierType.ShiftMask)
                    step = 20;

                if (e.Key == Gdk.Key.Left || e.Key == Gdk.Key.Up)
                    step = -step;

                if ((e.State & ModifierType.ControlMask) == ModifierType.ControlMask)
                {
                    Distance += step;
                }
                else
                {
                    if (e.Key == Gdk.Key.Up || e.Key == Gdk.Key.Down)
                    {
                        stepY = step;
                        step = 0;
                    }

                    SetWindowPosition(GetWindowPosition().Add(step, stepY));
                }
            }

            if (e.Key == Gdk.Key.r || e.Key == Gdk.Key.t || e.Key == Gdk.Key.R || e.Key == Gdk.Key.T)
            {
                var angleDistance = Math.PI / 2;
                if ((e.State & ModifierType.ShiftMask) == ModifierType.ShiftMask)
                {
                    angleDistance = Math.PI / 4;
                }
                if ((e.State & ModifierType.ControlMask) == ModifierType.ControlMask)
                {
                    angleDistance = DEG1;
                }

                if (e.Key == Gdk.Key.t || e.Key == Gdk.Key.T)
                {
                    angleDistance = -angleDistance;
                }

                SetAngle(Angle - angleDistance);
            }

            if (e.Key == Gdk.Key.v)
            {
                SetAngle(Math.PI / 2);
            }
            if (e.Key == Gdk.Key.h)
            {
                SetAngle(0);
            }

            if (e.Key == Gdk.Key.n)
            {
                Iconify();
            }

            if (e.Key == Gdk.Key.Home)
            {
                Distance = 0;
            }

            if (e.Key == Gdk.Key.End)
            {
                var mon = Screen.GetMonitorAtWindow(GdkWindow);
                var geo = Screen.GetMonitorGeometry(mon);
                if (Angle == 0)
                    Distance = geo.Width - 200;
                else
                    Distance = geo.Height - 200;
            }

            if (e.Key == Gdk.Key.c)
            {
                ShowColorChooser();
            }

            if ((e.Key == Gdk.Key.q || e.Key == Gdk.Key.w) && (e.State & ModifierType.ControlMask) == ModifierType.ControlMask)
            {
                Application.Quit();
            }

            return base.OnKeyPressEvent(e);
        }

        private POINT GetWindowPosition()
        {
            int x, y;
            GetPosition(out x, out y);
            return new POINT(x, y);
        }

        private void SetWindowPosition(POINT p)
        {
            Move(p.X, p.Y);
        }

        protected override bool OnButtonReleaseEvent(EventButton evnt)
        {
            if (evnt.Button == 1)
            {
                Resizing = false;
                Moving = false;
            }
            return base.OnButtonReleaseEvent(evnt);
        }

        public void UpdatePosition()
        {
            var p = RotationCenterRoot;

            var r = Helper.RotatePoint(RotationCenterImage, RotationCenterZero, Angle);

            p.X -= r.X;
            p.Y -= r.Y;

            p.X += RotatedRect.X;
            p.Y += RotatedRect.Y;

            Move(p.X, p.Y);
        }

        public void UpdateRotationCenter()
        {
            //return;
            int x, y;
            GetPosition(out x, out y);

            var p = new POINT(x, y);
            var r = Helper.RotatePoint(RotationCenterImage, RotationCenterZero, Angle);

            p.X -= RotatedRect.X;
            p.Y -= RotatedRect.Y;

            p.X += r.X;
            p.Y += r.Y;

            RotationCenterRoot = p;
        }

        public POINT ImagePosToRotatedPos(POINT imgPos)
        {
            var p = new POINT(0, 0);
            var r = Helper.RotatePoint(imgPos, RotationCenterZero, Angle);

            p.X -= RotatedRect.X;
            p.Y -= RotatedRect.Y;

            p.X += r.X;
            p.Y += r.Y;

            return p;
        }

        public bool Positioned;

        public void DrawImage(Context cr)
        {
            if (Image != null)
            {
                cr.SetSource(Image);
                cr.Rectangle(0, 0, Image.Width, Image.Height);
                cr.Fill();
            }
        }

        public void Redraw(Context cr)
        {
            //to avoid flickering
            DrawImage(cr);

            if (!Positioned)
                return;

            NeedRedraw = false;
            try
            {

                GenerateImage();
                GenerateMask();

                //sizing window bigger than screen is requied, because some window managers does not allow positioning windows outside the screen, when they fit
                SetSizeRequest(Math.Max(Image.Width, Screen.Width + 10), Image.Height);

                SetWindowShape();

                DrawImage(cr);

                UpdatePosition();
            }
            catch (Exception ex)
            {
                new MessageDialog(null, DialogFlags.Modal, MessageType.Error, ButtonsType.Ok, ex.ToString());
            }

        }

        protected override bool OnDrawn(Context cr)
        {
            Redraw(cr);
            return true;
        }

        //        protected override bool OnExposeEvent(EventExpose evnt)
        //        {
        //            if (needRedraw)
        //                redraw();
        //
        //            return base.OnExposeEvent(evnt);
        //        }
    }
}