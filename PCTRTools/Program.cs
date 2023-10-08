using Mono.Options;

namespace PCTRTools
{
    internal class Program
    {
        static int Main(string[] args)
        {
            CommandSet commands = new CommandSet("PCTRTools")
        {
          "Usage: PCTRTools COMMAND [OPTIONS]",
          "",
          "Available commands:",
          new FontCommand(),
          new TextImportCommand(),
          new TextExportCommand(),
          new ReplaceNarcCommand(),
        };

            return commands.Run(args);
        }
    }
}
