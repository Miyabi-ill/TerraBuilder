namespace TUBGWorldGenerator.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public static class Colors
    {
        public static (byte r, byte g, byte b) HSVtoRGB(double h, double s, double v)
        {
            if (h < 0 || h > 360)
            {
                throw new ArgumentException($"H must be 0-360. H: {h}");
            }

            if (s < 0 || s > 1)
            {
                throw new ArgumentException($"S must be 0-1. S: {s}");
            }

            if (v < 0 || v > 360)
            {
                throw new ArgumentException($"V must be 0-1. V: {v}");
            }

            double c = s * v;
            double x = c * (1 - Math.Abs(((h / 60) % 2) - 1));
            double m = v - c;
            if (h < 60)
            {
                return ((byte)((c + m) * 255), (byte)((x + m) * 255), (byte)((0 + m) * 255));
            }
            else if (h < 120)
            {
                return ((byte)((x + m) * 255), (byte)((c + m) * 255), (byte)((0 + m) * 255));
            }
            else if (h < 180)
            {
                return ((byte)((0 + m) * 255), (byte)((c + m) * 255), (byte)((x + m) * 255));
            }
            else if (h < 240)
            {
                return ((byte)((0 + m) * 255), (byte)((x + m) * 255), (byte)((c + m) * 255));
            }
            else if (h < 300)
            {
                return ((byte)((x + m) * 255), (byte)((0 + m) * 255), (byte)((c + m) * 255));
            }
            else
            {
                return ((byte)((c + m) * 255), (byte)((0 + m) * 255), (byte)((x + m) * 255));
            }
        }
    }
}
