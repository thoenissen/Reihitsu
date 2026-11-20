Role: skeptic
Commit: f7344b6
Verdict: proceed

Inputs: thoenissen/Reihitsu#851 (label `agent-filed`), `00-lead.md`, `01-spec-author.md`, change doc `sdd/changes/2026-11/rh5419-indexer-arrow-single-line.md`, spec `sdd/specs/analyzer/RH5419.md`, pointer diffs in RH5401, FMT-BREAK-026, SHARED-GETONLY (commit 7f9df95). The prompt carried no intended fix or verdict.

All fixtures below lived in the session scratchpad (outside the repository) and were run with `scripts/format.sh <dir>` (CLI, whole-document path) or `scripts/apply-fix.sh RH3210 <dir>`; every `format.sh` result was run a second time and was stable.

## Premise

Real. The case is reached by every caller:

- Analyzer users: no rule reports an expression-bodied indexer whose arrow wraps. RH5401 registers `SyntaxKind.PropertyDeclaration` only (`src/Reihitsu.Analyzer/Rules/Layout/RH5401ExpressionStyleGetOnlyPropertiesShouldBeSingleLinedAnalyzer.cs`, `Initialize`).
- Whole-document formatting / CLI: the formatter already joins the arrow of an indexer (FMT-BREAK-026; dependency thoenissen/Reihitsu#848 is in). Observed, input:

```csharp
class C : I
{
    int I.this[int i]
        => i;

    public ref int this[long i]
        => ref _a[i];

    int this
        [short i]
        => i;

    int this[byte i]
        => Foo(
            i);

    int this[uint a, uint b
        ]
        => 1;
}

interface J
{
    int this[int i]
        => i;
}
```

Output (abridged to the indexers):

```csharp
    int I.this[int i] => i;
    public ref int this[long i] => ref _a[i];
    int this[short i] => i;
    int this[byte i] => Foo(i);
    int this[uint a, uint b] => 1;
    ...
    int this[int i] => i;          // interface J
```

- RH3210's fix (get-only indexer → expression body) is the main producer of these indexers. `scripts/apply-fix.sh RH3210` over getters with a block body, a multi-line call, a switch expression, a wrapped `get\n=> a`, and a wrapped parameter list produced, under LF and CRLF, `public int this[int i] => Foo(i,\n i);`, `public int this[long i] => i switch\n{...};`, `public int this[short a,\n short b] => a;`, `public int this[byte a] => a;` — every result has its `=>` and first expression token on the line of the `]`, so RH5419-001 never reports RH3210's output (no FC-013 conflict between the two fixes).

Change-doc examples 2, 3 and 5 were re-run through `format.sh`: outputs match the change doc exactly (example 2's second indexer reflows to `int this[uint a,\n         uint b] => 1;`; example 3's second indexer becomes `int this[int i] => i\n                   + 1;`; example 5's third indexer gets a blank line behind `[Obsolete]`, its fourth loses the blank line inside the brackets and is split). Both passes stable.

FC-014 check: the rule's concern is layout, and FMT-BREAK-026 owns it for indexers; every reported shape I constructed is cleared by the formatter except the ones it refuses (FMT-UNSUP-012/013), which SHARED-DIAG-010 lets the rule keep reporting. No permanent diagnostic for CLI users.

## Mechanism and candidates

_N/A_ (new rule). Dispatch: one syntax kind, `SyntaxKind.IndexerDeclaration` (`IndexerDeclarationSyntax`), with `ExpressionBody != null`, as RH5401 checks for a property. The token in front of `ExpressionBody.ArrowToken` is always `ParameterList.CloseBracketToken`, which matches the formatter's join target.

## Breaking inputs

Failure classes that reach the new analyzer and fix, with the candidate inputs the test implementer must cover (expected outputs are the formatter's, all observed above or below):

- FC-024 / FC-025 (member kinds): explicit interface implementation `int I.this[int i]\n    => i;`; interface member with a default body (`interface J`); `ref`-returning indexer `public ref int this[long i]\n    => ref _a[i];` (first expression token is `ref`); indexer in a `struct`. All joined by the formatter (above).
- RH5419-001's "the line of `this` does not count": `int this\n    [short i]\n    => i;` is reported (`=>` not on the `]` line); `int this\n    [short i] => i;` is not reported, although the formatter joins `[` onto `this` (P11 case, see *Follow-up candidates*).
- FC-027 (multi-line forms), fix withheld per RH5419-004 because the expression stays on several lines: switch expression `int this[char i] =>\n    i switch\n    {\n        _ => 1,\n    };` (formatter: `int this[char i] => i switch\n                    {...};`); multi-line raw string (FC-022) `int this[string s]\n    => """\n       a\n       """.Length;`. Counter-case where the fix IS offered: `int this[byte i]\n    => Foo(\n        i);` collapses to `int this[byte i] => Foo(i);`.
- FC-019 / FC-020 / FC-021 (trivia in the gaps the fix touches). Around the arrow: refused (FMT-UNSUP-012/013), covered by RH5419-004. Inside or in front of the parameter list — under reading A of Q1 the fix is offered for these and writes the formatter's output (observed, stable):

```csharp
// input
class C
{
    int this // c
        [int i]
        => i;

    int this /* c */ [long i]
        => 1;

    int this[short i // c
        ]
        => i;

    int this[byte a, /* c */
             byte b]
        => a;

    int this[
#if DEBUG
        long a
#else
        short a
#endif
        ]
        => 1;

    int this[
        // lead
        uint a,
        uint b]
        => 1;
}
```

```csharp
// formatter output
class C
{
    int this // c
    [int i] => i;

    int this /* c */ [long i] => 1;

    int this[short i // c
             ] => i;

    int this[byte a, /* c */
             byte b] => a;

    int this[
#if DEBUG
        long a
#else
             short a
#endif
             ] => 1;

    int this[
             // lead
             uint a,
             uint b] => 1;
}
```

  No comment, directive, or disabled text is dropped or moved across a token; the shapes are rare (RH5035 reports comments inside a parameter list). Under reading B of Q1 every one of them is withheld, since the indexer's own code spans several lines.
- FC-028 (detached `FormatNode` vs in-document formatting): RH5401's provider decides the offer on a detached `FormatNode` result and applies through `BlankLineCodeFixUtilities.FormatMemberOnLinesOfItsOwnAsync` (in document). For RH5419 under reading A the parameter list's continuation column differs between the two (detached: relative to column 0). Candidate: a wrapped parameter list in a nested type, observed in document as `class D { class E { int this[\n            uint a,\n            uint b]\n            => 1; } }` → `        int this[uint a,\n                 uint b] => 1;`.
- FC-043 / SHARED-FIX-010: the indexer sharing a line with a sibling member (`int A => 1; int this[int i]\n    => i;`), where `FormatMemberOnLinesOfItsOwnAsync` first breaks the shared line (SHARED-FIX-007): the node asked about and the node formatted must be the same.
- FC-007 / FC-008: Fix All over several indexers of one type (change-doc examples 1, 2), and convergence in one pass under LF and CRLF.
- FC-003 boundaries of RH5419-001: arrow on the `]` line with expression on the next line (`int this[long i] =>\n    1;`, reported), arrow on the next line with expression behind it (`int this[int i]\n    => i;`, reported), both on the `]` line with a wrapped continuation (not reported), and a wrapped parameter list with both on the `]` line (not reported).
- Attribute lists: the RH5401-004 cases with an indexer in the property's place (gap blank-line count, blank line / directive / disabled text / own-line comment between brackets), and the offered cases of RH5401-003 (`[Obsolete] int this[int i]\n    => i;`, `[Obsolete, CLSCompliant(false)]` split).

## Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| Analyzer (`Reihitsu.Analyzer/Rules/Layout`, new RH5419) | Must change (new rule) |
| Code fix + Fix All (new RH5419 provider, `BatchCodeFixTestsBase`) | Must change |
| Resource texts (analyzer title/message, code-fix title) | Must change |
| RH5401 analyzer and fix | Must stay (properties only; spec non-goal pointer only) |
| Formatter (FMT-BREAK-026 indexer join, precheck FMT-UNSUP-012/013) | Must stay — already does what the rule needs |
| RH3210 | Must stay — its output is never reported by RH5419 (verified) |
| `Reihitsu.Core` | Not involved |
| Rule page, package README row | Already written by the spec author; must stay as the spec |
| CLI | Not involved |

## Refuse option

- **No code fix at all** (diagnostic only). Saves one provider (RH5401's is ~200 lines, most of it the attribute-list guards). Cost against the request: the issue asks for a fix, and a fix that delegates to the formatter is cheap and can be delivered comprehensively; refusing is not warranted.
- **Reading B of Q1** (withhold the fix whenever the indexer's own code spans several lines, as RH5401) is the narrower refuse option for wrapped parameter lists. It costs the same source as A (the measured span starts at the first token instead of the `=>`); the withheld class (an indexer whose parameter list wraps and whose arrow also wraps) is rare. It is a visible output choice, so it stays the maintainer's (Q1).
- **Declaring a shape unsupported**: the shapes the fix refuses around the arrow are already declared (FMT-UNSUP-012, FMT-UNSUP-013). The only other refusal is RH5401-004's attribute-list guard (a blank line, a directive, disabled text, or an own-line comment between an attribute list's brackets, from its `[` through its `]`). Declaring the comment/directive/disabled-text part would delete nothing: the same guard must still refuse a blank line between the brackets, which is not a shape of the precheck's kind, so the guard survives; and it would refuse whole files holding `#if` around an attribute inside one list (e.g. a platform attribute), which people do write. Under `AGENTS.md` ("A shape that deletes nothing is not declared") I find no declaration option for this run.
- Simpler options: _None_ beyond the above.

## Output questions

Q1 from the spec author's handover stands, and I agree with recommendation A — but its Input/After shows only the case where A's fix leaves the parameter list as written. The visible difference between A and B also includes a parameter list the fix re-lays out, which the maintainer should see. Suggested addition to Q1 (observed with `format.sh`):

Input:

```csharp
class C
{
    int this[
        uint a,
        uint b]
        => 1;
}
```

Before (`main`): not reported.

After, A: reported; the fix is offered and gives

```csharp
class C
{
    int this[uint a,
             uint b] => 1;
}
```

After, B: reported; no fix offered; the code stays as written.

Why A (my view): the formatter already writes exactly this on a whole-document run, the diagnostic covers the whole declaration (RH5419-002), and B withholds the fix for every wrapped parameter list although the formatter clears the diagnostic there. Against A: no Reihitsu rule reports the parameter-list layout of an indexer (RH5107, RH5108, RH5109 list it as a non-goal), so under A the fix also changes lines the diagnostic is not about.

Also for the batch: the new rule's title and message ("Expression style get-only indexers should be single lined.", mirroring RH5401's resource text) are visible output; the title reads "single lined" although a wrapped parameter list and a wrapped expression continuation are compliant. RH5401's title has the same looseness and the issue proposes this title, so I recommend keeping it; it only needs to appear in the batch heading so the maintainer sees it.

## Internal choices

- Referencing RH5401-003/-004 "with the indexer in the property's place" (spec author's choice) vs a shared spec: I recommend the reference, as written. It keeps one statement of the rule in the spec. See *Architecture findings* for the matching risk in code.
- RH5419-004's offer condition reads the formatter's result (as RH5401's provider does with `FormatNode`), not a prediction of a phase decision — question 10 raises nothing for it.

## Issue goals

- "Next free RH54## ID, currently RH5418": already adjusted to RH5419 (lead decision 1). No visible output in question beyond the ID itself.
- "Mirror RH5401's exceptions and guards (directives or comments between the signature, arrow, and expression)": RH5401's analyzer has no exception — it reports these and only its fix is withheld. The spec mirrors that (reported, no fix, RH5419-004). Keep as specified; if the goal is read as "do not report", it would contradict SHARED-DIAG-010 (a refused shape is not one the formatter keeps).
- "Offered when the whole declaration fits on one line": a prescribed solution carried over from RH5401, where a property's signature never wraps. For an indexer it is Q1 — visible, so the maintainer's.
- "Depends on #848": satisfied on `main` (formatter joins indexer arrows, observed above). Nothing to do.
- "RH5401 stays properties-only": kept.

## Architecture findings

_None_ in the code today. One structural risk the run's change can create (FC-015): RH5401-004's attribute-list decision lives in two private methods of `RH5401ExpressionStyleGetOnlyPropertiesShouldBeSingleLinedCodeFixProvider` (`HoldsBlankLineDirectiveDisabledTextOrOwnLineCommentBetweenBrackets`, `KeepsBlankLinesBehindAttributeLists`, the latter typed on `PropertyDeclarationSyntax` but reading only its attribute lists). RH5419-004 states that the indexer's fix follows the same rule by reference; a private copy in the RH5419 provider would put one rule in two owners. The auditor should check for it.

## Follow-up candidates

- P11 case, not this rule's construct: `int this\n    [short i] => i;` and `int this // c\n    [int i] => i;` — the formatter moves the indexer's `[` (with its parameter list) onto / relative to the `this` line, and no rule reports it. RH5108 (parameter list follows declaration) lists the bracketed parameter list of an indexer as a non-goal, so this is decided as unreported, not a gap. I recommend no follow-up: the shape is rare.
- Formatter output for a comment between `this` and the `[` (`int this // c\n    [int i]\n    => i;` → `int this // c\n    [int i] => i;`, the `[` lands in the member's own column) and for a `#if` group inside the bracketed parameter list (the active branch re-indented to column 13, the disabled branch left at column 8). Both are non-destructive and rare (RH5035 reports the comment cases); recorded only because under reading A the RH5419 fix would write them. No follow-up recommended.
