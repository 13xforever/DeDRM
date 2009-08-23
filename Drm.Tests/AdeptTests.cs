using System.Text;
using Drm.Adept;
using NUnit.Framework;

namespace Drm.Tests
{
	[TestFixture]
	public class AdeptTests
	{
		[Test]
		public void MakeEntropyTest()
		{
			const long systemDriveSerial = 942549858L;
			const string cpuVendor = "GenuineIntel";
			var signature = new byte[] {0x00, 0x06, 0xf7};
			const string username = "13xforever";
			byte[] entropy = KeyRetriever.MakeEntropy(systemDriveSerial, cpuVendor, signature, username);
			const string expected = @"8.+bGenuineIntel\x00\x06\xf713xforever\x00\x00\x00";
			Assert.AreEqual(expected, Encoding.ASCII.GetString(entropy));
		}
	}
}