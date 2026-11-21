# Lead — cyclomatic-complexity

- Issue: thoenissen/Reihitsu#352 — `agent-filed` (goals are proposals; the maintainer's clarifications below override the issue text)
- Slug: `cyclomatic-complexity` (kept as the run's slug although the metric is cognitive complexity, see Maintainer answers 1)
- Branch: `claude/affectionate-allen-njaq9m` (session-designated); draft PR thoenissen/Reihitsu#1909
- Lane: **behavior change / new rule** — the issue requests a new analyzer rule (new diagnostic, new rule page, new configuration settings).
- Roles: spec author → skeptic → (concept presented to the maintainer, see below) → test implementer → implementer → cleaner → spec checker → auditor → validator. None skipped.

## Maintainer clarifications (given with the run's prompt, summarized)

1. Present a concept of how the rule works to the maintainer **before anything is implemented** (no tests, no production code before the maintainer approves the concept). The lead presents it at the batching point before the test implementer.
2. The rule computes **cyclomatic complexity** (not cognitive complexity as the issue proposes). Default threshold **15**. The value is a starting point; it will be tried out when the rule is rolled out in this repository.
3. If the rule reports violations in the repository's own code (self-hosting), these are ignored for now; adjustments, and possibly a different default, come in a follow-up.
4. The threshold must be configurable through `reihitsu.json`, like the line-count rules (RH2008 `design.maximumMethodLines`, RH2111 `design.maximumTypeLines`).
5. The rule must not check complexity alone: a method that never reaches nesting level 2 — i.e. has no nesting — is never reported. Example: a method with only `if` statements one after another, none nested inside another, is never reported, whatever its complexity.

## Maintainer answers

1. Correction of clarification 2: the rule computes **cognitive complexity**, as the issue proposes — not cyclomatic. Default limit 15, configurability, the no-nesting exception, and the self-hosting handling stay as stated above.
2. Deeper nesting costs more points: a construct's increment grows with its nesting depth (the cognitive-complexity nesting increment).
3. Spec author Q1 (local function): **B** — a local function is judged only on its own; it does not count toward the member that holds it.
4. Spec author Q2 / skeptic C (nesting only through a lambda): **B** — nesting that comes only from a lambda does not lift the no-nesting exception ("lambdas are ok"). Whether a lambda still adds nesting depth to the score is asked separately (open).
5. Spec author Q3 (`and`/`or` patterns): **A** — they score like `&&`/`||`.
6. Spec author Q4 (test code): **A** — one setting for production and test code.
7. Spec author Q5 (recursion): **B** — each call of the member itself scores 1.
8. Skeptic A (chained `?:`): **B** — a `?:` in the false branch of another `?:` scores like `else if` (1, no nesting increment).
9. Skeptic B (`or` in switch arms / case labels): **B** — combinators in a switch arm's or case label's pattern score nothing; in `is` patterns and `when` clauses they count.
10. Skeptic D (lambdas): leave lambdas out for now — a lambda's body is **not counted toward the method** that holds it at all, and a lambda is not judged on its own. Checking lambdas becomes a follow-up. This supersedes answers 4 and the lambda-nesting question (maintainer also answered that one B: a lambda adds no nesting) — with lambdas excluded, both are moot.
11. Self-hosting mechanism: use the existing ignore list of the self-hosting scan (`AnalyzerViolationReport._ignoredDiagnosticIds`, currently empty) for RH2009 until the follow-up.
13. Sonar Q1 (`#if`): **A** — only active code is scored; directives score nothing.
14. Sonar Q2 (recursion, re-ask of answer 7): **A** — 1 once per member that calls itself directly; no indirect cycles. Supersedes answer 7.
15. Sonar Q3 (`&`, `|`, `^` on bool): **A** — score nothing.
16. Sonar Q4 (anonymous methods): **A** — treated like lambdas: not counted, not judged.
17. Sonar Q5 (parenthesized chained `?:`): not answered yet; maintainer asked to see the departures from the paper as code with per-line point comments — re-asked in that form.
18. A comprehensive documentation document explaining the whole scoring with examples is required. Placement asked (rule pages are short by convention).
19. Review of the departures from the paper (maintainer: prefers to stay closer to the paper; the discussion is still open and no decision is final — the updated concept goes back to the maintainer before any test is written):
   - Chained `?:` scored flat (answer 8): kept.
   - Lambdas and anonymous methods: **follow the paper** — their bodies count toward the member that holds them and raise the nesting level by one. Supersedes answers 10 and 16 for lambdas/anonymous methods; the lambda follow-up is dropped. Answer 4 (nesting that comes only from a lambda does not lift the no-nesting exception) still stands. A lambda in a field/property/event initializer stays unjudged (answer 10's part about initializer lambdas is not reopened).
   - Local functions: judged only on their own (answer 3), kept.
   - Recursion: our simple variant (answer 14), kept.
   - `#if`: only active code, directives score nothing (answer 13), kept.
   - `&`, `|`, `^` on `bool`: **follow the paper** — they count like `&&`/`||` (as binary boolean operators in sequences). Supersedes answer 15.
   - Patterns (answers 5, 9) and limit/no-nesting exception: kept.
20. Sonar Q5 (parenthesized chained `?:`): **A** — parentheses directly around the false branch do not break the chain.
21. Scoring guide placement: **A** — separate page `documentation/cognitive-complexity.md` with the full scoring guide (every construct with points and nesting, the departures from the paper, worked examples with per-line point comments); `documentation/rules/RH2009.md` stays short and links to it.
22. Lambda follow-up reinstated: a follow-up issue covers reporting lambdas that stand outside every judged member (field, property, and event initializers), possibly judged on their own. Not part of this run.
23. 04-spec-author Q1–Q4: **all A** — `&`/`|` on `bool?` count like on `bool`; `&=`/`|=`/`^=` score nothing; only an invocation `M(…)` of the member itself counts as recursion; a `?:` in a switch `when` clause is nested in the switch.
24. No reference to the SonarSource PDF / white paper anywhere in the repository (it was only looked at for comparison): no link, no citation, no "departures from the paper" framing in the guide, rule page, spec, or change doc. The rules are described on their own terms.
25. **Concept approved** (spec, change doc, rule page, scoring guide at 5fbfd58). Implementation may start.
12. The maintainer asks to cross-check all decisions against the published definition (SonarSource white paper, https://www.sonarsource.com/docs/CognitiveComplexity.pdf; local copy in the session scratchpad). Comparison requested from a fresh skeptic; results go back to the maintainer before the test implementer.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Skeptic | Member selection and location restated from RH2008 (architecture finding, FC-015) | Change the structure in this run: one shared owner in code for member → (body, expression body, name token), used by RH2008 and RH2009; the RH2009 spec references RH2008-002/004/005 instead of restating them | A second copy is exactly the drift the finding names; the move is small (a few dozen lines), within RH2008, its counterpart | RH2008 behavior unchanged; same members and location as the spec already states | – |
| 2 | Skeptic | Default-on-invalid stated twice (RH2009-001 vs SHARED-LINES-002) | Widen the shared statement to cover configured limits generally and reference it from RH2009; share the configuration-reading code instead of copying it | One statement of the rule | Wording only; behavior identical | – |
| 4 | Lead | Release.ruleset action for RH2009 while it is on the self-hosting ignore list | `Action="None"` until the follow-up (RulesetTests requires an entry; `Error` would break the next `/bump-version` build on the repo's own violations) | Carries out answer 11: violations ignored for now | Repository build configuration only; the shipped rule's default severity stays Warning | – |
| 5 | Implementer | Rule page examples fail `RuleDocumentationExampleTests` (bare methods, CS0106) | Spec author wraps the Violation and Correction examples in a class; no opt-out | The example stays verified; scores unchanged (16 vs 15 / below) | The page's content and scores are unchanged; only a surrounding `class` is added | – |
| 6 | Implementer | Default values of lambda parameters are scored where the lambda stands, not in its body | Keep; no spec text, no test | A `?:` in a constant parameter default is a rare input class nobody writes; dropped, not a follow-up | Rare class (AGENTS.md, Code-fix coverage / Supported inputs) | – |
| 3 | Skeptic | Internal wording points (`when` clauses count `&&`/`||`; `not` breaks a run like `!`; query clauses do not nest; rule page/change doc wording on `!(a && b) && c`; exception sentence aligned with Q2's answer) | Take all, route to spec author with the maintainer's answers | Removes two-reading ambiguity without changing any score | No score changes; rule page wording follows the answers | – |

## Follow-up candidates

- Self-hosting violations of the new rule and a review of the default threshold (maintainer clarification 3) — lead; includes removing RH2009 from the self-hosting ignore list and setting its Release.ruleset action to Error.
- Reporting lambdas outside every judged member — field, property, and event initializers — possibly judged on their own (maintainer answer 22) — lead.

## Open worklist / next step

- Implementer done (08, 2fae9e7/b41ee07). Asked maintainer output question 21: does `other.M(…)` (same method on another instance) count as recursion? Waiting. Then: spec author (decision 5 + answer) → cleaner → spec checker → auditor → validator.
- Answers 23–24 applied (05-spec-author.md, 5fbfd58). On approval: status-only follow-up (spec author sets Accepted), then test implementer.
- Updated concept (04-spec-author.md, guide `documentation/cognitive-complexity.md`) presented for approval with chat questions 17–20 = 04 Q1–Q4. Waiting. On approval: spec author sets Accepted with the answers, then test implementer.
- Done: fresh spec author applies answers 3–21 and decisions 1–4, writes the scoring guide; then lead presents the updated concept to the maintainer for approval (no tests before).
- Sonar comparison done (03-skeptic.md). Sent to maintainer as chat questions 11–15 = skeptic 03 Q1 (#if), Q2 (recursion re-ask), Q3 (& | ^), Q4 (anonymous methods), Q5 (parenthesized chained ?:). Waiting.
- Answers 3–7 received. Remaining questions sent: chained `?:`, `or` in switch arms, initializer lambdas, lambda nesting depth in the score, self-hosting mechanism.
- Earlier: concept + output questions 1–8 and escalation 6 (self-hosting mechanism, question 9) sent to the maintainer; waiting for answers. Then: route answers + decisions 1–3 to the spec author (fresh), spec author sets change doc Accepted, then test implementer.
