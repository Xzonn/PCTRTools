using Mono.Options;
using System.Collections.Generic;
using System.IO;

namespace PCTRTools.Commands;

public class AppendNarcCommand : Command
{
  private string _inputPath, _appendPath, _outputPath;
  private bool _showHelp;

  public AppendNarcCommand() : base("append-narc", "Append a file to a narc file")
  {
    Options = new OptionSet()
    {
      "Replace narc files in a directory.",
      "Usage: PCTRTools replace-narc -i [inputDir] -c [newFilesDir] -o [outputDir]",
      "",
      { "i|input-path=", "Input file path", i => _inputPath = i },
      { "a|append-path=", "New file for appending path", a => _appendPath = a },
      { "o|output-path=", "Output file path", o => _outputPath = o},
      { "h|help", "Shows this help screen", h => _showHelp = true },
    };
  }

  public override int Invoke(IEnumerable<string> arguments)
  {
    Options.Parse(arguments);

    if (_showHelp || string.IsNullOrEmpty(_inputPath) || string.IsNullOrEmpty(_appendPath) || string.IsNullOrEmpty(_outputPath))
    {
      int returnValue = 0;
      if (!_showHelp)
      {
        if (string.IsNullOrEmpty(_inputPath))
        {
          CommandSet.Error.WriteLine("Input file path not provided, please supply -i or --input-path");
          returnValue = 1;
        }
        if (string.IsNullOrEmpty(_appendPath))
        {
          CommandSet.Error.WriteLine("New file for appending not provided, please supply -a or --append-path");
          returnValue = 1;
        }
        if (string.IsNullOrEmpty(_outputPath))
        {
          CommandSet.Error.WriteLine("Output file path not provided, please supply -o or --output-path");
          returnValue = 1;
        }
      }
      Options.WriteOptionDescriptions(CommandSet.Out);
      return returnValue;
    }

    NARC narc = new(_inputPath);
    narc.Files.Add(File.ReadAllBytes(_appendPath));
    if (narc.FileNames.Count > 0) { narc.FileNames.Add(Path.GetFileName(_appendPath).ToCharArray()); }
    return narc.Save(_outputPath) ? 0 : -1;
  }
}
