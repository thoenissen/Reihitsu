Role: implementer
Commit: (this commit; see `git log -1 -- sdd/work/casing-fix-names/18-implementer.md`)
Verdict: tests pass

Structural repair for auditor `16-auditor.md` finding 3 and its architecture finding, built to lead decision 4, against the tests of `17-test-implementer.md`. The prompt carried no intended fix, suspected cause, or hoped-for verdict beyond pointing at decision 4.

## Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer/Rules/Naming/RH4116TupleElementCasingAnalyzer.cs` | `IsDictatedName` no longer selects the signature by a list of member syntax kinds (`GetSignatureTypes`, `GetParameters`, and the `member switch` with `_ => []` are deleted). It decides by cause: `GetOutermostType` climbs from the tuple type through the type it stands in (type arguments, element types, tuple elements, function pointer parameters) to the outermost type; `GetDeclaredMembers` asks the compiler for the symbol declared by the declaration that holds that type — for a parameter, the member it belongs to (`IParameterSymbol.ContainingSymbol`) and, for a constructor parameter, the property the compiler declares from that same parameter syntax (a record's positional property, matched by its declaring syntax reference); for a variable declaration, each declared event, field, or local; for a member declaration, the member whose type or return type it is. Each is judged by the unchanged `CasingAnalyzerBase.IsDictatedMember`. Summaries and remarks describe the new code. | SHARED-CASING-014 bullet 2, RH4116-001 (both unchanged) |

## Policy owners
- Dictated member: `rg -n "IsDictatedMember" src --glob '*.cs'` — the one owner is `CasingAnalyzerBase.IsDictatedMember`; RH4116 only names the declared symbols to ask, as before.
- Which declared symbol a record positional parameter stands for: `rg -n "DeclaringSyntaxReferences|MethodKind.Constructor" src --glob '*.cs'` — no other owner; the mapping is now private to RH4116 (`GetDeclaredMembers`). The RH4120 follow-up in `00-lead.md` needs the same mapping on the parameter's name; when it is taken, that mapping is the piece to lift into `CasingAnalyzerBase` (not done here: no second consumer in this run).

## Production diff size
`git diff --shortstat origin/main...HEAD -- src` before this commit: 12 files changed, 543 insertions(+), 74 deletions(-). Round delta: 1 file changed, 69 insertions(+), 45 deletions(-). No new type; two private methods replaced by two private methods (`GetOutermostType`, `GetDeclaredMembers`).

## Deleted special cases
_N/A._ (The syntax-kind list is replaced by the cause-based decision; no shape is declared unsupported.)

## Tests run
Before the change (file content from `HEAD` copied in, no stash): `RH4116TupleElementCasingAnalyzerTests` Failed 6 / Passed 18 — exactly the six the handover names.

After:
- `scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Naming.RH4116TupleElementCasingAnalyzerTests"` — Passed 24, Failed 0
- `scripts/test.sh --configuration Debug` (default set): Analyzer 11994, Formatter 7130, Core 884, Cli 440 (+1 skipped), Architecture 198, Tooling 204, Playground 23 — all Failed 0.

`scripts/format.sh` over the file: 0 of 1 changed. `scripts/build.sh --configuration Debug`: 0 warnings, 0 errors.

## Disputed tests or scope
_None._ Notes:
- The variable-declaration branch now asks the declared symbol of every variable declaration that holds a named tuple type (fields and locals included), not only event fields; `IsDictatedMember` returns false for fields and locals, so reporting there is unchanged, at the cost of one `GetDeclaredSymbol` per declarator for such declarations.
- Tuple names in an explicit interface specifier, a base list, a constraint, or an expression are not in a declared type and stay reported, as before (the auditor's hint on explicit interface specifiers is unchanged).
- I did not touch the `SplitIntoPascalCaseWords` comment (decision 5, cleaner).

## Architecture findings
_None._ This round resolves the one in `16-auditor.md` by decision 4.

## Follow-up candidates
_None_ beyond the RH4120 candidate in `00-lead.md`.
