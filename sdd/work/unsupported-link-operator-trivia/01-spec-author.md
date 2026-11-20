Role: spec author
Commit: a169191
Verdict: change doc written

Inputs read: thoenissen/Reihitsu#894 and its one comment (the maintainer's triage of 2026-11-17, which overrides the body: declare the shape unsupported in this change, no new rule, delete the named special cases), `00-lead.md`, `sdd/specs/formatter/unsupported-syntax.md`, `sdd/specs/shared/fluent-chain-layout.md`, FMT-BREAK-001/019/020, FMT-INDENT-029, RH5201, RH2001-007, RH7101-007, `ChainLineBreakRewriter`, `MethodChainAlignmentContributor`, `FluentChain`, `FluentChainLink`. My prompt carried no intended fix or verdict to ignore.

Files written:

- `sdd/changes/2026-11/unsupported-link-operator-trivia.md` — change doc, `Status: Draft`, with *Visible output changes* (1: refused files, 2: withheld fixes of RH5201, RH5112, RH3202, RH2001, RH7101) and *Special cases* (Deleted / Checked and kept).
- `sdd/specs/formatter/unsupported-syntax.md` — FMT-UNSUP-015, inserted after FMT-UNSUP-014; `Uses` gains FMT-BREAK-019, SHARED-CHAIN-004, SHARED-CHAIN-006.
- `sdd/specs/requirement-ids.txt` — FMT-UNSUP-015 at its sorted position.
- `sdd/specs/formatter/line-breaks.md` — FMT-BREAK-019 gains one paragraph; `Uses` gains FMT-UNSUP-015.
- `sdd/specs/analyzer/RH2001.md`, `sdd/specs/analyzer/RH7101.md` — RH2001-007 and RH7101-007 name the new shape; `Uses` gains FMT-UNSUP-015.
- `AGENTS.md` — the `PrecheckPhase` row of the phase-ownership table names the ninth shape.

No rule page changes: no rule is added, what the rules report does not change, and a withheld fix is covered by `documentation/rules/general-notes.md`.

## Changed requirements

- FMT-UNSUP-015 — Comments and directives inside a link operator are unsupported — added. The shape: a comment, directive, or disabled text between two tokens of `?.`, `!.`, or `!?.`, in the trailing trivia of a token other than the `.` or the leading trivia of a token other than the first. The tokens are defined by syntax kinds (conditional access `?` followed by a member binding's `.`; null-forgiving `!` that is the expression of a simple member access, or of a conditional access whose `?` is followed by `.`). Boundary by cause: the gaps inside the operator only; the gap in front of the operator, the `.`-to-name gap, and the `?[` gaps (FMT-UNSUP-014) are outside. On any line. Reason: `a comment or directive inside a ?. or !. operator`; location: first comment, directive, or disabled text in that place. Overlap with FMT-UNSUP-004/010 gives their reason, as in FMT-UNSUP-011/014.
- FMT-BREAK-019 — Chains follow the shared chain layout — changed: states that the join inside a link operator never meets the shape (FMT-UNSUP-015), so FMT-BREAK-001 never refuses it, and every token of a link operator stands on the line of its first token.
- RH2001-007 — The fix is withheld for a comment or directive inside the property — changed: a comment between the tokens of a `?.`, `!.`, or `!?.` operator in the initializer's value withholds the fix (`= Source? /* three */ .Length;`). Without this, the requirement's "a comment elsewhere in the value does not withhold the fix" would be false.
- RH7101-007 — The fix is withheld for a comment inside the declaration — changed: names FMT-UNSUP-015 in its list of shapes.

How the statements were established:

- Today's output for every *before* in the change doc was produced on this branch's base: `scripts/format.sh` on fixtures for change 1, `scripts/apply-fix.sh RH5201|RH5112|RH3202|RH2001|RH7101` for change 2 (each "fixed (1 action)" under LF and CRLF).
- The three FMT-UNSUP-015 examples and the twelve inputs of the change doc compile (one scratch library, each example in its own namespace, Debug and Release, so both branches of `#if DEBUG` are compiled).
- `scripts/format.sh` on the three spec examples: none is refused by an existing shape today; the third (the other side) is formatted.
- Locations were counted by hand, 1-based: spec example 1 `// c1` at (7,20), spec example 2 `#if DEBUG` at (8,1); change doc (5,20), (8,1), (8,24), (7,20).
- `scripts/test.sh --project architecture`: 197 of 198 pass. The one failure is `EveryRequirementOfAnEnforcedSpecIsCovered` for FMT-UNSUP-015, which has no covering test yet; that test belongs to the test implementer. The ledger, `Uses`, and spec-structure tests pass.

## Examples for the tests

- FMT-UNSUP-015 — three examples: the refused links (`?.` with `//`, `!.` with `/* */` on one line, `!?.` with an own-line comment between `!` and `?`, `!?.` with a block comment between `?` and `.`), the conditional group inside `!.` (refused in every build), and the other side (comments in front of a link, inside an argument list, inside `?[ ]`, behind a statement: formatted). The conditions' bullets name single-line probes for each side, among them the `.`-to-name gap `s?. /* c */ Length` and the FMT-UNSUP-014 gaps `n? /* c */ [0]`, `x! /* c */ ?[0]`, which keep FMT-UNSUP-014's reason.
- FMT-UNSUP-015's node-level paragraph (FMT-UNSUP-003): an input that holds the `.` but not the `?` or `!`, such as a conditional access's branch, is not refused.
- RH2001-007 — the `= Source? /* three */ .Length;` case, and the change doc's RH2001 input: no fix offered, diagnostic stays.
- RH7101-007 — the change doc's RH7101 input: no fix offered.
- The change doc's RH5201, RH5112, and RH3202 inputs: no fix offered, diagnostic stays (SHARED-FIX-010).

## Output questions

The change doc's visible output changes all follow from the maintainer's triage answer on thoenissen/Reihitsu#894 (declare the shape). Two members of it are worth confirming when the lead batches the outputs, though the answer's wording ("between the tokens of … `?.`, `!.`, `!?.`") already covers the first:

**1 · Formatter / CLI — a block comment inside a link operator with no line break**

Input:
```csharp
#nullable enable

class C
{
    int? M(string? s)
    {
        var a = s? /* c1 */ .Length;

        return a;
    }
}
```

Before (`main`): the space behind the comment is removed — `var a = s? /* c1 */.Length;`.

After, A (recommended): the file is unchanged; the CLI reports `error: <file>(7,20): unsupported syntax: a comment or directive inside a ?. or !. operator. The file was left unchanged.`

After, B: refuse only a gap that holds a line break; this input keeps today's output `var a = s? /* c1 */.Length;`.

Why A: the boundary is a place in the syntax tree, as for FMT-UNSUP-008, -011, and -014, so no later phase needs a line-break test of its own to tell the two apart. Requirement: FMT-UNSUP-015 (*On any line*).

**2 · CLI — the reason text of the new shape**

Input: the `?.` example above with a `// note` comment, `var r = a? // note⏎.B();`.

After, A (recommended): `error: <file>(5,20): unsupported syntax: a comment or directive inside a ?. or !. operator. The file was left unchanged.`

After, B: `error: <file>(5,20): unsupported syntax: a comment or directive between the tokens of a member access operator. The file was left unchanged.`

Why A: it names the operators users write, in the form of the other reasons; `!?.` combines the two.

Neither blocks writing tests beyond the reason string; the run can continue on A.

## Internal choices

- The new shape is a new requirement, FMT-UNSUP-015, rather than an extension of FMT-UNSUP-014: the gaps are different (inside a link operator, not in front of an attached part), the reason differs, and FMT-UNSUP-014 keeps the `?[` gaps unchanged. Alternative: widen FMT-UNSUP-014 and its reason.
- `?[`, which the issue lists among link operators, is left to FMT-UNSUP-014 (SHARED-CHAIN-001 makes it an attached part); recorded under *Decisions* in the change doc. Alternative: state it again in FMT-UNSUP-015.
- The shared chain spec (SHARED-CHAIN-006) is left unchanged: it states the analyzer–formatter policy, and the refusal is formatter-only (FMT-BREAK-019 carries it). Alternative: a sentence in SHARED-CHAIN-006.
- RH5201-003 (a line break that trivia keeps inside an operator does not count as a split) is left unchanged: analyzers do not consult the precheck, and the unsupported-syntax spec's non-goal excludes analyzer reporting. Its `HasInnerLineBreak(isJoinableOnly: true)` is listed under *Checked and kept*.
- The *Special cases* name `CloseGap` (today's name; the maintainer wrote `TryCloseGap`) and record that only its use for the gaps inside an operator becomes unnecessary: its check stays for the `.`-to-name gap and the gap in front of the first link. `MethodChainAlignmentContributor.AlignRestOfRun` is deleted whole.

## Follow-up candidates

**A comment between a link's `.` and its member name** — outside #894, which is about the operator's own tokens. Reached by whole-document formatting and the CLI.

Input:
```csharp
class C
{
    object M(dynamic a)
    {
        var r = a.B(). // c
            C();

        return r;
    }
}
```

Real output (`main`): the member name moves to the statement's column.

```csharp
        var r = a.B(). // c
        C();
```

Why it looks wrong: `C();` at the statement's column reads as a new statement; no requirement places a member name that a comment keeps below its dot (FMT-BREAK-020 rejoins a split name only when nothing blocks it). Readings: A — declare the shape unsupported (a comment, directive, or disabled text between a link's `.` and its name), refusing the file; B — align the name under its dot, which needs a column rule of its own in the indentation phase. Declaring is an option here (`AGENTS.md`, *Special cases do not grow the source*), so this is escalation 10, raised by this role. Recommendation: A, in a change of its own; it would delete the dot-to-name use of `CloseGap`'s check, though `CloseGap` keeps the check for the gap in front of the first link. Requirement IDs: FMT-BREAK-020, FMT-BREAK-001.
