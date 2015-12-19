namespace Drm
{
	public interface IDrmProcessor
	{
		byte[] Strip(byte[] bookData, string originalFilePath);
		string GetFileName(string originalFilePath);
	}
}