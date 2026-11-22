Role: spec author
Commit: eecea05
Verdict: spec written

Decision 9 in `00-lead.md`, clarification only; no change doc (C# 11+ output unchanged; the C# 10 output is the one FMT-BREAK-044 and FMT-PIPE-003 already demand). The prompt carried no intended fix, suspected cause, or hoped-for verdict; nothing was ignored. No new requirement ID: FMT-BREAK-044 already says "the formatter MUST NOT write a line break", so the blank-line phase's part needed naming, not a new rule.

## Changed requirements

- FMT-BREAK-044 — No line break is written into a non-verbatim interpolation before C# 11 — clarified in `sdd/specs/formatter/line-breaks.md`: it now names the blank-line phase. That phase inserts no blank line inside such an interpolation, because a blank line is written with a line break. This takes precedence over every blank-line requirement that would insert one, such as FMT-BLANK-001 in front of a `return`, so `F(() => { var a = 1; return a; })` keeps its line. FMT-BLANK-001 was already under the spec's `Uses`, so the header is unchanged.
- `sdd/specs/formatter/blank-lines.md`, non-goal *Line breaks that are not blank lines*: it gains a pointer saying the phase inserts no blank line where FMT-BREAK-044 forbids a line break, and that FMT-BREAK-044 states this phase's part and takes precedence. This is a pointer in a non-goal, so under `sdd/README.md` it is not a use, and `Uses` is unchanged. No requirement of the blank-line spec changes.

No code owner was added. `SingleLineInterpolationUtilities` is the shared loop both phases call. It implements FMT-BREAK-044, but it is called from two specs' phases, so I left it out of the headers (see *Internal choices*).

Not run: `scripts/test.sh --project architecture`, because it builds. No requirement ID reference, `Uses` row, or code owner was added or removed, so the spec-structure tests are unaffected.

## Examples for the tests

- FMT-BREAK-044: the blank-line part is covered by the already tagged test `NonVerbatimInterpolationLineBreakTests.LambdaBlockStatementsInInterpolationKeepTheirLineInCSharp10`, whose input `{ var a = 1; return a; }` is the sentence's example. No new tag and no new test are needed.

## Output questions

_None._

## Internal choices

1. The blank-line phase's part is stated in FMT-BREAK-044, and `blank-lines.md` points to it from its line-break non-goal. Alternative: a new FMT-BLANK requirement, like FMT-SWITCH-017. I rejected it because that would restate FMT-BREAK-044's position and conditions, and the existing FMT-BREAK-044 test already covers it.
2. `SingleLineInterpolationUtilities` is not made a code owner of `line-breaks.md`. Alternative: list it there, since FMT-BREAK-044 specifies its behavior (`sdd/README.md`, *Code owners*). That reading supports the alternative; the lead decides. If it is listed, it also goes in the `sdd/specs/README.md` row.
3. The `LambdaAlignmentContributor` guard from decision 9 is not specified. FMT-INDENT-028 aligns the opening brace's line, and per the implementer no output changed, so no spec text is needed.

## Follow-up candidates

_None_ beyond those already in `00-lead.md`.
