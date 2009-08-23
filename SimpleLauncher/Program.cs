using Drm.Adept;

namespace SimpleLauncher
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			byte[] key = KeyRetriever.Retrieve();
			Epub.Strip(key, @"D:\Documents\My Digital Editions\Shadowed_By_Wings.epub", @"C:\Users\13xforever\Desktop\out2.epub");
		}
	}
}