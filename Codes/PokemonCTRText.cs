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
                Text text = new Text(msg, charTable);
                if (o.ExtractPath != null)
                {
                    text.Extract(o.ExtractPath);
                } else if (o.ImportPath != null && o.OutputPath != null)
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
