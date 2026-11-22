Role: spec author
Commit: 900e83e (spec and change doc edits committed on top of it)
Verdict: spec written

The prompt held no intended fix or hoped-for verdict. Maintainer answers 1–7 in `00-lead.md` are written in as binding.

## Changed requirements

- PLAY-010 — Both panes are highlighted by the C# syntax tree — changed: answer 2A (an identifier token is an identifier whatever its text: `var`, `nameof`, `dynamic`, `value`; `set` added to the keyword list); answer 4A (a documentation comment trivia is a comment with every token inside it; a directive's tokens and message trivia are directive; a comment trivia behind a directive is a comment); a new paragraph states that the category is decided by token or trivia kind, never by text, with `/* c */` behind `#region X` and `// b` behind `#error a` as message (directive colour). The unspecified-case sentence is removed. The second example gains `#region Methods` … `#endregion // Methods`; a third example covers the doc comment with `cref="int"` and `partial`/`set`/`var`/`nameof`/`value`/`dynamic`.
- PLAY-011 — The input is highlighted while the user types — changed: a coloured copy of the text shown with the input MUST be hidden from assistive technology (skeptic wording gap 3).
- PLAY-014 — Error locations are marked in the input — changed: answer 3A as a three-case rule for empty spans (ordinary character → that character; line break → the place directly behind the line's last character, the line break not marked; end of text → behind the last character), each with an example; a mark never changes the text; answer 1A (every mark disappears at the first change of the input after the format that set it, comes back only with a later format that shows PLAY-006's or PLAY-008's message; editing never changes or removes the message box). The unspecified-case sentence is removed; the Why gains the reason.

Change doc `sdd/changes/2026-11/playground-highlighting.md`: `Status: Accepted`; the stale "see the open question in the spec author's handover" line in *Visible output changes* 1 replaced by the decided colouring; *Visible output changes* 4 gains the line-end example and the after-edit behavior; *Decisions* gains "Marks disappear at the first edit", "Contextual keywords follow the parser", "Documentation comments and directives are decided by trivia kind", "The coloured copy is hidden from assistive technology", and *Error positions* states the three-case empty-span rule.

`scripts/test.sh --project architecture`: only `EveryRequirementOfAnEnforcedSpecIsCovered` fails, for PLAY-010 to PLAY-014, as before; the test implementer covers them. No requirement ID, `Uses` row, or code owner was added or removed.

## Examples for the tests

PLAY-010 (three examples), PLAY-011, PLAY-014 (five examples). Verified with a Roslyn 5.12.0 probe in the scratchpad (invariant culture, default parse options):

- `class C\n{\n    int x\n}\n` → CS1002 `[19..19)`, line 3, column 10, `; expected`; the character at 19 is `\n`, so the mark sits behind `x`.
- `class C { /* open` → CS1035 `[10..10)` line 1, column 11, `End-of-file found, '*/' expected` (mark on `/`); CS1513 `[17..17)` line 1, column 18, `} expected` (mark behind the last character).
- The PLAY-010 region example parses without error: `RegionDirectiveTrivia`, `IfDirectiveTrivia`, `DisabledTextTrivia`, `EndIfDirectiveTrivia`, `SingleLineCommentTrivia`, and `EndRegionDirectiveTrivia` whose text is `#endregion // Methods` (the `// Methods` is comment trivia inside the directive's structure, as the skeptic found).
- The doc-comment example parses without error: `PartialKeyword`, `SetKeyword`, `IntKeyword`; `var`, `nameof`, `value`, `dynamic` are `IdentifierToken`.

For the after-edit example of PLAY-014, a test must edit the input after a failed format and assert no mark (the skeptic's note: storing offsets gives option B by default). For FC-026, the skeptic's inputs with several identical characters on a line (`class C { void M() { int x = 1\t}`) remain good choices.

## Output questions

_None._ Answers 7 (1A, 2A, 3A, 4A) close all four.

## Internal choices

- Skeptic wording gap 1 (precedence in documentation comments) and 2 (comment behind a directive by trivia kind) are taken in as above; they are the content of answer 4A.
- Skeptic wording gap 3 is phrased as "a coloured copy of the text shown with the input MUST be hidden from assistive technology", so it names behavior, not markup (`aria-hidden`). Alternative: name the attribute.
- Skeptic wording gaps 4 (PLAY-011 "on every change" kept, no coalescing) and 5 (one helper for line and column, an auditor check) were not spec changes and are left as they are.
- The change doc's first highlighting example (`string s = "text"`) is left as it is; the spec's example is the test seed.

## Follow-up candidates

_None._
