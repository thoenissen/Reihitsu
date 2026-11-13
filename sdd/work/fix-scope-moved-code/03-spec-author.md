Role: spec author
Commit: 633ef35
Verdict: #969, #1335, #1604: change doc written (`sdd/changes/2026-11/moved-code-keeps-its-lines.md`, Draft; the lane must widen to behavior change), with one output question (Q7, one-line accessor lists) that blocks only the RH7107/RH7108 one-line cases; #1263: spec written (clarification); #1151: spec written (clarification)

How the statements below were established: `00-lead.md` (batch 1 answers, decisions 2–9), `01-spec-author.md`, `02-skeptic.md` read; the cited specs read. "Before" outputs not already measured in 01/02 were measured with `scripts/apply-fix.sh` (RH7004, RH7107, RH7109) on fixtures in the scratchpad at 633ef35 (no code change since 82321f3), LF and CRLF agreeing. `scripts/test.sh --project architecture --filter "FullyQualifiedName~Sdd"`: 50 passed, 1 failed — `EveryRequirementOfAnEnforcedSpecIsCovered` for the three new IDs (RH7004-007, SHARED-ORDER-009, SHARED-ORDER-010), which the test implementer's tagged tests close.

Ignored input: none in my prompt. The issues' prescribed shared column-delta helper (#1604) stays dropped (decision 3); the change doc lists it under *Alternatives* as not a behavior.

Change doc: required. Q1 A, Q2 A, Q4 A, the whole-line rule, Q3 C and decisions 4 and 5 give output that is neither today's behavior nor decided by the previous text of SHARED-FIX-007 ("only the indentation of its lines follows its new nesting"), and they add refusals: requirements added and changed with new behavior (sdd/README.md, *When a change doc is required*). One doc covers #969, #1335 and #1604, which share the cause. #1263 and #1151 need none (see *Internal choices*).

## thoenissen/Reihitsu#969

### Changed requirements

- SHARED-FIX-007 — *Moved code* bullet — changed: for a member or accessor moved within its list, every line moves by the shift of its first token (from its column to its slot's column; none when both are indented alike), continuation lines included; a comment line in column 0 and a non-region directive line keep their column, `#region`/`#endregion` lines move. Examples added (RH7109, aligned and misindented).
- SHARED-ORDER-009 — A move moves whole lines — added (maintainer's addition to Q1).
- SHARED-ORDER-010 — A shifted declaration holds no line that cannot move — added (rare-class refusal, see *Internal choices*).
- RH7102-006, RH7103-007, RH7109-006 — changed: list SHARED-ORDER-009/010 among the refusals; `Uses` updated.

### Examples for the tests

SHARED-FIX-007 (RH7109 examples), SHARED-ORDER-009 (all four), SHARED-ORDER-010 (both), plus the unchanged SHARED-FIX-006/SHARED-ORDER-002 examples named in 01.

### Output questions

_None._ (Q7 below concerns the accessor moves of #1335.)

### Internal choices

- The column rule is written in SHARED-FIX-007 for member and accessor moves and RH7004 only, not for region blocks (RH7309) or brace fixes, which the bullet also covers: the maintainer answered for these fixes, and a general rule would decide output of fixes outside the run. Alternative: one rule for all moved code — rejected.

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1335

### Changed requirements

- SHARED-FIX-007 — *Moved code* — changed as above; for RH7004 the shift is one indentation level (Q2 A), a column-0 comment keeps its column (Q4 A), non-region directives keep their column and region directives move (decision 5). Examples added (continuation; `#pragma`, `#region`, column-0 comment).
- RH7004-007 — The fix is withheld for moved lines that cannot move — added: string literal of any kind or comment spanning lines (Q3 C), disabled text (decision 4), and a statement on the declaration's line (my rare-class refusal). `Uses` gains SHARED-FIX-007.
- SHARED-ORDER-009/010 apply to RH7107-005 and RH7108-005 as well — changed; `Uses` updated. SHARED-ORDER-009 is stated for accessor lists that span lines; the one-line list is Q7.

### Examples for the tests

SHARED-FIX-007 (both RH7004 examples), RH7004-007 (all five), SHARED-ORDER-009 (RH7107 example).

### Output questions

**Q7 · RH7107 · Property accessors must follow order — accessor list on one line** (the same for RH7108 · Event accessors must follow order)

The maintainer's answer to Q1 says a move must always move whole lines and lists RH7107/RH7108. Read literally, that withholds the fix for every accessor list written on one line. That is the RH7107 spec's own lead example (RH7107-004, `{ init; get; }` → `{ get; init; }`, tested by `FixMovesGetInFrontOfInit`) and the shape RH5408 asks for simple auto-properties, so the answer may not have meant it. Until it is answered, SHARED-ORDER-009 covers accessor lists that span lines only, and RH7107-004 is unchanged.

Input:
```csharp
class C
{
    private int _b;

    public int A { init; get; }

    public int B { set => _b = value; get => _b; }
}
```

Before (`main`, measured):
```csharp
class C
{
    private int _b;

    public int A { get; init; }

    public int B
    {
        get => _b;
        set => _b = value;
    }
}
```

After, A: no fix offered for `A` or `B`; the diagnostics stay.

After, B (recommended):
```csharp
class C
{
    private int _b;

    public int A { get; init; }

    public int B { get => _b; set => _b = value; }
}
```

Why B: when the whole list stands on one line, the accessors swap places within that line and no line break has to be decided, which is what the whole-line rule protects; a list that spans lines and holds a shared line is withheld under both options.

(For the lead: B adds "or the whole accessor list stands on one line" to SHARED-ORDER-009 and keeps RH7107-004 as is; A deletes SHARED-ORDER-009's scoping sentence and turns RH7107-004's first example into "no code fix is offered". The rest of the run does not depend on the answer.)

### Internal choices

- RH7004: a statement that starts on the declaration's line is withheld as a rare class (RH5103 reports it). Measured on `main`: the fix produces `}     }` on one line. Alternative: put the statement on its own line — it decides a line break the author did not write, against the whole-line principle of Q1. Listed in the change doc as the spec author's decision; the lead may strike it.
- The column-0 comment and directive rules are written once in SHARED-FIX-007, not in RH7004, so the member moves follow the same line rules when their shift is not zero.

### Follow-up candidates

_None_ beyond 00-lead.md (RH7004 fix for strings/comments spanning lines is already listed).

## thoenissen/Reihitsu#1263

### Changed requirements

- SHARED-FIX-007 — *Removed directive line* — clarified (Q5 A): the start and the end of a body are the lines behind an opening and in front of a closing brace of any construct — type, block, accessor list, switch statement, initializer; a single blank line behind a `case` label stays.
- RH7303-004 — clarified with an initializer example.

### Examples for the tests

RH7303-004 (four examples), SHARED-FIX-007 (RH7310 and RH7303 examples).

### Output questions

_None._

### Internal choices

- No change doc: "body" read as any pair of braces is the answer that fits the requirement's own reason (a blank line at a brace is one the removal leaves where none belongs), so this is a clarification of a bug fix. Alternative: a change doc for #1263 of its own — the lead decides; nothing visible is in question after Q5.

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1151

### Changed requirements

- RH7302-005 — clarified (Q6 A): the rename condition is now "the `#endregion` description is exactly the `#region` description, both read as SHARED-REGDESC-001 states", which ignores whitespace around either description; the `#endregion` keyword is never renamed (decision 8). Two examples added (trailing whitespace; `#region end` / `#region e`). `Uses` gains SHARED-REGDESC-001.

### Examples for the tests

RH7302-005 (all four examples). The trailing-whitespace example's first `#region` line ends with two spaces.

### Output questions

_None._

### Internal choices

- No change doc: SHARED-REGDESC-001 already defines a description without surrounding whitespace, and RH7301 compares descriptions that way; the clarification references it instead of the old condition "the `#region` line ends with its description". Alternative: a change doc — the lead decides.

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1604

### Changed requirements

Covered by #969 and #1335 above (SHARED-FIX-007, SHARED-ORDER-009/010, RH7004-007).

### Examples for the tests

As under #969 and #1335.

### Output questions

_None_ beyond Q7.

### Internal choices

- SHARED-ORDER-010 (shifted member or accessor with a multi-line string, a multi-line comment, or disabled text is withheld) is my decision for a rare class: a non-zero shift needs a declaration indented differently from its slot, which RH5204 reports, and it makes the member moves refuse the same line classes as RH7004-007 (Q3 C). Alternative: shift such lines anyway — changes literal values or comment text (SHARED-FIX-003).

### Follow-up candidates

_None._

## Whole run

- Files: `sdd/specs/shared/code-fixes.md`, `sdd/specs/shared/ordering-move-safety.md`, `sdd/specs/analyzer/RH7004.md`, `RH7102.md`, `RH7103.md`, `RH7107.md`, `RH7108.md`, `RH7109.md`, `RH7302.md`, `RH7303.md`, `sdd/specs/requirement-ids.txt` (RH7004-007, SHARED-ORDER-009, SHARED-ORDER-010), `sdd/changes/2026-11/moved-code-keeps-its-lines.md`.
- No rule page changed: RH7004, RH7107, RH7108, RH7302, RH7303 pages stay true; the new refusals are corner cases that `general-notes.md` covers.
- `sdd/specs/README.md` unchanged: no new code owner.
- Not in the change doc, because unchanged requirements demand them (bug fixes): no reformatting of untouched or moved code, no inserted blank lines, the fix offered in a type holding an unsupported shape elsewhere (decision 6).
