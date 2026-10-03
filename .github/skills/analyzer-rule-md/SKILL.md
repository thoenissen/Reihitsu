---
name: analyzer-rule-md
description: Write or update Reihitsu analyzer rule markdown files under documentation/rules/RH####.md. Use this when asked to create, rewrite, or review user-facing rule documentation for an analyzer rule.
---

# Reihitsu analyzer rule markdown

Use this skill when working on published rule documentation in `documentation/rules/RH####.md`.

## Goal

Write **short, user-facing rule documentation**, not implementation notes.

The page is what a developer opens from the diagnostic's help link. Within a few seconds they must know:

1. what the rule reports
2. why it is reported
3. how to fix it
4. what correct code looks like

Everything else is noise. A page should fit on one screen apart from its examples.

## Audience and scope

- The reader uses the **analyzer and its code fix**. Many users never install or run the formatter.
- **Never mention the formatter** or `reihitsu-format` — not as a fix path, not as a reason, not as context. The rule and its code fix must be described as working on their own. `RuleDocumentationConventionTests` fails on any prose line that mentions it.
- Do not describe how the analyzer or the code fix is implemented.

## Rule page vs. general notes

`documentation/rules/general-notes.md` owns every behavior that applies to all rules. Do **not** repeat it on a rule page:

- A comment, a preprocessor directive (`#if`, `#pragma`, `#region`, …), or code excluded by `#if` inside the code a rule would change: the rule is not reported there, or it is reported without a code fix.
- Code in an inactive `#if` branch is not analyzed.
- Generated code is not analyzed.
- A diagnostic reported without a code fix is intended behavior.
- Suppressing, disabling, or enabling a rule (`.editorconfig`, `#pragma warning`, `[SuppressMessage]`).

When a new limitation applies to every rule (or to every code fix), add it to `general-notes.md` instead of to a rule page. When a rule-specific limitation is just the general comment/directive limitation applied to that rule's code, leave it out of the rule page.

Every rule page ends with this footer, exactly (verified by `RuleDocumentationConventionTests`):

```md
---

See [General notes](general-notes.md) for behavior that applies to every rule: comments and preprocessor directives, generated code, and turning a rule off.
```

## Required structure

```md
# RH#### — Rule title

| Property | Value |
|----------|-------|
| **ID** | RH#### |
| **Category** | Analyzer/Performance/Design/Clarity/Naming/Layout/Spacing/Organization/Documentation |
| **Severity** | Warning |
| **Code Fix** | ✓ or ❌ |

## Description

Short explanation of what the rule reports.

## Why is this a problem?

The readability, maintainability, correctness, or consistency problem from a user perspective.

## How to fix it

Direct, practical advice. Say whether the code fix does it for you.

## Exceptions

Optional. Only cases specific to this rule where it deliberately does not report, or where its code fix is deliberately withheld. Keep it to a short bullet list.

## Examples

### Violation

```cs
// violating example
```

### Correction

```cs
// corrected example
```

---

See [General notes](general-notes.md) for behavior that applies to every rule: comments and preprocessor directives, generated code, and turning a rule off.
```

Add the `| **Enabled by default** | ❌ |` row only for a rule that is off by default.

## Style rules

- Keep the tone concise and practical. Prefer two short sentences over one long one.
- Write for **users of the analyzer**, not for analyzer maintainers.
- Use `##` section headings exactly as shown above.
- Use ` ```cs ` for code blocks.
- Keep examples minimal but realistic.
- Make the correction reflect the actual preferred style in this repository.
- When a rule has a code fix and accepts several equally valid layouts, the `### Correction` fenced block must show exactly what the shipped code fix produces for the `### Violation` example — this is verified mechanically by `Reihitsu.Analyzer.Test/SelfHosting/RuleDocumentationExampleTests.cs`. Describe the other accepted-but-not-rewritten forms in prose instead of listing them inside `### Correction`.
- If the rule has a code fix, mark **Code Fix** as `✓`; otherwise use `❌`.
- Severity should normally be `Warning` unless the repository clearly uses something else.

## What not to include

- The formatter or `reihitsu-format`, in any role
- Anything already covered by `general-notes.md` (see above)
- Corner cases a user will practically never meet, or long lists of special shapes — summarize the rule instead of enumerating its edge cases
- Roslyn API choices, analyzer registration details, syntax kinds, trivia, tokens, or semantic model discussion
- Check logic algorithms, code-fix internals, evaluation order, or convergence and idempotency notes
- Compiler error codes the code fix avoids, unless the user sees them
- Migration notes from StyleCop

If a sentence only makes sense to someone who has read the analyzer source, delete it.

## Repository-specific guidance

- Keep rule titles and descriptions aligned with the analyzer/package naming.
- The help link for analyzers points to `documentation/rules/RH####.md`, so the file should read like polished product documentation.
- Prefer explaining intent and developer benefit over repeating the diagnostic message verbatim.

## Writing process

1. Read the existing analyzer name, package README entry, nearby rule docs, and `documentation/rules/general-notes.md`.
2. Infer the user-visible rule intent.
3. Write the page in the required published format, including the footer.
4. Remove implementation details, formatter mentions, and anything the general notes already cover.
5. Check that the example code actually matches the rule and its preferred fix.

## Good example characteristics

- The violation is obviously wrong according to the rule.
- The correction is the smallest clear fix.
- The explanation is understandable without knowing Roslyn or compiler APIs.
- The document can stand alone as reference documentation for someone seeing the rule in an IDE or build output.
