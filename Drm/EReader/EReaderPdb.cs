using System;
using System.IO;
using Drm.Utils;

namespace Drm.EReader
{
	public class EReaderPdb
	{
		public EReaderPdb(Pdb pdbReader)
		{
			this.pdbReader = pdbReader;
			byte[] headerRawData = pdbReader.GetSection(0);
			if (headerRawData.Length != 132)
				throw new FormatException("Unknown eReader header format.");

			using (var stream = new MemoryStream(headerRawData))
			{
				var b = (byte)stream.ReadByte();
				if (b > 1) throw new FormatException(string.Format("Unknown DRM flag {0}", b));
				haveDrm = b == 1;
				b = (byte)stream.ReadByte();
				compressionMethod = (EReaderCompression)b;
				var buf = new byte[4];
				stream.Read(buf, 0, 4); //unknown, should be 0x00000000
				buf = new byte[2];
				stream.Read(buf, 0, 2);
				if (BitConverter.IsLittleEndian) buf = buf.Reverse();
				textEncoding = BitConverter.ToUInt16(buf, 0); //should be 2515 or 25152 for cp1251
				stream.Read(buf, 0, 2);
				if (BitConverter.IsLittleEndian) buf = buf.Reverse();
				numberOfSmallFontPages = BitConverter.ToUInt16(buf, 0);
				stream.Read(buf, 0, 2);
				if (BitConverter.IsLittleEndian) buf = buf.Reverse();
				numberOfLargeFontPages = BitConverter.ToUInt16(buf, 0);
				stream.Read(buf, 0, 2);
				if (BitConverter.IsLittleEndian) buf = buf.Reverse();
				nonTextFirstRecord = BitConverter.ToUInt16(buf, 0);
				stream.Read(buf, 0, 2);
				if (BitConverter.IsLittleEndian) buf = buf.Reverse();
				numberOfChapters = BitConverter.ToUInt16(buf, 0);
				stream.Read(buf, 0, 2);
				if (BitConverter.IsLittleEndian) buf = buf.Reverse();
				numberOfSmallFontIndexPages = BitConverter.ToUInt16(buf, 0);
				stream.Read(buf, 0, 2);
				if (BitConverter.IsLittleEndian) buf = buf.Reverse();
				numberOfLargeFontIndexPages = BitConverter.ToUInt16(buf, 0);
				stream.Read(buf, 0, 2);
				if (BitConverter.IsLittleEndian) buf = buf.Reverse();
				numberOfImages = BitConverter.ToUInt16(buf, 0);
				stream.Read(buf, 0, 2);
				if (BitConverter.IsLittleEndian) buf = buf.Reverse();
				numberOfLinks = BitConverter.ToUInt16(buf, 0);
				b = (byte)stream.ReadByte();
				if (b > 1) throw new FormatException(string.Format("Incorrect Metadata flag {0}", b));
				haveMetadata = b == 1;
				buf = new byte[3];
				stream.Read(buf, 0, 3); //unknown, should be 0
				buf = new byte[2];
				stream.Read(buf, 0, 2);
				if (BitConverter.IsLittleEndian) buf = buf.Reverse();
				numberOfFootnotes = BitConverter.ToUInt16(buf, 0);
				stream.Read(buf, 0, 2);
				if (BitConverter.IsLittleEndian) buf = buf.Reverse();
				numberOfSidebars = BitConverter.ToUInt16(buf, 0);
				stream.Read(buf, 0, 2);
				if (BitConverter.IsLittleEndian) buf = buf.Reverse();
				chapterIndexFirstRecord = BitConverter.ToUInt16(buf, 0);
				stream.Read(buf, 0, 2);
				if (BitConverter.IsLittleEndian) buf = buf.Reverse();
				magickValue = BitConverter.ToUInt16(buf, 0);
				//if (magickValue != 2560) throw new FormatException(string.Format("Unknown Magick Value {0}", magickValue));
				stream.Read(buf, 0, 2);
				if (BitConverter.IsLittleEndian) buf = buf.Reverse();
				smallFontPageIndexeFirstRecord = BitConverter.ToUInt16(buf, 0);
				stream.Read(buf, 0, 2);
				if (BitConverter.IsLittleEndian) buf = buf.Reverse();
				largeFontPageIndexeFirstRecord = BitConverter.ToUInt16(buf, 0);
				stream.Read(buf, 0, 2);
				if (BitConverter.IsLittleEndian) buf = buf.Reverse();
				imageDataFirstRecord = BitConverter.ToUInt16(buf, 0);
				stream.Read(buf, 0, 2);
				if (BitConverter.IsLittleEndian) buf = buf.Reverse();
				linksFirstRecord = BitConverter.ToUInt16(buf, 0);
				stream.Read(buf, 0, 2);
				if (BitConverter.IsLittleEndian) buf = buf.Reverse();
				metadataFirstRecord = BitConverter.ToUInt16(buf, 0);
				stream.Read(buf, 0, 2); //unknown, should be 0x0000
				stream.Read(buf, 0, 2);
				if (BitConverter.IsLittleEndian) buf = buf.Reverse();
				footnotesFirstRecord = BitConverter.ToUInt16(buf, 0);
				stream.Read(buf, 0, 2);
				if (BitConverter.IsLittleEndian) buf = buf.Reverse();
				sidebarFirstRecord = BitConverter.ToUInt16(buf, 0);
				stream.Read(buf, 0, 2);
				if (BitConverter.IsLittleEndian) buf = buf.Reverse();
				lastRecord = BitConverter.ToUInt16(buf, 0);
			}
		}

		public bool HaveDrm { get { return haveDrm; } set { haveDrm = value; } }
		public EReaderCompression CompressionMethod { get { return compressionMethod; } set { compressionMethod = value; } }
		public int TextEncoding { get { return textEncoding; } set { textEncoding = value; } }
		public int NumberOfSmallFontPages { get { return numberOfSmallFontPages; } set { numberOfSmallFontPages = value; } }
		public int NumberOfLargeFontPages { get { return numberOfLargeFontPages; } set { numberOfLargeFontPages = value; } }
		public int NonTextFirstRecord { get { return nonTextFirstRecord; } set { nonTextFirstRecord = value; } }
		public int NumberOfChapters { get { return numberOfChapters; } set { numberOfChapters = value; } }
		public int NumberOfSmallFontIndexPages { get { return numberOfSmallFontIndexPages; } set { numberOfSmallFontIndexPages = value; } }
		public int NumberOfLargeFontIndexPages { get { return numberOfLargeFontIndexPages; } set { numberOfLargeFontIndexPages = value; } }
		public int NumberOfImages { get { return numberOfImages; } set { numberOfImages = value; } }
		public int NumberOfLinks { get { return numberOfLinks; } set { numberOfLinks = value; } }
		public bool HaveMetadata { get { return haveMetadata; } set { haveMetadata = value; } }
		public int NumberOfFootnotes { get { return numberOfFootnotes; } set { numberOfFootnotes = value; } }
		public int NumberOfSidebars { get { return numberOfSidebars; } set { numberOfSidebars = value; } }
		public int ChapterIndexFirstRecord { get { return chapterIndexFirstRecord; } set { chapterIndexFirstRecord = value; } }
		public int MagickValue { get { return magickValue; } set { magickValue = value; } }
		public int SmallFontPageIndexeFirstRecord { get { return smallFontPageIndexeFirstRecord; } set { smallFontPageIndexeFirstRecord = value; } }
		public int LargeFontPageIndexeFirstRecord { get { return largeFontPageIndexeFirstRecord; } set { largeFontPageIndexeFirstRecord = value; } }
		public int ImageDataFirstRecord { get { return imageDataFirstRecord; } set { imageDataFirstRecord = value; } }
		public int LinksFirstRecord { get { return linksFirstRecord; } set { linksFirstRecord = value; } }
		public int MetadataFirstRecord { get { return metadataFirstRecord; } set { metadataFirstRecord = value; } }
		public int FootnotesFirstRecord { get { return footnotesFirstRecord; } set { footnotesFirstRecord = value; } }
		public int SidebarFirstRecord { get { return sidebarFirstRecord; } set { sidebarFirstRecord = value; } }
		public int LastRecord { get { return lastRecord; } set { lastRecord = value; } }
		private Pdb pdbReader;
		private bool haveDrm;
		private EReaderCompression compressionMethod;
		private int textEncoding;
		private int numberOfSmallFontPages;
		private int numberOfLargeFontPages;
		private int nonTextFirstRecord;
		private int numberOfChapters;
		private int numberOfSmallFontIndexPages;
		private int numberOfLargeFontIndexPages;
		private int numberOfImages;
		private int numberOfLinks;
		private bool haveMetadata;
		private int numberOfFootnotes;
		private int numberOfSidebars;
		private int chapterIndexFirstRecord;
		private int magickValue;
		private int smallFontPageIndexeFirstRecord;
		private int largeFontPageIndexeFirstRecord;
		private int imageDataFirstRecord;
		private int linksFirstRecord;
		private int metadataFirstRecord;
		private int footnotesFirstRecord;
		private int sidebarFirstRecord;
		private int lastRecord;
	}
}