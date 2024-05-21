using System;

namespace Drm;

public interface IDrmProcessor
{
	ReadOnlySpan<byte> Strip(ReadOnlySpan<byte> bookData, string originalFilePath);
	string GetFileName(string originalFilePath);
}