Role: cleaner (repair round 1)
Verdict: clean

## Finding 3
Removed the redundant `using System.Linq;` from `src/Reihitsu.Formatter/Pipeline/UsingDirectives/Utilities/UsingLeadingTriviaBuilder.cs`. `src/Reihitsu.Formatter/Properties/GlobalUsings.cs` declares `global using System.Linq;`. The file is now unchanged against `main`.

## Format and build
- `scripts/format.sh` over the four production files the run changed: 0 of 4 reformatted.
- `scripts/build.sh` (Release): the summary reports 0 warnings and 0 errors. The raw output once listed three S1125 lines in `src/Reihitsu.Cli/Diagnostics/PipelineTraceCommand.cs` (473, 478, 522); that file is unchanged against `main`, so they are not from this run.
