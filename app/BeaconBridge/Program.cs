using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using BeaconBridge.Commands.Helpers;
using BeaconBridge.Startup.Cli;
using BeaconBridge.Startup.EfCoreMigrations;
using BeaconBridge.Startup.Web;

// Enable EF Core tooling to get a DbContext configuration
EfCoreMigrations.BootstrapDbContext(args);

await new CommandLineBuilder(new CliEntrypoint())
  .UseDefaults()
  .UseRootCommandBypass(args, WebEntrypoint.Run)
  .UseCliHostDefaults(args)
  .Build()
  .InvokeAsync(args);
