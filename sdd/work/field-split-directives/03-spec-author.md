Role: spec author
Commit: f355de9
Verdict: #1728: spec written (amended at the leading-trivia boundary); #1586: change doc amended, output question open

Follow-up to 01-spec-author.md, on the lead's decision 2 (00-lead.md): disabled text in the leading trivia of a field declaration's first token keeps the declaration whole, and the RH7101 fix is withheld for it — `main`'s output for that class, refused locally as a rare class. The change doc `sdd/changes/2026-11/field-declaration-directives.md` stays `Status: Draft` until the maintainer answers question 1 below. The prompt carried no intended fix, suspected cause, or hoped-for verdict beyond the lead's recorded decision.

## Changed requirements

- FMT-UNSUP-010 — changed: the leading-trivia condition stays (disabled text there is not refused) and now says the split decides by FMT-STRUCT-022 whether the declaration is split; the `Why` says disabled text there can hold an attribute or modifier of another build and is left whole rather than refused; the other-side example's lead-in names the kept-whole case. No example output changed (none of its inputs holds disabled text in a leading trivia).
- FMT-STRUCT-022 — changed: the directive paragraph covers directives only (a region, a group around the declaration, the `#endif` of a group whose code is all active); new normative paragraph: the formatter MUST NOT split a field declaration whose leading trivia holds disabled text, whatever it holds; the `Why` gives the cause (FMT-PIPE-003, FMT-STRUCT-024) and the cheap kind check; new example with both sides — inactive attribute group and inactive modifier group (kept whole), and an active `#if D` group in front (split).
- FMT-STRUCT-023 — retired status text: a directive in the leading trivia splits, disabled text there keeps the declaration whole.
- FMT-PIPE-031 — changed: true also requires no disabled text in the leading trivia (FMT-STRUCT-022); the `Why` says this is no refused shape, so the answer carries it; new example (region: true; inactive attribute group: false; `FindUnsupportedSyntax` null for both). `Uses` gains FMT-STRUCT-022.
- RH7101-005 — retitled "The fix is withheld for a directive inside the declaration or disabled text in front of it" — changed: also MUST NOT be offered for disabled text in the leading trivia (through FMT-PIPE-031); a directive there without disabled text does not withhold the fix; new example (inactive `DebuggerBrowsable` group: reported, no fix).
- Change doc: Affected requirements, *What*, *Why* (new paragraph), *Visible output changes* (disabled text in front changes nothing — `main` outputs quoted), new decision "Disabled text in the leading trivia keeps the declaration whole", *Special cases* (`CarriesDirective` deleted but for the disabled-text check; `SplitsFieldDeclaration` narrowed; the check listed under *Checked and kept*), alternatives (refuse the leading trivia too — now deletes the check; declare disabled text there unsupported; split it as first drafted; the `#endif`-order variant).
- `AGENTS.md` unchanged: the precheck row's fourth shape is still exact; the structural row names no guard for the narrowed check because none exists yet — the implementer adds its name there if it becomes a named member.

## Examples for the tests

- FMT-STRUCT-022: the new disabled-text example — `_a, _b` (inactive attribute group, L5) and `_c, _d` (inactive modifier group, L6) stay whole; `_m, _n` behind an active `#if D` group (`#define D` at the top) is split. Add the skeptic's L9 (`#if DEBUG` / `#elif TRACE` attributes), L3 (inactive group holding a whole member) and L4 (declaration in the active `#else`) on the kept-whole side; R, I, L7 on the split side.
- FMT-PIPE-031: the new example (true for `_a, _b`, false for `_c, _d`).
- RH7101-005: the new example (reported, no fix); the earlier examples stand.
- FMT-UNSUP-010: inputs with disabled text in the leading trivia are not refused (FC-003 other side).
- Checked: the new FMT-STRUCT-022 input through `scripts/format.sh` on the current code — every kept-whole part matches today's output (today keeps all three whole; only the final newline goes); the `_m, _n` split is the change not yet built. The RH7101-005 / question input through `scripts/format.sh` (unchanged but the final newline) and `scripts/apply-fix.sh RH7101` (`no-fix-offered`, LF and CRLF).
- Existing tests: unchanged from 01 — `FieldDeclarationSplitTests.DirectiveBeforeFirstDeclaratorLeavesTheFieldIntact`, `FieldDeclarationSplitTests.DisabledTextBeforeFirstDeclaratorLeavesTheFieldIntact`, `DirectivePreservationTests.FieldDeclarationSplitKeepsDirectivesInDeclaration` invert to refusals; `RH7101DoNotCombineFieldsAnalyzerTests.RegionDirectiveInTheDeclarationWithholdsTheFix` and `VerifyFixIsNotOfferedWhenFieldCarriesDirective` (active `#if DEBUG` group, DEBUG defined, no disabled text) invert to "fix offered". `scripts/test.sh --project architecture`: 151 passed, 2 failed — the same expected two (the three retired FMT-STRUCT-023 tags; FMT-UNSUP-010 uncovered).

## Output questions

Replaces question 1 of 01-spec-author.md. Its first input and options A, B, C are as drafted; C's deletion line is corrected; option D (escalation 10) is added, with a second input because D differs from A only on disabled text. A and B give the same output for input 2.

**1 · RH7101 · Do not combine fields — a directive or inactive code in front of a combined field**

Input 1:
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

Before (`main`): the formatter keeps both declarations combined; RH7101 is reported on both, no fix offered.

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

After, D: as A.

Input 2 (built without `DEBUG` defined, as the CLI always builds):
```csharp
using System.Diagnostics;

class C
{
#if DEBUG
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
#endif
    private int _a, _b;
}
```

Before (`main`): the formatter keeps the declaration combined; RH7101 is reported, no fix offered.

After, A (recommended): as before — combined, RH7101 reported without a fix.

After, B: as before — combined, RH7101 reported without a fix.

After, C: the input unchanged.
The CLI reports: `error: <file>(5,1): unsupported syntax: a directive inside a field declaration with several declarators. The file was left unchanged.`
No code fix that formats this type or field is offered here (RH7101, RH5110, RH5404, RH5113, RH5111, among others).
Declared in this work.

After, D: the input unchanged.
The CLI reports: `error: <file>(6,1): unsupported syntax: a directive inside a field declaration with several declarators. The file was left unchanged.`
No code fix that formats this type or field is offered here (RH7101, RH5110, RH5404, RH5113, RH5111, among others).
Declared in this work.

Why A: a region or a group around the field stays in front of its first field, so splitting moves nothing, while inactive code in front may be an attribute of these very fields in another build, so that rare case stays combined rather than refusing the file.
C would delete the field split's check for inactive code in front of a combined field, and refuses every file where a region or a conditional group directly precedes a combined field; D would delete that same check, and refuses every file where inactive conditional code (for the CLI, every `#if DEBUG` group) directly precedes a combined field.

Notes for the lead (not part of the question): the outputs of A, B and the Befores are runs (`scripts/format.sh`, `scripts/apply-fix.sh RH7101` on the current code, which matches `main`; A on input 1 from the spec author's guard-removed run in 01). The C and D report lines are readings of the spec, not runs: C's location is the first directive in the leading trivia (`#region` at 3,5; `#if DEBUG` at 5,1); D's is the first disabled text, which starts the line after `#if DEBUG` (6,1). Both reuse FMT-UNSUP-010's reason text; a separate reason for D is the lead's to settle if D is chosen. Spec consequence: A — as now written. B — FMT-STRUCT-022's directive paragraph and example go, the split check covers every directive in the leading trivia, RH7101-005 withholds for it. C — FMT-UNSUP-010 drops its leading-trivia exception and other-side example, FMT-STRUCT-022's directive and disabled-text paragraphs go, FMT-PIPE-031 loses the narrowed condition. D — FMT-UNSUP-010 (or a new FMT-UNSUP requirement) adds disabled text in the leading trivia, FMT-STRUCT-022's disabled-text paragraph and FMT-PIPE-031's narrowed condition go, RH7101-005 withholds through the precheck.

## Internal choices

- The kept-whole condition lives in FMT-STRUCT-022, next to the leading-trivia rule (skeptic's recommendation), rather than reinstating FMT-STRUCT-023 narrowed; FMT-STRUCT-023 stays retired.
- The check is by trivia kind (any disabled text in the first token's leading trivia), not by directive order; so L3 and L4 stay whole. Alternative: keep whole only when an `#endif` follows the disabled text — rejected in the change doc.
- RH7101-005 changed in place again (retitled) rather than a new ID: it still states when the fix is withheld for preprocessor trivia.
- Rule page `documentation/rules/RH7101.md` unchanged: a withheld fix in a corner case is covered by `general-notes.md`.

## Follow-up candidates

Unchanged from 01-spec-author.md (event field / local declarator behind an `#endif` loses its continuation indent).
