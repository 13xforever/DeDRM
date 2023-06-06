using System.IO;

namespace Drm;

public class PassThrough : IDrmProcessor
{
	public byte[] Strip(byte[] bookData, string originalFilePath) => bookData;
	public string GetFileName(string originalFilePath) => Path.GetFileName(originalFilePath);
}