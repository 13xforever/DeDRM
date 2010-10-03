using System;
using System.IO;
using Drm.Adept;

namespace SimpleLauncher
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			IsOk = true;
			//IsOk &= Epub.Strip(@"D:\Documents\My Books\Reader Library\Eros,_Philia,_Agape.epub", @"D:\Documents\Downloads\Books\Eros,_Philia,_Agape.epub");
			foreach (var file in Directory.EnumerateFiles(@"E:\Books\Orson Scott Card\tmp\", "*.epub", SearchOption.TopDirectoryOnly))
			{
				string bookName = Path.GetFileNameWithoutExtension(file);
				Console.Write(bookName);
				bool isBookProcessedOk = Epub.Strip(file, @"E:\Books\Orson Scott Card\" + Path.GetFileName(file));
				PrintResult(bookName, isBookProcessedOk);
				IsOk &= isBookProcessedOk;
			}

			//var eReaderPdb = new EReaderPdb(new Pdb(@"D:\Documents\Downloads\Books\Ysabel_45498.pdb"));

			if (!IsOk) Console.ReadKey();
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