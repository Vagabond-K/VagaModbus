using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;

namespace VagaModbusAnalyzer.Infrastructures
{
    public static class ColorAPI
    {
        public static IEnumerable<Color> GenerateAccentColors(this Color baseColor)
        {
            Color accent = baseColor;

            ColorHSV colorHSV = ToHSV(baseColor);

            ColorHSV lightHSV = Lighter(colorHSV, colorHSV);
            Color light = ToRGB(lightHSV);
            lightHSV = Lighter(lightHSV, colorHSV);
            Color lighter = ToRGB(lightHSV);
            lightHSV = Lighter(lightHSV, colorHSV);
            Color lightest = ToRGB(lightHSV);

            ColorHSV darkHSV = Darker(colorHSV, colorHSV);
            Color dark = ToRGB(darkHSV);
            darkHSV = Darker(darkHSV, colorHSV);
            Color darker = ToRGB(darkHSV);
            darkHSV = Darker(darkHSV, colorHSV);
            Color darkest = ToRGB(darkHSV);

            yield return lightest;
            yield return lighter;
            yield return light;
            yield return accent;
            yield return dark;
            yield return darker;
            yield return darkest;
        }

        public static ColorHSV ToHSV(this Color color)
        {
            double h = 0;
            double s = 0;
            double v = 0;

            double R = (double)(color.R) / 255;
            double G = (double)(color.G) / 255;
            double B = (double)(color.B) / 255;

            double maxComp = Math.Max(R, Math.Max(G, B));
            double minComp = Math.Min(R, Math.Min(G, B));

            double minMaxDiff = maxComp - minComp;

            if (minMaxDiff != 0)
            {
                if (maxComp == R)
                {
                    h = (((G - B)) / minMaxDiff);
                    if (h < 0) h += (6);
                }
                else if (maxComp == G)
                {
                    h = (((B - R)) / minMaxDiff) + (2);
                }
                else
                {
                    h = (((R - G)) / minMaxDiff) + (4);
                }
            }

            v = maxComp;
            s = v != 0 ? (minMaxDiff) / v : 0;

            return new ColorHSV((h * 60) % 360, s * 100, v * 100);
        }

        public static Color ToRGB(this ColorHSV color)
        {
            double H = color.H;
            double S = color.S / 100;
            double V = color.V / 100;

            while (H < 0) { H += 360; };
            while (H >= 360) { H -= 360; };
            double R, G, B;

            if (V <= 0)
            {
                R = G = B = 0;
            }
            else if (S <= 0)
            {
                R = G = B = V;
            }
            else
            {
                double hf = H / 60.0;
                int i = (int)Math.Floor(hf);
                double f = hf - i;
                double pv = V * (1 - S);
                double qv = V * (1 - S * f);
                double tv = V * (1 - S * (1 - f));
                switch (i)
                {
                    // Red is the dominant color
                    case 0:
                        R = V;
                        G = tv;
                        B = pv;
                        break;
                    // Green is the dominant color
                    case 1:
                        R = qv;
                        G = V;
                        B = pv;
                        break;
                    case 2:
                        R = pv;
                        G = V;
                        B = tv;
                        break;
                    // Blue is the dominant color
                    case 3:
                        R = pv;
                        G = qv;
                        B = V;
                        break;
                    case 4:
                        R = tv;
                        G = pv;
                        B = V;
                        break;
                    // Red is the dominant color
                    case 5:
                        R = V;
                        G = pv;
                        B = qv;
                        break;
                    // Just in case we overshoot on our math by a little, we put these here. Since its a switch it won't slow us down at all to put these here.
                    case 6:
                        R = V;
                        G = tv;
                        B = pv;
                        break;
                    case -1:
                        R = V;
                        G = pv;
                        B = qv;
                        break;
                    // The color is not defined, we should throw an error.
                    default:
                        //LFATAL("i Value error in Pixel conversion, Value is %d", i);
                        R = G = B = V; // Just pretend its black/white
                        break;
                }
            }

            return Color.FromArgb(0xff,
                (byte)Math.Min(Math.Round(R * 255), 255),
                (byte)Math.Min(Math.Round(G * 255), 255),
                (byte)Math.Min(Math.Round(B * 255), 255));
        }

        private static ColorHSV Lighter(ColorHSV prevHSV, ColorHSV baseHSV)
        {
            double v = prevHSV.V;
            double s = prevHSV.S;

            // Shade: 18% of V
            double Vstep = baseHSV.V * 0.18;

            v = Math.Min(prevHSV.V + Vstep, 100);

            // If V >= 70%, reduce sat to 75% rel
            s = (v >= 70) ? prevHSV.S * 0.75 : prevHSV.S;

            return new ColorHSV(prevHSV.H, s, v);
        }

        private static ColorHSV Darker(ColorHSV prevHSV, ColorHSV baseHSV)
        {
            double v = prevHSV.V;

            // Shade: 20% of V
            double Vstep = baseHSV.V * 0.20;

            v = Math.Max(prevHSV.V - Vstep, 0);

            return new ColorHSV(prevHSV.H, prevHSV.S, v);
        }
    }

    public struct ColorHSV
    {
        public ColorHSV(double h, double s, double v)
        {
            H = h;
            S = s;
            V = v;
        }

        public double H { get; }
        public double S { get; }
        public double V { get; }
    };
}
