Role: spec author
Commit: cd621a5cf388e41dd21b34378db01c8eb73e35e7
Verdict: change doc written

Repair of auditor finding #3 (`13-auditor.md`). My prompt carried no intended fix, suspected cause, or hoped-for verdict beyond naming the finding.

## Changed requirements

_None._ Only visible change 7 (*Repository source*) of `sdd/changes/2026-11/prefix-chain-anchor.md` changed:

- The old `binaries/` analyzers are described with `main`'s full anchor rule: the first call in front of the first wrapped link, otherwise the chain's first link.
- The conflict is narrowed to a chain with two or more links in front of its first wrapped link **and no call among them**. A counterexample with a call (`x.Self().Next` ⏎ `.Next`) shows that both anchors agree there.
- "The four repository chains of that kind" now refers to that narrower set.

No spec, rule page, requirement ID, `Uses` row, or code owner changed.

## Examples for the tests

_None._

## Output questions

_None._

## Internal choices

_None._

## Follow-up candidates

_None._
