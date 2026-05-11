---
name: issue-drafts
description: Create Reihitsu issue draft markdown files under plans/issues that pass scripts/upload-issues.ps1 validation and can be uploaded with the repository script.
---

# Reihitsu issue drafts

Use this skill when you need to create or update issue draft files in `plans/issues` for upload with `scripts/upload-issues.ps1`.

## Goal

Produce Markdown issue drafts that:

1. use the repository's supported templates
2. satisfy the script's YAML front matter validation
3. satisfy the script's required `###` heading validation
4. can be uploaded without manual reformatting

## File location and naming

- Store drafts in `plans/issues`.
- Use a Markdown file with a sortable numeric prefix and a short slug, for example:
  - `plans/issues/08-analyzer-feature-rh0387a-alternative-member-regions.md`
  - `plans/issues/09-analyzer-feature-require-copyright-header.md`
- Keep filenames descriptive and stable.

## Required front matter

Every draft must start with YAML front matter:

```md
---
template: analyzer_feature_request
title: "[ANALYZER FEATURE] Example title"
labels: enhancement, analyzer
---
```

Rules:

- `template` is required.
- `title` is required.
- `labels` is optional, but when present it should be a comma-separated list.
- Do not add extra prose before the opening `---`.

## Supported templates

### `analyzer_feature_request`

Required headings:

```md
### Motivation
### Proposed Solution
### Should this feature include a code fix?
### Alternatives Considered
### Example That Should Trigger the Rule
### Preferred Compliant Example
### Additional Context
```

Recommended default labels:

```text
enhancement, analyzer
```

### `analyzer_bug_report`

Required headings:

```md
### Description
### Expected Behavior
### Actual Behavior
### Minimal Reproducible Example
### Diagnostic ID
### Does the issue involve a code fix?
### Analyzer Version
### .NET Version
### IDE
### Operating System
### Logs or Additional Context
```

### `formatter_feature_request`

Required headings:

```md
### Motivation
### Proposed Behavior
### Affected Surface
### Alternatives Considered
### Input Example
### Desired Formatted Output
### Additional Context
```

### `formatter_bug_report`

Required headings:

```md
### Description
### Expected Behavior
### Actual Behavior
### Input Code
### Expected Formatted Output
### Actual Formatted Output
### CLI Version
### Operating System
### Logs or Additional Context
```

### `code_review_report`

Required headings:

```md
### Context
### Report Goal
### Questions To Answer
### Acceptance Criteria
### References
### Additional Context
```

## Writing rules

- Use the exact heading text expected by the script.
- Keep the title aligned with the issue type prefix, such as `[ANALYZER FEATURE]`.
- Keep examples minimal but realistic.
- Put code examples in fenced code blocks and use `csharp` when the issue concerns analyzer examples.
- Do not leave sections blank; the body should read like a ready-to-upload issue.
- Prefer labels that already exist in the repository. Missing labels are skipped during upload.

## Upload workflow

Validate and upload drafts with:

```powershell
.\scripts\upload-issues.ps1 -DraftDirectory plans\issues -WhatIf
```

## What the script checks

The upload script rejects drafts when:

- YAML front matter is missing or unclosed
- `template` or `title` is missing
- the body is empty
- the template name is unknown
- any required `###` heading is missing

Write drafts so they pass those checks without manual cleanup.
