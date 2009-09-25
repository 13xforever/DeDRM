using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Drm.EReader
{
	internal class Sectionizer
	{
		private readonly byte[] content;
		private readonly List<RecordInfoEntry> sectionList = new List<RecordInfoEntry>();
		private readonly ushort sectionCount;

		public Sectionizer(string path, string signature)
		{
			content = File.ReadAllBytes(path);
			sectionCount = (ushort)(content[76] << 8 | content[77]);
			sectionList = new List<RecordInfoEntry>(sectionCount);
			byte[] sig = content.Skip(0x3c).Take(8).ToArray();
			if (Encoding.ASCII.GetString(sig) != signature) throw new FormatException("Invalid eReader file.");
			for (int i = 0; i < sectionCount; i++)
			{
				int si = 78 + i * 8;
				int  offset = content[si] << 24 | content[si + 1] << 16 | content[si + 2] << 8 | content[si + 3];
				byte flags = content[si + 4];
				var value = content[si + 5] << 16 | content[si + 6] << 8 | content[si + 7];
				sectionList.Add(new RecordInfoEntry(offset, flags, value));
			}
		}

		public byte[] GetSection(int sectionNumber)
		{
			long endOfSectionOffset = sectionNumber + 1 == sectionCount ? content.Length : sectionList[sectionNumber + 1].offset;
			long startOfSectionOffset = sectionList[sectionNumber].offset;
			return content.Skip((int)startOfSectionOffset).Take((int)(endOfSectionOffset - startOfSectionOffset)).ToArray();
		}
	}
}