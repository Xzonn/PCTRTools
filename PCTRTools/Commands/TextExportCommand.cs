using Mono.Options;
using System;
using System.Collections.Generic;

namespace PCTRTools.Commands;

public class TextExportCommand : Command
{
  private string _inputNarc, _chartableFile, _outputTextFile, _generation;
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
      { "g|generation=", "Specify generation version", g => _generation = g},
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

    CharTable charTable = new(_chartableFile);
    NARC msg = new(_inputNarc);
    Generation.Gen gen = _generation switch
    {
      "4" => Generation.Gen.Gen4,
      "5" => Generation.Gen.Gen5,
      _ => Generation.IdentifyGeneration(msg),
    };
    Text text = gen switch
    {
      Generation.Gen.Gen4 => new TextGen4(msg, charTable),
      Generation.Gen.Gen5 => new TextGen5(msg),
      _ => throw new FormatException("Unknown generation, please specify through \"-g 4\" or \"-g 5\"."),
    };
    text.Extract(_outputTextFile);

    return 0;
  }
}
