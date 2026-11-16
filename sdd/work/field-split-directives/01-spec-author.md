Role: spec author
Commit: 675bedf
Verdict: #1728: change doc written; #1586: change doc written, output question open

One change doc covers both issues, because they share their cause — the split's one directive guard (`FieldDeclarationSplitTransform.CarriesDirective`): `sdd/changes/2026-11/field-declaration-directives.md` (`Status: Draft`). The spec is written on the recommendation of the open #1586 question (option A); answers B or C need a spec amendment, named below.

## thoenissen/Reihitsu#1728

### Changed requirements

- FMT-UNSUP-010 — Directives inside a field declaration with several declarators are unsupported — added (`sdd/specs/formatter/unsupported-syntax.md`, ledger updated).
- FMT-UNSUP-009 — Comments and directives between a list element and the comma behind it are unsupported — changed: a directive in front of a declarator comma on a later line is refused under both FMT-UNSUP-009 and FMT-UNSUP-010; the reason and location stay FMT-UNSUP-009's (today's output).
- FMT-STRUCT-023 — A field with directives is not split — retired.
- FMT-STRUCT-024 — A rewrite that cannot be completed safely is not applied — changed: no longer names FMT-STRUCT-023.
- FMT-PIPE-029 — changed: the RH7101 input sentence no longer names FMT-STRUCT-023.
- FMT-PIPE-031 — The formatter answers whether it splits a field declaration — changed: no directive condition; the `Why` names FMT-UNSUP-010 next to FMT-UNSUP-004; the first example now returns true for `_c, _d, _e`, with `FindUnsupportedSyntax` returning `#if DEBUG` for the declaration and the class.
- SHARED-FIX-011 — changed: its example reference FMT-STRUCT-023 becomes FMT-STRUCT-030.
- RH7101-005 — retitled "The fix is withheld for a directive inside the declaration" — changed: withheld through the precheck (FMT-UNSUP-010, SHARED-FIX-010), also for another declaration of the same type; not withheld for a directive in the leading trivia.
- `Uses` rows updated in `unsupported-syntax.md`, `structural-transforms.md`, `pipeline.md`, `shared/code-fixes.md`, `analyzer/RH7101.md`.
- `AGENTS.md`: the precheck row names the fourth shape, and the structural row no longer lists `FieldDeclarationSplitTransform.CarriesDirective` (the implementer deletes it).

### Examples for the tests

- FMT-UNSUP-010: the refused example (four declarations: `#if X` between declarators, `#pragma` between declarators, `#if X` between attribute and modifier, `#if DEBUG` inside a lambda value; location = `#if X` in front of `b,`), the other-side example (region, `#if !RELEASE` group, one-declarator field with a directive among modifiers — formatted and split), the declarator-count condition (`private int _a⏎#if X⏎, _b⏎#endif⏎;` refused only where `X` is defined), event field and local not refused, a node-level input holding only part of the declaration not refused, and the tie with FMT-UNSUP-009 (reason stays FMT-UNSUP-009's).
- FMT-PIPE-031: the changed first example.
- RH7101-005: example 1 (`#pragma` between declarators: reported, no fix), example 2 (another declaration of the same type loses its fix), example 3 (region and `#if !RELEASE` group: fix splits; output checked).
- Existing tests to re-tag or invert (architecture tests fail on them now): `FieldDeclarationSplitTests.DirectiveBeforeFirstDeclaratorLeavesTheFieldIntact`, `FieldDeclarationSplitTests.DisabledTextBeforeFirstDeclaratorLeavesTheFieldIntact`, `DirectivePreservationTests.FieldDeclarationSplitKeepsDirectivesInDeclaration` (tagged retired FMT-STRUCT-023; all three inputs hold the shape, so the input is now refused). `scripts/test.sh --project architecture` gives 2 failures, both expected: these three tags, and FMT-UNSUP-010 uncovered. No other spec-structure test fails.

### Output questions

_None._ The shape is the maintainer's escalation-10 answer carried by the issue text ("from its first token through its `;`"); the one boundary that answer leaves open — the leading trivia of the first token — is the #1586 question below.

### Internal choices

- No initializer value is excepted (unlike FMT-UNSUP-004): a group that crosses a comma would reach the split, and telling it from one inside one value needs directives matched up. The maintainer's span "from its first token through its `;`" includes the values, so I read this as decided by the answer, not a new reading. Alternative: except values and keep a guard for groups crossing a comma — rejected, not a cheap check.
- The shape is any field declaration with several declarators, decided from its own tokens, whatever declares it (also the non-compiling extension-block field): derived from FMT-UNSUP-003, which forbids the owner to decide.
- At the tie with FMT-UNSUP-009 the reason stays FMT-UNSUP-009's, so no refused input changes its message. Alternative: FMT-UNSUP-010's reason — would change today's CLI text.
- RH7101-005 changed in place (retitled) rather than retired plus a new ID: it still states when the fix is withheld for a directive, and its two existing tests keep a valid tag (the pragma one stays correct; the region and `#if DEBUG` ones invert under option A).
- One change doc for both issues.
- Rule page `documentation/rules/RH7101.md` unchanged: it says nothing about directives; withheld fixes are covered by `general-notes.md`.

### Follow-up candidates

- **Formatter · a declarator behind a directive loses its continuation indentation (event field and local declaration).** Outside the run: the event field and the local are not split and not refused.

  Input:
  ```csharp
  using System;

  class C
  {
      public event Action A,
  #if X
          B,
  #endif
          D;

      void M()
      {
          int a,
  #if X
              b,
  #endif
              c;
      }
  }
  ```
  Real output (`main`, `scripts/format.sh`): `D;` moves to column 4 and `c;` to column 8 — the declaration's own column, while `B,` and `b,` (in disabled text) keep their continuation indent:
  ```csharp
      public event Action A,
  #if X
          B,
  #endif
      D;
  ...
          int a,
  #if X
              b,
  #endif
          c;
  ```
  Why it looks wrong: a continuation line of a declaration lands at the declaration's column, as if it started a new statement or member. Reached through the CLI. Readings: (A) indent `D;`/`c;` one level as a continuation, as without the directive; (B) keep as is. Recommendation: A. Requirement IDs: FMT-INDENT (declarator continuation), not identified further.

## thoenissen/Reihitsu#1586

### Changed requirements

- FMT-STRUCT-022 — retitled "Split fields keep their comments and directives" — changed: the leading trivia stays on the first field also when it holds a directive or disabled text; it is the only place a split declaration can hold one; new example (region and `#if !RELEASE` group, split).
- FMT-STRUCT-023 retired and RH7101-005 changed, as under #1728 — the same edits carry this issue's outcome (written on recommendation A below).

### Examples for the tests

- FMT-STRUCT-022: the new region / `#if !RELEASE` example (output checked: `main` with only the `CarriesDirective` operand removed, `scripts/format.sh`, gives exactly the stated output; `main` today gives the input unchanged but the final newline).
- RH7101-005 example 3 (same check through `scripts/apply-fix.sh RH7101`: fixed in one iteration, LF and CRLF). The issue's "Done when" asks for a region, an `#if`, and a directive between declarators: the region and `#if !RELEASE` cases are these examples, the directive between declarators is FMT-UNSUP-010's refused example and RH7101-005 example 1.
- Existing analyzer tests that invert under A: `RH7101DoNotCombineFieldsAnalyzerTests.RegionDirectiveInTheDeclarationWithholdsTheFix` and `VerifyFixIsNotOfferedWhenFieldCarriesDirective` (`#if DEBUG` around the field, DEBUG defined) — both now get the fix.

### Output questions

**1 · RH7101 · Do not combine fields — a directive in front of a combined field**

Input:
```csharp
class C
{
    #region Fields

    private int _x, _y;

    #endregion // Fields

#if !RELEASE
    private int _p, _q = 1;
#endif
}
```

Before (`main`): formatter keeps both declarations combined; RH7101 is reported on both, no fix offered.

After, A (recommended) — formatter and RH7101 fix:
```csharp
class C
{
    #region Fields

    private int _x;
    private int _y;

    #endregion // Fields

#if !RELEASE
    private int _p;
    private int _q = 1;
#endif
}
```

After, B: as before — combined, RH7101 reported without a fix.

After, C: the input unchanged.
The CLI reports: `error: <file>(3,5): unsupported syntax: a directive inside a field declaration with several declarators. The file was left unchanged.`
No code fix that formats this type or field is offered here (RH7101, RH5110, RH5404, RH5113, RH5111, among others).
Declared in this work.

Why A: the directive stands in front of the declaration and stays in front of its first field, so a region still opens in front of the same code and an `#if` group holds all the fields; nothing is dropped or moved. (C would remove no further special case beyond A's, and it refuses common code — every file where a region, or the start or end of an `#if` group, directly precedes a combined field.)

Spec consequence of each answer: A — as written. B — FMT-UNSUP-010 unchanged; FMT-STRUCT-023 is not retired but narrowed to the leading trivia, FMT-STRUCT-022's directive paragraph and example are dropped, RH7101-005 keeps a withholding for the leading-trivia case, FMT-PIPE-031 keeps a narrowed directive condition. C — FMT-UNSUP-010 drops its leading-trivia exception and its other-side example, FMT-STRUCT-022's directive paragraph is dropped, RH7101-005's third example becomes "no fix".

### Internal choices

_None_ beyond those under #1728.

### Follow-up candidates

_None._

## Whole run

- Commands run: `scripts/format.sh` and `scripts/apply-fix.sh RH7101` on fixtures (main, and a throwaway worktree of main with only the `CarriesDirective` operand removed, now deleted) to produce every Before/After in the change doc and the examples; `scripts/apply-fix.sh RH5110` to confirm a fix that formats the type also splits its fields; `scripts/test.sh --project architecture`: 151 passed, 2 failed — both the expected coverage/retired-tag findings named above.
- The prompt carried no intended fix or suspected cause.
- Files: `sdd/changes/2026-11/field-declaration-directives.md`, `sdd/specs/formatter/unsupported-syntax.md`, `sdd/specs/formatter/structural-transforms.md`, `sdd/specs/formatter/pipeline.md`, `sdd/specs/shared/code-fixes.md`, `sdd/specs/analyzer/RH7101.md`, `sdd/specs/requirement-ids.txt`, `AGENTS.md`.
