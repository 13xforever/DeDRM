using System;
using System.Collections.Generic;
using System.IO;
using Drm;
using Drm.Adept;

namespace SimpleLauncher
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			var inPath = args.Length > 0 ? args : new[] {@".\*.epub", @".\*.pdb"};
			Console.WriteLine("Removing DRM...");
			var inFiles = GetInFiles(inPath);
			foreach (var file in inFiles)
				{
					string bookName = Path.GetFileNameWithoutExtension(file);
					Console.Write(bookName);
					ProcessResult isBookProcessedOk;
					string error = null;
					try
					{
						isBookProcessedOk = Epub.Strip(file, @"C:\Documents\Downloads\Books\" + Path.GetFileName(file));
					}
					catch(Exception e)
					{
						error = e.Message;
						isBookProcessedOk = ProcessResult.Fail;
					}
					Logger.PrintResult(isBookProcessedOk);
					if (isBookProcessedOk == ProcessResult.Fail)
						Console.WriteLine("\tError: " + error);
				}

			//var eReaderPdb = new EReaderPdb(new Pdb(@"D:\Documents\Downloads\Books\Ysabel_45498.pdb"));

			Console.WriteLine("Done.");
		}

		private static IEnumerable<string> GetInFiles(string[] inPath)
		{
			string dir, mask;
			foreach (var path in inPath)
			{
				if (Directory.Exists(path))
				{
					dir = path;
					mask = "*";
				}
				else
				{
					dir = Path.GetPathRoot(path);
					mask = Path.GetFileName(path);
				}
				if (!Directory.Exists(dir))
					continue;

				foreach (var file in Directory.EnumerateFiles(dir, mask, SearchOption.TopDirectoryOnly))
					yield return Path.Combine(dir, file);
			}
		}


	}
}