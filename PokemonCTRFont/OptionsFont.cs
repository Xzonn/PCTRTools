using CommandLine;

namespace PokemonCTR
{
    class Options
    {
        [Option('c', "chartable", HelpText = "Character table file path.", MetaValue = "FILE", Required = true)]
        public string ChartablePath { get; set; }

        [Option('f', "font", HelpText = "Font file path.", MetaValue = "FILE", Required = true)]
        public string FontPath { get; set; }

        [Option('o', "output", HelpText = "Output file path.", MetaValue = "FILE", Required = true)]
        public string OutputPath { get; set; }
    }
}
