---
name: analyzer-rule-md
description: Write or update Reihitsu analyzer rule markdown files under documentation/rules/RH####.md. Use this when asked to create, rewrite, or review user-facing rule documentation for an analyzer rule.
---

# Reihitsu analyzer rule markdown

Use this skill when working on published rule documentation in `documentation/rules/RH####.md`.

## Goal

Write **user-facing rule documentation**, not implementation notes.

The document should help a developer understand:

1. what the rule means
2. why the rule exists
3. how to fix a violation
4. what correct code looks like

## Required structure

Follow this structure closely:

```md
# RH#### — Rule title

| Property | Value |
|----------|-------|
| **ID** | RH#### |
| **Category** | Clarity/Design/Naming/Formatting/Documentation/Performance/Ordering |
| **Severity** | Warning |
| **Code Fix** | ✓ or ❌ |

## Description

Short explanation of what the rule enforces.

## Why is this a problem?

Explain the readability, maintainability, correctness, or consistency problem from a user perspective.

## How to fix it

Give direct, practical advice.

## Examples

### Violation

```cs
// violating example
```

### Correction

```cs
// corrected example
```
```

## Style rules

- Keep the tone concise and practical.
- Write for **users of the analyzer**, not for analyzer maintainers.
- Prefer short paragraphs over long explanations.
- Use `##` section headings exactly as shown above.
- Use ` ```cs ` for code blocks.
- Keep examples minimal but realistic.
- Make the correction reflect the actual preferred style in this repository.
- If the rule has a code fix, mark **Code Fix** as `✓`; otherwise use `❌`.
- Severity should normally be `Warning` unless the repository clearly uses something else.

## What not to include

Do **not** include planning or implementation material such as:

- Roslyn API choices
- analyzer registration details
- syntax kinds
- semantic model discussion
- check logic algorithms
- code-fix internals
- edge-case implementation notes
- migration notes from StyleCop

If the content sounds like internal technical design documentation, it does not belong in the published rule doc.

## Repository-specific guidance

- Match the format used by existing docs such as:
  - `documentation/rules/RH0001.md`
  - `documentation/rules/RH0104.md`
  - `documentation/rules/RH0325.md`
- Keep rule titles and descriptions aligned with the analyzer/package naming.
- The help link for analyzers points to `documentation/rules/RH####.md`, so the file should read like polished product documentation.
- Prefer explaining intent and developer benefit over repeating the diagnostic message verbatim.

## Writing process

When creating or rewriting a rule doc:

1. Read the existing analyzer name, package README entry, and nearby rule docs.
2. Infer the user-visible rule intent.
3. Write the page in the required published format.
4. Remove any implementation-specific sections if they exist.
5. Check that the example code actually matches the rule and its preferred fix.

## Good example characteristics

- The violation is obviously wrong according to the rule.
- The correction is the smallest clear fix.
- The explanation is understandable without knowing Roslyn or compiler APIs.
- The document can stand alone as reference documentation for someone seeing the rule in an IDE or build output.
