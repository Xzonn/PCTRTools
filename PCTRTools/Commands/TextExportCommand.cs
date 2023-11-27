using Mono.Options;
using System;
using System.Collections.Generic;

namespace PCTRTools
{
  public class TextExportCommand : Command
  {
    private string _inputNarc, _chartableFile, _outputTextFile;
    private bool _showHelp;

    public TextExportCommand() : base("text-export", "Export text file")
    {
      Options = new OptionSet()
      {
        "Export texts to a txt file based on character table.",
        "Usage: PCTRTools text-export -i [inputNarc] -c [chartableFile] -o [outputTextFile]",
        "",
        { "i|input-narc=", "Input narc path", i => _inputNarc = i },
        { "c|chartable-file=", "Character table file path", c => _chartableFile = c },
        { "o|output-text-file=", "Output text file path", o => _outputTextFile = o},
        { "h|help", "Shows this help screen", h => _showHelp = true },
      };
    }

    public override int Invoke(IEnumerable<string> arguments)
    {
      Options.Parse(arguments);

      if (_showHelp || string.IsNullOrEmpty(_inputNarc) || string.IsNullOrEmpty(_chartableFile) || string.IsNullOrEmpty(_outputTextFile))
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
          if (string.IsNullOrEmpty(_outputTextFile))
          {
            CommandSet.Error.WriteLine("Output text file not provided, please supply -o or --output-text-file");
            returnValue = 1;
          }
        }
        Options.WriteOptionDescriptions(CommandSet.Out);
        return returnValue;
      }

      CharTable charTable = new CharTable(_chartableFile);
      NARC msg = new NARC(_inputNarc);
      Text text;
      switch (Generation.IdentifyGeneration(msg))
      {
        case Generation.Gen.Gen4:
          text = new TextGen4(msg, charTable);
          break;
        case Generation.Gen.Gen5:
          text = new TextGen5(msg);
          break;
        default:
          throw new FormatException();
      }
      text.Extract(_outputTextFile);

      return 0;
    }
  }
}
