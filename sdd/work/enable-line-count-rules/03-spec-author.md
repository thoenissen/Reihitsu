Role: spec author
Commit: 47971c8a
Verdict: #1865: spec written, change doc accepted; #1866: no spec change — tooling only

The prompt carried no intended fix, suspected cause, or hoped-for verdict beyond the maintainer's answers 1A, 2A, 3C, which this handover carries out as recorded in `00-lead.md`.

## thoenissen/Reihitsu#1865

Files changed:

- `sdd/changes/2026-11/enable-line-count-rules.md` — rewritten for 1A, 2A, 3C; `Status: Accepted` (set here on the lead's instruction; `sdd/README.md` gives that step to the lead). *Visible output changes* lists the three default changes and the rule pages, each with input, before, and after; *Decisions* records 1A, 2A, 3C; *Alternatives* records what they replaced.
- `sdd/specs/analyzer/RH2008.md` — summary, RH2008-001 (default test limit 120, `Why:` with the maintainer's purpose of a default, example 120/121).
- `sdd/specs/analyzer/RH2111.md` — summary, RH2111-001 (defaults 1500 and 3000, `Why:` rewritten, examples 1500/1501, 3000/3001, invalid setting against 1500).
- `sdd/specs/shared/line-count.md` — the SHARED-LINES-002 example: "the default test limit of 120 lines". The requirement itself is unchanged.
- `documentation/rules/RH2008.md`, `documentation/rules/RH2111.md` — the default sentence and the configuration example (numbers only).
- `.claude/commands/add-resource-texts.md` — analyzer texts have no wrapper property; the base derives `<ID>Title`/`<ID>MessageFormat` from the ID and publishes `Title`/`MessageFormat` on each analyzer; tests read the message from that property; code-fix procedure unchanged.

### Changed requirements

- RH2008-001 — The limits are `design.maximumMethodLines` and `design.maximumTestMethodLines` — changed (test default 100 → 120)
- RH2111-001 — The limits are `design.maximumTypeLines` and `design.maximumTestTypeLines` — changed (production default 500 → 1500, test default 1000 → 3000)
- SHARED-LINES-002 — example sentence only (100 → 120); the normative text is unchanged

No requirement ID was added or retired. `scripts/test.sh --project architecture`: 198 passed.

### Examples for the tests

- RH2008-001: in test code, 120 lines not reported, 121 reported with "…exceeds the limit of 120."; `maximumMethodLines: 40` alone leaves a 70-line test method unreported. Production 60/61 unchanged.
- RH2111-001: production 1500 not reported, 1501 reported "…limit of 1500."; test 3000/3001 "…limit of 3000."; `maximumTypeLines: 400` alone leaves a 700-line test class unreported; `maximumTypeLines: 0` reports a 1501-line production type against 1500.
- SHARED-LINES-002 example: in a test project, `{ "design": { "maximumMethodLines": 2 } }` leaves both methods unreported (test default 120).
- Code that carries the defaults: `RH2008MethodsShouldNotExceedTheLineLimitAnalyzer.DefaultTestLimit` (100), `RH2111TypesShouldNotExceedTheLineLimitAnalyzer.DefaultProductionLimit` (500) and `.DefaultTestLimit` (1000); `RH2008…AnalyzerTests` and `RH2111…AnalyzerTests` assert the old values.

What the repository work has to establish (no requirement; from the change doc *What* and *Decisions*):

- Root `reihitsu.json` exactly `{ "design": { "maximumTypeLines": 500 } }`, passed to every project's analyzers as `AdditionalFiles` by the repository build, and read by the self-hosting scan as the build reads it. RH0001 must report nothing on it. It has no `copyright` section, so RH8402 still checks nothing (RH8402-001), and no `naming` section.
- With it: 18 production methods over 60, 9 test methods over 120, 5 production types over 500 (`CasingCodeFixProviderBase{T}`, `RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer`, `OrderingDeclarationUtilities`, `SyntaxTriviaUtilities`, `AnalyzerResources`), 11 test types over 3000 — lists in `01-spec-author.md` (test methods: the nine of the twenty listed there with more than 120).
- `AnalyzerResources` design (3C): the class keeps `ResourceManager` and no per-rule property; `DiagnosticAnalyzerBase` takes the ID (and category, enabled-by-default) and derives `<ID>Title`/`<ID>MessageFormat`; it publishes both texts as properties of the analyzer; a unit test enumerates every analyzer of the assembly and asserts both texts are non-empty (replacing `AnalyzerResourcesTests`' reflection over properties); the ~380 test call sites of `AnalyzerResources.RH####MessageFormat` read the analyzer's property. Checked at 47971c8a: all 349 analyzers using `nameof(AnalyzerResources.…)` name exactly `<their DiagnosticId>Title`/`MessageFormat`, so the derivation changes no text. The `.resx` holds 698 rule keys (the four other `<data name=…>` matches are the template's schema comment).

### Output questions

_None._ 1A, 2A, 3C settle every visible output of this issue.

### Internal choices

- The `AnalyzerResources` resource naming is recorded in the change doc and in `.claude/commands/add-resource-texts.md`, not as a spec requirement: titles and messages an end user sees do not change, and which resource holds them is implementation. Alternative: a SHARED-DIAG requirement "a rule's texts are the resources `<ID>Title`/`<ID>MessageFormat`" — rejected, it specifies no visible behavior.
- The property names `Title` and `MessageFormat` are written into `add-resource-texts.md` as the recommendation; whether they are static per analyzer or instance properties on the base is left to the implementer. If the implementer picks other names, the procedure doc follows the code. Alternative: leave the names out of the doc — rejected, the test example needs a concrete name.
- Suggest a cheap test that the root `reihitsu.json` sets no limit above the shipped default (the maintainer's "never higher"), since the rule otherwise lives only in the change doc. Lead's call; no visible output.
- `line-count-rules.md` (the earlier change doc) is left as written: change docs are a historical record; the new doc names the decisions it replaces (11A, 2B/9B defaults, 14A).
- Rule pages: number changes only, as the page is derived from the spec; no other wording changed. `RuleDocumentationConventionTests` was not run (no prose added that could mention the formatter; footer untouched).

### Non-goals

- Changing a default other than the three 1A names (production methods stay 60).
- A repository limit above a shipped default, or one equal to it (the `reihitsu.json` names only `maximumTypeLines`).
- Partial classes for `AnalyzerResources`; any change to `CodeFixResources`.
- Changing a title or a message text of any rule.
- Rebuilding `binaries/` (`/bump-version`).

### Follow-up candidates

_None._ (Observation only: `AGENTS.md` *Key conventions* names `DiagnosticAnalyzerBase<TAnalyzer>`, but the class is not generic; if the implementer reshapes the base for 3C, the sentence should match the result. In scope of this run's change, so it is a note for the implementer, not a candidate.)

## thoenissen/Reihitsu#1866

Unchanged from `01-spec-author.md`: no spec, change doc, or rule page; the rulesets are tooling. The new root `reihitsu.json` has no `copyright` section, so RH8402 still reports nothing in either configuration (RH8402-001), and the facts there still hold.

### Changed requirements

_None._

### Examples for the tests

_None._

### Output questions

_None._

### Internal choices

_None_ beyond `01-spec-author.md` (rename `RH08402` → `RH8402`).

### Follow-up candidates

_None._
