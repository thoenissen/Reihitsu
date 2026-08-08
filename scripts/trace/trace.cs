#:project ../../Reihitsu.Cli/Reihitsu.Cli.csproj
#:property TargetFramework=net10.0
#:property AssemblyName=Reihitsu.Trace
#:property PublishAot=false
#:property PackAsTool=false

using Reihitsu.Cli.Diagnostics;

var exitCode = await PipelineTraceCommand.ExecuteAsync(args, Console.Out, Console.Error);

// dotnet run also uses process exit code 1 for build and launch failures. Use a private
// transport code so the wrappers can distinguish trace exhaustion from tooling failure.
return exitCode == Reihitsu.Cli.ExitCodes.FormattingNeeded ? 11 : exitCode;
