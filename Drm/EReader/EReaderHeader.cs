using System.IO;
using System.Linq;
using System.Text;
using Drm.Utils;

namespace Drm.EReader
{
	public class EReaderHeader
	{
		private readonly byte[] rawData;
		private bool haveDrm;
		private EReaderCompression compressionMethod;

		public EReaderHeader(byte[] section0Data)
		{
			rawData = section0Data.Copy(0);

			using (var stream = new MemoryStream(section0Data))
			{
				haveDrm = stream.ReadByte() == 1;
				compressionMethod = (EReaderCompression)stream.ReadByte();
				var buf = new byte[1];
				

			}
		}
	}
}