Role: test implementer
Commit: de1fe1d
Verdict: reproduced

Repair round 3, Decision 10 (`00-lead.md`), finding 8 of `28-auditor.md`. The prompt held no intended fix, suspected cause, or hoped-for verdict. Only `tests/` changed; `src/` is untouched (`git status` clean after the scratch mutation below was restored).

On de1fe1d (production code unchanged): the 5 null-character tests fail, the other 98 Playground tests pass (103 in all), `scripts/test.sh --project architecture` passes (198).

## How the panes are read

bUnit's DOM cannot show this defect, nor its fix: bUnit writes every frame into one markup string and reads it back through AngleSharp's HTML parser. That parser turns a markup frame's `&#0;` into U+FFFD, as the browser does, but it also drops a raw U+0000 a **text frame** wrote, which the browser keeps in a text node (probed: a text frame `x\0y\rz` reads back as `x y \n z` in bUnit's DOM). Asserting `TextContent` on bUnit's DOM would fail today and after a correct fix alike.

The new helper `tests/Reihitsu.Playground.Test/PlaygroundBrowserDocument.cs` builds the document the browser builds from the page's current render tree, frame by frame, the way Blazor's browser renderer inserts frames: an element frame becomes an element, a text frame a text node with exactly its text, a markup frame the nodes the HTML parser makes of it inside a `template` element (AngleSharp, empty markup as a space, as Blazor does). It reads `Renderer.GetCurrentRenderTreeFrames` by reflection (it is not public), with `BL0006` suppressed in that one file and the reason in a comment. Checked on the unchanged page: for text without U+0000 the copy's and the output's `InnerHtml` equal bUnit's; a markup frame `x\0y&#0;z&#13;w` gives `x y U+FFFD z CR w` (raw NUL dropped, `&#0;` replaced, `&#13;` kept), which is the browser behavior the auditor probed in Chrome.

The tests therefore name no mechanism: they assert the text the browser shows. Text frames, markup, or anything else that leaves U+0000 in the shown text passes.

## Tests

All in `tests/Reihitsu.Playground.Test/NullCharacterTests.cs`; text is compared as hexadecimal code units, so a failure names the changed character.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `NullCharacterTypedIntoTheInputIsKeptInTheCopy` (`class C { string s = "a\0b"; }` typed) | PLAY-010 | bUnit page, `PlaygroundBrowserDocument` | fails: copy shows U+FFFD for U+0000 |
| `NullCharacterIsKeptInTheCopyOnEveryKindOfLine` (U+0000 alone on a line, in a comment, in a string, in `#if A` disabled text, two in a row, at start and end of the text, LF and CR LF lines) | PLAY-010 | same | fails: every U+0000 shows as U+FFFD |
| `NullCharacterIsKeptInTheCopyAfterFormat` (`// x\0y\r\nclass C { string s = "a\0b"; }\r\n`, format without message) | PLAY-010 | same, `PlaygroundMarkup.Format` | fails: U+FFFD in the copy |
| `NullCharacterIsShownInTheOutput` (same input; output `// x\0y\r\nclass C\r\n{\r\n    string s = "a\0b";\r\n}`) | PLAY-010 | same | fails: U+FFFD in the output |
| `NullCharacterIsKeptInACopyWithErrorMarks` (`class C { string s = "a\0b"; `: message CS1513 at column 29, mark `28:` one character wide at the end) | PLAY-010, PLAY-014 | same, `PlaygroundMarkup.GetMarks` | fails: U+FFFD in the marked copy; the mark is asserted after the text |
| `CarriageReturnAndOtherControlCharactersAreKeptInTheCopyAndTheOutput` (`&`, `<`, `>`, lone CR, U+0001, U+0008, U+000B, U+000C, U+007F, U+FEFF, U+0085, U+2028 in comments; output against `PlaygroundFormatter.Format(input).Text`) | PLAY-010 | same | passes — the counterpart of finding 8's class, the characters the auditor found to hold |

Each failing test fails only on U+0000 → U+FFFD (watched in the detailed log: every differing code unit is `0000` expected, `fffd` actual). The output test is separate from the copy test after format, so the output is checked even while the copy fails.

### Watched pass: a scratch fix

To check the tests do not pin a mechanism, `PlaygroundHighlighter.CreateContent` was mutated in scratch (copy at `/tmp/claude-0/-home-user-Reihitsu/7ccecb21-b72f-5c00-b88a-569dde938365/scratchpad/mut29/orig.cs`, restored, nothing committed): a line whose markup holds `&#0;` is emitted as element and text frames (spans kept, U+0000 as a character in a text frame) instead of a markup frame. All 6 tests pass with it, the mark test included. This is not a recommendation; the implementer chooses.

## Not covered, and why

- **Colours of a line that holds U+0000.** Decision 10 accepts losing them as a rare-class refusal; no test asserts them either way.
- **Character references in `page.Markup`.** A check that bUnit's markup holds no `&#0;` would miss a fix that writes a raw U+0000 into a markup frame (the browser drops it) and is subsumed by the browser document.
- **Lone surrogates and U+FFFE.** The auditor found them kept in Chrome; Roslyn and the formatter reach them through the same encoding path as the characters in the counterpart test. Left out to keep the counterpart test to text Roslyn reads as comments without errors.
- **The incremental sweep of 25 with U+0000.** Those tests compare against the same `Highlight` path, so they cannot see this class (finding 8); the page tests above read the browser text instead.
- **The `AppendEncoded` summary and the CR at the end of `app.css` (finding 9).** No test: a comment and a byte of the stylesheet with no visible effect; the implementer's.
- FC-033 (sibling shapes): marked and unmarked paths, input copy and output, typed and formatted, LF and CR LF are each a test above.

## Commands

```shell
scripts/test.sh --project playground --filter "FullyQualifiedName~NullCharacterTests"
scripts/test.sh --project playground
```

## Follow-up candidates

_None._
