﻿using Mono.Options;
using PCTRTools.Commands;

CommandSet commands = new("PCTRTools")
{
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
