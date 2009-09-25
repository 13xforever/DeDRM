using Drm.Utils;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;

namespace Drm.Tests
{
	[TestFixture]
	public class UtilsTests
	{
		[Test]
		public void ArrayUtilsTest()
		{
			var test = new byte[] {0, 1, 2, 3, 4, 5};
			Assert.That(test.Copy(4), Is.EqualTo(new byte[] {4, 5}));
			Assert.That(test.Copy(-2), Is.EqualTo(new byte[] {4, 5}));
			Assert.That(test.SubRange(2, 5), Is.EqualTo(new byte[] {2, 3, 4}));
			Assert.That(test.SubRange(0, 6), Is.EqualTo(new byte[] {0, 1, 2, 3, 4, 5}));
			Assert.That(test.Reverse(), Is.EqualTo(new byte[] {5, 4, 3, 2, 1, 0}));
		}
	}
}