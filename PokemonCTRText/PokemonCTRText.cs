using CommandLine;
using System;

namespace PokemonCTR
{
    class PokemonCTRText
    {
        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args).WithParsed(o =>
            {
                CharTable charTable = new CharTable(o.ChartablePath);
                Narc msg = new Narc(o.MessagePath);
                Text text;
                switch (msg.Files.Count)
                {
                    case 610: // DP
                    case 709: // Pt
                    case 814: // HGSS
                        text = new Text_4(msg, charTable);
                        break;
                    case 273:
                    case 472:
                        text = new Text_5(msg, charTable);
                        break;
                    default:
                        throw new FormatException();
                }
                if (o.ExtractPath != null)
                {
                    text.Extract(o.ExtractPath);
                }
                if (o.ImportPath != null && o.OutputPath != null)
                {
                    text.Import(o.ImportPath);
                    text.Save(ref msg, charTable);
                    msg.Save(o.OutputPath);
                }
                if (charTable.NoCode.Count > 0)
                {
                    Console.WriteLine("以下字符未在码表中：" + string.Join("", charTable.NoCode));
                }
                if (charTable.NoChar.Count > 0)
                {
                    Console.WriteLine("以下编码未在码表中：" + string.Join(", ", charTable.NoChar));
                }
            });
        }
    }
}
