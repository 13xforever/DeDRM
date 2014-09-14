namespace Drm
{
	public class PassThrough : IDrmProcessor
	{
		public byte[] Strip(byte[] bookData) { return bookData; }
	}
}