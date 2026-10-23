Role: implementer
Commit: 27b967c
Verdict: #1423: not involved in this round; #1424: tests pass (no test covers this Markdown; the text is changed and the lane check confirms it)

Repair round for the findings of `04-auditor.md` on thoenissen/Reihitsu#1424, under the maintainer's answer 1B (`squad/decisions/2026-10-23-1424-fc014-formatter-owned-concerns.md`). That answer replaces the "narrow to layout concerns" clarification in `00-lead.md`. I did not take the auditor's suggested wording. I wrote the text from SHARED-DIAG-010 and its `Why:` (`sdd/specs/shared/diagnostics.md:530-536`) and from the formatter requirements the finding cites.

## thoenissen/Reihitsu#1423

Nothing to do in this round. The auditor passed it, and the `## Follow-up candidates` template section in `squad/roles/skeptic.md` is unchanged.

## thoenissen/Reihitsu#1424

### Changes
| File | What changed | Requirement |
|---|---|---|
| `squad/failure-classes.md` (FC-014 row) | The class is now a new rule whose concern is of a kind the formatter judges: layout, or an order or a syntax form that a formatter phase rewrites (examples: using order, expression bodies, trailing commas, braces of empty types, field splitting). It applies when no formatter phase owns that concern for the rule's construct. Excluded: order or content the formatter does not judge, such as the order of members or the content of comments. The reason given: the formatter keeps such code without judging it, so SHARED-DIAG-010 (linked) lets a rule about it report what the formatter keeps | Decision 1B; SHARED-DIAG-010 `Why:`; auditor findings 1 and 2 |
| `squad/roles/skeptic.md` (FC-014 sentences of check 3) | The same scope and the same reason, in two sentences, without the list of examples. SHARED-DIAG-010 is linked earlier in the check, so it is named here without a link | Decision 1B; auditor findings 1 and 2 |

New text, FC-014 row:

> A new rule's concern is of a kind the formatter judges — layout, or an order or a syntax form that a formatter phase rewrites, such as using order, expression bodies, trailing commas, braces of empty types, or field splitting — but no formatter phase owns it for the rule's construct, leaving permanent diagnostics for CLI users. Order or content that the formatter does not judge, such as the order of members or the content of comments, is not this class: the formatter keeps it without judging it, so [SHARED-DIAG-010](../sdd/specs/shared/diagnostics.md) lets a rule about it report what the formatter keeps

New text, check 3:

> A new rule whose concern is of a kind the formatter judges — layout, or an order or a syntax form that a formatter phase rewrites — but that no formatter phase owns for the rule's construct leaves permanent diagnostics for CLI users (FC-014). Order or content that the formatter does not judge, such as the order of members or the content of comments, is not this class: the formatter keeps it without judging it, so SHARED-DIAG-010 lets a rule about it report what the formatter keeps.

Where the wording comes from:
- **Exclusion.** "The order of members and the content of comments" and "keeps them because it does not judge them" restate SHARED-DIAG-010's `Why:` (`diagnostics.md:536`). The exclusion is limited to those cases, which fixes finding 2: the previous universal claim, "the formatter does not judge order or content", is gone from both places. Both places now give the same reason.
- **Examples of formatter-judged non-layout concerns.** Each is a formatter requirement whose `Why:` names an analyzer rule:
  - using order: FMT-USING-001, `sdd/specs/formatter/using-directives.md:21-25`;
  - expression bodies: FMT-STRUCT-004, FMT-STRUCT-005;
  - braces of empty types: FMT-STRUCT-017;
  - trailing commas: FMT-STRUCT-019, FMT-STRUCT-020;
  - field splitting: FMT-STRUCT-021.

  All are in `sdd/specs/formatter/structural-transforms.md`. This fixes finding 1.
- **"For the rule's construct".** This covers the auditor's counterexample. A trailing-comma rule for object initializers has a concern of a kind the formatter judges (FMT-STRUCT-019 and FMT-STRUCT-020 remove trailing commas), but no phase removes the comma from an object initializer, so the new FC-014 covers that rule.
- **What stays the same.** A layout rule whose concern no phase owns stays in the class, because layout is a kind the formatter judges. This keeps the width the auditor rated "ok" in *Guard scope* (`04-auditor.md`).

Unchanged: the FC-013 row, the FC-013 sentence of check 3 ("an order or content rule may report what the formatter keeps"), every other class and check, and the #1423 section. The FC-013 sentence restates SHARED-DIAG-010's normative exclusion (`diagnostics.md:534`): which rules may report shapes the formatter keeps. FC-014 now asks a different question: whether the formatter produces what a new rule asks for. So a using-order rule can be outside SHARED-DIAG-010 for FC-013 and still inside FC-014. The two sentences do not contradict each other.

### Policy owners
`rg -n "FC-014|permanent diagnostics" squad --glob '!sdd/work/**'`: `squad/failure-classes.md:60` and `squad/roles/skeptic.md:23` only. Both changed, with the same scope and the same reason. The decisions entries that mention FC-014 are history and stay as written.

### Tests run
None apply (Markdown only). See *Whole run*.

### Disputed tests or scope
_None._

### Follow-up candidates
_None._

## Whole run

- `scripts/lane-check.sh tooling --head worktree`, run before the commit with only the two edits uncommitted: `TEXT-ONLY PROOF: PASS — base=c857b2d head=worktree — 8 changed file(s) (8 non-compiled, 0 C# comment/layout-only); xml-doc impact: none` and `LANE CHECK: confirmed — claimed tooling, diff needs tooling`.
- Not touched: `sdd/` outside this run's `sdd/work/`, `squad/roles/test-implementer.md`, `squad/lead.md`, and any code, spec, or test file.
- The coordinator's prompt pointed at the auditor's suggested wording but asked me to derive the text myself, which I did. No other intended fix or hoped-for verdict was carried.
