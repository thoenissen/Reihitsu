# Open questions — cleanup.md (FMT-CLEAN)

## Q1 — Trailing whitespace inside comments and documentation comments survives the pipeline

Routed from documentation-comments.md Q1 ("is it Cleanup's?").

Input (shown with `$` at each line end as `cat -A` prints it; the `//` line, the trailing comment, the first block-comment line, and the documentation content line each end in three spaces):

```
class C$
{$
    // note   $
    int x; // trailing   $
$
    /* a   $
       b */$
    int y;$
$
    /// <summary>$
    /// Gets the value.   $
    /// </summary>$
    int A;$
}$
```

Output: identical except that the final line break is removed (FMT-CLEAN-005). Phases: `Cleanup`. Idempotent.

Analysis: `CleanupPhase` only removes `WhitespaceTrivia` that precedes an `EndOfLineTrivia` (and converts tabs in `WhitespaceTrivia`). The spaces above are not whitespace trivia: after `//` they are part of the `SingleLineCommentTrivia` text, inside `/* */` part of the `MultiLineCommentTrivia` text, and after `/// Gets the value.` part of an `XmlTextLiteralToken` inside the structured documentation trivia. So by construction no phase owns them today — Cleanup does not reach them, and the documentation-comment phase only strips whitespace on lines that carry nothing else (FMT-DOC-006). Answer to the routed question: it is not Cleanup's under the current design, and it is not anybody else's either; this is a gap, not a deliberate preservation.

Decision needed:

1. Keep it as user-authored comment text under FMT-PIPE-002 (then FMT-CLEAN gains a requirement saying so, and the empty-summary `/// ` line of documentation-comments Q1 is accepted as it is), or
2. Remove it. Suggested ownership: whitespace at the end of a documentation line belongs to the documentation-comment phase, which already rewrites `XmlTextLiteralToken`s and owns FMT-DOC-006 (it would also fix the `/// ` line of documentation-comments Q1 at its source); whitespace at the end of a `//` comment and at the end of each line of a `/* */` comment belongs to Cleanup. Either way FMT-PIPE-002 needs an explicit allowance, because it currently forbids changing comment text.

Touches: FMT-CLEAN-001, FMT-CLEAN-004 (both deliberately make no claim about comment text; the spec lists it as a non-goal), FMT-DOC-006.

## Q2 — Whitespace at the end of a directive line survives, and a trailing tab becomes trailing spaces

Input (`→` marks a tab; the `#if` line ends in a tab, the `#endif` line in three spaces):

```
class C
{
#if DEBUG→
    int x;
#endif   
}
```

Output (`#if DEBUG` followed by four spaces, `#endif` followed by three spaces):

```
class C
{
#if DEBUG    
    int x;
#endif   
}
```

Phases: `Cleanup`. Idempotent.

Why it looks wrong: the whitespace at the end of a directive line is trivia inside the directive's structure and is not followed by an `EndOfLineTrivia` in the same list, so `CleanWhitespaceBeforeEndOfLine` never sees it. `NormalizeDirectiveTriviaTabs` does descend into the directive, but only converts tabs — so the phase actively turns a trailing tab into four trailing spaces instead of removing it. FMT-CLEAN-001 removes trailing whitespace everywhere else outside content. Unlike a region description (directive message text, FMT-CLEAN-003), the whitespace after `DEBUG` or `#endif` is not user content. Should the directive pass also strip whitespace that ends the directive line?

Touches: FMT-CLEAN-001, FMT-CLEAN-003 (the spec lists directive-line trailing whitespace as a non-goal and makes no claim about it).

## Q3 — Whitespace after the last token of a file without a final line break survives

Input (the closing brace is followed by three spaces and no line break):

```
class C
{
    int x;
}   
```

Output: unchanged. Phases: none.

Why it looks wrong: `StripTrailingWhitespaceAtEndOfLine` only strips a token's trailing whitespace when the next token's leading trivia starts with a line break, and the end-of-file token has empty leading trivia here. With a final line break (`}   ` + `\n`) the spaces are removed together with the line break. The two inputs differ only in the line break the formatter removes anyway, yet format differently.

Touches: FMT-CLEAN-001, FMT-CLEAN-005 (the spec lists this case as a non-goal and makes no claim about it).
