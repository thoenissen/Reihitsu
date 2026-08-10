#:project ../../Reihitsu.Tooling/Reihitsu.Tooling.csproj
#:property TargetFramework=net10.0
#:property AssemblyName=Reihitsu.ApplyFix
#:property PublishAot=false
#:property PackAsTool=false

using Reihitsu.Tooling;

var exitCode = await CodeFixRunCommand.ExecuteAsync(args, Console.Out, Console.Error);

// dotnet run also uses process exit code 1 for build and launch failures. Use a private
// transport code so the wrappers can distinguish a non-converging fixture from tooling failure.
return exitCode == ExitCodes.NotConverged ? 11 : exitCode;
