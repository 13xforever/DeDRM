using Drm.Adept;
using Drm.EReader;

namespace SimpleLauncher
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			//Epub.Strip(@"D:\Documents\My Digital Editions\Shadowed_By_Wings.epub", @"C:\Users\13xforever\Desktop\out2.epub");
			//Epub.Strip(@"C:\Users\ilya_veselov\Documents\My Digital Editions\Shadowed_By_Wings.epub", @"C:\Users\ilya_veselov\Documents\My Digital Editions\y.epub");
			EReaderProcessor.Strip(@"D:\Documents\Downloads\Books\Ysabel_45498.pdb", @"D:\Documents\Downloads\Books", "Name Surname", "00000000");
		}
	}
}