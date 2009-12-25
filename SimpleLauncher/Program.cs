using System.IO;
using Drm.Adept;
using Drm.EReader;

namespace SimpleLauncher
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			foreach (string file in Directory.GetFiles(@"D:\Documents\My Books\Reader Library\", "The_Girl_*.epub", SearchOption.TopDirectoryOnly))
				Epub.Strip(file, @"D:\Documents\My Books\Reader Library\out\" + Path.GetFileName(file));
			//Epub.Strip(@"D:\Documents\My Books\Reader Library\New_Moon.epub", @"D:\Documents\My Books\Reader Library\New Moon.epub");
			//var eReaderPdb = new EReaderPdb(new Pdb(@"D:\Documents\Downloads\Books\Ysabel_45498.pdb"));
		}
	}
}