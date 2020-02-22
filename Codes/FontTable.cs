using NARCFileReadingDLL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonCTR
{
    class FontTable
    {
        static readonly int ChineseCharStart = 0x01FE;
        static readonly string ChinesePunctuation = "、，。？！：；“”‘’《》（）—～";
        public SimpleFontTable Table;

        public FontTable(byte[] bytes)
        {
            BinaryReader br = new BinaryReader(new MemoryStream(bytes));
            Table = new SimpleFontTable(br);
            br.Close();
        }

        public bool Export(string path)
        {
            try
            {
                for (int i = 1; i < Table.Items.Length; i++)
                {
                    DrawChar.ValuesToBitmap(Table.Items[i - 1].Item).Save(Path.Combine(path, $"{i:X4}.png"));
                }
                return true;
            }
            catch (IOException ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        public byte[] Save(CharTable charTable, StyleType style = StyleType.OUT)
        {
            while (Table.Items.Length <= charTable.maxCharCode)
            {
                Table.AddNewItem();
            }
            for (int i = 1; i <= charTable.maxCharCode; i++)
            {
                char c = charTable.GetCharacter(i);
                if (ChinesePunctuation.Contains(c) || i >= ChineseCharStart)
                {
                    Table.Items[i - 1].Item = DrawChar.CharToValues(c, style);
                    Table.Items[i - 1].Width = 13;
                }
            }
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);
            Table.WriteTo(bw);
            byte[] bytes = new byte[ms.Length];
            ms.Position = 0;
            ms.Read(bytes, 0, bytes.Length);
            bw.Close();
            return bytes;
        }
    }
}
