Role: auditor
Commit: 3523e98baa07aa29105b6875d82acdead3b7c04e
Verdict: #1423: pass; #1424: findings

My prompt carried no intended fix, suspected cause, or hoped-for verdict.

## thoenissen/Reihitsu#1423

### Scope
Head 3523e98baa07aa29105b6875d82acdead3b7c04e, base `origin/main` c857b2d97e7d4822cc9adebd0388b4953de5ebd7 (merged); full audit. Diff for this issue: `squad/roles/skeptic.md:73-75`, a new final `## Follow-up candidates` section after `## Questions for the maintainer`.

### Findings
| # | Severity | Location | Failure class | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|
| _None._ | | | | | | |

Checked: the issue asks for the section with `_None._` when empty; the placeholder `<new-rule candidates and other out-of-scope finds; or _None._>` says so and follows the `<...; or _None._>` style of the other skeptic sections. The heading matches the implementer template (`squad/roles/implementer.md:66`), the consumer `squad/lead.md:83` (*Follow-ups*, "from any handover"), check 3's "which you name as a follow-up candidate" (`squad/roles/skeptic.md:23`), and the rule in `squad/README.md:103`. The closing sentence after the template block is unchanged. No test, script, or agent wrapper parses the template (`rg -ln "squad/|failure-classes|Questions for the maintainer|skeptic" src scripts .github` finds only the `.claude/` wrappers, none of which lists handover sections).

### Guard scope
_N/A — the change adds a template section; it adds or changes no predicate, guard, or exemption._

### Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| Out-of-scope finds go into the handover as follow-up candidates | `squad/README.md:103` (rule); `squad/lead.md:83` (consumer); templates with the slot: `squad/roles/implementer.md:66`, `squad/roles/skeptic.md:74` (`rg -n "Follow-up candidates" squad`) | ok — one rule, one consumer, and the two templates use the same heading. The other role templates have no such slot; see *Hints* |

### Assertion adequacy
_N/A — no test added or changed; no test covers squad Markdown._

### Failure classes dismissed
- FC-015 — checked: the template slot and its consumer agree (*Policy ownership*).
- FC-016 — checked: sibling templates without the slot are pre-existing and outside the issue; see *Hints*.
- FC-031 — checked: no other text in `squad/roles/skeptic.md` describes the template as ending at `## Questions for the maintainer`.
- FC-037 — no C# file changed; the lane check's text-only proof passed (*Whole run*).
- Every other class (FC-001 to FC-014, FC-017 to FC-030, FC-032 to FC-036) — no analyzer, formatter phase, code fix, test, or compiled file changed, so none can reach a template heading.

### Hints
- Of the eight role templates only the skeptic's and the implementer's carry `## Follow-up candidates`, while `squad/README.md:103` sends every role's out-of-scope find to "your handover as a follow-up candidate" (spec author, test implementer, cleaner, spec checker, validator, and auditor have no slot; this run's spec checker used an ad-hoc *Observations* section). Pre-existing and outside the issue, which names only the skeptic, and outside the maintainer's clarification in `00-lead.md`; `squad/roles/test-implementer.md` is also being edited by the parallel run thoenissen/Reihitsu#998. Follow-up candidate.

## thoenissen/Reihitsu#1424

### Scope
Head 3523e98baa07aa29105b6875d82acdead3b7c04e, base c857b2d97e7d4822cc9adebd0388b4953de5ebd7; full audit. Diff for this issue: the FC-014 row (`squad/failure-classes.md:60`) and the FC-014 sentence of check 3 (`squad/roles/skeptic.md:23`). `git diff --word-diff` confirms that nothing else on either line changed, so the FC-013 wording is untouched.

### Findings
| # | Severity | Location | Failure class | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|
| 1 | medium | `squad/failure-classes.md:60`; `squad/roles/skeptic.md:23` | FC-001 (by analogy: the narrowed class covers less than the decision it protects) | same requirement | FC-014 now names only a new rule whose subject is line breaks, blank lines, indentation, or spacing, and it says that every order or content rule is outside the class. The formatter, however, also owns non-layout concerns on purpose, so that formatted code carries none of these rules' diagnostics: using order (`sdd/specs/formatter/using-directives.md:25`, FMT-USING-001: "The formatter and the RH72xx code fixes must produce the order the analyzer checks, or a formatted file would still carry diagnostics"), expression bodies (`structural-transforms.md:143`, `:209`; RH3202, RH3203, RH3205 to RH3208), trailing commas (`:831`, `:883`; RH5409 to RH5411), empty-type braces (`:752`; RH5412 to RH5416), and split fields (`:937`; RH7101). None of these rules has a layout subject. Counterexample: a new rule saying that object initializers have no trailing comma. Its subject is a token, not layout, so the narrowed FC-014 does not cover it, and FC-013 (`:59`) lets an order or content rule report what the formatter keeps. Nothing in the catalog flags the rule any more. Yet `scripts/format.sh` at head on `new[] { 1, 2, }` and `new C { A = items[0], }` removes only the array's comma (FMT-STRUCT-020) and keeps the object initializer's. Unless the run also extends `StructuralTransformPhase`, the new rule reports formatted code for good, which is exactly the "permanent diagnostics for CLI users" FC-014 exists for. Before this change, "A new analyzer's concern is owned by no formatter phase" covered that case. The narrowing does what the issue and the maintainer's clarification (`00-lead.md`) ask for. Its premise, that the formatter does not judge order or content, holds for the cases SHARED-DIAG-010's `Why:` names (`diagnostics.md:536`: "the order of members and the content of comments") but not for the formatter-owned concerns above. | The lead takes this to the maintainer, since any fix changes the scope the clarification set. (A) Keep the class to layout rules, fix finding 2, and accept that formatter-owned non-layout concerns have no catalog cue. (B) Define FC-014 by what the formatter judges: a new rule whose concern is layout, or a shape a formatter phase rewrites for related constructs (using order, braces, trailing commas, expression bodies, field splitting), that no phase produces for the new construct. Exclude only order or content the formatter does not judge (member order, comment text), citing SHARED-DIAG-010. Change both owners the same way. |
| 2 | low | `squad/failure-classes.md:60` (second sentence) | FC-002 (by analogy: SHARED-DIAG-010's qualifier is dropped) | PR-introduced | The new reason "the formatter does not judge order or content" is stated as universal, and it is false in this repository. The formatter reorders using directives (FMT-USING-001, `using-directives.md:21-25`), rewrites region descriptions (FMT-REGION-001, `region-formatting.md:24`; FMT-REGION-002), and rewrites the syntax shapes listed in finding 1. SHARED-DIAG-010 limits that reason to "the order of members and the content of comments" (`diagnostics.md:536`); its normative exclusion (`diagnostics.md:534`) needs no such premise. Check 3 (`skeptic.md:23`) gives only the accurate part ("because SHARED-DIAG-010 lets it report what the formatter keeps"), so the two owners also give different reasons. | Drop the clause, as check 3 does, or narrow it to SHARED-DIAG-010's own scope, for example "the formatter keeps the order of members and the content of comments because it does not judge them". This is needed under either answer to finding 1. |

`pass, text cleanup` is not used. Both findings are in Markdown, but that Markdown is the deliverable: it is the policy the skeptic and the auditor apply. Finding 1 also needs a maintainer decision. A cleanup routed without a second audit would bypass both.

### Guard scope
| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| FC-014 membership: which new rule counts | The rule's subject: line breaks, blank lines, indentation, or spacing (`failure-classes.md:60`) | SHARED-DIAG-010's subject definition, "the layout of code — its line breaks, blank lines, indentation, or spacing" (`diagnostics.md:532`) | The words match. The decision FC-014 protects (does the formatter own what the rule asks for?) is wider than layout: it includes the non-layout concerns the formatter owns on purpose (finding 1) | finding 1 |
| FC-014 versus FC-013 | FC-014: every new layout rule, with or without a formatter-delegated fix. FC-013 (`:59`): only layout rules whose fix lays code out as the formatter does | SHARED-DIAG-010's coverage (`diagnostics.md:534`, last sentence) | Deliberately wider than FC-013: a fixless layout rule that the formatter does not serve still leaves diagnostics no formatter run clears, and SHARED-DIAG-010's `Why:` ("a layout rule that asks for more than the formatter is the reverse disagreement, and the formatter's decision stands") supports flagging it. The two classes do not contradict each other | ok |
| Exclusion of order and content rules | "A new order or content rule", unqualified (both owners) | SHARED-DIAG-010's exclusion (`:534`) and its reason (`:536`, members and comments only) | The exclusion matches `:534`. The reason in the catalog is wider than `:536` | finding 2 |
| Check 3's restatement | "A new layout analyzer whose concern no formatter phase owns" (`skeptic.md:23`) | The FC-014 row | Same scope. "Layout" is not defined again in check 3, but earlier in the same check it is tied to SHARED-DIAG-010 (see *Hints*) | ok |

### Policy ownership
| Policy | Owners found by rg | Verdict |
|---|---|---|
| FC-014 scope and its order/content exclusion | `squad/failure-classes.md:60`, `squad/roles/skeptic.md:23` (`rg -n "FC-014\|permanent diagnostic\|no formatter phase owns" -g '!sdd/work/**'`); the other hits are RH5101, RH5105 to RH5108, and RH5113 code comments about SHARED-DIAG-010/SHARED-JOIN-001 silence, which is FC-013's concern and unchanged | Two owners, both changed with the same scope. They give different reasons (finding 2) |
| Which non-layout concerns the formatter owns | `sdd/specs/formatter/using-directives.md:25`; `sdd/specs/formatter/structural-transforms.md:28,143,209,752,831,883,937`; `sdd/specs/formatter/region-formatting.md:24` | The formatter specs own this; the narrowed FC-014 no longer reflects it (finding 1) |

### Assertion adequacy
_N/A — no test added or changed; no test reads `squad/` (the architecture tests read `sdd/specs/` and the AGENTS.md phase table only)._

### Failure classes dismissed
- FC-001, FC-002 — apply by analogy to the policy text; reported as findings 1 and 2.
- FC-013 — the FC-013 row and its clause in check 3 are unchanged (`git diff --word-diff`). The interaction with FC-014 is covered in *Guard scope*.
- FC-014 — this is the class being changed; audited above.
- FC-015 — two owners, same scope; the difference in reasons is finding 2.
- FC-016 — `rg` finds no third place that restates FC-014.
- FC-031 — no other squad text describes the old FC-014 scope; the decisions entry `squad/decisions/2026-10-23-1419-fc013-layout-rules.md` is history and stays as written.
- FC-037 — no C# file changed; the text-only proof passed.
- FC-003 to FC-012, FC-017 to FC-030, FC-032 to FC-036 — no analyzer, formatter phase, code fix, test, or compiled file changed, so none of them can reach a catalog row or a skeptic check.

### Hints
- Check 3 says "a new layout analyzer" without the subject definition that the catalog row has. A skeptic could read "layout" as the RH5### ID range (AGENTS.md calls it "Layout") rather than SHARED-DIAG-010's subject definition, which also covers RH6### spacing rules and RH83xx documentation-layout rules such as RH8307. This is mitigated: check 3 already ties "layout rule" to SHARED-DIAG-010 earlier in the same sentence chain, and check 5 makes the skeptic read the catalog. Not a finding.
- The spec checker asked whether the FC-014 narrowing needs a `squad/decisions/` entry. The catalog's intro requires one only for a new class (`failure-classes.md:3`), and the narrowing came from the maintainer's own issue and clarifications, not from an escalation answer. Not a finding.

## Whole run

- State: `git rev-parse HEAD` = 3523e98baa07aa29105b6875d82acdead3b7c04e; `git rev-parse origin/main` = c857b2d97e7d4822cc9adebd0388b4953de5ebd7, the prompt's `ls-remote` SHA; `git merge-base --is-ancestor` confirms that main is merged; the working tree was clean.
- The diff matches the handovers: `git diff --stat c857b2d..3523e98` lists the four handovers, `squad/failure-classes.md` (1 line), and `squad/roles/skeptic.md` (1 changed line, 3 added), nothing else.
- `scripts/lane-check.sh tooling` at head: `LANE CHECK: confirmed — claimed tooling, diff needs tooling`.
- The release build result is taken from `02-cleaner.md` (`0 Warning(s) 0 Error(s)`); nothing compiled changed after it.
- Execution for finding 1: `scripts/format.sh` on a temporary fixture outside the repository (deleted afterwards) with `var items = new[] { 1, 2, };` and `return new C { A = items[0], };`. Output: `new[] { 1, 2 }`, and the object initializer wrapped across lines with `A = items[0],` and its trailing comma kept.
