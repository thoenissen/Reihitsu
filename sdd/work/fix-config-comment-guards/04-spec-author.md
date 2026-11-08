Role: spec author
Commit: ef5882a
Verdict: spec written

Amendment round after `03-skeptic.md`, the batch 2 answer, and decisions 12–14 in `00-lead.md`. The coordinator's message carried no intended fix beyond those decisions. The `Sdd` architecture tests pass except `RequirementCoverageTests` on RH2001-009, which waits for the test implementer's tag.

## thoenissen/Reihitsu#1595

### Changed requirements

- RH7102-006 — clarified (decision 12):
  - "When no `#if` … takes part in the refusal, the diagnostic is still reported" is now "The diagnostic is still reported".
  - The second paragraph now speaks of "a constant that no `#if`, `#elif`, or `#else` splits".
  - A new sentence says that a declaration which is `const` only in some configurations counts as a split member under SHARED-ORDER-007.
- SHARED-ORDER-007 closing paragraph — clarified the same way.

### Examples for the tests

The skeptic's rows s1 and s2 (and s3 as the counterpart), tagged RH7102-006 and SHARED-ORDER-007. I added no spec example for them, for the reason below.

### Output questions

_None._

### Internal choices

- **Not decided by me; for the lead: s1 and s2 should not be reported at all according to the specs as written.** Both inputs put a conditional directive into the constant's modifier list:

  ```csharp
  private
  #if DEBUG
      static readonly
  #else
      const
  #endif
      int K = 1;
  ```

  - RH7102-001 promises a report only for "a `const` field declaration that contains no `#if`, `#elif`, `#else`, or `#endif` directive".
  - SHARED-DIAG-008 says a rule MUST NOT report a construct when a conditional directive lies in the span it judges. A modifier list is its own example, and the `const` and accessibility modifiers are what RH7102 judges.
  - `RH7102ConstantsMustAppearBeforeFieldsAnalyzer.OnTypeDeclaration` checks no directive, so it reports them today (the skeptic's `fixed` rows).

  A test that asserts "reported, no code fix offered" for s1 and s2 therefore locks in a report that SHARED-DIAG-008 forbids. The same holds for the RH7103 row 1593c/a of `01-skeptic.md` (split `static` in the reported member's modifier list), which RH7103 also reports today. The amended spec text is true either way: it describes how the guard counts a split constant, which matters as long as the analyzer reports one. That is why I added no "is reported" example.
  - My recommendation: the s1 and s2 tests assert today's diagnostic and the withheld fix, as decision 12 says, and the analyzer gap becomes a follow-up candidate (below).
  - The alternative: fix the analyzer in this run, so s1 and s2 become "not reported" tests. That widens #1595 from the fix to the analyzer.

## thoenissen/Reihitsu#1593

### Changed requirements

- SHARED-ORDER-007 — clarified (decision 13):
  - "the members of excluded code in its leading trivia or inside it; no other member moves".
  - The first split-member bullet also covers "its initializer is active in every configuration and only one configuration declares it without `const`".
  - The RH7109 counterpart example stays out.
- **Corrected my own example (SHARED-DIAG-008):** the split-`static` example used RH7103 with `#if !DEBUG static #endif` in the reported member's modifier list and said "is reported by RH7103". SHARED-DIAG-008 forbids that report, because the modifier list is the span RH7103 judges. The same problem as s1 and s2 under #1595. I replaced it with the skeptic's RH7309 row 1593/g (struct, the `Fields` block holds the split `_b`, `IDisposable` holds instance `_a`). RH7309 judges the region order, not the modifiers, so its report is correct. Today the fix is offered, so this example is required behavior.

### Examples for the tests

Unchanged from `02-spec-author.md`.

### Output questions

_None._

### Internal choices

_None._

## thoenissen/Reihitsu#1616

### Changed requirements

- RH2001-004 — changed (decision 14):
  - The reorder paragraph applies whenever the fix adds `readonly`, "also when the property's modifiers already are in that order".
  - The leading-trivia list now includes "a comment in front of it on its line".
  - The coverage sentence points to RH2001-009 for a comment between two modifiers and for a comment between the last modifier and the type.
  - New example: `/* lead */ static private int Value { get; }` → `/* lead */ private static readonly int _value;` (required; today no fix is offered).
- RH2001-009 — amended:
  - A comment **or directive** between modifiers that keep their order keeps its place and the fix (decision 14).
  - A comment between the last modifier and the type does not withhold the fix.
  - New paragraph (batch 2 answer A): `readonly` goes at its RH7105-001 place, and when a comment stands at that place it is written behind the comment. "That place" means between the last modifier in front of `readonly` and the first behind it, or between the last modifier and the type when none follows it.
  - A directive at that place is not covered.
  - The coverage sentence now includes comments between the last modifier and the type.
  - Examples added: `static private /* c */ int Third` → `private static /* c */ readonly int _third;` (required; this is the batch 2 change) and `private static /* d */ int Fourth` → `private static /* d */ readonly int _fourth;` (today's output).
- Change doc `sdd/changes/2026-11/rh2001-modifier-reorder-trivia.md`:
  - *What* gains the `readonly` placement.
  - *Visible output changes* gains `static private /* a */ int Value { get; }`: before `private /* a */     static readonly int _value;`, after `private static /* a */ readonly int _value;`. It also lists the two inputs whose output stays the same.
  - *Decisions* records batch 2 option A; *Alternatives considered* records options B and C.
  - `Status: Accepted`: every visible change it lists has the maintainer's answer, and the lead accepted it with this amendment.

### Examples for the tests

- RH2001-004: the new `/* lead */` example.
- RH2001-009: the extended "no modifier crosses" example.
- The skeptic's fixtures `u1`/`g` (`unsafe`; these need `AllowUnsafeBlocks`, so they are not in the spec), `u2`, `c`/`d` (a directive between kept-order modifiers), and `h`, all under LF and CRLF.

### Output questions

_None._ My question 1 from `02-spec-author.md` is replaced by the skeptic's question, which is answered (batch 2, A).

### Internal choices

- A directive at the place where `readonly` goes (for example `private` / `#pragma …` / `unsafe string Value { get; }`) is left out of RH2001-009's coverage. Batch 2 asked only about comments, and such an input needs `unsafe` or a similar modifier that sorts behind `readonly`. The alternative was to treat it like a comment, which would decide an output nobody was asked about.

## Follow-up candidates

- **RH7102 (and RH7103) report a member whose judged modifier an `#if` splits.** This goes against RH7102-001 and SHARED-DIAG-008 (input above). It is reached by the analyzer and so by every user. Readings: A, not reported (the specs as written; recommended); B, today's report. Unless the lead pulls it into the run, it is outside the run's issues: #1595 is about the fix. RH7103 has the same gap for a split `static` (row 1593c/a, reported today). I did not check RH7109 for a split `readonly`.
- Unchanged from `02-spec-author.md` and `03-skeptic.md`: the member-moving fixes format the whole type. The skeptic's merged form of that candidate applies.
