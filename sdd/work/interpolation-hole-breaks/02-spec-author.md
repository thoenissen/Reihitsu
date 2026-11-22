Role: spec author
Commit: ce538c1
Verdict: spec written

The prompt carried no intended fix, suspected cause, or hoped-for verdict; nothing was ignored. Clarification only, per `00-lead.md` Decisions taken rows 1–3; no change doc (C# 11+ output unchanged, C# 10 output brought back to FMT-PIPE-003).

## Changed requirements

- FMT-BREAK-044 — No line break is written into a non-verbatim interpolation before C# 11 — added (clarification) in `sdd/specs/formatter/line-breaks.md`; ID added to `sdd/specs/requirement-ids.txt`; `FMT-PIPE-008` added to the spec's `Uses` (FMT-PIPE-003 was already there). No code owner added: the existing line-break code owners cover it; the Core predicate of decision 4 is a general-purpose utility unless the implementer's type turns out to be specified only here (then the lead decides whether it becomes a code owner).

Qualifiers stated as explicit conditions with both sides in one example (FC-002): *not verbatim* (`a` vs `b`), *at any depth* (`c` verbatim-in-regular, `d` regular-in-verbatim vs `e` delegate that merely contains a `$"` string), *C# 10 or earlier* (C# 10 vs C# 11 output). Precedence over FMT-BREAK-002/006/021 is stated; every other phase's requirements still apply inside the hole (decision 2). A line break already in such a hole (input that does not compile) is explicitly not the requirement's subject.

Example checked: the input run through `scripts/format.sh --lang-version 11` and `--lang-version 10` on `main` gives exactly the spec's C# 11 output (identical today for both versions). The C# 10 output differs from today only on lines `a`, `c`, `d`, which keep the input's one-line form; `b` and `e` match today's output. The `a`/`c`/`d` forms are the skeptic's c01/c16/c17 inputs, which compile under C# 10.

`scripts/test.sh --project architecture`: 197 passed, 1 failed — `EveryRequirementOfAnEnforcedSpecIsCovered`: FMT-BREAK-044 has no covering test yet. The test implementer's tagged tests resolve it in this run.

## Examples for the tests

- FMT-BREAK-044 — the example (C# 10 and C# 11 outputs, exact), plus the skeptic's class matrix (01-skeptic.md, *Inputs the test implementer must cover*), all tagged FMT-BREAK-044.

## Output questions

_None._

## Internal choices

1. New requirement FMT-BREAK-044 in the line-break spec rather than an example under FMT-PIPE-008 (alternative): the FMT-BREAK requirements are the ones it overrides, and the tests need a line-break ID to tag.
2. The requirement covers only the line-break phase (decision 3). Whether `SwitchCaseBracePhase` writes a line break into a one-line switch section inside such a hole (skeptic c28) is not specified here; if the c28 test shows it does, it is a bug against FMT-PIPE-003 and the lead decides whether FMT-SWITCH needs the same clarification.

## Follow-up candidates

_None_ beyond those already in `00-lead.md`.
