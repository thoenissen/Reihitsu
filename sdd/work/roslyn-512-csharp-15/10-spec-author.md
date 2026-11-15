Role: spec author
Commit: 4d30fc793ea6337c4ca1f7311aafa7516b2cbe58
Verdict: change doc written

Repair round 1. The prompt carried no intended fix, suspected cause, or hoped-for verdict to ignore.

Changes to `sdd/changes/2026-11/roslyn-512-csharp-15.md` (status stays `Accepted`):
- *What*: the list of input that parses differently now names a top-level statement that starts with a local named `union`, besides the `union`/`closed` type names.
- *Visible output changes*: new entry "CLI, a top-level statement that starts with a local named `union`" (auditor 09, finding #1), with the failing statement starts, the holding positions (declaration of the local, `union` elsewhere in a statement, the same statements in a block or method body, every position with `closed`), and an input/before/after example. Before/after were run on the head's Release CLI: default gives `Skipped 1 file(s) with syntax errors.` and leaves the file unchanged; `--lang-version 14` gives the "before" text (only `1,2` -> `1, 2` changes).
- *Visible output changes*: new entry "Playground and pipeline trace tool": both parse with Roslyn's default options and now parse C# 15; PLAY-005 already requires this for the Playground, and its text does not change. The trace tool has no spec.
- *Decisions*: new section "A top-level statement that starts with a local named `union` follows C# 15" recording maintainer answer 5A.

## Changed requirements
_None._ (FMT-PIPE-008, CLI-004, and PLAY-005 unchanged.)

## Examples for the tests
_None new._ The new change-doc example matches the auditor's counterexample; whether a test pins it is the lead's call (no requirement adds one).

## Output questions
_None._ (Answer 5A covers the new entry.)

## Internal choices
- Listed the `union` local case as its own *Visible output changes* entry and its own *Decisions* section rather than folding it into the type-name entry, because answer 5 is a separate answer from 3; alternative: one merged entry.
- Did not add PLAY-005 to *Affected requirements*, because its text does not change; alternative: list it there.

## Follow-up candidates
- **Formatter, array initializer spacing after `{`.** Outside this run (reproduces under `--lang-version 14`, so not caused by the change). Input `class C { int[] M() => new[]{1,2}; }` (and top-level `var a = new[]{1,2};`) is written as `return new[] {1, 2 };` / `var a = new[] {1, 2 };`: a space is added before `}` and after `,`, but none after `{`. Expected reading: `new[] { 1, 2 }`, symmetric as with input `new[] { 1,2 }`. Readings: A `new[] { 1, 2 }` (recommended, matches the existing output for spaced braces); B today's `new[] {1, 2 }`. Reached through the CLI (whole-document formatting). Needs a check against the horizontal-spacing spec before filing.
