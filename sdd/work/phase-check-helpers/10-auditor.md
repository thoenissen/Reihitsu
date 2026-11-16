Role: auditor
Commit: e6e041f
Verdict: #1505: pass; #1577: pass; #1576: pass

My prompt carried no intended fix, suspected cause, or hoped-for verdict. I worked from `00-lead.md`, the first audit `06-auditor.md`, handovers 07 to 09, and `git diff 1b8a1bd e6e041f`.

State verified: the checkout is `e6e041fc18b3cdd1badc0cccc15dc74bc251e34e`; `git ls-remote origin refs/heads/main` is `372daeee7a757fe7a3026896a594ece4df03fa8a`, which is an ancestor of the head. The repair diff matches handovers 07 and 08: five test-project files, all in `93fc307`. Later commits change no file under `src/` or `scripts/`. `scripts/lane-check.sh tooling` → `LANE CHECK: confirmed — claimed tooling, diff needs tooling`. The repair did not change scope: it touches only files the first audit covered, plus one new test method in `ReihitsuFormatterEntryPointTests` (decision 10), and changes no spec. So this is a delta audit.

Status of the previous findings: #1 closed, #2 closed, #3 closed, #4 closed, #5 closed. The audit found no new finding, so the next number stays `#6`.

## thoenissen/Reihitsu#1505

### Scope
Repair delta against 1b8a1bd, head e6e041f, base 372daee.

### Findings
_N/A — no finding._

| Previous | Status | Evidence |
|---|---|---|
| #2 | closed | `Helpers/SecondPassInput.cs:9-16` no longer makes a universal claim. It names four users: the passes of `FormatterTestsBase`, `ObservedPipeline`, the phases the check runs alone, and the check's own fixed-point assertion. `rg "SecondPassInput\.Create" src` finds exactly those four: `FormatterTestsBase.cs:338`, `ObservedPipeline.cs:61`, `PhaseStabilityCheck.cs:224`, and `PhaseStabilityCheckTests.cs:260`. The test implementer rejected my earlier wording ("the shared helpers under `Helpers/`") because `FormatterPhaseTestsBase.ApplyPhase` contradicts it. That reason is correct. |

### Guard scope
_N/A — the repair changes no predicate for this issue (comment only)._

### Policy ownership
_N/A — unchanged since 06._

### Assertion adequacy
_N/A — no test was added or changed for this issue._

### Failure classes dismissed
See *Whole run*.

### Architecture findings
_None._

### Hints
_None._

### Follow-up candidates
_None._

## thoenissen/Reihitsu#1577

### Scope
Repair delta against 1b8a1bd.

### Findings
_N/A — no finding._

| Previous | Status | Evidence |
|---|---|---|
| #1 | closed | `PhaseStabilityAssertionTests.AssertionPassesOnTheOutputTreeOfTheFormatter` now uses `BranchesMisindentedSource` (`#if FEATURE` class `C` / `#else` class `D`). It asserts that the formatter output leaves `D`'s member at 8 spaces, that the tree overload passes on the output tree, and that the same text parsed without `FEATURE` fails with `phase returned "    int _other;`. Two mutations now fail the test: `FormatSyntaxTree` dropping the options (`WithRootAndOptions(…, null)` makes `D` active, so `AssertNoPhaseChanges(output)` throws), and the tree overload ignoring `outputTree.Options` (the test implementer ran this mutation). The test now proves the summary's claim that the options are carried to the output. |
| #3 | closed | The class summary (`PhaseStabilityCheck.cs:21-24`) now says what the session-free methods do and no longer says who calls them. The `GetCurrentSession` message (`:274`) now says the session exists only inside the base and points to `AssertNoPhaseChanges`. `rg "runs only inside a test"` finds nothing, so no test asserted the old message. |

### Guard scope
_N/A — no predicate added or changed for this issue._

### Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| FMT-PIPE-027 failure text | `PhaseStabilityCheck.DescribeFailure` only. The new private `DescribeChangingPhases(string, string, CSharpParseOptions)` maps through it, and the public `DescribeChangingPhases(SyntaxTree)` and both `AssertNoPhaseChanges` overloads go through that overload. | one owner, ok |
| Joining the failure texts and failing once | `PhaseStabilityCheck.Verify:70-77` (on `main`, unchanged) and the new private `FailOnChangingPhases:258-264`, both `Assert.Fail(string.Join(Environment.NewLine, failures))` | Not a policy decision. It is a three-line body that `Verify` could call. Recorded as a hint, not a finding. |

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `PhaseStabilityAssertionTests.AssertionPassesOnTheOutputTreeOfTheFormatter` (reworked) | The tree overload passes on real formatter output, with the input's parse options carried to the output tree | Options dropped by the formatter or by the overload: class `D` becomes active and the indentation phase changes it, so the assertion throws. The test's last assertion shows that the fixture tells the two cases apart. | `AssertNoPhaseChanges(SyntaxTree)`, LF and CRLF | adequate |

`DescribeChangingPhases(SyntaxTree)` has no direct unit test. It is the body of `AssertNoPhaseChanges(SyntaxTree)`, whose four tests assert its texts. The self-hosting scan, which passed on 1810 rows (07), checks that it returns an empty list for formatted output. Adequate for a pass-through.

### Failure classes dismissed
See *Whole run*.

### Architecture findings
_None._

### Hints
- `PhaseStabilityCheck.Verify` (`:70-77`) repeats the body of the new `FailOnChangingPhases`. It could call that method, which would leave one place that joins failure texts. This is cosmetic and has no behavioral effect.
- `.editorconfig` sets `charset = utf-8-bom` for `*.cs`. The repair added the BOM to `PhaseStabilityAssertionTests.cs`, but `Unit/Pipeline/ObservedPipelineTests.cs`, which is also new in this run, has none. The repository does not enforce the setting (256 tracked `.cs` files lack a BOM), so this is not a finding.

### Follow-up candidates
_None_ beyond `00-lead.md`.

## thoenissen/Reihitsu#1576

### Scope
Repair delta against 1b8a1bd.

### Findings
_N/A — no finding._

| Previous | Status | Evidence |
|---|---|---|
| #4 | closed | `NoPhaseChangesTheFormattedOutputOfSourceFile` now skips exactly when `ReferenceEquals(output, syntaxTree)`. `ReihitsuFormatter.FormatSyntaxTree:96-115` returns the input instance for all four unchanged-return causes: syntax error, generated header, hiding carrier, and unsupported. A formatted input gets `WithRootAndOptions`. The generated-header gap is closed. |
| #5 | closed | The method collects `DescribeChangingPhases(output)` for both line endings and fails once with `A phase run alone changes the formatted output of {relativePath}:\n\n…`. This matches the shape of its siblings (`FormatterIsIdempotentOnSourceFile`, `FormatterProducesNoChangesOnSourceFile`). Each text names its line ending (`DescribeFailure`). |

### Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Which scanned outputs the per-phase check runs on | Identity of the output tree and the input tree | The four unchanged-return branches of `FormatSyntaxTree` | Exact for those branches. A formatted output is a new tree in every case I tried: a probe over all 1914 formattable `.cs` files under `src/` plus seven tiny inputs (empty file, comment only, `class C;`, file-scoped namespace, `#nullable`, two classes) found 0 same-instance outputs, also where the text is unchanged. The structural reason is that `CleanupPhase.Execute` runs `root.ReplaceTokens(root.DescendantTokens(), …)`. The skip also could not hide a failure if that ever changed: a same-instance output means every phase in turn returned its input unchanged, so every phase run alone on the output, under the same detected line ending and options (`SecondPassInput.Create`), changes nothing. | ok |

### Policy ownership
_N/A — unchanged since 06. Self-hosting still reaches the single owner `PhaseStabilityCheck` through the project reference._

### Assertion adequacy
| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `FormatterSelfHostingTests.NoPhaseChangesTheFormattedOutputOfSourceFile` (changed) | No production phase run alone changes any scanned file's formatted output under LF or CRLF; all failures are reported together | Any phase change: the row fails and names the file, phase, and line ending | `DescribeChangingPhases(SyntaxTree)` | adequate |
| `ReihitsuFormatterEntryPointTests.FormatSyntaxTreeReturnsANewTreeForAnInputItFormatsWithoutChangingIt` (new, untagged) | `FormatSyntaxTree` returns a new tree for an input it formats, even when the text is unchanged | `Assert.AreNotSame` fails if the input tree is returned | `FormatSyntaxTree`, LF and CRLF | adequate. It pins the documented return contract that the skip summary relies on. One input is enough because the property is structural (see *Guard scope*). The other direction, returning the same instance for unformatted input, is already pinned with `Assert.AreSame` by the FMT-PIPE-005/006 tests (07). |

### Failure classes dismissed
See *Whole run*.

### Architecture findings
_None._

### Hints
_None._

### Follow-up candidates
_None._

## Whole run

### Failure classes dismissed (delta)
- FC-001/FC-002: the only changed predicate is the self-hosting skip, which agrees with its counterpart (table under #1576).
- FC-015/FC-016: the failure text still has one owner. The duplicated join is recorded as a hint.
- FC-017: the reworked and new tests are adequate (tables above).
- FC-031: I checked every summary and message the repair changed against the code: `PhaseStabilityCheck` class and both `DescribeChangingPhases`, `FailOnChangingPhases`, `GetCurrentSession`'s message, `SecondPassInput`, the reworked test and the new test, and `NoPhaseChangesTheFormattedOutputOfSourceFile`. All describe the new code.
- FC-032: no compiled change after `93fc307`, where 07 ran the focused tests (134 passed) and the self-hosting method (1810 passed); 08's Release build reported 0 warnings.
- FC-033: the new test parses with the same options and line ending it formats with. Its negative case deliberately drops the options.
- All other classes: dismissed for the reasons in `06-auditor.md`, *Whole run*, which still hold. The repair touches no production code, rewrite, code fix, analyzer, or unsupported shape.

### Other checks
- Visible output: the reworked test expects a disabled `#else` branch to be left as written and the active member to be indented. Unchanged requirements demand both (disabled text is never touched; member indentation). Nothing is undecided.
- Cost of special cases: no production code.
- Comments: the repair's `src/` diff has no `TODO`, no commented-out code, and no tracker numbers.
- Scope: everything stays within decisions 9 and 10.
