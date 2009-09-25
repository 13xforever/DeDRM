namespace Drm.EReader
{
	internal class HeaderEntry
	{
		public readonly int offset;
		public readonly byte flags;
		public readonly int value;

		public HeaderEntry(int offset, byte flags, int value)
		{
			this.offset = offset;
			this.flags = flags;
			this.value = value;
		}
	}
}