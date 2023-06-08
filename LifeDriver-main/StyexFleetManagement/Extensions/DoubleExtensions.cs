using System;

namespace StyexFleetManagement.Extensions
{
    public static class DoubleExtensions
    {
        private const double MinNormal = 2.2250738585072014E-308d;

        public static bool NearlyEqual(this double a, double b, double epsilon = 2)
        {
            var absA = Math.Abs(a);
            var absB = Math.Abs(b);
            var diff = Math.Abs(a - b);

            if (a.Equals(b))
            {
                // shortcut, handles infinities
                return true;
            }
            else if (a == 0 || b == 0 || absA + absB < MinNormal)
            {
                // a or b is zero or both are extremely close to it
                // relative error is less meaningful here
                return diff < (epsilon * MinNormal);
            }
            else
            {
                // use relative error
                return diff / (absA + absB) < epsilon;
            }
        }
    }
}