using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonCTR
{
    class PokemonCTRFont
    {
        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args).WithParsed(o =>
            {
                CharTable charTable = new CharTable(o.ChartablePath);
                Narc font = new Narc(o.FontPath);
                FontTable fontTable_0 = new FontTable(font.Files[0]);
                Generation.Gen gen = fontTable_0.Gen;
                switch (gen)
                {
                    case Generation.Gen.Gen4:
                        font.Files[0] = fontTable_0.Save(charTable);
                        font.Files[1] = new FontTable(font.Files[1]).Save(charTable);
                        font.Files[2] = new FontTable(font.Files[2]).Save(charTable, StyleType.TOP_LEFT);
                        if (font.Files.Count == 10) // HGSS
                        {
                            font.Files[4] = new FontTable(font.Files[4]).Save(charTable, StyleType.ROUND);
                        }
                        break;
                    case Generation.Gen.Gen5:
                        font.Files[0] = fontTable_0.Save(charTable, StyleType.BOTTOM_RIGHT_5);
                        font.Files[1] = new FontTable(font.Files[1]).Save(charTable, StyleType.BOTTOM_RIGHT_5, FontType.PIXEL_9);
                        font.Files[2] = new FontTable(font.Files[2]).Save(charTable, StyleType.ROUND, FontType.PIXEL_9);
                        break;
                    default:
                        return;
                }
                font.Save(o.OutputPath);
            });
        }
    }
}
