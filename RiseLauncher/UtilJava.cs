using System;
using System.Diagnostics;
using System.IO;

namespace RiseLauncher
{
	internal class UtilJava
	{
		public static string getJavaMainFolderPath()
		{
			string launcherFolderPath = UtilJava.getLauncherFolderPath();
			string str = launcherFolderPath + Path.DirectorySeparatorChar.ToString() + "java";
			return str + Path.DirectorySeparatorChar.ToString();
		}

		public static string getJavaFolderPath()
		{
			string launcherFolderPath = UtilJava.getLauncherFolderPath();
			string str = launcherFolderPath + Path.DirectorySeparatorChar.ToString() + "java";
			bool is64BitOperatingSystem = Environment.Is64BitOperatingSystem;
			bool flag = is64BitOperatingSystem;
			if (flag)
			{
				str = str + Path.DirectorySeparatorChar.ToString() + FormMain.getSelectedJavaType() + "-x64";
			}
			else
			{
				str = str + Path.DirectorySeparatorChar.ToString() + FormMain.getSelectedJavaType() + "-x32";
			}
			return str + Path.DirectorySeparatorChar.ToString();
		}

		public static string getJavaExePath()
		{
			string launcherFolderPath = UtilJava.getLauncherFolderPath();
			string text = launcherFolderPath + Path.DirectorySeparatorChar.ToString() + "java";
			bool is64BitOperatingSystem = Environment.Is64BitOperatingSystem;
			bool flag = is64BitOperatingSystem;
			if (flag)
			{
				text = text + Path.DirectorySeparatorChar.ToString() + FormMain.getSelectedJavaType() + "-x64";
			}
			else
			{
				text = text + Path.DirectorySeparatorChar.ToString() + FormMain.getSelectedJavaType() + "-x32";
			}
			return string.Concat(new string[]
			{
				text,
				Path.DirectorySeparatorChar.ToString(),
				"bin",
				Path.DirectorySeparatorChar.ToString(),
				"java.exe"
			});
		}

		public static string getJavaWExePath()
		{
			string launcherFolderPath = UtilJava.getLauncherFolderPath();
			string text = launcherFolderPath + Path.DirectorySeparatorChar.ToString() + "java";
			bool is64BitOperatingSystem = Environment.Is64BitOperatingSystem;
			bool flag = is64BitOperatingSystem;
			if (flag)
			{
				text = text + Path.DirectorySeparatorChar.ToString() + FormMain.getSelectedJavaType() + "-x64";
			}
			else
			{
				text = text + Path.DirectorySeparatorChar.ToString() + FormMain.getSelectedJavaType() + "-x32";
			}
			return string.Concat(new string[]
			{
				text,
				Path.DirectorySeparatorChar.ToString(),
				"bin",
				Path.DirectorySeparatorChar.ToString(),
				"java.exe"
			});
		}

		public static string getLauncherFolderPath()
		{
			string str = ".craftrise";
			string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
			string str2 = folderPath + Path.DirectorySeparatorChar.ToString() + str;
			return str2 + Path.DirectorySeparatorChar.ToString();
		}

		public static string getCurrentJavaVersion()
		{
			string result;
			try
			{
				string text = null;
				Process process = Process.Start(new ProcessStartInfo
				{
					FileName = UtilJava.getJavaExePath(),
					Arguments = " -version",
					CreateNoWindow = true,
					RedirectStandardError = true,
					UseShellExecute = false
				});
				while (!process.StandardError.EndOfStream)
				{
					string text2 = process.StandardError.ReadLine().ToLower();
					bool flag = text2.StartsWith("java version \"");
					if (flag)
					{
						text = text2.Split(new char[]
						{
							' '
						})[2].Replace("\"", "");
						break;
					}
				}
				process.WaitForExit();
				result = text;
			}
			catch (Exception ex)
			{
				result = null;
			}
			return result;
		}
	}
}
