using System;
using System.IO;
using Drm.Utils;

namespace Drm.Format.EReader
{
	public class EReaderPdb
	{
		public EReaderPdb(Pdb pdbReader)
		{
			this.pdbReader = pdbReader;
			var headerRawData = pdbReader.GetSection(0);
			if (headerRawData.Length != 132)
				throw new FormatException("Unknown eReader header format.");

			using (var stream = new MemoryStream(headerRawData))
			{
				var b = (byte)stream.ReadByte();
				if (b > 1) throw new FormatException($"Unknown DRM flag {b}");
				HaveDrm = b == 1;
				b = (byte)stream.ReadByte();
				CompressionMethod = (EReaderCompression)b;
				var buf = new byte[4];
				stream.Read(buf, 0, 4); //unknown, should be 0x00000000
				buf = new byte[2];
				stream.Read(buf, 0, 2);
				if (BitConverter.IsLittleEndian)
					buf = buf.Reverse();
				TextEncoding = BitConverter.ToUInt16(buf, 0); //should be 2515 or 25152 for cp1251
				stream.Read(buf, 0, 2);
				if (BitConverter.IsLittleEndian)
					buf = buf.Reverse();
				NumberOfSmallFontPages = BitConverter.ToUInt16(buf, 0);
				stream.Read(buf, 0, 2);
				if (BitConverter.IsLittleEndian)
					buf = buf.Reverse();
				NumberOfLargeFontPages = BitConverter.ToUInt16(buf, 0);
				stream.Read(buf, 0, 2);
				if (BitConverter.IsLittleEndian)
					buf = buf.Reverse();
				NonTextFirstRecord = BitConverter.ToUInt16(buf, 0);
				stream.Read(buf, 0, 2);
				if (BitConverter.IsLittleEndian)
					buf = buf.Reverse();
				NumberOfChapters = BitConverter.ToUInt16(buf, 0);
				stream.Read(buf, 0, 2);
				if (BitConverter.IsLittleEndian)
					buf = buf.Reverse();
				NumberOfSmallFontIndexPages = BitConverter.ToUInt16(buf, 0);
				stream.Read(buf, 0, 2);
				if (BitConverter.IsLittleEndian)
					buf = buf.Reverse();
				NumberOfLargeFontIndexPages = BitConverter.ToUInt16(buf, 0);
				stream.Read(buf, 0, 2);
				if (BitConverter.IsLittleEndian)
					buf = buf.Reverse();
				NumberOfImages = BitConverter.ToUInt16(buf, 0);
				stream.Read(buf, 0, 2);
				if (BitConverter.IsLittleEndian)
					buf = buf.Reverse();
				NumberOfLinks = BitConverter.ToUInt16(buf, 0);
				b = (byte)stream.ReadByte();
				if (b > 1)
					throw new FormatException($"Incorrect Metadata flag {b}");

				HaveMetadata = b == 1;
				buf = new byte[3];
				stream.Read(buf, 0, 3); //unknown, should be 0
				buf = new byte[2];
				stream.Read(buf, 0, 2);
				if (BitConverter.IsLittleEndian)
					buf = buf.Reverse();
				NumberOfFootnotes = BitConverter.ToUInt16(buf, 0);
				stream.Read(buf, 0, 2);
				if (BitConverter.IsLittleEndian)
					buf = buf.Reverse();
				NumberOfSidebars = BitConverter.ToUInt16(buf, 0);
				stream.Read(buf, 0, 2);
				if (BitConverter.IsLittleEndian)
					buf = buf.Reverse();
				ChapterIndexFirstRecord = BitConverter.ToUInt16(buf, 0);
				stream.Read(buf, 0, 2);
				if (BitConverter.IsLittleEndian)
					buf = buf.Reverse();
				MagicValue = BitConverter.ToUInt16(buf, 0);
				//if (magickValue != 2560) throw new FormatException(string.Format("Unknown Magick Value {0}", magickValue));
				stream.Read(buf, 0, 2);
				if (BitConverter.IsLittleEndian)
					buf = buf.Reverse();
				SmallFontPageIndexFirstRecord = BitConverter.ToUInt16(buf, 0);
				stream.Read(buf, 0, 2);
				if (BitConverter.IsLittleEndian)
					buf = buf.Reverse();
				LargeFontPageIndexFirstRecord = BitConverter.ToUInt16(buf, 0);
				stream.Read(buf, 0, 2);
				if (BitConverter.IsLittleEndian)
					buf = buf.Reverse();
				ImageDataFirstRecord = BitConverter.ToUInt16(buf, 0);
				stream.Read(buf, 0, 2);
				if (BitConverter.IsLittleEndian)
					buf = buf.Reverse();
				LinksFirstRecord = BitConverter.ToUInt16(buf, 0);
				stream.Read(buf, 0, 2);
				if (BitConverter.IsLittleEndian)
					buf = buf.Reverse();
				MetadataFirstRecord = BitConverter.ToUInt16(buf, 0);
				stream.Read(buf, 0, 2); //unknown, should be 0x0000
				stream.Read(buf, 0, 2);
				if (BitConverter.IsLittleEndian)
					buf = buf.Reverse();
				FootnotesFirstRecord = BitConverter.ToUInt16(buf, 0);
				stream.Read(buf, 0, 2);
				if (BitConverter.IsLittleEndian)
					buf = buf.Reverse();
				SidebarFirstRecord = BitConverter.ToUInt16(buf, 0);
				stream.Read(buf, 0, 2);
				if (BitConverter.IsLittleEndian)
					buf = buf.Reverse();
				LastRecord = BitConverter.ToUInt16(buf, 0);
			}
		}

		public bool HaveDrm { get; }
		public EReaderCompression CompressionMethod { get; }
		public int TextEncoding { get; }
		public int NumberOfSmallFontPages { get; }
		public int NumberOfLargeFontPages { get; }
		public int NonTextFirstRecord { get; }
		public int NumberOfChapters { get; }
		public int NumberOfSmallFontIndexPages { get; }
		public int NumberOfLargeFontIndexPages { get; }
		public int NumberOfImages { get; }
		public int NumberOfLinks { get; }
		public bool HaveMetadata { get; }
		public int NumberOfFootnotes { get; }
		public int NumberOfSidebars { get; }
		public int ChapterIndexFirstRecord { get; }
		public int MagicValue { get; }
		public int SmallFontPageIndexFirstRecord { get; }
		public int LargeFontPageIndexFirstRecord { get; }
		public int ImageDataFirstRecord { get; }
		public int LinksFirstRecord { get; }
		public int MetadataFirstRecord { get; }
		public int FootnotesFirstRecord { get; }
		public int SidebarFirstRecord { get; }
		public int LastRecord { get; }

		private Pdb pdbReader;
	}
}