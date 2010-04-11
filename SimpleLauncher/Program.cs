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
			//IsOk &= Epub.Strip(@"D:\Documents\My Books\Reader Library\Poison_Sleep.epub", @"D:\Documents\Downloads\Books\Poison Sleep.epub");
			foreach (var file in Directory.GetFiles(@"D:\Documents\Downloads\Books\", "*.epub", SearchOption.TopDirectoryOnly))
				IsOk &= Epub.Strip(file, @"E:\Books\John Scalzi\Old Man’s War\" + Path.GetFileName(file));

			//var eReaderPdb = new EReaderPdb(new Pdb(@"D:\Documents\Downloads\Books\Ysabel_45498.pdb"));

			if (!IsOk) Console.ReadKey();
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