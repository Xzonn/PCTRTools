using Mono.Options;
using System;
using System.Collections.Generic;

namespace PCTRTools.Commands;

public class TextImportCommand : Command
{
  private string _inputNarc, _chartableFile, _textFile, _outputNarc, _generation;
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
      { "g|generation=", "Specify generation version", g => _generation = g.Trim()},
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
    text.Import(_textFile);
    text.Save(ref msg, charTable);
    msg.Save(_outputNarc);

    return 0;
  }
}
