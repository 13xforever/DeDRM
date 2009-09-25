using System;
using System.Linq;

namespace Drm.EReader
{
	public class PdbRecordInfo
	{
		public readonly long offset;
		public readonly PdbRecordAttributes flags;
		public readonly int uniqueId;

		public PdbRecordInfo(long offset, byte flags, int uniqueId)
		{
			this.offset = offset;
			this.flags = (PdbRecordAttributes)(flags >> 4 & 0x0f);
			this.uniqueId = uniqueId;
		}

		public PdbRecordInfo(byte[] bytes)
		{
			if (bytes.Length != 8) throw new ArgumentException("Invalid data", "bytes");
			var offsetData = bytes.Take(4);
			if (BitConverter.IsLittleEndian) offsetData = offsetData.Reverse();
			offset = BitConverter.ToUInt32(offsetData.ToArray(), 0);
			flags = (PdbRecordAttributes)(bytes[4] >> 4 & 0x0f); //other bits are Category, which I don't know
			uniqueId = bytes[5] << 16 | bytes[6] << 8 | bytes[7];
		}
	}
}