using System;
using System.Linq;
using Drm.Format.Epub;
using Drm.Format.EReader;
using Drm.Utils;
using NUnit.Framework;

namespace Drm.Tests;

[TestFixture]
public class UtilsTests
{
	[Test]
	public void ArrayUtilsTest()
	{
		var test = new byte[] {0, 1, 2, 3, 4, 5};
		Assert.Multiple(() =>
		{
			Assert.That(test.Copy(4), Is.EqualTo(new byte[] { 4, 5 }));
			Assert.That(test.Copy(-2), Is.EqualTo(new byte[] { 4, 5 }));
			Assert.That(test.SubRange(2, 5), Is.EqualTo(new byte[] { 2, 3, 4 }));
			Assert.That(test.SubRange(0, 6), Is.EqualTo(new byte[] { 0, 1, 2, 3, 4, 5 }));
			Assert.That(test.SubRange(0, -1), Is.EqualTo(new byte[] { 0, 1, 2, 3, 4 }));
			Assert.That(test.Reverse(), Is.EqualTo(new byte[] { 5, 4, 3, 2, 1, 0 }));
		});
	}

	[Test]
	public void EnumTest()
	{
		var e1 = (EReaderCompression)0x04;
		var e2 = (EReaderCompression)0x01;
		Assert.Multiple(() =>
		{
			Assert.That(e1, Is.EqualTo(EReaderCompression.Drm1));
			Assert.That(e2, Is.Not.EqualTo(EReaderCompression.Drm1));
			Assert.That(e2, Is.Not.EqualTo(EReaderCompression.Drm2));
			Assert.That(e2, Is.Not.EqualTo(EReaderCompression.PalmDoc));
			Assert.That(e2, Is.Not.EqualTo(EReaderCompression.Zlib));
		});
	}

	[Test]
	public void SerialConvertTest()
	{
		var val = 0x1122334455667788ul;
		var result = BitConverter.GetBytes((uint)val).Reverse().ToArray();
		Assert.That(result, Is.EquivalentTo(new byte[] { 0x55, 0x66, 0x77, 0x88 }));
	}

	[Test, Explicit("Manual debug")]
	public void AdeptMasterKeyTest()
	{
		var result = AdeptMasterKeys.Retrieve();
		Assert.That(result, Is.Not.Null.And.Not.Empty);
	}
}