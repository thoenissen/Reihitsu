# Task Completion Checklist

1. Run the repository formatter on changed C# files: `dotnet run --project Reihitsu.Cli -- <changed-path-1> [<changed-path-2> ...]`
2. Run the relevant MSTest projects for the touched runtime surfaces, including `Reihitsu.Core.Test` when `Reihitsu.Core` changes
3. Build the full solution in Release: `dotnet build Reihitsu.sln -c Release --verbosity minimal`
4. If adding/modifying a rule, update README.MD rule table
5. Ensure no auto-generated files were manually edited
