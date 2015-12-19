using System;
using Drm;

namespace SimpleLauncher
{
	internal class Logger
	{
		internal static void PrintResult(BookFormat format)
		{
			string result = format.ToString();
			ConsoleColor? color;
			switch (format)
			{
				case BookFormat.EPub:
				case BookFormat.EReader:
					color = ConsoleColor.Green;
					break;
				default:
					color = ConsoleColor.Red;
					break;
			}
			PrintResult(result, 40, color);
		}

		internal static void PrintResult(PrivateKeyScheme drm)
		{
			string result = drm.ToString();
			ConsoleColor? color;
			switch (drm)
			{
				case PrivateKeyScheme.None:
				case PrivateKeyScheme.KoboNone:
					color = null;
					break;
				case PrivateKeyScheme.Adept:
				case PrivateKeyScheme.Kobo:
					color = ConsoleColor.Green;
					break;
				default:
					color = ConsoleColor.Red;
					break;
			}
			PrintResult(result, 50, color);
		}

		internal static void PrintResult(ProcessResult status)
		{
			string result = status.ToString();
			ConsoleColor? color = null;
			switch (status)
			{
				case ProcessResult.Skipped:
					break;
				case ProcessResult.Success:
					color = ConsoleColor.Green;
					break;
				case ProcessResult.Fail:
					color = ConsoleColor.Red;
					break;
				default:
					color = ConsoleColor.Yellow;
					break;
			}
			PrintResult(result, 60, color);
		}

		internal static void PrintResult(string outFilename)
		{
			PrintResult(outFilename, 70);
			Console.WriteLine();
		}

		private static void PrintResult(string str, int position, ConsoleColor? color = null)
		{
			if (Console.CursorLeft < position)
				Console.CursorLeft = position;
			if (color.HasValue)
			{
				var lastColor = Console.ForegroundColor;
				Console.ForegroundColor = color.Value;
				Console.Write(str);
				Console.ForegroundColor = lastColor;
			}
			else
				Console.Write(str);
		}

		internal static void Log(string message)
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