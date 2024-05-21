using System;
using System.IO;

namespace Drm;

public class PassThrough : IDrmProcessor
{
	public ReadOnlySpan<byte> Strip(ReadOnlySpan<byte> bookData, string originalFilePath) => bookData;
	public string GetFileName(string originalFilePath) => Path.GetFileName(originalFilePath);
}