using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Drm.Utils;

namespace Drm.Format.EReader
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
				if (BitConverter.IsLittleEndian) buf = buf.Reverse();
				attributes = (PdbAttributes)BitConverter.ToUInt16(buf, 0);
				stream.Read(buf, 0, 2);
				if (BitConverter.IsLittleEndian) buf = buf.Reverse();
				fileVersion = BitConverter.ToUInt16(buf, 0);
				buf = new byte[4];
				stream.Read(buf, 0, 4);
				if (BitConverter.IsLittleEndian) buf = buf.Reverse();
				creationDate = BitConverter.ToUInt32(buf, 0);
				stream.Read(buf, 0, 4);
				if (BitConverter.IsLittleEndian) buf = buf.Reverse();
				modificationDate = BitConverter.ToUInt32(buf, 0);
				stream.Read(buf, 0, 4);
				if (BitConverter.IsLittleEndian) buf = buf.Reverse();
				lastBackupDate = BitConverter.ToUInt32(buf, 0);
				stream.Read(buf, 0, 4);
				if (BitConverter.IsLittleEndian) buf = buf.Reverse();
				modificationNumber = BitConverter.ToInt32(buf, 0);
				stream.Read(buf, 0, 4);
				if (BitConverter.IsLittleEndian) buf = buf.Reverse();
				long appInfoOffset = BitConverter.ToUInt32(buf, 0);
				stream.Read(buf, 0, 4);
				if (BitConverter.IsLittleEndian) buf = buf.Reverse();
				long sortInfoOffset = BitConverter.ToUInt32(buf, 0);
				stream.Read(buf, 0, 4);
				filetype = Encoding.ASCII.GetString(buf);
				stream.Read(buf, 0, 4);
				creator = Encoding.ASCII.GetString(buf);
				stream.Read(buf, 0, 4);
				if (BitConverter.IsLittleEndian) buf = buf.Reverse();
				uniqueIdSeed = BitConverter.ToInt32(buf, 0);
				stream.Read(buf, 0, 4); //nextRecordListID = always 0x00000000
				buf = new byte[2];
				stream.Read(buf, 0, 2);
				if (BitConverter.IsLittleEndian) buf = buf.Reverse();
				numberOfRecords = BitConverter.ToUInt16(buf, 0);
				records = new List<PdbRecordInfo>(numberOfRecords);
				buf = new byte[8];
				for (int i = 0; i < numberOfRecords; i++)
				{
					stream.Read(buf, 0, 8);
					records.Add(new PdbRecordInfo(buf));
				}
				if (appInfoOffset != 0) appInfo = ReadAppInfo(stream, appInfoOffset);
				if (sortInfoOffset != 0) sortInfo = ReadSortInfo(stream, sortInfoOffset);
			}
		}

		public byte[] GetSection(int sectionNumber)
		{
			long endOfSectionOffset = sectionNumber + 1 == numberOfRecords ? rawData.Length : records[sectionNumber + 1].offset;
			long startOfSectionOffset = records[sectionNumber].offset;
			return rawData.SubRange(startOfSectionOffset, endOfSectionOffset);
		}


		public string Filename { get { return filename; } }
		public PdbAttributes Attributes { get { return attributes; } }
		public int FileVersion { get { return fileVersion; } }
		public DateTime CreationDate { get { return PalmTimeToDateTime(creationDate); } }
		public DateTime ModificationDate { get { return PalmTimeToDateTime(modificationDate); } }
		public DateTime LastBackupDate { get { return PalmTimeToDateTime(lastBackupDate); } }
		public int ModificationNumber { get { return modificationNumber; } }
		public AppInfo AppInfo { get { return appInfo; } }
		public SortInfo SortInfo { get { return sortInfo; } }
		public string Creator { get { return creator; } }
		public string Filetype { get { return filetype; } }
		public int UniqueIdSeed { get { return uniqueIdSeed; } }
		public int NumberOfRecords { get { return numberOfRecords; } }
		public List<PdbRecordInfo> Records { get { return records; } }

		private static DateTime PalmTimeToDateTime(long palmTime)
		{
			int startDate = 1904;
			if ((palmTime & 0x80000000) > 0) startDate = 1970;
			return new DateTime(startDate, 1, 1).AddSeconds(palmTime);
		}

		private static SortInfo ReadSortInfo(MemoryStream stream, long offset)
		{
			return null; //todo: find info
		}

		private static AppInfo ReadAppInfo(MemoryStream stream, long offset)
		{
			return null; //todo: find info
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
		private readonly List<PdbRecordInfo> records;
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