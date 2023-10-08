using Mono.Options;
using System.Collections.Generic;

namespace PCTRTools
{
  public class FontCommand : Command
  {
    private string _inputNarc, _chartableFile, _outputNarc;
    private bool _showHelp;

    public FontCommand() : base("font", "Create font file")
    {
      Options = new OptionSet()
      {
        "Create new font file based on character table.",
        "Usage: PCTRTools font -i [inputNarc] -c [chartableFile] -o [outputNarc]",
        "",
        { "i|input-narc=", "Input narc path", i => _inputNarc = i },
        { "c|chartable-file=", "Character table file path", c => _chartableFile = c },
        { "o|output-narc=", "Output narc path", o => _outputNarc = o},
        { "h|help", "Shows this help screen", h => _showHelp = true },
      };
    }

    public override int Invoke(IEnumerable<string> arguments)
    {
      Options.Parse(arguments);

      if (_showHelp || string.IsNullOrEmpty(_inputNarc) || string.IsNullOrEmpty(_chartableFile) || string.IsNullOrEmpty(_outputNarc))
      {
        int returnValue = 0;
        if (string.IsNullOrEmpty(_inputNarc))
        {
          CommandSet.Error.WriteLine("Input narc not provided, please supply -i or --input-narc");
          returnValue = 1;
        }
        if (string.IsNullOrEmpty(_chartableFile))
        {
          CommandSet.Error.WriteLine("Character table file not provided, please supply -c or --chartable-file");
          returnValue = 1;
        }
        if (string.IsNullOrEmpty(_outputNarc))
        {
          CommandSet.Error.WriteLine("Output narc not provided, please supply -o or --output-narc");
          returnValue = 1;
        }
        Options.WriteOptionDescriptions(CommandSet.Out);
        return returnValue;
      }

      CharTable charTable = new CharTable(_chartableFile);
      NARC font = new NARC(_inputNarc);
      FontTable fontTable_0 = new FontTable(font.Files[0]);
      Generation.Gen gen = fontTable_0.Gen;
      switch (gen)
      {
        case Generation.Gen.Gen4:
          font.Files[0] = fontTable_0.Save(charTable);
          font.Files[1] = new FontTable(font.Files[1]).Save(charTable);
          font.Files[2] = new FontTable(font.Files[2]).Save(charTable, DrawChar.StyleType.TOP_LEFT);
          if (font.Files.Count == 10) // HGSS
          {
            font.Files[4] = new FontTable(font.Files[4]).Save(charTable, DrawChar.StyleType.ROUND);
          }
          break;
        case Generation.Gen.Gen5:
          font.Files[0] = fontTable_0.Save(charTable, DrawChar.StyleType.BOTTOM_RIGHT_5);
          font.Files[1] = new FontTable(font.Files[1]).Save(charTable, DrawChar.StyleType.BOTTOM_RIGHT_5, DrawChar.FontType.PIXEL_9);
          font.Files[2] = new FontTable(font.Files[2]).Save(charTable, DrawChar.StyleType.ROUND, DrawChar.FontType.PIXEL_9);
          break;
        default:
          return -1;
      }
      font.Save(_outputNarc);

      return 0;
    }
  }
}
