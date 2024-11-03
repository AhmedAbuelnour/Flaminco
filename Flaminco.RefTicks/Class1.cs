using System.Diagnostics;

namespace Flaminco.RefTicks
{
    public static class UniqueTickReference
    {
        public static string GetRef()
        {
            return Stopwatch.GetTimestamp().ToString("x");
        }

        public static string GetRef(long ticks)
        {
            return ticks.ToString("x");
        }

        public static Guid ToGuid(string refNumber)
        {
            long revertedTicks = Convert.ToInt64(refNumber, 16);

            byte[] revertedTickBytes = BitConverter.GetBytes(revertedTicks);

            byte[] recreatedBytes = new byte[16];

            Array.Copy(revertedTickBytes, 0, recreatedBytes, 0, revertedTickBytes.Length); // Copy the ticks bytes

            return new(recreatedBytes);
        }
    }
}
