using CommandLine;

namespace PokemonCTR
{
    class Options
    {
        [Option('c', "chartable", HelpText = "Character table file path.", MetaValue = "FILE", Required = true)]
        public string ChartablePath { get; set; }

        [Option('m', "msg", HelpText = "Message file path.", MetaValue = "FILE", Required = true)]
        public string MessagePath { get; set; }

        [Option('e', "extract", HelpText = "Extract texts into a text file.", MetaValue = "FILE")]
        public string ExtractPath { get; set; }

        [Option('i', "import", HelpText = "Import texts from a text file.", MetaValue = "FILE")]
        public string ImportPath { get; set; }

        [Option('o', "output", HelpText = "Output file path.", MetaValue = "FILE")]
        public string OutputPath { get; set; }
    }
}
