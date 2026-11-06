Role: implementer
Commit: 886f13c
Verdict: tests pass

## Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Core/OrderingDeclarationUtilities.cs` | `ChangesInitializerExecutionOrder` is `public` again, as on `main`. It already sat between the public `ChangesMemberOrder` overloads and the private members, so the member order stays public → private (the file has no `internal` member now). Its XML summary still holds: it ranks the members as `ChangesMemberOrder` does without the layout check, and code fixes call `ChangesMemberOrder`. `FieldOrderSetsLayout`, `HasInitializer`, and `IsLayoutField` stay private. | Decision 17 |
| `src/Reihitsu.Core/Reihitsu.Core.csproj` | Removed the `<InternalsVisibleTo Include="Reihitsu.Core.Test" />` item group; the file is identical to `main`. | Decision 17 |

No test changed.

## Policy owners
Policy owners are unchanged: only the visibility changed. `rg ChangesInitializerExecutionOrder src --glob '*.cs'` finds the declaration and the `Reihitsu.Core.Test` callers, plus test names in `Reihitsu.Analyzer.Test` that do not call the method. No production code calls it.

## Tests run
- `scripts/format.sh src/Reihitsu.Core/OrderingDeclarationUtilities.cs`: Formatted 0 of 1 file(s).
- `scripts/build.sh --configuration Debug`: Build succeeded. 0 Warning(s) 0 Error(s)
- `scripts/test.sh --configuration Debug --project core`: Passed!  - Failed:     0, Passed:   607, Skipped:     0, Total:   607 - Reihitsu.Core.Test.dll (net10.0)
- `scripts/test.sh --configuration Debug --project analyzer`: Passed!  - Failed:     0, Passed:  6635, Skipped:     0, Total:  6635 - Reihitsu.Analyzer.Test.dll (net10.0)
- `scripts/test.sh --project selfhosting` (run on 886f13c with this directory still present):

      Failed!  - Failed:     1, Passed:  3835, Skipped:     0, Total:  3836, Duration: 15 m 5 s - Reihitsu.SelfHosting.Test.dll (net10.0)

  The CS0122 compilation error is gone, and so are the 340 failures it caused. The one failing test is `WorkingDocumentsDirectoryContainsNoFiles`: "sdd/work/ must be deleted before a pull request is ready, but contains: withhold-unsafe-fixes/00-lead.md". That is this CI repair's own working directory, which has to be deleted before the push.

## Disputed tests or scope
_None._ The prompt carried no intended fix beyond decision 17.

## Architecture findings
_None._

## Follow-up candidates
_None._
