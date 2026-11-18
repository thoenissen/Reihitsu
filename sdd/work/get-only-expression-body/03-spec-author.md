Role: spec author
Commit: 16d2eb1
Verdict: #849: spec written; #850: spec written

My prompt listed the maintainer's answers and the lead's decisions to carry out. It carried no intended fix or hoped-for verdict beyond those recorded answers. I worked from `00-lead.md`, `01-spec-author.md`, and `02-skeptic.md`. The change doc is `Status: Accepted`, as the lead instructed. One case under *Output questions* was not part of batch 1, and the lead should confirm it.

## Changed requirements (both issues; the rules share SHARED-GETONLY)

- SHARED-GETONLY-001 — A get-only member the formatter converts is reported — clarified: the *Why* no longer claims that formatting clears every diagnostic.
- SHARED-GETONLY-003 — A member whose comment the formatter keeps is reported without a fix — changed (Q2 B). It now covers comments only: comments in the accessor-list seams, a comment behind both the `;` and the `}`, and comments in the FMT-STRUCT-012 getter-block seams.
- SHARED-GETONLY-005 — The fix writes the formatter's expression body — changed (decision 4). The fix takes the converted member from the formatter's conversion. The gap in front of `=>` is the one the formatter writes, not one the fix writes.
- SHARED-GETONLY-006 — No fix for a getter the formatter refuses — changed (decision 3). The fix asks `FindUnsupportedSyntax` about the getter as written, with SHARED-FIX-003 as the reason, and SHARED-FIX-010 still applies to the member the fix builds. A directive example was added (Q3 A).
- SHARED-GETONLY-007 — A member whose directive the formatter keeps is not reported — added. Directives were split out of the old SHARED-GETONLY-003. The ID is registered in `requirement-ids.txt`.
- RH3209-001, RH3210-001 — clarified: they now refer to SHARED-GETONLY-002/-007 as the not-reported members and to SHARED-GETONLY-003/-006 as the members reported without a fix. The intro and `Uses` were updated.
- Code owners (Q6 A): SHARED-GETONLY is now owned by `GetOnlyMemberExpressionBodyRewriteUtilities` in `Reihitsu.Core`, in both the spec header and the `sdd/specs/README.md` row. The spec's intro says that the formatter's get-only transform, both analyzers, and both fixes use it. FMT-STRUCT code owners are unchanged, following the `ExpressionBodyRewriteUtilities` precedent.

Change doc `sdd/changes/2026-11/get-only-expression-body-rules.md`:
- Visible change 4 now reads "reported, no fix" and gains the getter-block comment `E`.
- Visible change 5 shows a directive input (`get` / `#if` / `=> 1;` / `#else` / `=> 2;` / `#endif`).
- New visible change 6: a directive the formatter keeps the member for is not reported.
- New visible change 7: the fix keeps `public  int  A` as written.
- The rule pages are now visible change 8.
- The Decisions and Alternatives sections were rewritten for Q2, Q5, Q6 and decisions 3 and 4.

Rule pages `documentation/rules/RH3209.md` and `RH3210.md` are unchanged. What they describe is still accurate. The fix is withheld only for comment and directive corner cases, and `general-notes.md` already covers those.

## Examples for the tests

SHARED-GETONLY-001 to SHARED-GETONLY-007, RH3209-001, RH3210-001. The new or changed examples are in SHARED-GETONLY-003, -006, and -007.

## Output questions

**1 · RH3209 · Get-only properties should use an expression body — directive the formatter keeps the accessor list for** (RH3210 alike)

Batch 1 Q2 decided comments only. Directives were in the draft's SHARED-GETONLY-003 as not reported, but no visible change showed them. The spec is now written on A (SHARED-GETONLY-007, visible change 6). The test implementer can write every other test meanwhile.

Input:
```csharp
class C
{
    private int _value;

    public int A
    {
        get => _value;
#if WRITABLE
        set => _value = value;
#endif
    }

    public int B
    {
#if DEBUG
        get => 1;
#else
        get => 2;
#endif
    }
}
```

Before (`main`): not reported. The formatter leaves both members as written (checked with `scripts/format.sh`).

After, A (recommended): not reported.

After, B: RH3209 on the accessor list of A and B, with no code fix offered.

Why A: with `WRITABLE` defined, `A` has a setter, so no expression-bodied form compiles in every build. The expression-bodied member rules leave directives in the converted span alone for the same reason (SHARED-BODY-001).

## Internal choices

- The Core type is named `GetOnlyMemberExpressionBodyRewriteUtilities`, after `ExpressionBodyRewriteUtilities`. The implementer must use this name, or the spec header and the index change with it.
- Directives are a requirement of their own (SHARED-GETONLY-007) rather than a sentence in SHARED-GETONLY-003, because the two outcomes differ. Alternative: one requirement with two outcomes.
- The boundary of SHARED-GETONLY-007 is stated by position: any directive or disabled text from the trailing trivia of the token in front of `{` through `}`. Two exceptions apply: the two gaps around the getter's `=>` (SHARED-GETONLY-006 reports those), and a conditional group wholly between the first and last token of the getter's expression (FMT-STRUCT-013 converts it). A member that holds both such a directive and a comment is not reported.
- The SHARED-GETONLY-006 query is scoped to the getter, "from `get` through its semicolon or block", as decision 3 asks.

## Follow-up candidates

_None_ beyond the one already in `00-lead.md`.

## Checks

- Ran with `scripts/format.sh`. The directive members `A`, `B`, and `D` of SHARED-GETONLY-007, together with the comment members of SHARED-GETONLY-003, are left unchanged apart from the final newline. The `#if` inside the expression converts. `P` and `Q` with one trailing comment convert to `=> _value; // a` / `// b`. The directive between `get` and `=>` is refused. `public  int  A { get { return _value; } }` formats to `public int A => _value;`, which is the line visible change 7 quotes for the document.
- Every new example compiles: `dotnet build` of a scratch `net11.0` library at `LangVersion` 14 (the fixtures are under the scratchpad `sa3/`) gave 0 errors.
- `scripts/test.sh --project architecture`: 195 passed, 3 failed. The three failures are the same expected ones as in `01-spec-author.md`: `AnalyzerSpecHeaderMatchesItsRule`, `EveryCodeOwnerNamesAnExistingType` (now also the Core type), and `EveryRequirementOfAnEnforcedSpecIsCovered` (now also SHARED-GETONLY-007). They clear once the types and tests exist.
