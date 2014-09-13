using System;
using Drm;

namespace SimpleLauncher
{
	internal class Logger
	{
		internal static void PrintResult(string drmName)
		{
			PrintResult(drmName, 50, ConsoleColor.White);
		}

		internal static void PrintResult(ProcessResult status)
		{
			string result = status.ToString();
			var color = Console.ForegroundColor;
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
			PrintResult(result, Math.Max(60 - result.Length, 1), color);
			Console.WriteLine();
			Console.ResetColor();
		}

		private static void PrintResult(string str, int position, ConsoleColor color)
		{
			if (Console.CursorLeft < position)
				Console.CursorLeft = position;
			var lastColor = Console.ForegroundColor;
			Console.ForegroundColor = color;
			Console.Write(str);
			Console.ForegroundColor = lastColor;
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