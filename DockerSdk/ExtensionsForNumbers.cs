using System.Globalization;

namespace DockerSdk
{
    internal static class ExtensionsForNumbers
    {
        public static string ToStringI(this ushort number) => number.ToString(CultureInfo.InvariantCulture);

        public static string ToStringI(this int number) => number.ToString(CultureInfo.InvariantCulture);
    }
}
