# Generate Release Notes

Write the release notes of one Reihitsu release for the people who use it: users of the `Reihitsu.Analyzer` NuGet package and of the `reihitsu-format` CLI (`Reihitsu.Cli`). **$ARGUMENTS** names the release and its predecessor as git tags, release first: `v1.2.0 v1.1.0`. A missing `v` prefix is added. If either is missing, ask for it and stop.

The result is one Markdown code block the maintainer pastes into the GitHub release. GitHub's *Generate release notes* button adds the list of pull requests and the *Full Changelog* link afterwards, so the notes contain neither. You write nothing to the repository or to GitHub.

## 1. Collect

1. Make sure the history is complete. A cloud checkout is often shallow, and a shallow clone silently hides most of the range:

   ```shell
   git rev-parse --is-shallow-repository   # true → git fetch --unshallow origin
   git fetch --tags origin
   ```

2. Check that both tags exist and that the predecessor is an ancestor of the release (`git merge-base --is-ancestor <previous> <release>`). Stop and report if not.

3. List the pull requests of the range. Every commit on `main` is a squash merge whose subject ends in `(#<N>)`:

   ```shell
   git log --format='%h %an %s' <previous>..<release>
   ```

   Drop dependency bumps (author `dependabot[bot]`, subject `Bump …`) unless they change a dependency of a shipped project (step 5).

4. Read the description of every remaining pull request, through the GitHub MCP server (`pull_request_read`) or the public API (`curl -sS https://api.github.com/repos/thoenissen/Reihitsu/pulls/<N>`). The `Summary` section says what changes for a user; the `Review notes` say where the result differs from the issue. Ignore run reports, role tables, and lane checks.

5. Check the facts the descriptions may not state, from the diff of the two tags:
   - new, removed, or renamed rules: `git diff --name-status <previous> <release> -- documentation/rules`, titles from the first line of each rule page;
   - a rule's code-fix and formatter coverage: the rule table in `src/Reihitsu.Analyzer.Package/README.MD`;
   - CLI options and behavior: `src/Reihitsu.Cli` and `src/Reihitsu.Cli/README.MD`;
   - dependencies and target frameworks of the shipped projects: the `.csproj` files of `Reihitsu.Analyzer`, `Reihitsu.Analyzer.CodeFixes`, `Reihitsu.Formatter`, `Reihitsu.Core`, and `Reihitsu.Cli`. A raised minimum Roslyn or .NET version is a heads-up.

   Paths can have moved between the tags; compare with `git diff <previous>:<old path> <release>:<new path>` when a directory was moved.

6. Read the previous release's notes as the style reference: `curl -sS https://api.github.com/repos/thoenissen/Reihitsu/releases/tags/<previous>`.

## 2. Sort

Judge every pull request by what a user of the analyzer or the CLI notices, not by its title or labels.

| Goes into the notes | Stays out |
|---|---|
| New rules; rules that report more or less | Specs and requirement IDs under `sdd/`, change docs |
| Code fixes that are now offered, withheld, or produce different output | The squad, agent instructions, `.claude/`, slash commands |
| Formatter output that changes, or now settles in one pass | Tests, test infrastructure, self-hosting scans |
| Formatter or code-fix output that no longer breaks code, comments, or literals | Scripts, CI, repository tooling, community files |
| CLI options, exit codes, and messages | Refactoring with identical output ("one owner", "no phase predicts …") |
| Package changes a user must act on | Dependency bumps of test or tooling projects |

A refactoring pull request often carries a visible change in its `Summary` or `Review notes` ("Visible result: …", "the output changes for …"). Keep that change and drop the rest. Internal work may get one closing sentence at most, never a section.

## 3. Write

Write in English, in the voice of the previous release. Use this outline and leave out any section that would be empty:

```markdown
# Reihitsu <version>

<Two or three sentences: what this release is about for a user.>

> **Heads-up:** <Only when upgrading changes what users see: formatter output that produces a one-time diff, rules that report code they did not report before. Say what to do.>

## New rules

| Rule | Title | Code fix |
|---|---|---|
| `RH####` | <title from the rule page> | ✔ (Fix All) / ✔ / – |

<One or two bullets on what the new rules have in common or how to adopt them.>

## Changes to existing rules

## Safer code fixes

## Formatter

## CLI

## Bug fixes

## Upgrading

    dotnet add package Reihitsu.Analyzer --version <version>
    dotnet tool update -g Reihitsu.Cli --version <version>
```

Rules for the text:

- Name every rule by its ID; give the title where the reader cannot guess it. Group rules that changed for one reason in one bullet (`RH3101, RH3104–RH3107 …`).
- Show a formatter change as a short before → after example in inline code, or in a small `csharp` block when it spans lines.
- Describe the effect, not the mechanism: no requirement IDs (`FMT-…`, `SHARED-…`, `RH####-00x`), no class, method, or phase names, no spec paths, no issue or pull-request numbers, no squad terms.
- A withheld fix is described from the user's side: "the diagnostic stays, but no fix is offered when …". It is an improvement, not a regression (`AGENTS.md`, *Code-fix coverage*).
- Keep each bullet to one or two sentences. When a pull request lists many corner cases, name the class of cases once.
- Do not invent: every statement must trace back to a pull request description or the diff. When a description is unclear about whether output changed, check the tests or the change doc under `sdd/changes/` before you write it.

## 4. Deliver

Reply with the notes in **one** fenced code block tagged `markdown`, ready to copy. Fence it with four backticks (` ```` `) so the code blocks inside it survive. Below the block, add at most three short lines for the maintainer: pull requests you were unsure about, and anything that looks like it belongs in a heads-up but was left out. Remind them that *Generate release notes* on GitHub appends the pull-request list.
