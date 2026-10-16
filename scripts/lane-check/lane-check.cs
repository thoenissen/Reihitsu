#:project ../../src/Reihitsu.Tooling/Reihitsu.Tooling.csproj
#:property TargetFramework=net10.0
#:property AssemblyName=Reihitsu.LaneCheck
#:property PublishAot=false
#:property PackAsTool=false

using Reihitsu.Tooling;

var exitCode = LaneCheckCommand.Execute(args, Console.In, Console.Out, Console.Error);

// dotnet run also uses process exit code 1 for build and launch failures. Use private transport codes so the
// wrappers can distinguish a lane mismatch and a diff no lane admits from a tooling failure.
return exitCode switch
       {
           ExitCodes.LaneMismatch => 11,
           ExitCodes.NoLane => 13,
           _ => exitCode
       };
