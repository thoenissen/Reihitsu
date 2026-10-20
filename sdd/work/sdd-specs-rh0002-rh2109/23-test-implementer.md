Role: test implementer
Commit: 3fbc1e2
Verdict: tests written

Repair round after `22-spec-author.md`, which bounds SHARED-EQUAL-002 and RH1001-003. I implemented its worklist of three items, with the inputs exactly as given there. I also handled hint 2 of `21-auditor.md`. I followed the conventions of `16-test-implementer.md` and `20-test-implementer.md`:
- numbered `{|#n:…|}` markup with the `Diagnostics(…, n)` helper, so the message is checked too;
- characterization tests carry no `[Requirement]` tag;
- a characterization test's doc comment starts with "Characterizes …" and says that the case is an open question.

No production code, spec, or rule page was touched. Every new test passes on the unchanged code, and no test contradicted a spec, so there is no finding against a spec. My prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Tests

All new tests are in `src/Reihitsu.Analyzer.Test/Performance/`, at the end of their classes.

| Test (class prefix · method) | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| RH1002 · VerifyConstantWithValueNullIsNotCustomComparer | SHARED-EQUAL-002. FC-003: the not-custom side is a `const` field, a `const` local, and `flag ? null : NoComparer`; the custom side is a non-`const` local holding `null` | `Verify` + `Diagnostics(…, 3)` | pass |
| RH1001 · VerifyCreationWithConstantNullComparerIsNotExempt | RH1001-003, SHARED-EQUAL-002. FC-003: the reported side is `new Dictionary<S, int>(NoComparer)` and `new(none)` with a `const` local; the exempt side is `new(notConst)` | `Verify` + `Diagnostics(…, 2)` | pass |
| RH1002 · VerifyDefaultOfTypeParameterNotConstrainedToValueTypeCountsAsNoComparer | Char. struct-equality Q6 (RH1002 input) | `Verify` + `Diagnostics(…, 3)` | pass: the `default(TC)`, `default(TN)`, and `default(TR)` calls are reported, the `default(TV)` call is not |
| RH1001 · VerifyCreationWithDefaultOfTypeParameterNotConstrainedToValueTypeIsReported | Char. struct-equality Q6 (RH1001 input) | `Verify` | pass: only the `S` of `map` is reported |

How the inputs match their sources:
- I checked this with a Python script in the scratchpad, run with `-I`.
- The script extracted the three code blocks of `22-spec-author.md` ("Examples for the tests" items 1 and 2, and the Q6 issue text) and each test's raw string. It removed the indentation, the `> ` quote prefix, and the markup, then compared the texts.
- All four are identical.
- For the two tagged tests, the script also replaced each `{|RH100x:` and `{|#n:` with one placeholder and compared again. The markup sits at the same positions: 3 markups in RH1002 and 2 in RH1001.
- The Q6 issue text has no markup. The markup follows item 3 of the worklist:
  - RH1002: `{|#n:Distinct|}` on the three calls `default(TC)`, `default(TN)`, and `default(TR)`;
  - RH1001: the `S` of `Dictionary<S, int> map`.
- As item 3 says, both Q6 tests use the same full input, `using System.Linq;` and the `M` method included. RH1001 reports nothing in `M`, and RH1002 reports nothing in `N`.

Each new test can fail. I mutated each test's expectation temporarily, in one batch, and then restored both files from backups. The final diff contains no mutation (checked with `git diff` and `grep`). Results of the focused run on the mutated tree: "Failed: 4, Passed: 74, Total: 78". Each test failed for the expected reason:

| Test | Mutation | Failure |
|---|---|---|
| VerifyConstantWithValueNullIsNotCustomComparer | expects a report on `items.Distinct(notConst)` | expected "4" actual "3" |
| VerifyCreationWithConstantNullComparerIsNotExempt | expects a report on the `S` of `c` | expected "3" actual "2" |
| VerifyDefaultOfTypeParameterNotConstrainedToValueTypeCountsAsNoComparer | expects a report on `items.Distinct(default(TV))` | expected "4" actual "3" |
| VerifyCreationWithDefaultOfTypeParameterNotConstrainedToValueTypeIsReported | expects a report on the `S` of `other` | expected "2" actual "1" |

### Hint 2 of `21-auditor.md`: the summary of `RH1002 · VerifyOnlyArgumentsThatCanProduceAComparerCountAsCustom`

The summary misstated what SHARED-EQUAL-002 now says, so I corrected the summary text. I changed nothing else: the name, the tags, the input, and the assertion are unchanged.
- **Old text.** The summary said that "expressions that can only produce" `null` or `Default` count as not custom "through parentheses, `!`, casts, …".
- **Why it was wrong.** SHARED-EQUAL-002 now decides by form and names its forms in two bounded lists.
  - Under those lists, a non-`const` local holding `null` is custom, although it can only produce `null`.
  - The Q5 sentence leaves out a whole argument that wraps a composite form, such as `(flag ? null : EqualityComparer<S>.Default)`.
  - So the old general claim is not what the requirement says.
- **New text.** The summary now says that the argument's form decides. It names only the forms the test contains:
  - Custom: an object creation; a variable passed as a named argument or a non-trailing named argument; a conditional or `switch` with a variable branch or arm; `default` of a non-nullable value-type comparer.
  - Not custom: `null`; `default`; `EqualityComparer<T>.Default`; a cast of `null` inside parentheses behind `!`; a conditional or `switch` whose branches or arms all have such a form; a constant condition whose chosen branch has one; a `??` of two such forms; a null-conditional access on a cast of `null`; `default` of a nullable value-type comparer.
- **Name.** The method name still uses the old "can produce a comparer" framing. Renaming it is outside a summary-only change, so I left it for the lead.

## Not covered, and why

- **The further `const` forms in `22-spec-author.md` finding 1**, such as `Other.Nothing`, `NoComparer!`, `NoComparer as …`, and `NullHolder?.Comparer`. The worklist gives the two tagged inputs. The other forms reach the same constant-value check, and the spec author checked them in a scratch harness. The tagged tests cover the first list item (`const` field, `const` local) and its conditional, on both sides. I added no inputs beyond the worklist.
- **`switch` without arms.** Following `22-spec-author.md`, it gets no question and no test. Both requirements leave it unspecified through their coverage sentences.
- **FC-010 to FC-014, FC-019, FC-007/FC-008, FC-033 (line endings).** No code fix and no formatter phase is involved. RH1001 and RH1002 offer no fix (SHARED-EQUAL non-goal), and the diagnostics are located by markup, not by layout.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Performance.RH1001|FullyQualifiedName~Reihitsu.Analyzer.Test.Performance.RH1002"
scripts/test.sh --project architecture
```

Results on this tree:

| Command | Result |
|---|---|
| Focused RH1001/RH1002 run | "Passed! - Failed: 0, Passed: 78, Skipped: 0, Total: 78" |
| `scripts/test.sh --project architecture` | "Passed! - Failed: 0, Passed: 72, Skipped: 0, Total: 72" (includes `RequirementCoverageTests`) |
| `scripts/format.sh` on the two test files | "Formatted 0 of 2 file(s)." |
| `scripts/build.sh` | "Build succeeded. 0 Warning(s) 0 Error(s)" |
| `scripts/build.sh --no-incremental` | "Build succeeded. 0 Warning(s) 0 Error(s)" |

Note on warnings: the build that `scripts/format.sh` runs for its own CLI printed `S1125` warnings in `src/Reihitsu.Cli/FormatCommandHandler.cs` (lines 430, 441, 452). That is not part of this change. The Release build of the solution shows no warnings.
