using System.IO;

namespace Drm.EReader
{
	public static class Pdb
	{
		public static void Strip(string ebookPath, string outputDir, string name, string ccNumber)
		{
			var sect = new Sectionizer(ebookPath, "PNRdPPrs");
			var processor = new EreaderProcessor(sect, name, ccNumber);
			if (!Directory.Exists(outputDir)) Directory.CreateDirectory(outputDir);
			string path;
			for (int i = 0; i < processor.NumImages; i++)
			{
				ImageInfo img = processor.GetImage(i);
				path = Path.Combine(outputDir, img.filename);
				using (FileStream stream = File.Create(path)) stream.Write(img.content, 0, img.content.Length);
			}
			string pml = processor.GetText();
			path = Path.Combine(outputDir, Path.GetFileNameWithoutExtension(ebookPath) + ".pml");
			using (StreamWriter stream = File.CreateText(path)) stream.Write(pml);
		}
	}
}