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
using System.IO;

namespace gcaliper
{
    public class AppConfig
    {
        public static string appRootDir;
        private static INIFile config;

        public static string themeName { get { return config.GetValue("config", "theme", "caliper"); } set { config.SetValue("config", "theme", value); } }

        public static byte jawColorR { get { return config.GetValue("config", "jawColorR", (byte)150); } set { config.SetValue("config", "jawColorR", value); } }

        public static byte jawColorG { get { return config.GetValue("config", "jawColorG", (byte)0); } set { config.SetValue("config", "jawColorG", value); } }

        public static byte jawColorB { get { return config.GetValue("config", "jawColorB", (byte)0); } set { config.SetValue("config", "jawColorB", value); } }

        public static TColor jawColor
        {
            get
            {
                return new TColor(jawColorR, jawColorG, jawColorB);
            }
            set
            {
                jawColorR = value.r;
                jawColorG = value.g;
                jawColorB = value.b;
            }
        }

        public static string themesDir
        {
            get
            {
                return Path.Combine(appRootDir, "themes");
            }
        }

        public static string currThemeDir
        {
            get
            {
                return Path.Combine(themesDir, themeName);
            }
        }

        public static void init()
        {
            appRootDir = new DirectoryInfo(Path.GetDirectoryName(typeof(Program).Assembly.Location)).FullName;
            config = new INIFile(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".gcaliper.ini"));
        }

        public static void save()
        {
            config.Flush();
        }
    }
}

