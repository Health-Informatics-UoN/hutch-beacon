using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using BeaconBridge.Commands.Helpers;
using BeaconBridge.Startup.Cli;
using BeaconBridge.Startup.Web;

await new CommandLineBuilder(new CliEntrypoint())
  .UseDefaults()
  .UseRootCommandBypass(args, WebEntrypoint.Run)
  .UseCliHostDefaults(args)
  .Build()
  .InvokeAsync(args);
