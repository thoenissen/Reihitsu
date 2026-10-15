# Open questions — line-endings.md (FMT-EOL)

## Q1 — Line breaks inside block comments and disabled text are not normalized

Input (an `\n` file; `␍` marks a `\r`, so the first block-comment line and the disabled-text line end in `\r\n`):

```csharp
class C
{
    /* a␍
       b */
    int y;
#if NEVER
    disabled␍
#endif
}
```

Output: identical apart from the removed final line break — both `\r\n` line breaks are kept. Phases: `Cleanup` (`scripts/trace.sh`; the harness also lists `LineEndingNormalization`, but only because its phase observer guesses `\r\n` from any `\r\n` in the text, while the real detection of FMT-PIPE-007 picks `\n`). Idempotent. The same holds for a `\n` line break inside a `/* */` comment of a `\r\n` file.

Why it looks wrong: FMT-PIPE-007 is titled "One line ending per file" and says FMT-EOL normalizes the remaining line breaks, but the line breaks inside `MultiLineCommentTrivia` and `DisabledTextTrivia` are part of the trivia's text, not `EndOfLineTrivia`, so the phase never sees them and the file keeps mixed line endings. FMT-PIPE-002 forbids changing comment and disabled-text text, which may be why — but a line terminator inside a comment or disabled text carries no meaning (unlike one inside a string literal, FMT-EOL-004), and documentation comments are normalized (FMT-EOL-003). Should the phase normalize line breaks inside block comments and disabled text (with an explicit allowance in FMT-PIPE-002), or is the mixed result intended?

Touches: FMT-EOL-001, FMT-EOL-004, FMT-PIPE-002, FMT-PIPE-007 (the spec lists this as a non-goal and makes no claim about it).
