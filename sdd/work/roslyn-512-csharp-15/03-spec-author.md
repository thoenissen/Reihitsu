Role: spec author
Commit: 80b2180
Verdict: change doc written

Repair round after the skeptic (02-skeptic.md) and the maintainer's answers (00-lead.md, batch 1). The lead's message carried no intended fix or verdict beyond the amendments listed in 00-lead.md row 5; nothing to ignore.

## Changed requirements

- FMT-PIPE-008 — Version-dependent rules follow the language version — changed (wording): `Why:` now says the clamp keeps a new Roslyn `Latest`/`Preview` from switching on version-gated rules that were never reviewed for that version, and that the maximum does not mean every construct of that version has a reviewed layout. The normative sentence is unchanged since 01 (C# 15).
- CLI-004 — unchanged since 01.

Change doc `sdd/changes/2026-11/roslyn-512-csharp-15.md`:
- `Decisions` records answers 1A (maximum raised to C# 15), 2A (interim union layout accepted until #1762), 3A (`union`/`closed` type names skipped under the default; a breaking existing test is fixed by renaming its type), 4A (host floor Roslyn 5.12, 1.3.0 release-note item).
- *What* and *Formatter entry points*: "valid C# 14 input is formatted exactly as before" replaced by "input that Roslyn parses the same way under C# 14 and C# 15"; the gate statement corrected to "none above C# 12".
- The type-name section now covers `union` and `closed`, with the positions from the skeptic's probe table and a `closed` before/after block.
- The worst-case one-line union (skeptic question B) is added under the default-version change.
- *Why* no longer says Roslyn 5.9 cannot parse C# 15: it parses a union under `Preview`, but lacks `LanguageVersion.CSharp15` and maps `Latest` to C# 14.
- Status stays `Draft`: the role file has the spec author write `Draft`; `sdd/README.md` gives setting `Accepted` to the lead.
- Union test infrastructure is not planned anywhere (decision 4: follow-up).

## Examples for the tests

- FMT-PIPE-008 — as in 01; the reworded `Why:` adds no example.
- CLI-004 — as in 01. The change doc's `closed` and `union` type-name blocks are input for a CLI test that such a file is skipped under the default and formatted under `--lang-version 14` (answer 3A).

## Output questions

_None._ All answered (1A, 2A, 3A, 4A).

## Internal choices

- The positions where `union`/`closed` break are stated in the change doc as a list from the skeptic's probe, not as a claim that no other position breaks; I did not re-probe them.
- CLI-004 keeps only the `union` example; `closed` is in the change doc, since CLI-004 states the general rule (parse with the given version, FMT-PIPE-005 for errors).

## Follow-up candidates

_None_ new from this round. The skeptic's candidates (union test infrastructure, blank line after `(` before a first comment in a parameter list) are in 02-skeptic.md for the lead.
