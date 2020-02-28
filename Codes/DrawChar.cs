using NARCFileReadingDLL;
using System;
using System.Drawing;

namespace PokemonCTR
{
    public enum StyleType
    {
        BOTTOM_RIGHT = 0,
        TOP_LEFT = 1,
        ROUND = 2
    }

    public enum FontType : int
    {
        SONG_TI = 0,
        HEI_TI = 1,
        MS_GOTHIC = 2
    }

    class DrawChar
    {

        static readonly Color[] Colors = { Color.FromArgb(0, 255, 255, 0), Color.FromArgb(255, 0, 0, 0), Color.FromArgb(255, 128, 128, 128), Color.FromArgb(128, 255, 0, 0) };
        static readonly Font[] Fonts = { new Font("新宋体", 12, GraphicsUnit.Pixel), new Font("黑体", 12, GraphicsUnit.Pixel), new Font("MS Gothic", 12, GraphicsUnit.Pixel) };

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
        static public VALUE[,] CharToValues(char c, StyleType type = StyleType.BOTTOM_RIGHT, FontType fontType = FontType.SONG_TI, int posX = -2, int posY = 1)
        {
            Bitmap b = new Bitmap(16, 16);
            Graphics g = Graphics.FromImage(b);
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
            g.DrawString(c.ToString(), Fonts[(int)fontType], new SolidBrush(Color.Black), new Point(posX, posY));
            int x, y;
            VALUE[,] v = new VALUE[16, 16];
            for (x = 0; x < 16; x++)
            {
                for (y = 0; y < 16; y++)
                {
                    v[y, x] = type == StyleType.BOTTOM_RIGHT ? VALUE.VALUE_3 : VALUE.VALUE_0;
                }
            }
            switch (type)
            {
                case StyleType.BOTTOM_RIGHT:
                    for (x = 0; x < 15; x++)
                    {
                        for (y = 0; y < 15; y++)
                        {
                            if (b.GetPixel(x, y).A > 200)
                            {
                                v[y, x] = VALUE.VALUE_1;
                                v[y + 1, x] = VALUE.VALUE_2;
                                v[y, x + 1] = VALUE.VALUE_2;
                                v[y + 1, x + 1] = VALUE.VALUE_2;
                            }
                        }
                    }
                    break;
                case StyleType.TOP_LEFT:
                    for (x = 15; x > -1; x--)
                    {
                        for (y = 14; y > -1; y--)
                        {
                            if (b.GetPixel(x, y).A > 200)
                            {
                                v[y + 1, x + 1] = VALUE.VALUE_1;
                                v[y, x + 1] = VALUE.VALUE_3;
                                v[y + 1, x] = VALUE.VALUE_3;
                                v[y, x] = VALUE.VALUE_3;
                            }
                        }
                    }
                    break;
                case StyleType.ROUND:
                    for (x = 0; x < 14; x++)
                    {
                        for (y = 0; y < 14; y++)
                        {
                            if (b.GetPixel(x, y).A > 200)
                            {
                                v[y + 1, x + 1] = VALUE.VALUE_1;
                                v[y, x] = v[y, x] == VALUE.VALUE_0 ? VALUE.VALUE_2 : v[y, x];
                                v[y + 1, x] = v[y + 1, x] == VALUE.VALUE_0 ? VALUE.VALUE_2 : v[y + 1, x];
                                v[y + 2, x] = v[y + 2, x] == VALUE.VALUE_0 ? VALUE.VALUE_2 : v[y + 2, x];
                                v[y, x + 1] = v[y, x + 1] == VALUE.VALUE_0 ? VALUE.VALUE_2 : v[y, x + 1];
                                v[y + 2, x + 1] = VALUE.VALUE_2;
                                v[y, x + 2] = v[y, x + 2] == VALUE.VALUE_0 ? VALUE.VALUE_2 : v[y, x + 2];
                                v[y + 1, x + 2] = VALUE.VALUE_2;
                                v[y + 2, x + 2] = VALUE.VALUE_2;
                            }
                        }
                    }
                    break;
            }
            return v;
        }

        static public Size ValuesToSize(VALUE[,] v, StyleType type = StyleType.BOTTOM_RIGHT, int w = 16, int h = 16)
        {
            int x, y, minX, maxX, minY, maxY;
            for (minX = 0; minX < w; minX++)
            {
                for (y = 0; y < h; y++)
                {
                    if ((type == StyleType.BOTTOM_RIGHT && v[y, minX] != VALUE.VALUE_3) || (type != StyleType.BOTTOM_RIGHT && v[y, minX] != VALUE.VALUE_0))
                    {
                        break;
                    }
                }
                if (y < h)
                {
                    break;
                }
            }
            for (maxX = w - 1; maxX > -1; maxX--)
            {
                for (y = 0; y < h; y++)
                {
                    if ((type == StyleType.BOTTOM_RIGHT && v[y, maxX] != VALUE.VALUE_3) || (type != StyleType.BOTTOM_RIGHT && v[y, maxX] != VALUE.VALUE_0))
                    {
                        break;
                    }
                }
                if (y < h)
                {
                    break;
                }
            }
            for (minY = 0; minY < h; minY++)
            {
                for (x = 0; x < w; x++)
                {
                    if ((type == StyleType.BOTTOM_RIGHT && v[minY, x] != VALUE.VALUE_3) || (type != StyleType.BOTTOM_RIGHT && v[minY, x] != VALUE.VALUE_0))
                    {
                        break;
                    }
                }
                if (x < w)
                {
                    break;
                }
            }
            for (maxY = h - 1; maxY > -1; maxY--)
            {
                for (x = 0; x < w; x++)
                {
                    if ((type == StyleType.BOTTOM_RIGHT && v[maxY, x] != VALUE.VALUE_3) || (type != StyleType.BOTTOM_RIGHT && v[maxY, x] != VALUE.VALUE_0))
                    {
                        break;
                    }
                }
                if (x < w)
                {
                    break;
                }
            }
            Console.WriteLine(maxX);
            if (maxX < minX)
            {
                maxX = minX - 1;
            }
            if (maxY < minY)
            {
                maxY = minY - 1;
            }
            return new Size(maxX - minX + 1, maxY - minY + 1);
        }
    }
}
