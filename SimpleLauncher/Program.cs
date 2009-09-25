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
			Pdb.Strip(@"C:\Users\ilya_veselov\Downloads\SpookCountry_49226.pdb", @"C:\Users\ilya_veselov\Downloads\SpookCountry\", "Name Surname", "00000000");
		}
	}
}