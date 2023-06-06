using System.Linq;
using System.Net.NetworkInformation;

namespace Drm.Utils;

public static class PhysicalAddressUtils
{
	public static string ToMacString(this PhysicalAddress address)
	{
		var bytes = address.GetAddressBytes();
		return string.Join(":", bytes.Select(b => b.ToString("X2")));
	}
}