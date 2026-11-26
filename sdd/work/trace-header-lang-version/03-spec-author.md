Role: spec author
Commit: 9ebf8e4
Verdict: #1924: change doc written; #1949: change doc written

Follow-up amendment on `00-lead.md` *Decisions taken* rows 5 to 7. Read: `squad/roles/spec-author.md`, `00-lead.md`, `01-spec-author.md`, `02-skeptic.md`, both change docs, `scripts/README.md`. My prompt carried no intended fix, suspected cause, or hoped-for verdict beyond the lead's recorded decisions.

## thoenissen/Reihitsu#1949 — `sdd/changes/2026-11/tooling-lang-version.md`

- Decision 6: the closing paragraph of *What* no longer says the tools "behave as today" with Roslyn's default options and `LanguageVersion.Latest`. It now states that both tools always parse with the resolved version — the value's resolution, or the newest supported version without the option — with no second path, and that today's output is unchanged because Roslyn 5.12.0 maps both old options to C# 15. New decision *The default is the resolved version, not Roslyn's*; rejected alternative added.
- Decision 5: new decision *One owner of the newest supported version* — both tools resolve and decide the warning through the formatter's own resolution (FMT-PIPE-008); the fixture runner reaches it through the formatter's internal visibility to the tooling assembly, as the CLI does; no copy, no public widening. Two rejected alternatives added (copy; make public). The doc names no type, per the change-doc rule against an implementation plan; the type and the `AssemblyInfo.cs` line are in `02-skeptic.md`, internal choice 1 A.
- Status: Accepted.

## thoenissen/Reihitsu#1924 — `sdd/changes/2026-11/trace-diff-header.md`

- Decision 7 was stated only in part (the doc took the "change the header" outcome but said nothing of the issue's wrong premise about the existing test). New decision *The issue's goals this change does not carry out*: the "trace spec" outcome is dropped (no trace spec); the constraint about the existing test is dropped as resting on a wrong premise (the test passes a full path outside the current directory and gets updated); the "a test covers a full path" goal is kept.
- Status: Accepted.

## Changed requirements

None. No requirement ID, `Uses` row, or code owner touched, so `scripts/test.sh --project architecture` was not run.

## Examples for the tests

Unchanged from `01-spec-author.md` and `02-skeptic.md`. Added for #1949: a test that the default run parses as the newest supported version is not distinguishable from today on Roslyn 5.12.0; none is required for it.

## Output questions

_None._

## Internal choices

- Decision 5 is written as a behavior of the tools (one owner, internal visibility), without naming `LanguageVersionResolver` or `AssemblyInfo.cs`. Alternative: name them in the change doc.
- `scripts/README.md` already states the default as "the newest C# version the formatter supports" for both tools; it needed no edit.

## Follow-up candidates

_None._
