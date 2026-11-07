Role: spec author
Commit: 840a00d
Verdict: spec written

Amendment round: code owners only, per `00-lead.md` decisions 12 and 14. The prompt carried no intended fix, suspected cause, or hoped-for verdict. No requirement text changed. No new requirement ID was added, so `requirement-ids.txt` is unchanged. No change doc and no rule page.

The rule specs (RH2001, RH2002, RH2004, RH2005, RH2101, RH7002, RH74nn, RH8306) name only their analyzers, fix providers, and `PropertySymbolExtensions`/`DeclarationModifierUtilities`. They name no shared helper, and no requirement body names a type, so none of them changed. Per `sdd/README.md`, a type that implements shared behavior is a code owner of that shared spec, and the caller's spec lists the IDs it relies on under `Uses`.

## Changed requirements

None. Only header code owners changed, with the index `sdd/specs/README.md` kept in step:

- `sdd/specs/shared/diagnostics.md`: `PartialMemberUtilities` added. It implements SHARED-DIAG-012. Its callers' specs (RH2002, RH2004, RH8306, RH7401–RH7412) already list SHARED-DIAG-012 under `Uses`.
- `sdd/specs/shared/casing.md`: `ExcludedCodeUtilities` added. It implements SHARED-CASING-007's excluded-`#if` coverage condition, including the "document where a symbol of the rename cascade can be referred to" scope. `CasingCodeFixProviderBase<T>` stays, because it still owns the rename.

## Examples for the tests

_None._ Nothing behavioral changed.

## Output questions

_None._

## Internal choices

- `ExcludedCodeUtilities` is owned by `shared/casing.md`, not left unowned as a general-purpose utility, and not given to `shared/code-fixes.md` (which has `—`). The reason is that its scope rule (referring documents, dependent projects) is what SHARED-CASING-007 specifies. The alternative is to treat it as a general-purpose identifier search with no owner.
- RH2005's `Uses` is unchanged and does not list SHARED-CASING-007. RH2005-004 decides excluded code through SHARED-FIX-003, not through the casing requirement, so RH2005 calls the helper without relying on that requirement. RH2001 already lists SHARED-CASING-007.
- The generated-code rule inside `PartialMemberUtilities` (decision 14) needs no spec change. SHARED-DIAG-001 already names the compiler's rule.

## Follow-up candidates

_None._

## Tests

`scripts/test.sh --configuration Debug --project architecture --filter "FullyQualifiedName~Reihitsu.ArchitectureTests.Sdd"`: 51 passed, 0 failed. This includes `SpecConsistencyTests`, `SpecStructureTests`, and `RequirementCoverageTests`.
