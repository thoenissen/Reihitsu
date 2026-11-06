Role: implementer
Commit: 6641079
Verdict: tests pass

## Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Core/OrderingDeclarationUtilities.cs` | `ChangesInitializerExecutionOrder` is `internal` again, as at 036b3b4. It stays between the public `ChangesMemberOrder` overloads and the private members, so the member order is public → internal → private. Its XML summary still holds: it does not describe accessibility, and it names the overload a code fix calls. `FieldOrderSetsLayout`, `HasInitializer`, and `IsLayoutField` stay private. | Decision 19 |
| `src/Reihitsu.Core/Properties/AssemblyInfo.cs` (new) | `[assembly: InternalsVisibleTo("Reihitsu.Core.Test")]`, in the same form as `src/Reihitsu.Formatter/Properties/AssemblyInfo.cs` and `src/Reihitsu.Cli/Properties/AssemblyInfo.cs`: UTF-8 BOM, `using System.Runtime.CompilerServices;`, a blank line, the attribute, LF line endings, no final newline, no file header. The self-hosting compilation builds each project from its `*.cs` files, so it sees this attribute. A csproj `<InternalsVisibleTo>` item would be invisible to it. | Decision 19 |

`src/Reihitsu.Core/Reihitsu.Core.csproj` stays identical to `main`. No test changed.

## Policy owners
No policy changed; only one method's accessibility changed. `rg ChangesInitializerExecutionOrder src --glob '*.cs'` finds the declaration, the four calls in `src/Reihitsu.Core.Test/OrderingDeclarationUtilitiesTests.cs` (lines 540, 569, 598, 625), and three analyzer-test method names that do not call it. No production code calls it, and no assembly other than `Reihitsu.Core.Test` can reach it.

## Tests run
- `scripts/format.sh src/Reihitsu.Core/OrderingDeclarationUtilities.cs src/Reihitsu.Core/Properties/AssemblyInfo.cs`: Formatted 0 of 2 file(s).
- `scripts/build.sh --configuration Debug`: Build succeeded. 0 Warning(s) 0 Error(s)
- `scripts/test.sh --configuration Debug --project core`: Passed!  - Failed:     0, Passed:   607, Skipped:     0, Total:   607, Duration: 528 ms - Reihitsu.Core.Test.dll (net10.0)
- `scripts/test.sh --configuration Debug --project analyzer`: Passed!  - Failed:     0, Passed:  6635, Skipped:     0, Total:  6635, Duration: 1 m 59 s - Reihitsu.Analyzer.Test.dll (net10.0)
- `scripts/test.sh --project selfhosting` (on 6641079, with this directory still present):

      Failed!  - Failed:     1, Passed:  3837, Skipped:     0, Total:  3838, Duration: 15 m 3 s - Reihitsu.SelfHosting.Test.dll (net10.0)

  No CS0122 in the log. The one failing test is `WorkingDocumentsDirectoryContainsNoFiles`: "sdd/work/ must be deleted before a pull request is ready, but contains: withhold-unsafe-fixes/00-lead.md, withhold-unsafe-fixes/01-implementer.md, withhold-unsafe-fixes/03-auditor.md, withhold-unsafe-fixes/04-implementer.md". That is this run's own working directory. The total is two more than the 3836 at 886f13c. The new source file is a likely cause, because the scans seem to run once per file, but I did not check this.

## Disputed tests or scope
_None._ The prompt carried no intended fix beyond decision 19.

## Architecture findings
_None._

## Follow-up candidates
_None._
