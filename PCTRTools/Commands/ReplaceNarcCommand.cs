using Mono.Options;
using System;
using System.Collections.Generic;
using System.IO;

namespace PCTRTools
{
  public class ReplaceNarcCommand : Command
  {
    private string _inputDirectory, _newFilesDirectory, _outputDirectory;
    private bool _showHelp;

    public ReplaceNarcCommand() : base("replace-narc", "Replace narc files")
    {
      Options = new OptionSet()
      {
        "Replace narc files in a directory.",
        "Usage: PCTRTools replace-narc -i [inputDir] -c [newFilesDir] -o [outputDir]",
        "",
        { "i|input-dir=", "Input directory", i => _inputDirectory = i },
        { "n|new-files-dir=", "New files directory", n => _newFilesDirectory = n },
        { "o|output-dir=", "Output directory", o => _outputDirectory = o},
        { "h|help", "Shows this help screen", h => _showHelp = true },
      };
    }

    public override int Invoke(IEnumerable<string> arguments)
    {
      Options.Parse(arguments);

      if (_showHelp || string.IsNullOrEmpty(_inputDirectory) || string.IsNullOrEmpty(_outputDirectory))
      {
        int returnValue = 0;
        if (!_showHelp)
        {
          if (string.IsNullOrEmpty(_inputDirectory))
          {
            CommandSet.Error.WriteLine("Input directory not provided, please supply -i or --input-narc");
            returnValue = 1;
          }
          if (string.IsNullOrEmpty(_newFilesDirectory))
          {
            CommandSet.Error.WriteLine("New files directory not provided, please supply -n or --new-files");
            returnValue = 1;
          }
          if (string.IsNullOrEmpty(_outputDirectory))
          {
            CommandSet.Error.WriteLine("Output directory not provided, please supply -o or --output-narc");
            returnValue = 1;
          }
        }
        Options.WriteOptionDescriptions(CommandSet.Out);
        return returnValue;
      }

      var inputDirInfo = new DirectoryInfo(_inputDirectory);
      var newFilesDirInfo = new DirectoryInfo(_newFilesDirectory);
      var outputDirInfo = new DirectoryInfo(_outputDirectory);
      foreach (string path in Directory.EnumerateFiles(inputDirInfo.FullName, "*", SearchOption.AllDirectories))
      {
        var pathInDiffDir = path.Replace(inputDirInfo.FullName, newFilesDirInfo.FullName);
        if (!Directory.Exists(pathInDiffDir)) { continue; }
        NARC narc;
        try
        {
          narc = new NARC(path);
        }
        catch (Exception ex)
        {
          Console.WriteLine($"Could not open narc file: {path}");
          Console.WriteLine(ex.Message);
          continue;
        }
        for (int i = 0; i < narc.Files.Count; i++)
        {
          var binPath = Path.Combine(pathInDiffDir, $"{i:d04}.bin");
          if (!File.Exists(binPath)) { continue; }
          var newData = File.ReadAllBytes(binPath);
          if (narc.Files[i][0] == 0x10)
          {
            var size = narc.Files[i][1] + (narc.Files[i][2] << 8) + (narc.Files[i][3] << 16);
            var newFileSize = new FileInfo(binPath).Length;
            if (size == newFileSize)
            {
              // Compressed
              newData = CompressHelper.CompressLzss(newData);
            }
            else if (narc.Files[i].Length != newFileSize)
            {
              Console.WriteLine($"Warning: size mismatch: {binPath}");
            }
          }
          narc.Files[i] = newData;
        }
        var newPath = path.Replace(inputDirInfo.FullName, outputDirInfo.FullName);
        Directory.CreateDirectory(Path.GetDirectoryName(newPath));
        narc.Save(newPath);
        Console.WriteLine($"Saved: {newPath}");
      }

      return 0;
    }
  }
}
