using System;
using System.Collections.Generic;
using System.IO;
using Drm;
using Drm.Format;
using Drm.Utils;

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

				BookFormat format = FormatGuesser.Guess(file);
				Logger.PrintResult(format);

				var scheme = SchemeGuesser.Guess(file, format);
				Logger.PrintResult(scheme);

				if (format == BookFormat.Unknown)
				{
					Logger.PrintResult(ProcessResult.Skipped);
					continue;
				}

				ProcessResult processResult;
				string error = null;
				try
				{
					var processor = DrmProcessorFactory.Get(format, scheme);
					var data = File.ReadAllBytes(file);
					var result = processor.Strip(data, file);

					var outDir = Path.Combine(Path.GetDirectoryName(file), "out");
					if (!Directory.Exists(outDir))
						Directory.CreateDirectory(outDir);

					var fileName = processor.GetFileName(file).ReplaceInvalidChars();
					var outFile = Path.Combine(outDir, fileName);
					File.WriteAllBytes(outFile, result);
					processResult = ProcessResult.Success;
				}
				catch(Exception e)
				{
					error = e.Message;
					processResult = ProcessResult.Fail;
				}
				Logger.PrintResult(processResult);
				if (processResult == ProcessResult.Fail)
					Console.WriteLine("\tError: " + error);
			}
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
					dir = Path.GetDirectoryName(path);
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