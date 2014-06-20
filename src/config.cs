using System;
using System.IO;

namespace gcaliper
{
	public class AppConfig
	{
		public static string appRootDir;
		private static INIFile config;

		public static string themeName { get { return config.GetValue ("config", "theme", "caliper"); } set { config.SetValue ("config", "theme", value); } }

		public static byte jawColorR { get { return config.GetValue ("config", "jawColorR", (byte)150); } set { config.SetValue ("config", "jawColorR", value); } }

		public static byte jawColorG { get { return config.GetValue ("config", "jawColorG", (byte)0); } set { config.SetValue ("config", "jawColorG", value); } }

		public static byte jawColorB { get { return config.GetValue ("config", "jawColorB", (byte)0); } set { config.SetValue ("config", "jawColorB", value); } }

		public static TColor jawColor {
			get { 
				return new TColor (jawColorR, jawColorG, jawColorB);
			}
			set {
				jawColorR = value.r; 
				jawColorG = value.g; 
				jawColorB = value.b; 
			}
		}

		public static void init ()
		{
			appRootDir = new DirectoryInfo (Path.GetDirectoryName (typeof(Program).Assembly.Location)).Parent.FullName + Path.DirectorySeparatorChar;
			config = new INIFile (Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.UserProfile), ".gcaliper.ini"));
		}

		public static void save ()
		{
			config.Flush ();
		}
	}
}

