using Mono.Options;
using PCTRTools.Commands;

CommandSet commands = new("PCTRTools")
{
  "PCTRTools 4.1.0 (c) Xzonn 2018-",
  "",
  "Usage: PCTRTools COMMAND [OPTIONS]",
  "",
  "Available commands:",
  new FontCommand(),
  new TextImportCommand(),
  new TextExportCommand(),
  new ReplaceNarcCommand(),
  new AppendNarcCommand(),
};

return commands.Run(args);
