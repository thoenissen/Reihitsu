Role: spec author
Commit: d04e5d5
Verdict: #1507: no text needed; #1428: text written

The prompt carried no intended fix, suspected cause, or hoped-for verdict beyond the issues themselves.

## thoenissen/Reihitsu#1507

No spec, change doc, or documentation text is needed. The phase-ownership table in `AGENTS.md` is a repository convention, not a specified surface: no spec under `sdd/specs/` covers it, and `sdd/README.md` *Checks* lists only the SDD checks. The new test explains itself in its own documentation comment. Writing a sentence about it into `AGENTS.md` would widen the run to the tooling lane for no gain, so I left `AGENTS.md` alone.

### Premise check (at d04e5d5)

- Confirmed: `PhaseOwnershipDocumentationTests.OwnershipMapMatchesRuntimePhaseOrder` checks only that each phase appears once and in order. No architecture test reads the guard column.
- Already resolved: the stale names the issue cites (`CanRewrite`, `IsAfterSourceOnSameLine`) are no longer in the table. So the test guards against regressions; it repairs nothing today.
- The issue's goal does not hold as written. The guard column ("Primary policy/guards", the third column) holds 63 distinct backticked spans. I resolved each one with `rg` against production projects under `src/` (test projects excluded):
  - 58 dotted or plain identifiers are declared there. A qualified `Type.Member` has the member declared inside that type.
  - `EndOfLineTrivia` and `XmlTextLiteralNewLineToken` (PreparationPhase/LineEndings row) are Roslyn `SyntaxKind` members. Nothing under `src/` declares them, so "every backticked identifier resolves to a declaration under `src/`" fails today on these two.
  - `?` and `:` (PrecheckPhase row: "between a conditional expression's `?` or `:`") are C# tokens, not identifiers.
  - `sdd/specs/formatter/unsupported-syntax.md` (PrecheckPhase row) is a path.

### Changed requirements

_None._

### Examples for the tests

_None from a spec._ Seed cases for the test implementer:

- the real table passes;
- a fixture table whose guard cell names an undeclared `IsAfterSourceOnSameLine` fails, and the message names it;
- a fixture `Type.Member` whose type exists but declares no such member fails;
- a span that is not an identifier, such as `?` or a path, is skipped;
- `EndOfLineTrivia` resolves, while a misspelled `EndOfLineTrivai` fails.

### Output questions

_None._ The change affects only an architecture test; nothing an end user sees.

### Internal choices

- **Which spans count as guard names.** Only spans that form an identifier or a dotted identifier (`^[A-Za-z_][A-Za-z0-9_]*(\.[A-Za-z_][A-Za-z0-9_]*)*$`) in the third column, between the `formatter-phase-ownership` markers. Other spans are skipped. Alternative: check every span, which forces `AGENTS.md` edits (tooling lane) to remove the tokens and the path.
- **Roslyn `SyntaxKind` names.** A plain name also resolves when it is a member of `Microsoft.CodeAnalysis.CSharp.SyntaxKind`; the architecture tests already reference that assembly. A misspelled kind therefore still fails. Alternatives: an allow-list in the test, which goes stale, or rewording the `AGENTS.md` row, which widens the lane. Both are rejected.
- **What "resolves" means.** A plain name resolves when a production project under `src/` declares a type or member with that name. `Type.Member` resolves when such a project declares the type and the type declares the member (in any partial part). Production projects only, which matches the code-owner check in `SpecConsistencyTests`. Alternative: count test projects too, which would let a guard that exists only as a test helper pass.

### Non-goals

- The second column ("Decisions it owns") and every backticked name outside the marked table, such as those under *Key conventions*.
- Editing `AGENTS.md` or any guard name in it.
- Checking that a guard is actually used by the phase whose row names it. The check covers existence only.

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1428

Text written: `sdd/README.md`, *Tests and requirements* (the coverage bullet) and *Checks* (the `RequirementCoverageTests` bullets).

### Premise check (at d04e5d5)

- Confirmed: `RequirementCoverageTests.IsEnforced` keeps a requirement of an exempted spec enforced through `Uses` only when `SpecPath` starts with `shared/`.
- Confirmed: exactly eight enforced analyzer specs list `FMT-CLEAN-001` under `Uses`: RH5602, RH6006, RH6007, RH6013, RH6018, RH6019, RH6020, RH6022.
- Confirmed: no check reports an exemption key that names no spec file. `CoverageCheckDoesNotExemptTheSpecsOfAnExemptedDirectory` shows that a directory key exempts nothing. `DescribeCoverageOfExemptedSpecs` builds its lines only from requirements whose spec path equals a key, so with only such keys it prints "no spec is exempted".
- Both gaps are latent: `ExemptedSpecs` is empty today, and the issue keeps it empty.

### Text changes

- *Tests and requirements*: each exemption names one spec file under `specs/` by its path relative to that directory. An exemption that names no spec file (a misspelled, renamed, or removed path, a directory, or the index `specs/README.md`) exempts nothing and fails the check. A requirement of an exempted spec, wherever that spec lies, stays enforced when a spec that is not exempted lists it under `Uses`.
- *Checks*: the enforced case drops the `shared/` condition. A new failure case reads "an exemption names no spec file, or states no reason".

### Changed requirements

_None._ No spec or requirement ID changes, so no `requirement-ids.txt` edit and no architecture run were needed for my edit.

### Examples for the tests

_None from a spec._ The cases the text fixes:

- key `formatter/cleanu.md` (misspelled) fails and names the key;
- key `formatter/` (directory) fails and names the key;
- key `README.md` (the index) fails;
- key `formatter/cleanup.md`, where the file exists, passes;
- the real, empty `ExemptedSpecs` passes.
- `Uses`, on each side of the condition: `formatter/cleanup.md` exempted, with an enforced spec listing `FMT-CLEAN-001` under `Uses`, keeps `FMT-CLEAN-001` enforced (reported when uncovered). With the using spec also exempted, or with no spec using it, it is released.
- The existing `shared/` tests stay valid. `CoverageCheckEnforcesExemptedSharedRequirementsThatAnEnforcedSpecUses` and `CoverageCheckDoesNotEnforceExemptedSharedRequirementsThatOnlyAnExemptedSpecUses` still hold under the wider rule.

### Output questions

_None._ Only an architecture test and the SDD rulebook change.

### Internal choices

- **What "names a spec file" means.** The key must equal the relative path of a spec that `SpecCatalog.EnumerateSpecFiles` yields: ordinal comparison, forward slashes, index excluded. This is the same set the coverage check iterates. Alternative: `File.Exists`, which would accept the index and `requirement-ids.txt`.
- **The class-level documentation comment and `SharedDirectory`.** The `RequirementCoverageTests` summary, the `ExemptedSpecs` and `IsEnforced` comments, the `ReadSharedIdsUsedByEnforcedSpecs` name, and the `SharedDirectory` constant all state the `shared/`-only rule. They are test code, so the test implementer updates or removes them with the change. I did not edit them.
- **No change doc.** No spec, requirement, rule, phase, or surface changes (`sdd/README.md`, *When a change doc is required*). The alternative, a process change doc, is not required by the rulebook.
- **The report.** With the new check in place, an orphan key fails before the "no spec is exempted" report can mislead, so the report itself needs no change. Alternative: list orphan keys in the report too, which would duplicate the check.

### Non-goals

- Populating or changing `ExemptedSpecs`.
- Requiring every test to carry a tag.
- Changing how `Uses` is validated (`SpecStructureTests`).

### Follow-up candidates

_None._
