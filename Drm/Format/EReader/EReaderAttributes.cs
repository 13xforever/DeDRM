namespace Drm.Format.EReader
{
	public enum EReaderCompression : byte
	{
		PalmDoc = 0x02,
		Drm1	= 0x04,
		Zlib	= 0x0A,
		Drm2	= 0x10,
	}
}