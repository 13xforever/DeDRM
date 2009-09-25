using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Drm.EReader
{
	public class Pdb
	{
		public Pdb(string filePath)
		{
			rawData = File.ReadAllBytes(filePath);
			using (var stream = new MemoryStream(rawData))
			{
				var buf = new byte[32];
				stream.Read(buf, 0, 32);
				filename = Encoding.ASCII.GetString(buf.TakeWhile(b => b > 0).ToArray());
				buf = new byte[2];
				stream.Read(buf, 0, 2);
				if (BitConverter.IsLittleEndian) buf = buf.Reverse().ToArray();
				attributes = (PdbAttributes)BitConverter.ToUInt16(buf, 0);
				stream.Read(buf, 0, 2);
				if (BitConverter.IsLittleEndian) buf = buf.Reverse().ToArray();
				fileVersion = BitConverter.ToUInt16(buf, 0);
				buf = new byte[4];
				stream.Read(buf, 0, 4);
				if (BitConverter.IsLittleEndian) buf = buf.Reverse().ToArray();
				creationDate = BitConverter.ToUInt32(buf, 0);
				stream.Read(buf, 0, 4);
				if (BitConverter.IsLittleEndian) buf = buf.Reverse().ToArray();
				modificationDate = BitConverter.ToUInt32(buf, 0);
				stream.Read(buf, 0, 4);
				if (BitConverter.IsLittleEndian) buf = buf.Reverse().ToArray();
				lastBackupDate = BitConverter.ToUInt32(buf, 0);
				stream.Read(buf, 0, 4);
				if (BitConverter.IsLittleEndian) buf = buf.Reverse().ToArray();
				modificationNumber = BitConverter.ToInt32(buf, 0);
				stream.Read(buf, 0, 4);
				if (BitConverter.IsLittleEndian) buf = buf.Reverse().ToArray();
				long appInfoOffset = BitConverter.ToUInt32(buf, 0);
				stream.Read(buf, 0, 4);
				if (BitConverter.IsLittleEndian) buf = buf.Reverse().ToArray();
				long sortInfoOffset = BitConverter.ToUInt32(buf, 0);
				stream.Read(buf, 0, 4);
				filetype = Encoding.ASCII.GetString(buf);
				stream.Read(buf, 0, 4);
				creator = Encoding.ASCII.GetString(buf);
				stream.Read(buf, 0, 4);
				if (BitConverter.IsLittleEndian) buf = buf.Reverse().ToArray();
				uniqueIdSeed = BitConverter.ToInt32(buf, 0);
				stream.Read(buf, 0, 4); //nextRecordListID = allways 0x00000000
				buf = new byte[2];
				stream.Read(buf, 0, 2);
				if (BitConverter.IsLittleEndian) buf = buf.Reverse().ToArray();
				numberOfRecords = BitConverter.ToUInt16(buf, 0);
				records = new List<RecordInfoEntry>(numberOfRecords);
				buf = new byte[8];
				for (int i = 0; i < numberOfRecords; i++)
				{
					stream.Read(buf, 0, 8);
					records.Add(new RecordInfoEntry(buf));
				}
				if (appInfoOffset != 0) appInfo = ReadAppInfo(stream);
				if (sortInfoOffset != 0) sortInfo = ReadSortInfo(stream);
			}
		}

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

		public string Filename { get { return filename; } }
		public PdbAttributes Attributes { get { return attributes; } }
		public int FileVersion { get { return fileVersion; } }
		public DateTime CreationDate { get { return new DateTime(1904, 1, 1).AddSeconds(creationDate); } }
		public DateTime ModificationDate { get { return new DateTime(1904, 1, 1).AddSeconds(modificationDate); } }
		public DateTime LastBackupDate { get { return new DateTime(1904, 1, 1).AddSeconds(lastBackupDate); } }
		public int ModificationNumber { get { return modificationNumber; } }
		public AppInfo AppInfo { get { return appInfo; } }
		public SortInfo SortInfo { get { return sortInfo; } }
		public string Creator { get { return creator; } }
		public string Filetype { get { return filetype; } }
		public int UniqueIdSeed { get { return uniqueIdSeed; } }
		public int NumberOfRecords { get { return numberOfRecords; } }
		public List<RecordInfoEntry> Records { get { return records; } }

		private SortInfo ReadSortInfo(MemoryStream stream)
		{
			throw new NotImplementedException();
		}

		private AppInfo ReadAppInfo(MemoryStream stream)
		{
			throw new NotImplementedException();
		}

		private readonly byte[] rawData;
		private readonly string filename;
		private readonly PdbAttributes attributes;
		private readonly int fileVersion;
		private readonly long creationDate;
		private readonly long modificationDate;
		private readonly long lastBackupDate;
		private readonly int modificationNumber;
		private readonly string filetype;
		private readonly string creator;
		private readonly int uniqueIdSeed;
		private readonly int numberOfRecords;
		private readonly List<RecordInfoEntry> records;
		private readonly AppInfo appInfo;
		private readonly SortInfo sortInfo;
	}

	public class AppInfo
	{
	}

	public class SortInfo
	{
	}
}