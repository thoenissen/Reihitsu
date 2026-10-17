Role: skeptic
Commit: 2f9d32f (spec commit 37982a6; production code unchanged since 33a29f7)
Verdict: proceed narrower

The reworded normative sentences of RH5005-002 and RH5006-002 match the code and leave the `await` form out. The new `Why:` of both requirements does not: it points at `await`. That is the one item left. Everything else from Finding 2 of `02-skeptic.md` is resolved.

The lead's message carried evidence: what thoenissen/Reihitsu#946 states, and that #1000 and #1001 were filed. It carried no intended fix or hoped-for verdict. I did not read #946 myself (no GitHub in this run). I rely on the lead's summary that it states only the expected output, one blank line in front of `await`, and does not decide the token.

## Premise

### The normative sentences match the code

The two sentences now read "When the `using` statement starts with the `using` keyword, the analyzer MUST report the diagnostic on that keyword", and the same for `foreach`.

What the code does:

- `RH5005UsingStatementsShouldBePrecededByABlankLineAnalyzer.GetLocation` returns `UsingKeyword`.
- `RH5006ForeachStatementsShouldBePrecededByABlankLineAnalyzer.GetLocation` returns `ForEachKeyword`.
- Both do this unconditionally.

When the statement starts with that keyword, the keyword is also the statement's first token, so the sentence is exactly today's behavior. Both kinds dispatched by RH5006, `ForEachStatement` and `ForEachVariableStatement`, start with `foreach` unless `await` precedes it. The existing markup `{|#0:using|}` and `{|#0:foreach|}` sits on statements that start with the keyword, so those tests cover the reworded requirements as they stand.

### What "starts with the keyword" leaves out

The spec author's handover says a C# statement can carry no attribute or other prefix. That holds for compilable code, not for the syntax tree.

- Roslyn parses an attribute list in front of any statement. `[A] using (resource) { … }` produced no parse error under `scripts/apply-fix.sh RH5005`.
- The compiler rejects it with `error CS7014: Attributes are not valid in this context`, observed with `dotnet build` on the fixture.
- So the reworded sentence leaves two forms unspecified: the `await` form and the non-compilable attribute-prefixed form.
- Leaving the second form out is harmless. A rule's behavior on code that does not compile is not something a spec needs to pin.
- For the record, that input also ends `not-converged` under LF and CRLF.

No change is needed for this.

### The new `Why:` decides the open case

Both requirements now say "Why: The statement's first keyword is where the missing separator becomes visible …".

- `sdd/README.md` (*Spec format*) says the `Why:` "is what decides an edge case the requirement does not spell out".
- The `await` form is exactly that edge case.
- The first keyword of `await using (…)` / `await foreach (…)` is `await`.
- So the `Why:` now leans the open #946 question toward a diagnostic on `await`. That is the opposite of today's token (`using` / `foreach`), and the same kind of decision the rewording was meant to avoid.

The previous `Why:` ("The keyword is where …") leaned the other way. To leave the case out, the `Why:` has to name neither the first token nor the keyword behind `await` as the reason. How to word it is the spec author's call.

### No example on the other side of the qualifier

`sdd/README.md` asks for an example on either side of a qualifier. The other side here is the open case, and *Open questions* forbids writing it down, so the gap is forced. I accept it.

### RH5005-001 and RH5006-001 are unchanged, and I agree

- `GetPreviousToken` measures the gap in front of `AwaitKeyword` when one is present, which is the statement's first token.
- That matches SHARED-STMT-001 and the formatter, which puts one blank line before `await` (observed in `02-skeptic.md`).
- It also matches the expected output of #946 as the lead reports it.

### Deconstruction (thoenissen/Reihitsu#1001): unchanged, and I agree

- None of RH5021-001, RH5021-002, RH5029-001, or RH5029-002 defines whether `var (b, c) = t;` is a local declaration, and no example contains one.
- RH5021-002 ("an assignment, simple or compound") gives the same output under both readings for a deconstruction directly after a declaration: nothing reported.
- The `Why:` of RH5029-001 ("A local declaration starts a new group of local state …") is the rule's original rationale. It does not say which syntax counts as a local declaration, so it does not settle #1001.

## Mechanism and candidates

_N/A._ Not a bug fix. The candidates are those of `02-skeptic.md`, plus the attribute-prefixed statement above (non-compilable, no test).

## Breaking inputs

Unchanged from `02-skeptic.md`. RH5005-002 and RH5006-002 are covered only by a statement that starts with its keyword, with the markup span on that keyword.

Still excluded from tests:

- the `await` form (#946);
- an attribute-prefixed statement (CS7014);
- deconstruction declarations (#1001);
- the multi-line `case` label over-indent (#1000).

## Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `sdd/specs/analyzer/RH5005.md`, `RH5006.md`: normative sentence and heading of RH5005-002 / RH5006-002 | must stay as reworded in 37982a6 |
| `sdd/specs/analyzer/RH5005.md`, `RH5006.md`: `Why:` of RH5005-002 / RH5006-002 | must change, so it no longer names the statement's first keyword |
| RH5005-001, RH5006-001, RH5021-*, RH5029-* | must stay |
| Analyzers, code fixes, Fix All, formatter, `Reihitsu.Core` | not involved (must stay) |
| `documentation/rules/RH5005.md`, `RH5006.md` | not involved: neither page states the location |
| Tests | as listed in `02-skeptic.md` |

## Simpler option

_None._

## Questions for the maintainer

_None._ #946, #1000, and #1001 hold the open cases.
