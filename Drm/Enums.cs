namespace Drm
{
	public enum ProcessResult
	{
		Skipped,
		Success,
		Fail,
	}

	public enum BookFormat
	{
		Unknown,
		EReader,
		EPub,
	}

	public enum PrivateKeyScheme
	{
		Unknown,
		None,
		Adept,
		Kobo,
		KoboNone,
	}
}