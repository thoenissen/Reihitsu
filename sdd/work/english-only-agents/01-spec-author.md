Role: spec author
Commit: d2e7c50
Verdict: text written

## Changed requirements

_None._ No spec under `sdd/specs/` covers agent instructions, and the change records no new behavior of a shipped surface, so no change doc.

Text change, `AGENTS.md`, section `Instructions`, first bullet:

- Before: "The repository language is English. All communication, documentation, and code should be in English, even if user input is in another language."
- After: "The repository language is English. Everything must be in English: code, comments, documentation, commit messages, issues, pull requests, review comments, and every agent's chat replies to the maintainer and contributors — regardless of the language the user writes in."

The "should" becomes "must", and the bullet names chat replies explicitly, which the old "all communication" left open to reading as repository artifacts only.

## Examples for the tests

_None._ Text only; no test applies.

## Contradictions checked

I searched `AGENTS.md`, `CLAUDE.md`, `squad/**`, `.claude/**`, `.github/**`, and `sdd/README.md` for language rules. Nothing contradicts the new bullet:

- `squad/README.md` (`Rules for every role`) says "English only."
- `squad/lead.md` and `squad/decisions/README.md` say the maintainer's clarifications are summarized in English.
- `.claude/agents/*`, `.claude/commands/*`, and `.github/` have no language statement.

Follow-up candidate (optional): `squad/README.md`'s "English only." is consistent with the new bullet but terse. It could point at `AGENTS.md` so the squad rule doesn't drift from it. I left it unedited, as instructed.

## Non-goals

- No change to `CLAUDE.md`, `squad/`, `.claude/`, or `sdd/README.md`. The repository has no non-English `*.<culture>.resx` files, so the stricter wording conflicts with no existing content.

## Open questions to file

_None._
