using System;
using System.IO;
using Drm.Adept;

namespace SimpleLauncher
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			Console.WriteLine("Removing DRM...");
			IsOk = true;
			//IsOk &= Epub.Strip(@"D:\Documents\My Books\Reader Library\Eros,_Philia,_Agape.epub", @"D:\Documents\Downloads\Books\Eros,_Philia,_Agape.epub");
			foreach (var file in Directory.EnumerateFiles(@"D:\Documents\Downloads\Books\", "*.epub", SearchOption.TopDirectoryOnly))
			{
				string bookName = Path.GetFileNameWithoutExtension(file);
				Console.Write(bookName);
				bool isBookProcessedOk = false;
				string error = null;
				try
				{
					isBookProcessedOk = Epub.Strip(file, @"D:\Documents\Downloads\Books\1\" + Path.GetFileName(file));
				}
				catch(Exception e)
				{
					error = e.Message;
					isBookProcessedOk = false;
				}
				PrintResult(bookName, isBookProcessedOk);
				if (!isBookProcessedOk)
					Console.WriteLine("\tError: " + error);
				IsOk &= isBookProcessedOk;
			}

			//var eReaderPdb = new EReaderPdb(new Pdb(@"D:\Documents\Downloads\Books\Ysabel_45498.pdb"));

			Console.WriteLine("Done.");
			//if (!IsOk)
				Console.ReadKey();
		}

		private static void PrintResult(string fileName, bool isBookProcessedOk)
		{
			string result = isBookProcessedOk ? "ok" : "failed";
			Console.Write(new string(' ', Math.Max(40 - fileName.Length - result.Length, 1)));
			Console.ForegroundColor = isBookProcessedOk ? ConsoleColor.Green : ConsoleColor.Red;
			Console.WriteLine(result);
			Console.ResetColor();
		}

		private static void Log(string message)
		{
			Console.WriteLine(message);
			IsOk = false;
/*
			Console.WriteLine("Warning! Decompression failed for '{0}'", file.FileName);
			string outFileName = Path.Combine(Path.GetDirectoryName(outputPath), file.FileName) + ".gz";
			string outFolder = Path.GetDirectoryName(outFileName);
			if (!Directory.Exists(outFolder)) Directory.CreateDirectory(outFolder);
			var outData = cipher.CreateDecryptor().TransformFinalBlock(data, 0, data.Length).ToArray();
			using (var debugOut = new FileStream(outFileName, FileMode.Create, FileAccess.Write, FileShare.Read))
				debugOut.Write(outData, 0, outData.Length);
*/
		}

		private static bool IsOk;
	}
}