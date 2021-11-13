using System;
using System.IO;
using System.Security.Cryptography;
using SevenZip.Compression.LZMA;

namespace RiseLauncher
{
	public static class UtilFile
	{
		public static string getHashFile(string pathName)
		{
			string result;
			try
			{
				SHA1CryptoServiceProvider sha1CryptoServiceProvider = new SHA1CryptoServiceProvider();
				FileStream fileStream = UtilFile.GetFileStream(pathName);
				byte[] value = sha1CryptoServiceProvider.ComputeHash(fileStream);
				fileStream.Close();
				string text = BitConverter.ToString(value);
				text = text.Replace("-", "");
				result = text.ToLower();
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
				result = "";
			}
			return result;
		}

		private static FileStream GetFileStream(string pathName)
		{
			return new FileStream(pathName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
		}

		public static void ClearFolder(this DirectoryInfo directory)
		{
			try
			{
				foreach (FileInfo fileInfo in directory.GetFiles())
				{
					fileInfo.Delete();
				}
				foreach (DirectoryInfo directoryInfo in directory.GetDirectories())
				{
					directoryInfo.Delete(true);
				}
			}
			catch (Exception ex)
			{
			}
		}

		public static bool DecompressLZMA(string inFile, string outFile)
		{
			bool result;
			try
			{
				Decoder decoder = new Decoder();
				FileStream fileStream = new FileStream(inFile, FileMode.Open);
				FileStream fileStream2 = new FileStream(outFile, FileMode.Create);
				byte[] array = new byte[5];
				fileStream.Read(array, 0, 5);
				byte[] array2 = new byte[8];
				fileStream.Read(array2, 0, 8);
				long num = BitConverter.ToInt64(array2, 0);
				decoder.SetDecoderProperties(array);
				decoder.Code(fileStream, fileStream2, fileStream.Length, num, null);
				fileStream2.Flush();
				fileStream2.Close();
				result = true;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
				result = false;
			}
			return result;
		}
	}
}
