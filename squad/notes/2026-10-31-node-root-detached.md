# node-root-detached

| | |
|---|---|
| Date | 2026-10-31 |
| Run | issue #1489 |
| Pull request | #1520 |

## Questions to the maintainer

Questions 1–7 were asked under the old process (before the squad changed on `main` during the run); question 8 under the new one.

| # | Kind | Question in one line | Answer | As recommended |
|---|---|---|---|---|
| 1 | 2 (old) | Approve the first change doc | Approved | yes |
| 2 | 1 (old) | Member-body target keeps `{` on the declaration's line: clarify or change | Follow-up behavior change (#1521) | yes |
| 3 | 5 (old) | Node-in-document outputs change because decisions read outside a detached root | Add plain-value facts for them | yes |
| 4 | 12 (old) | Two owners of a non-line-starting target's leading trivia | Narrow fix; structure into #1521 | yes |
| 5 | 9 (old) | RH5405 drops an `else if` comment on `main` | Separate follow-up (#1527); this run then fixed it | yes |
| 6 | 1 (old) | Depth of a target behind a multi-line comment | Indentation phase's depth | yes |
| 7 | 2 (old) | Approve the amended change doc | Approved | yes |
| 8 | output question | RH5111/RH6023 on a wrapped member after `?.`: keep the line break or join | Keep the line break | yes |

## Cost

- Stops: eight questions in five messages, plus one follow-up-text approval and three maintainer process interventions (escalate only visible output, no decisions entries, supported inputs).
- Repair rounds: 2 of 3. Round 1 fixed an RH6023 regression through a structural change (one owner of the outermost-chain rule); round 2 only pinned an output answer.
- Roles resumed: spec author nine times, test implementer five, implementer three, cleaner, spec checker, and auditor three each. Each resume carries the agent's whole context, so the spec author and implementer dominated the cost.
- Most expensive: scratch-copy simulations over all test inputs by the spec author, the skeptic, the implementer, and the auditor. A large share measured node-level inputs no code fix passes — the cases the maintainer later declared unsupported.

## Decisions taken

19 decisions by the lead. The one the maintainer may want to see: decision 18 — the chain alignment was folded into the single outermost-chain owner, which changed one output no caller reaches.

## Suggestions

- Check reachability before measuring: the skeptic and the spec author should name the code fix that reaches a case before a simulation counts it. The *Supported inputs* rule this run added states this; the run shows the cost when it is missing.
- Accept an output answer phrased as "ok" to a single recommended option without asking back; this run did, and the maintainer confirmed the pattern of answering "as recommended" throughout.
- A run whose issue is a structural formatter package should expect its first audit to find a reachable case the inventory missed; budget one repair round for it.
