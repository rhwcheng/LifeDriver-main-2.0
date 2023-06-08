using System.Globalization;
using System.Text;

namespace StyexFleetManagement.MProfiler.Common
{
	/// <summary>
	/// Class for common utility functions
	/// </summary>
	public static class Utils
	{
		
		/// <summary>
		/// Convert binary data as hex string (0x01, 0x0A => "010A")
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		public static string AsHexString(byte[] data)
		{
			var sb = new StringBuilder();
			foreach (byte b in data)
				sb.AppendFormat(CultureInfo.InvariantCulture, "{0:X2}", b);

			return sb.ToString();
		}
	}
}
