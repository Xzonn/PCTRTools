using Mono.Options;
using System;
using System.Collections.Generic;

namespace PCTRTools
{
  public class TextImportCommand : Command
  {
    private string _inputNarc, _chartableFile, _textFile, _outputNarc;
    private bool _showHelp;

    public TextImportCommand() : base("text-import", "Import text file")
    {
      Options = new OptionSet()
      {
        "Import texts from a txt file based on character table.",
        "Usage: PCTRTools text-import -i [inputNarc] -c [chartableFile] -t [txtFile] -o [outputNarc]",
        "",
        { "i|input-narc=", "Input narc path", i => _inputNarc = i },
        { "c|chartable-file=", "Character table file path", c => _chartableFile = c },
        { "t|text-file=", "Text file path", t => _textFile = t},
        { "o|output-narc=", "Output narc path", o => _outputNarc = o},
        { "h|help", "Shows this help screen", h => _showHelp = true },
      };
    }

    public override int Invoke(IEnumerable<string> arguments)
    {
      Options.Parse(arguments);

      if (_showHelp || string.IsNullOrEmpty(_inputNarc) || string.IsNullOrEmpty(_chartableFile) || string.IsNullOrEmpty(_textFile) || string.IsNullOrEmpty(_outputNarc))
      {
        int returnValue = 0;
        if (!_showHelp)
        {
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
          if (string.IsNullOrEmpty(_textFile))
          {
            CommandSet.Error.WriteLine("Text file not provided, please supply -t or --text-file");
            returnValue = 1;
          }
          if (string.IsNullOrEmpty(_outputNarc))
          {
            CommandSet.Error.WriteLine("Output narc not provided, please supply -o or --output-narc");
            returnValue = 1;
          }
        }
        Options.WriteOptionDescriptions(CommandSet.Out);
        return returnValue;
      }

      CharTable charTable = new CharTable(_chartableFile);
      NARC msg = new NARC(_inputNarc);
      Text text;
      switch (msg.Files.Count)
      {
        case 610: // DP
        case 624: // DP_USA
        case 709: // Pt
        case 814: // HGSS
          text = new TextGen4(msg, charTable);
          break;
        case 273:
        case 472:
          text = new TextGen5(msg);
          break;
        default:
          throw new FormatException();
      }
      text.Import(_textFile);
      text.Save(ref msg, charTable);
      msg.Save(_outputNarc);

      return 0;
    }
  }
}
