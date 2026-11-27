Role: spec author
Commit: 72f3e49
Verdict: not text-only

## Lane

The only file the issue names, `squad/roles/implementer.md`, is classified **Tooling**, not Text: `scripts/README.md`, *The lane check*, puts `squad/**` (except `squad/notes/**`) in the Tooling row. With the edit below in the worktree, `scripts/lane-check.sh docs --head worktree` printed:

```
TEXT-ONLY PROOF: PASS — base=f21c69c head=worktree — 2 changed file(s) (2 non-compiled, 0 C# comment/layout-only); xml-doc impact: none
Text       sdd/work/implementer-inputs/00-lead.md
Tooling    squad/roles/implementer.md
LANE CHECK: mismatch — claimed docs, diff needs tooling
```

The proof passes (no compiled file), but the docs lane cannot hold this diff; the lane has to widen to tooling. I reverted my edit, so this commit holds only the handover; the role file is unchanged at HEAD.

## Proposed text (for whoever writes it in the tooling lane)

`squad/roles/implementer.md`, *Inputs*, first sentence extended:

> `00-lead.md`, the test implementer's handover, the spec diff and change doc, and the specs they name. In the tooling lane, which runs no spec author and a test implementer only when the tool has tests, also every issue of the run by number, read through the GitHub MCP tool `issue_read`. The architecture and the formatter's phase ownership are in `AGENTS.md`.

Why this shape: "every issue of the run by number" is the phrase the test implementer, skeptic, and auditor role files already use; naming `issue_read` ties the clause to the `mcp__github__issue_read` entry in `.claude/agents/implementer.md` (the issue's "Done when"); the lane reason comes from the lane table in `squad/README.md` (Tooling: spec author –, test implementer "when the tool has tests").

## Changed requirements

_None._ No spec covers the squad role files.

## Examples for the tests

_None._

## Output questions

_None._ Nothing an end user sees changes.

## Internal choices

- Scoped the clause to the tooling lane, as the issue proposes, rather than making the issues an input of the implementer in every lane; in the other lanes the spec author's, skeptic's, and test implementer's handovers already carry the issue's checked content. Alternative: list the issues unconditionally, like the auditor.
- Did not add "when there is one" to the test implementer's handover in the same sentence, although it is absent in a tooling run without tests; the issue does not ask for it (non-goal).

## Non-goals

- No change to `.claude/agents/implementer.md` (it already allows `mcp__github__issue_read`), to `squad/README.md`, or to `squad/lead.md`.
- No change to the lane rule that the tooling lane has no premise-check role (raised in earlier squad notes; not this issue).

## Follow-up candidates

_None._
