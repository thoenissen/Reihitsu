# Triage

Sort every open GitHub issue of `thoenissen/Reihitsu` that has no milestone into one. **$ARGUMENTS** may narrow the run to the issue numbers it names; without arguments, the run covers every open issue without a milestone.

You assess and propose; the maintainer decides. Nothing is written to GitHub before the maintainer has confirmed the proposal.

## 1. Collect

1. Read the open milestones. The GitHub MCP server has no milestone listing, so read them from the public REST API:

   ```shell
   curl -sS "https://api.github.com/repos/thoenissen/Reihitsu/milestones?state=open&per_page=100"
   ```

2. Read the open issues without a milestone, skipping pull requests:

   ```shell
   curl -sS "https://api.github.com/repos/thoenissen/Reihitsu/issues?state=open&milestone=none&per_page=100&page=1"
   ```

   Page until a page returns fewer than 100 entries.

3. Read each issue's title, body, labels, and comments. Open the code an issue names only as far as the assessment needs it.

When nothing is left without a milestone, say so and stop.

## 2. Assess

Put every issue into exactly one group. Judge what the issue changes for a user of the analyzer package or the `reihitsu-format` tool, not the label it carries or the lane it names.

| Group | What belongs in it |
|---|---|
| **Extension** | New or changed behavior a user sees: a new rule, a rule that reports more or less, a new code fix, a formatter that changes different output, a new CLI option. A *(behavior change)* issue is an extension even where it also fixes a shortcoming. |
| **Maintenance** | Internal work a user never notices: refactoring with no change in behavior, code comments, tests, specs and clarifications under `sdd/`, the squad, scripts, CI, repository tooling. |
| **Critical bug** | A defect that damages the user's code or blocks the user. |
| **Minor bug** | Every other defect. |

A bug is **critical** when it does at least one of these:

- changes what the user's program does: a code fix or the formatter alters a literal's value or the executed logic;
- produces code that no longer compiles;
- deletes or relocates user-authored comments, directives, or disabled text (`SHARED-FIX-003`);
- crashes the analyzer (`AD0001`) or the CLI, or turns the formatter non-idempotent on ordinary code;
- reports false positives on common, correct code that the user can only silence by suppression.

Everything else is a **minor bug**, for example a missing report, a fix that is withheld or makes no progress, or a layout outcome that differs from the spec without damaging code. A diagnostic without a code fix is not a defect by itself (see `Code-fix coverage` in `AGENTS.md`).

Treat the report as a hypothesis, as `Issue intake` in `AGENTS.md` requires. Assess severity from the reported behavior and say when you triaged without reproducing it. When the report is wrong or overstated, say so in the table rather than inflating or deflating the group.

Also determine the **labels** to add: one of `bug`, `enhancement`, `maintenance` that matches the group, plus `analyzer`, `formatter`, `cli`, or `documentation` where the issue touches that surface. Never remove a label the issue already has.

## 3. Propose

Present everything in one chat message, so that a single short answer can confirm it.

1. **Milestones.** Map each group that occurs to a milestone and recommend one per group. Name the open milestones that come into question. The usual mapping is:

   | Group | Recommended milestone |
   |---|---|
   | Extension | `vNext` |
   | Maintenance | `Maintenance` |
   | Critical bug | the current release: the lowest open version-number milestone |
   | Minor bug | ask: the current release, or a later version-number milestone |

   The maintainer may name a milestone that does not exist yet and create it before answering. When no fitting milestone exists, say so and suggest a name.

2. **Issues.** One table with one row per issue, ordered by group:

   | # | Title | Group | Reason | Labels to add | Milestone |
   |---|---|---|---|---|---|

   - `#` is a link to the issue.
   - `Reason` is one short sentence: for a bug, why it is critical or minor; for any other issue, what makes it an extension or maintenance.
   - `Milestone` is the recommended milestone of its group. For a minor bug, write the recommendation followed by `?`, so the open decision is visible.

3. **Answer format.** End with the answer the maintainer can give, for example:

   > `ok` — apply everything as proposed.
   > `ok, minor bugs → 1.2.1` — change one group's milestone.
   > `ok, #1459 → 1.2.0, #1463 Extension` — override single issues.

Then stop and wait. Do not apply the recommendation on silence or on an answer that does not clearly confirm it; ask back briefly instead.

## 4. Apply

After the maintainer's answer:

1. Re-read the open milestones, since the maintainer may have created new ones. Resolve every milestone name to its number; when a named milestone does not exist, ask instead of creating it.
2. Update every issue with `mcp__github__issue_write` (`method: update`): set the milestone and pass the full label list — the existing labels plus the ones to add.
3. Re-read the open issues without a milestone and confirm that none of the triaged issues is left.
4. Report the result as a short table of issue, milestone, and added labels, and name anything that failed.
