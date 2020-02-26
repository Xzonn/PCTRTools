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
                font.Files[0] = new FontTable(font.Files[0]).Save(charTable);
                font.Files[1] = new FontTable(font.Files[1]).Save(charTable);
                font.Files[2] = new FontTable(font.Files[2]).Save(charTable, StyleType.TOP_LEFT);
                if (font.Files.Count == 10) // HGSS
                {
                    font.Files[4] = new FontTable(font.Files[4]).Save(charTable, StyleType.ROUND);
                }
                font.Save(o.OutputPath);
            });
        }
    }
}
