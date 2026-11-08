Role: spec author
Commit: acf0007
Verdict: spec written

The prompt carried no intended fix, suspected cause, or hoped-for verdict beyond pointing at the lead's decisions. Every example output below was checked with `scripts/apply-fix.sh` at `acf0007` (LF and CRLF identical), unless it states behavior the run has to bring in (marked "required").

## thoenissen/Reihitsu#1595

### Changed requirements

- SHARED-ORDER-007 — Initializer execution order is kept — clarified. The normative sentence now names every member reordering fix (RH7102, RH7103, RH7109) instead of "a fix that can move fields … with a runtime initializer", which read as excluding RH7102. The layout guard is now a MUST of every member reordering fix (it was "a fix that withholds a move because …"). The closing paragraph says every member reordering fix applies both guards, and why RH7102 needs them although a constant has no initializer and no layout place (decision 5; decision 4 puts the guard in the base class, which the spec states as "every member reordering fix").
- RH7102-006 — The fix is withheld when the move is unsafe — clarified: adds SHARED-ORDER-007, a paragraph stating that only excluded code in the constant's leading trivia can trip it, and three examples.
- RH7102 header `Uses` — adds SHARED-FIX-003 and SHARED-ORDER-007.

No change doc: SHARED-FIX-003 already required the withholding; no intended behavior changes.

### Examples for the tests

- RH7102-006: struct with `_y` excluded in front of `K` (withheld, skeptic row a2); class with `_i = Log("i")` crossed and `_c = Log("c")` excluded (withheld, row b2); class with `_i` uninitialized (fixed, excluded block moves with `K`, row c2-like). The `Log` helper is block-bodied on purpose: with an expression body, the RH7102 fix rewrites it to a block body (see Follow-up candidates).
- The rest of the skeptic's matrix for #1595 is covered by SHARED-ORDER-007 and SHARED-FIX-003 as written; tag those tests with RH7102-006 and SHARED-ORDER-007.

### Output questions

_None._

### Internal choices

- SHARED-ORDER-007 names the three member reordering fixes in its normative sentence rather than saying "every fix that moves a member with its leading trivia". Alternative: the abstract wording. The named list matches the spec's own intro and is checkable.
- The RH7102-specific examples live in RH7102-006, not in SHARED-ORDER-007, so the shared spec does not reference a rule requirement under `Uses`.

## thoenissen/Reihitsu#1593

### Changed requirements

- SHARED-ORDER-007 — clarified (decision 6, option A): a new paragraph states that a member whose declaration an `#if`/`#elif`/`#else` splits counts with what any configuration gives it: an initializer (also behind a property's accessor list), both contexts when only one configuration declares it `static`, an instance field when one configuration makes it one, and a further member that excluded code inside its declaration declares counts at that position. A split member that gives nothing conflicting keeps the move. The `Why` gains one sentence. Three examples added: RH7109 with `= Log("b")` under `#if DEBUG` inside the moved member (withheld, skeptic row 1593b/a), RH7103 with `static` under `#if !DEBUG` in a struct (withheld, row 1593c/a), and the issue's own RH7309 shape (fixed — the counterpart; output checked).

The existing text did not decide this: it counted only members "that code excluded by an `#if` … declares", not parts of an active member.

RH7309-006 is unchanged: it already counts initializers "as SHARED-ORDER-007 counts them".

### Examples for the tests

- SHARED-ORDER-007: the three new examples. The skeptic's other failing rows (1593/b, /c, /e, /g, 1593d/b, 1593b/b, /c) follow from the bullets; tag them SHARED-ORDER-007 (and RH7309-006 for the RH7309 rows).

### Output questions

_None._

### Internal choices

- The split-member rule is stated in SHARED-ORDER-007 rather than in RH7309-006, because the decision has one owner (`OrderingDeclarationUtilities`) serving all four consumers. Alternative: a sentence per rule spec.
- No RH7109 counterpart example (split member, crossed member without an initializer, fix offered): its output today moves the `;` that stands alone behind `#endif` from column 8 to column 4, which I did not want to write into a spec as intended. The RH7309 counterpart has a clean output.

## thoenissen/Reihitsu#1616

### Changed requirements

- RH2001-004 — The fix replaces the property with a private field — changed: a paragraph states that when the fix adds `readonly` to modifiers that are not in RH7105-001 order, it writes them in that order and rewrites the list as SHARED-MODORDER-002 states (leading trivia — indentation, blank lines, comments, directives, documentation comment — once in front of the new first modifier, one space after each modifier, one line); a property that gets no `readonly` keeps its written order. One example: `// comment` + blank line, documentation comment with initializer and trailing comment, and a `{ get; set; }` counterpart (required: the first two are not fixed correctly today; the counterpart is today's output). Covers the maintainer's answer 2A and decision 8 (fixtures a, c, d, j, k, l).
- RH2001-009 — The fix is withheld for a comment or directive that a reordered modifier would cross — added (answer 1A, decision 9). Withheld when a comment or directive stands between two modifiers that the new order puts the other way round; a comment between modifiers that keep their order keeps its place and its fix. Examples: `static /* a */ private`, `static // b` on its own line, `new /* c */ static private` (withheld, required); `#pragma` between `static` and `private` (withheld, today's output); `private /* a */ static` and `private /* b */ new static` (fixed, today's output `private /* b */ static new readonly int _second;`).
- RH2001 header `Uses` — adds RH7105-001 and SHARED-MODORDER-002.
- `sdd/specs/requirement-ids.txt` — RH2001-009 inserted.
- Change doc `sdd/changes/2026-11/rh2001-modifier-reorder-trivia.md`, `Status: Draft`: RH2001-009 adds behavior that differs from today's (a fix is withdrawn), and the blank-line layout was decided by the maintainer rather than by an existing requirement, so `sdd/README.md` (*When a change doc is required*) asks for one. Both visible changes already have the maintainer's answers (1A, 2A), recorded under `Decisions`; the lead can set `Accepted`.

`RequirementCoverageTests` fails on RH2001-009 until the test implementer tags a test; every other `Sdd` architecture test passes.

Rule page `documentation/rules/RH2001.md`: unchanged — what is reported and how to fix it do not change in the normal case.

### Examples for the tests

RH2001-004 (new reorder example), RH2001-009 (all four examples). The skeptic's fixtures e (`#pragma` above), n (`#region` above), i (three modifiers), and m also belong here, under LF and CRLF.

### Output questions

**1 · RH2001 · Private auto-implemented properties should not be used — comment between the last modifier and the type, modifiers reordered**

Not covered by the answers: no modifier crosses the comment, so RH2001-009 does not withhold, but where the added `readonly` goes is open. RH2001-004's new paragraph and RH2001-009 both leave this case out.

Input:
```csharp
class C
{
    static private /* a */ int Value { get; }
}
```

Before (`main`):
```csharp
class C
{
    private /* a */     static readonly int _value;
}
```

After, A (recommended):
```csharp
class C
{
    private static /* a */ readonly int _value;
}
```

After, B:
```csharp
class C
{
    private static readonly /* a */ int _value;
}
```

After, C: no fix offered; the diagnostic stays.

Why A: it is what the fix writes today for the already-ordered `private static /* a */ int Value { get; }` (`private static /* a */ readonly int _value;`), so both inputs give the same field.

Requirement IDs: RH2001-004, RH2001-009. Only this case waits for the answer; nothing else in the run depends on it.

### Internal choices

- The trivia rule is stated by reference to SHARED-MODORDER-002, not by using SHARED-MODORDER-003 for the withholding: SHARED-MODORDER-003 would also withhold `private /* a */ static`, against decision 9. Alternative: widen `modifier-ordering.md` to name RH2001; rejected because its withholding rule differs.
- RH2001-009 is a new requirement rather than a further bullet of RH2001-007, because RH2001-007 lists places inside the property where any comment withholds the fix, while RH2001-009 depends on the reorder.

## Follow-up candidates

- **RH7102 and RH7109 fixes reformat a method they do not move.** Outside the run's issues; reached by the code fixes (single fix). Input (RH7102):

  ```csharp
  public class C
  {
      private int _i;
      private const int K = 1;

      private static int Log(string name) => 0;
  }
  ```

  Output at `acf0007` (`scripts/apply-fix.sh RH7102`, LF and CRLF):

  ```csharp
  public class C
  {
      private const int K = 1;
      private int _i;

      private static int Log(string name)
      {
          return 0;
      }
  }
  ```

  Why it looks wrong: SHARED-FIX-006 (*Formatting*) says a fix formats only the node it replaces and not the type that contains it; `Log` is neither moved nor crossed. Readings: A — `Log` keeps its expression body (SHARED-FIX-006 as written); B — today's output. Recommendation: A. The RH7109 fix does the same (seen with the split-member fixtures).
- Seen, not a candidate yet: the RH7109 fix moves a `;` that stands alone behind `#endif` from column 8 to column 4 (`private readonly int _b` / `#if DEBUG` / `= Log("b")` / `#endif` / `;`). Whether that is the formatter's intended layout for such a line was not checked.
- RH7102 analyzer ignores regions — already in `00-lead.md` from the skeptic; unchanged.
