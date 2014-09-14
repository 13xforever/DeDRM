using System.IO;

namespace Drm
{
	public class PassThrough : IDrmProcessor
	{
		public byte[] Strip(byte[] bookData, string originalFilePath) { return bookData; }
		public string GetFileName(string originalFilePath) { return Path.GetFileName(originalFilePath); }
	}
}