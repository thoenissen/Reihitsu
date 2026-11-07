# Follow-up issue

The format of an issue that a squad run, a coordinator, or the maintainer writes for work found in passing. It has three variants; pick the one that fits, copy its skeleton into the issue body, and fill in every field.

| Variant | When |
|---|---|
| [Spec bug](#spec-bug) | The code differs from a requirement that a spec already states. |
| [Clarification](#clarification) | A spec gains a requirement for behavior that already exists. |
| [Internal](#internal) | Cleanup, refactoring, tests, docs, or tooling — no change of intended behavior. |

This format is not a public issue template and does not go under `.github/ISSUE_TEMPLATE/`, where every form appears in the "New issue" chooser. The public forms stay what they are for:

- **Behavior changes and new rules** — including a gap in a rule's own construct whose form the rule's spec does not cover, and a new-rule candidate ([P11](../README.md#the-formatter-may-do-more-than-the-analyzers-p11)) — use the public feature-request form of their surface (`.github/ISSUE_TEMPLATE/analyzer_feature_request.yml` or `formatter_feature_request.yml`).
- **A bug in behavior that no spec covers yet** uses the public bug-report form of its surface.

## Common to every variant

- **Title:** `<Scope>: <outcome>` — the scope is a rule ID (`RH3202`), a spec's ID prefix (`FMT-BLANK`, `CLI`), or an internal area (`squad`, `scripts`, `Reihitsu.Tooling`); the outcome says what is true once the issue is done, not the symptom. No `[ANALYZER BUG]`-style prefix.
- **`Lane:`** — one of the lanes in [`squad/README.md`](../../squad/README.md#lanes), spelled as `scripts/lane-check.sh` takes it: `docs`, `tests`, `bug-fix`, `behavior-change`, or `tooling`.
- **`Principle:`** — optional. The principle the issue's decision rests on: its label or requirement ID and where it is stated, for example `P11 — sdd/README.md, The formatter may do more than the analyzers` or `SHARED-FIX-003 — sdd/specs/shared/code-fixes.md`. Leave the line out when no decision rests on one.
- **Self-contained body.** The issue is read later, by someone without the run's context: no "see chat", "as discussed", or pointer to `sdd/work/`, which is deleted before merge. Quote the input and output it needs; name files as repository-relative paths in backticks, since relative links do not resolve in an issue.
- **Label:** an issue an agent writes carries `agent-filed`, so its text reads as not reviewed by the maintainer; only the maintainer removes the label.
- English, and short: one line per field where one line is enough.

## Spec bug

The fields correspond to those of the public bug-report forms: *Requirement* takes the place of the description, *Input* is the minimal reproducible example (the formatter form's input code), *Today's output* is the actual behavior, and *Required output* is the expected behavior.

````markdown
Lane: bug-fix
Principle: <label or requirement ID — where it is stated; remove the line when none>

## Requirement

- <REQUIREMENT-ID> — <its current wording, in one line>

## Input

```csharp
<minimal input>
```

## Today's output

```csharp
<what the code produces today, or the diagnostics it reports>
```

## Required output

```csharp
<what the requirement demands>
```

## Affected

<rules, code fixes, or formatter phases that produce today's output>

## Blocked by

<#N — what it has to settle first, or None>

## Done when

<the behavior that shows the requirement holds, with the covering test it needs>
````

## Clarification

A clarification writes down what the code already does; it needs no change doc (`sdd/README.md`, *When a change doc is required*). When the current behavior looks wrong, it is not a clarification but an open question for the maintainer.

````markdown
Lane: tests
Principle: <label or requirement ID — where it is stated; remove the line when none>

## Spec

`sdd/specs/<path>.md`

## Behavior to write down

<one sentence: the behavior as the requirement would state it>

```csharp
// before
```

```csharp
// after, or the diagnostics reported
```

## Covering test

<the test the new requirement needs, or the existing test that already covers it and gains the tag>

## Done when

<the requirement exists in the spec and its covering test carries its tag>
````

## Internal

````markdown
Lane: <docs, tests, bug-fix, or tooling>
Principle: <label or requirement ID — where it is stated; remove the line when none>

## What and why

<what changes and why it is worth doing>

## Files or areas

- `<path>` or <area>

## Done when

<the observable state that closes the issue>
````
