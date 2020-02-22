using NARCFileReadingDLL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonCTR
{
    public enum StyleType
    {
        OUT,
        IN
    }

    public enum FontType : int
    {
        SONG_TI = 0,
        HEI_TI = 1,
        MS_GOTHIC = 1
    }

    class DrawChar
    {

        static readonly Color[] Colors = { Color.FromArgb(0, 255, 255, 0), Color.FromArgb(255, 0, 0, 0), Color.FromArgb(255, 255, 255, 255), Color.FromArgb(0, 255, 0, 0) };
        static readonly Font[] Fonts = { new Font("宋体", 12, GraphicsUnit.Pixel), new Font("MS Gothic", 12, GraphicsUnit.Pixel) };

        static public Bitmap ValuesToBitmap(VALUE[,] v, int w = 16, int h = 16)
        {
            Bitmap b = new Bitmap(w, h);
            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    b.SetPixel(x, y, Colors[(int)v[y, x]]);
                }
            }
            return b;
        }
        static public VALUE[,] CharToValues(char c, StyleType type = StyleType.OUT, FontType fontType = FontType.SONG_TI)
        {
            Bitmap b = new Bitmap(16, 16);
            Graphics g = Graphics.FromImage(b);
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
            g.DrawString(c.ToString(), Fonts[(int)fontType], new SolidBrush(Color.Black), new Point(-2, (int)fontType > 1 ? -1 : 0));
            int x, y;
            VALUE[,] v = new VALUE[16, 16];
            for (x = 0; x < 12; x++)
                for (y = 2; y < 14; y++)
                    v[y, x] = (type == 0 ? VALUE.VALUE_3 : VALUE.VALUE_0);
            switch (type)
            {
                case StyleType.OUT:
                    for (x = 0; x < 12; x++)
                    {
                        for (y = 0; y < 12; y++)
                        {
                            if (b.GetPixel(x, y).A > 200)
                            {
                                v[y + 2, x] = VALUE.VALUE_1;
                                v[y + 3, x] = VALUE.VALUE_2;
                                v[y + 2, x + 1] = VALUE.VALUE_2;
                                v[y + 3, x + 1] = VALUE.VALUE_2;
                            }
                        }
                    }
                    break;
                case StyleType.IN:
                    for (x = 11; x > -1; x--)
                    {
                        for (y = 11; y > -1; y--)
                        {
                            if (b.GetPixel(x, y).A > 200)
                            {
                                v[y + 2, x] = VALUE.VALUE_1;
                                v[y + 1, x] = VALUE.VALUE_3;
                                if (x > 0)
                                {
                                    v[y + 2, x - 1] = VALUE.VALUE_3;
                                    v[y + 1, x - 1] = VALUE.VALUE_3;
                                }
                            }
                        }
                    }
                    break;
            }
            return v;
        }
    }
}
