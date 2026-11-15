Role: test implementer
Commit: (this commit, on 1c480b1; `src` production unchanged since the head d283edd named in the prompt — 1c480b1 is the spec author's parallel `sdd/` commit)
Verdict: tests written

Narrowing round, for decision 26 / auditor finding #8 (`00-lead.md`, `27-auditor.md`). The prompt carried no intended fix, suspected cause, or hoped-for verdict.

All 3 tests are new and sit in `src/Reihitsu.Analyzer.Test/Design/RH2001PrivateAutoPropertiesShouldNotBeUsedAnalyzerTests.cs`, right before `#endregion // Tests`. Each is tagged `RH2001-011` only. No existing test changed. Inputs are the auditor's fixtures `scratchpad/fx27n/o1.cs`, `n2.cs`, `n1.cs` verbatim, with the property marked `{|RH2001:Value|}`.

"`main`" is 4838e9a (`origin/main`). I ran the new tests against `main`'s production code in a temporary worktree (since removed): `/tmp/claude-0/-home-user-Reihitsu/4271dfb1-87ee-51f4-9b4e-ea7505ebffbd/scratchpad/r28-main.txt` (3/3 passed). Head: `/tmp/claude-0/-home-user-Reihitsu/4271dfb1-87ee-51f4-9b4e-ea7505ebffbd/scratchpad/r28-head.txt` (3/3 passed). The head withholds none of them; the asserted outputs are `main`'s exact output and match the auditor's apply-fix output `fx27-head.txt` (LF and CRLF).

## Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `VerifyFixIsOfferedWhenThePropertyIsAnElementOfATupleConvertedToObject` (o1, `object pair = (Value, 1);`) | RH2001-011; FC-003, FC-017 | `Verify` | green on head; green on `main` (exact output `object pair = (_value, 1);`) |
| `VerifyFixIsOfferedWhenThePropertyIsAnElementOfATupleConvertedToANullableUnnamedTupleType` (n2, `(int, int)? pair = (Value, 1);`) | RH2001-011; FC-003, FC-017 | `Verify` | green on head; green on `main` (exact output `(int, int)? pair = (_value, 1);`) |
| `VerifyFixIsOfferedWhenThePropertyIsAnElementOfATupleConvertedToANullableNamedTupleType` (n1, `(int Value, int Other)? pair = (Value, 1);`, `return pair.Value.Value;`) | RH2001-011; FC-003, FC-017 | `Verify` | green on head; green on `main` (exact output `(int Value, int Other)? pair = (_value, 1);`) |

These are offered-side boundary tests: each passes on the unchanged code by design. The falsifying observation is the auditor's mutant M2 (return `true` for a converted named type that is not a tuple), which withheld o1, n1, and n2 (`27-auditor.md`); `Verify` fails when no action is registered, so each test falsifies M2. I did not rebuild M2 myself.

## Not covered, and why

- **CRLF variants.** Binding check, no layout; the auditor's apply-fix runs over these three fixtures gave identical LF and CRLF results.
- **Fix All.** The guard is per diagnostic; t1 already runs the batch path.
- **Other non-tuple targets (`dynamic`, `ValueType`, interface such as `IComparable`).** Same branch of `TupleTypeKeepsElementName` as `object` (the converted type is not a tuple type); the lead asked for these three.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~ATupleConvertedTo"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Design.RH2001PrivateAutoPropertiesShouldNotBeUsedAnalyzerTests"
```
