# Every formatter phase returns a tree that round-trips and keeps foreign annotations

| | |
|---|---|
| Status | Accepted |
| Date | 2026-11 |
| Affected requirements | FMT-PIPE-021 (new), FMT-PIPE-022 (new), FMT-STRUCT-026 (new), FMT-BREAK-029 (new), FMT-PIPE-017 (third gap case), FMT-PIPE-019 (example changed), the first non-goal of `structural-transforms.md` |
| Depends on | [One line-start fact for analyzers and every formatter phase](../2026-10/line-start-fact.md) |

## What

Every phase of the pipeline returns a tree whose text, parsed again, is that tree (FMT-PIPE-021), and keeps every annotation of its input on the elements it keeps (FMT-PIPE-022). Today two phases break this:

- The structural-transform phase returns text that is another program. `int Get() => 42;` becomes `int Get(){return42;}`, where `return42` is one identifier, and `Foo(); // trailing` in an unbraced `if` body becomes `{Foo(); // trailing}`, where the inserted `}` is part of the comment. Later phases repair both by accident: the spacing phase writes a space after `return` because `return` is one of its keywords (FMT-SPACE-012), and the line-break phase puts `}` on its own line (FMT-BREAK-002).
- The indentation phase moves a misaligned multi-line raw string literal by parsing a new literal from text. Every token of the literal is new, so the annotations of the old ones are gone. `FormatNodeInDocumentWithContextAsync` finds its target through such an annotation (FMT-PIPE-018); with a target inside a literal that moves, it finds none and returns the document unchanged. Once the annotation survives, the entry point formats such a target, and the result falls into the gap FMT-PIPE-017 records, as a third case: the phases move the line the target starts on, the text in front of the target on that line is written back as in the document, and the target's other lines take the phases' columns. For the invocation `Call(1, 2)` in `$"""` / `            a {Call(1,` / `            2)} b` / `            """`, with the local declaration as context, `1` stays in column 20 and `2` is written in column 25, where the phases place it under `1` in the moved literal. Closing the gap stays with thoenissen/Reihitsu#1501.

The structural transforms write the separators their text needs and no other layout (FMT-STRUCT-026): one space after a created keyword that would otherwise fuse with the next token, a line break after a single-line comment that a created token follows, and a line break after a created token that a leading comment or a directive follows. They write no line break after an inserted `}` and none behind a comment that leads a braced statement; those comments keep the line breaks and spaces the author wrote, which FMT-PIPE-013 already asks of the final output. The field split keeps its own layout until the RH7101 fix stops running it alone (thoenissen/Reihitsu#1502).

The line-break phase states the whitespace it leaves behind (FMT-BREAK-029): whitespace may remain in front of a line break it inserts, and the cleanup phase removes it (FMT-CLEAN-001). FMT-CLEAN-001's `Why` already counts on this; FMT-BREAK did not say it.

The formatter's final output is unchanged by these requirements wherever it already met the specs. Two inputs change, toward a requirement they violate today, because the brace transform no longer rebuilds the line breaks of the comments that lead a braced statement:

- `if (a)` followed by `/* x */ /* y */ Foo();` keeps the two comments in front of `Foo();` on one line, as FMT-PIPE-013 states; today each comment and `Foo();` get a line of their own.
- A blank line between two comments that lead an unbraced `if` body is kept, as FMT-PIPE-013 states; today it is dropped.

## Why

A phase receives a tree, but the documentation phase reads its text, the trace prints the text and parses it again between passes, and the next formatter run parses it as well. When text and tree are two programs, each of them is formatted correctly only as long as some later phase happens to repair the difference. The repair is not guaranteed: for a token pair that no spacing rule covers, FMT-SPACE-002 forbids inserting a gap, so a transform that fused such a pair would ship it. A test that parses the phase's output passes on an error-recovered tree, which is how `ControlFlowBraceTransformTests.TrailingCommentOnIfBodyIsPreserved` passes today.

Layout that a structural transform writes beyond its separators is decided again by the phase that owns it — line breaks by FMT-BREAK, spaces by FMT-SPACE, columns by FMT-INDENT. Writing it twice is the pattern FMT-PIPE-001 forbids for columns: one decision in two places, which can disagree.

Annotations are how a caller finds its nodes again. An element that a phase rebuilds instead of editing loses them, and the context entry point is the first caller that depends on them.

## Decisions

### "The same tree" holds tokens, structure, and the owner of every comment

FMT-PIPE-021 compares the reparsed tree with the phase's tree by its nodes and their structure, its tokens with their kinds, texts, and places, the absence of any added or skipped token, and, for each comment, documentation comment, directive, and disabled-text span, the token that holds it and on which side. Whitespace and line breaks need only stand at the same places; the token that holds them may differ, and two runs of whitespace may join.

The owner of a comment decides what the phases do with it: a comment that trails a token stays at the end of that token's line (FMT-BREAK-001), a comment that leads one starts its own line. A tree that puts `// leading` behind an inserted `{` on its line, as leading trivia of the next statement, reads back with the comment trailing the brace, and the two are formatted differently. The owner of a line break decides nothing a phase relies on, and the line-break phase writes every line break it inserts in front of the token it moves, where the parser gives it to the token before (FMT-BREAK-001).

The comparison covers the tree that `FormatDocumentAsync` and `FormatSyntaxTree` pass through the phases. The node-level entry points pass a node, whose text is not a compilation unit; FMT-PIPE-022 covers what they need, the annotation on their target.

### Phases keep the annotations they do not own

A phase returns every annotation of its input on each node, token, or trivia it keeps, also where it changes that element's text, trivia, or children or moves it into a node it creates. An element the phase removes takes its annotations along, an element it creates carries none of the input's, and a phase's own annotations are its own business. An element written more than once, such as the modifiers each split field repeats, is left out: which copy keeps the annotation is not decided.

The raw-string move therefore edits the literal's tokens — the text tokens of its content and closing lines and the whitespace in front of its hole lines — instead of parsing a new literal. It no longer needs parse options, so the question which language version a reparse would use disappears with it.

### The structural transforms write separators only

FMT-STRUCT-026 names the separators the transforms write and forbids every other space, line break, and indentation next to a token a transform creates or in the trivia it carries over. The decided separators follow from FMT-PIPE-021: a space where two tokens would read as one, a line break behind a single-line comment, and a line break in front of a comment that has to stay leading trivia of the next token or in front of a directive, which has to start its line. This applies to every structural transform: besides the brace and expression-body transforms the issue names, the accessor and get-only-member transforms, which write spaces around the `=>` they create, are covered.

The comments that lead a braced statement keep the line breaks and spaces the author wrote between them and in front of the statement. That is the phase-level form of FMT-PIPE-013, which the final output already has to meet; the brace transform violates it today for the two inputs listed under What.

The field split keeps its line breaks and indentation, recorded as an exception in FMT-STRUCT-026: the RH7101 fix runs it without the phases after it, and the exception goes when that fix formats through a named entry point (thoenissen/Reihitsu#1502).

### The line-break phase leaves whitespace before its line breaks to the cleanup phase

FMT-BREAK-029 allows the line-break phase's output to keep the whitespace that ends a line in front of a line break it inserts; FMT-CLEAN-001 removes it. The phase trims that whitespace today only where the token in front of the line break lies inside the node it rewrites, so its output depends on where a node starts; the cleanup phase removes it everywhere. The requirement allows the residue and does not forbid the trim, so it does not force the line-break rewriters that remove whitespace while they join or break a list to change.

Which rewriter places the braces of a member body is not specified behavior and is not decided here.

### A target inside a moving literal is the third case of FMT-PIPE-017's gap

The context entry point keeps writing back only the target (FMT-PIPE-010, FMT-PIPE-018). For a target whose line the phases move, its first token keeps the document column and its other lines take the phases' columns, the same misalignment the two recorded cases of the gap produce. FMT-PIPE-017 lists the case and its example; laying out the target from its column in the document is the restructuring of thoenissen/Reihitsu#1501 (`squad/decisions/2026-10-31-phase-tree-round-trip-target-in-moving-literal.md`).

## Alternatives considered

- **Require only the final output to be correct, and state in FMT-PIPE that later phases repair a structural transform's separators** — rejected: it writes the accidental repair into the spec, makes every later phase a co-owner of the separators, and still lets a fused pair that no spacing rule covers ship (FMT-SPACE-002).
- **Full structural equality, the owner of every whitespace and line break included** — rejected: the line-break phase writes the line breaks it inserts into the moved token's leading trivia (FMT-BREAK-001), which the parser gives to the token before. The requirement would force every line-break rewriter to be rebuilt for a difference no phase relies on.
- **Equal token sequences and equal trivia kinds, without the owner of a comment** — rejected: it accepts a comment behind an inserted `{` that reads back as trailing trivia of the brace, which the phases format differently from the tree they were given.
- **Keep the line break the brace transform writes after an inserted `}`, as the line ending the statement's line moved there** — rejected: the line break phase places `}` and the token after it on their lines (FMT-BREAK); a line break the transform writes there is a second owner of that decision. A line break stays only where a directive follows, because the text needs it.
- **Place `2` under `1` in its document column (20) for a target inside a moving literal** — rejected for this change: the entry point would have to lay out the target from its document column against the raw-string move, the restructuring left to thoenissen/Reihitsu#1501.
- **Write back the moved literal lines outside the target** — rejected: FMT-PIPE-010 lets the entry point rewrite only its target.
- **Keep returning the document unchanged for such a target** — rejected: the entry point would refuse a target it can find once annotations survive.
