Role: implementer
Commit: d629d44
Verdict: #353: tests pass; #354: tests pass

The prompt carried no intended fix, suspected cause, or hoped-for verdict. Every handed-over test failed as 06-test-implementer.md states (the analyzer test project did not compile on 6aa416c: the two analyzer types were missing) and passes on d629d44 without any test change.

## thoenissen/Reihitsu#353

### Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer/Base/LineCountAnalyzerBase.cs` (new) | Shared owner (lead decision 3): `CountLinesOfCode` (lines on which a character of a non-empty token inside the span lies, each line once, a multi-line token counts every line it covers, trivia never), `IsTestCode` (`build_property.IsTestProject` or `build_property.IsTestingPlatformApplication` equal to `true`, `OrdinalIgnoreCase`), `ResolveLimit` (the kind of code's setting when valid, else that setting's default), compilation-start registration, and `count > limit` comparison with the message arguments count and limit; helpers `TryGetBodySpan` (block: between the braces; expression body: the `=>` clause, `;` excluded) and `GetSpanBetweenBraces` | SHARED-LINES-001, -002, -003 |
| `src/Reihitsu.Analyzer/Rules/Design/RH2008MethodsShouldNotExceedTheLineLimitAnalyzer.cs` (new) | Judges methods, constructors, finalizers, operators, conversion operators, local functions, `get`/`set`/`init`/`add`/`remove` accessors with a body, and properties/indexers with an expression body; location on identifier / `this` / operator token / `operator` keyword / accessor keyword; defaults 60 / 100 | RH2008-001 to RH2008-005 |
| `src/Reihitsu.Analyzer/Data/ConfigurationCategoryDesign.cs` (new), `Configuration.cs` | `Design` section with four nullable limits | RH0001-007 |
| `src/Reihitsu.Analyzer/Data/ConfigurationManager.cs` | `ParseDesign`/`ParseLineLimit`/`IsWrittenAsDigits`: the `design` section must be an object; the four settings accept a JSON number of ASCII digits only that fits `int` and is at least 1, other values are reported on the value and keep the setting `null`; other settings reported as unknown on the property name | RH0001-004, RH0001-007; RH8402-001 (a valid `design` section no longer produces an error, so RH8402 is no longer silenced) |
| `src/Reihitsu.Analyzer/AnalyzerResources.resx`, `AnalyzerResources.cs` | `RH2008Title`, `RH2008MessageFormat` ("This member has {0} lines of code, which exceeds the limit of {1}.") | RH2008 |
| `Reihitsu.sln` | `documentation/rules/RH2008.md` solution item | — |

The package README row and the rule page already existed (spec author).

### Policy owners

- `rg -n "IsTestProject|IsTestingPlatformApplication|build_property" src --glob '*.cs'` — production owner only `LineCountAnalyzerBase`; the rest are tests.
- `rg -l "GetLineFromPosition\(.*LineNumber" src/Reihitsu.Analyzer src/Reihitsu.Core` — layout and comment rules computing single lines (RH5104, RH3201, RH6004, RH8305, Core layout utilities); none counts lines of code of a span, so no private copy of the counting exists.
- Limit loading: `ConfigurationManager.GetConfiguration` stays the one loader (RH4009, RH8402, RH0001 use it too).

### Production diff size

`git diff --shortstat origin/main...HEAD -- src ':(exclude,glob)src/*Test*/**'`: 10 files changed, 690 insertions(+) (both issues together). New types: `LineCountAnalyzerBase` (268 lines with docs), `ConfigurationCategoryDesign`, and the two analyzers.

### Deleted special cases

_N/A._

### Tests run

- `scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~RH2008MethodsShouldNotExceedTheLineLimitAnalyzerTests|FullyQualifiedName~RH2111TypesShouldNotExceedTheLineLimitAnalyzerTests|FullyQualifiedName~RH0001ConfigurationFileMustBeValidAnalyzerTests|FullyQualifiedName~RH8402FileMustStartWithConfiguredXmlStyleCopyrightHeaderAnalyzerTests|FullyQualifiedName~RuleDocumentationExampleTests"` — Passed! Failed: 0, Passed: 326.
- `scripts/test.sh --configuration Debug --project analyzer` — Passed! Failed: 0, Passed: 10858.

### Disputed tests or scope

_None._ (See *Whole run* for the ruleset entries.)

### Architecture findings

_None._

### Follow-up candidates

_None_ new.

## thoenissen/Reihitsu#354

### Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer/Rules/Design/RH2111TypesShouldNotExceedTheLineLimitAnalyzer.cs` (new) | Judges class, struct, interface, record, record struct, and union declarations whose `OpenBraceToken` is a brace (a `;` declaration is not judged), counting between the braces through `LineCountAnalyzerBase`; location on the identifier; defaults 500 / 1000. Each part of a partial type is its own syntax node and is judged alone; generated files are skipped by the base `GeneratedCodeAnalysisFlags.None` | RH2111-001 to RH2111-006 |
| `AnalyzerResources.resx`/`.cs` | `RH2111Title`, `RH2111MessageFormat` ("This type has {0} lines of code, which exceeds the limit of {1}.") | RH2111 |
| `Reihitsu.sln` | `documentation/rules/RH2111.md` solution item | — |

Counting, test-code recognition, limit resolution, and configuration: shared with #353 (above).

### Policy owners

As under #353.

### Production diff size

Included in the figure under #353.

### Deleted special cases

_N/A._

### Tests run

Included in the two runs under #353 (the RH2111 class is in the focused filter and in the project run).

### Disputed tests or scope

_None._

### Architecture findings

_None._

### Follow-up candidates

_None._

## Whole run

### Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer.Package/buildTransitive/Reihitsu.Analyzer.targets` | `CompilerVisibleProperty` for `IsTestProject` and `IsTestingPlatformApplication` | SHARED-LINES-003 (second sentence) |
| `Release.ruleset` | `RH2008` and `RH2111` with `Action="None"` at their sorted places | maintainer answer 11A |
| `Debug.ruleset` | `RH2008` and `RH2111` with `Action="None"` in the `Reihitsu.Analyzer` rules group | maintainer answer 11A |
| `src/Reihitsu.SelfHosting.Test/Analyzers/AnalyzerSelfHostingTests.cs` | `_analyzersLeftOut` (RH2008, RH2111) filtered out of `AnalyzerNames`, with the reason in its summary; `AnalyzerCatalog` unchanged, so `CreateAnalyzer` still finds every type | maintainer answer 11A |
| `sdd/specs/shared/line-count.md`, `sdd/specs/README.md` | Code owner `LineCountAnalyzerBase` | lead decision 3 |

The ruleset entries are the maintainer's 11A answer the lead routed to this role, not a warning suppression; implementer.md forbids ruleset changes for warnings, so the lead may want to note that in the run's note.

### Tests run

- `scripts/test.sh --configuration Debug --project selfhosting --filter "FullyQualifiedName~AnalyzerViolationReportTests"` — Passed! Failed: 0, Passed: 7.
- `scripts/test.sh --configuration Debug --project architecture` — Passed! Failed: 0, Passed: 198 (spec header, code owner, and requirement-coverage tests included).
- Not run: the full self-hosting scan (outside the default run).

### Build

`scripts/build.sh --configuration Debug`: 0 errors. Warnings left for the cleaner, all in new files: RH7103 and RH7110 (member order in `LineCountAnalyzerBase`), RH8201 (`TryGetCountedSpan` overrides carry their own summary instead of `<inheritdoc/>`), and RH5603 (final newline), which `scripts/format.sh` resolved before the commit (no new file ends with a newline now).

### Disputed tests or scope

_None._

### Follow-up candidates

_None_ new. The candidates already in `00-lead.md` stand (this repository's own `CompilerVisibleProperty`/limits when the rules are turned on here; duplicate keys in `design`).
