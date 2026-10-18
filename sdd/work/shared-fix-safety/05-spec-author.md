Role: spec author
Commit: 845e670
Verdict: spec written

This round revises SHARED-FIX-003 to state the maintainer's answers to questions A, B and C of `04-spec-author.md`, recorded in `squad/decisions/2026-10-18-shared-fix-003-layout-fields-endregion.md`. The three earlier SHARED-FIX-003 decisions and the `00-lead.md` clarifications still hold. My prompt carried no intended fix, suspected cause, or hoped-for verdict. It carried constraints, and I followed them:

- no change doc;
- no region-name tab exception;
- no requirement the issue does not name;
- the `AGENTS.md` sentence unchanged;
- examples that match today's code;
- no code, no tests, no `sdd/work/coordinator/`.

Files changed: `sdd/specs/shared/code-fixes.md` only. `sdd/specs/requirement-ids.txt`, `sdd/specs/README.md` and `AGENTS.md` keep their state from 1bf3eeb. `Uses` stays `—`.

Check: `scripts/test.sh --project architecture --filter "FullyQualifiedName~SpecStructureTests|FullyQualifiedName~SpecConsistencyTests|FullyQualifiedName~RequirementCoverageTests|FullyQualifiedName~RuleDocumentation"` gives 38 passed and 1 failed. The failure is `EveryRequirementOfAnEnforcedSpecIsCovered`, for SHARED-FIX-003, -004 and -005. That is expected until the covering tests exist.

## Changed requirements

SHARED-FIX-003 — A fix that could break the code is withheld — added (revised in this round). SHARED-FIX-001, -002, -004, -005 and the non-goals are unchanged from 54b6489.

- **Answer A, layout.** The layout sentence no longer exempts "the whitespace, line breaks, and indentation of a comment or a directive". It now exempts the whitespace around a comment or a directive and the indentation of its lines. The text of a comment or a directive is content, including the whitespace and line breaks inside it, such as between two words of a `//` or `/* */` comment or of a directive's message.

  For documentation comments, the `///` exterior and the whitespace and line breaks next to an XML tag stay layout. This comes from skeptic-scope answer 2, whose question named the RH3202 doc-comment reflow and decided it was layout. Question A was asked only about `//`, `/* */` and directive text, so answer A does not reverse that. The combined wording keeps the RH3202 and RH8107 examples true. A line break or whitespace between two words of a documentation comment's text counts as content, as answer A's general wording says. No example depends on that case.

  The RH3202 example sentence now names exactly what changes: the `///` exterior, the line breaks next to the XML tags, and the raw-literal indentation.
- **Answer B, field order.** The run-time bullet now names the types: a struct, which has sequential layout unless it declares `LayoutKind.Auto` or `LayoutKind.Explicit`, and any other type declared with `[StructLayout(LayoutKind.Sequential)]`. It also says why explicit layout is outside: `[FieldOffset]` sets each offset.

  `LayoutKind.Auto` is excluded by the decision's own criterion ("every type whose field order actually sets its layout"). Its "structs (sequential by default)" covers only a struct that keeps the default.
- **Answer C, `#endregion`.** The comment-text bullet now lists two kinds of text that change together with a name the fix changes:
  - the binding doc-comment name (from the earlier decision);
  - the comment of an `#endregion` that repeats the name of the region the fix renames. An `#endregion` comment that does not repeat that name stays unchanged.

  `Why:` now says a rename reaches every place that "refers to the symbol or repeats the region's name".
- **New examples:**
  - RH7109, a reorder that changes no layout: an explicit-layout struct, plus a sentence for an `Auto` struct and for a class without `StructLayout`;
  - RH7302, two regions: one whose `#endregion` comment repeats the name and one whose comment does not;
  - RH5601, a tab inside a `//` comment's text, which the fix keeps.

Qualifiers whose triggering side has no passing example today. FC-002 wants an example on each side, and only the non-triggering side is shown. Each triggering side is a known bug, and its regression test belongs to the follow-up:

| Qualifier | Triggering input | Today | Owner |
|---|---|---|---|
| Field order sets the layout (B) | RH7109 on `public struct S { public int _a; public readonly long _b; }`, and on the same class with `[StructLayout(LayoutKind.Sequential)]` | fix offered, moves `_b` (verified, LF and CRLF) | the RH7109 follow-up from skeptic-scope answer 3 |
| Whitespace inside comment text (A) | RH5011 on `break; /* a⏎b */` | inserts a line inside the comment | thoenissen/Reihitsu#1075 |
| Changed word outside the reported text | RH7103 on a type with an unrelated `#region helpers` | `#region Helpers` | the RH7103 follow-up from skeptic-scope answer 2 |
| Excluded branch names the symbol | RH4106 with `return Value + 1;` under `#if NEVER` | excluded use stays `Value` | thoenissen/Reihitsu#1073 |

The issue's three examples remain removed, as listed in `04-spec-author.md`. Each stays a bug against the normative sentences.

## Examples for the tests

Every example in SHARED-FIX-001, -003, -004 and -005 matches today's code (commit 845e670; no production file changed since 54b6489). I re-ran each one with `scripts/apply-fix.sh <RH####> <fixture-dir> --no-install`, LF and CRLF, on fixtures in the scratchpad. Where the fixture had no markup, I compared the LF diff with the spec's after-block.

- **SHARED-FIX-001**
  - RH5002, `a++;` then `if`: 1 application inserts the blank line.
  - RH5028, three leading blank lines: removed in 1 application (RH5028-003 tests).
  - RH5023, three blank lines in a method body: 2 applications leave one (RH5023-004 tests).
  - The leading run `⏎⏎⏎class C⏎{⏎}⏎`: RH5023 leaves the first line after 2 applications, and RH5028 removes all three in 1.
- **SHARED-FIX-002:** RH2005 on `first` and `second`: both become `private` (`FixAllCorrectsEveryDiagnostic`).
- **SHARED-FIX-003**
  - RH4106 `Value` beside `_value`: `no-fix-offered` (SHARED-CASING-009 test).
  - RH4106 `nameof(Value)` becomes `nameof(_value)`.
  - RH7103 `_a`/`_b`. On the original input, `_b` has no fix and `_a` has one (`RH7103StaticElementsMustAppearBeforeInstanceElementsAnalyzerTests`, SHARED-ORDER-007).
    - The runner reports `fixed` in 2 iterations: once `_a` has moved, `_b` crosses only `_instance`.
    - A test for this example has to assert on the original document, not on the iterated result.
  - **New:** RH7109 on the `LayoutKind.Explicit` struct. `_b` moves in front of `_a` and keeps its `[FieldOffset(8)]`, exactly the spec block. The same passes for `[StructLayout(LayoutKind.Auto)] public struct S` and for `public class C` without the attribute. Both are fixed, and `_b` moves in front of `_a`.
  - RH4106 `<see cref="Value"/>` becomes `<see cref="_value"/>`. The summary word `Value` stays.
  - **New:** RH7302 on the two-region class. The result is exactly the spec block: `#region Members` / `#endregion // Members`, then `#region Values` / `#endregion // helpers` (2 applications). The existing tests `VerifyCodeFixUpdatesMatchingEndRegionComment` and `VerifyCodeFixDoesNotChangeUnrelatedEndRegionText` in `RH7302RegionsShouldStartWithAUpperCaseLetterAnalyzerTests` pin the two halves separately.
  - RH3202 on the two-method class: exactly the spec block (2 applications).
  - **New:** RH5601 on `→int x;→// a→b`: `    int x;    // a→b`, with the tab inside the comment kept (2 applications, one per diagnostic).
  - RH8107 single-line `<returns>`: the element and the space in front of it are removed.
  - RH5604: covered by the RH5604-003 and RH5604-005 tests.
  - RH4106 with `return 0;` under `#if NEVER`: fixed, and the excluded branch is unchanged.
- **SHARED-FIX-004:** RH2005 on `public string first;` gives `private string first;`. RH4106 on that result gives `private string _first;`. Assert the text after both fixes (FC-017).
- **SHARED-FIX-005:** RH4106 `_1st`: `no-fix-offered`. Assert that no action is registered (FC-009).

## Open questions to file

### RH7302 renames every occurrence of the region's name inside the `#endregion` text, also inside other words

Belongs to `SDD open questions: sdd/specs/shared/code-fixes.md`. That issue was thoenissen/Reihitsu#1066, which is closed, so the lead reopens it or opens a new one. It touches SHARED-FIX-003. RH7302 has no spec. It does not block this run, because the spec's RH7302 example uses a comment that is exactly the name, and a comment that does not contain it.

**Inputs** (RH7302), run with `scripts/apply-fix.sh`, LF and CRLF:

```csharp
class C
{
    #region a
    #endregion // a cat
}
```

```csharp
class C
{
    #region members
    #endregion // end of members
}
```

**Real output:**
- `#region A` / `#endregion // A cAt`.
- `#region Members` / `#endregion // end of Members`.

The fix replaces every ordinal occurrence of the old name in the `#endregion` directive's text.

**Why it looks wrong:** decision C makes the `#endregion` comment that repeats the region's name part of the rename. In the first input, `cat` becoming `cAt` changes a word that is not the name. SHARED-FIX-003 counts a changed word as content, so that reads as a violation. In the second input, the comment contains the name but is not only the name. Whether it "repeats the region's name" decides whether the fix may change it.

**Readings:**
- **(a) Only a comment that is exactly the name** (ignoring surrounding whitespace) is renamed. Output: `// a cat` and `// end of members` stay as they are.
- **(b) Every whole-word occurrence of the name** in the comment is renamed. Output: `// A cat` and `// end of Members`.
- **(c) Every substring occurrence**, as today. Output as today. That includes `cAt`, which changes a word that is not the name.

## Follow-up candidates, out of scope

As listed in `04-spec-author.md`, unchanged:
- the FC-004 relocation gap in what `AGENTS.md` says SHARED-FIX-003 covers;
- the RH5022 to RH5027 and SHARED-WS-001 contradiction (thoenissen/Reihitsu#1076);
- the stale non-goals in `RH2005.md` and `RH7103.md`;
- the RH7103 region and RH7109 layout follow-ups.

The RH7109 follow-up now has its scope from answer B. It covers the default struct and the sequential class shown in the table above, and not explicit or `Auto` layout.

`sdd/specs/shared/ordering-move-safety.md` owns the move guards of RH7109. When the RH7109 follow-up is implemented, that spec is the place for the layout guard. This run does not touch it.

## Decisions needed

_None._
