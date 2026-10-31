# The documentation phase moves every documentation comment written behind code

| | |
|---|---|
| Status | Draft |
| Date | 2026-10 |
| Affected requirements | FMT-DOC-001, FMT-DOC-002 (clarified), FMT-DOC-003, FMT-DOC-004 (clarified), FMT-DOC-012, FMT-BLANK-012 |
| Depends on | [One line-start fact for analyzers and every formatter phase](line-start-fact.md), [Every formatter phase returns a tree that round-trips and keeps foreign annotations](phase-tree-round-trip.md) |

## What

One phase decides whether a documentation comment written behind code starts a new line: the documentation-comment phase, for single-line (`///`) and delimited (`/** … */`) comments alike. Brace placement keeps one case that no phase decides: a delimited comment behind an opening brace with code behind it on the brace's line goes onto the next line together with that code, as today.

- FMT-DOC-001 is widened to delimited documentation comments. The phase starts a new line in front of such a comment, removes the whitespace in front of it, and keeps the preceding code on its line; code the author wrote behind the comment on its line stays behind it. It moves exactly the delimited comments that are moved today: the comment stands behind the token in front of it, or behind a comment that trails that token, and the token behind it opens the node it belongs to.
- FMT-DOC-003 is widened to delimited comments: a delimited comment that documents nothing stays behind code, as it does today.
- FMT-BLANK-012 is narrowed to the blank line. A documentation comment that FMT-DOC-001 moved gets the blank line of FMT-BLANK-011 in the same run; the blank-line phase inserts no line break in front of any documentation comment.
- FMT-DOC-012 states that FMT-DOC-001 and FMT-DOC-003 decide where a delimited comment starts its line, and that moving it rewrites none of its text. FMT-DOC-002 and FMT-DOC-004, which act on single-line comments only, now say so in their normative sentence, since the spec as a whole no longer covers single-line comments only.
- The non-goals of FMT-DOC and FMT-BLANK follow: FMT-DOC excludes only the text of delimited comments; FMT-BLANK inserts no line break in front of any documentation comment.

The reach of FMT-DOC-001 does not grow. It decides comments in front of a type or member declaration, enum members included, for both forms, and leaves every other position undecided for both forms, as it does for `///` today: before a statement, a using or `extern alias` directive, or an assembly attribute list; before an `else` or `catch` clause, an accessor, a parameter or type parameter, a base-list entry, or an initializer element; between an attribute list and its declaration; and inside an expression. For a delimited comment it also leaves undecided one behind an earlier comment of the same token's leading trivia (the case FMT-DOC-002 decides for `///`), one behind the colon of a switch label, and one behind an opening brace.

## Why

Today three phases each decide with a predicate of their own whether a documentation comment written behind code starts a new line: the documentation-comment phase for `///`, the blank-line phase for `/** */` (inserting the line break together with the blank line), and brace placement in the line-break phase for whatever reaches it without a line break in front. The output is right only because the documentation-comment phase runs first, so the later copies see comments that already start their line. A phase order in which the line-break phase runs before the blank-line phase would place a brace behind a delimited comment that still stands behind code. One owner, deciding before every phase that places lines or blank lines, removes that dependency on the phase order.

Brace placement stays the only component that acts on a delimited comment behind an opening brace with code behind it on the brace's line. No other phase moves that comment, so its result does not depend on the phase order.

## Visible output changes

None — no output an end user sees changes. Every input keeps today's output, including the delimited cases the blank-line phase handles today (behind code before a member, behind a comment that trails code, before a block statement, before a using directive) and the delimited cases that stay where they are today (stranded, behind an opening brace at the end of its line, behind a switch label, behind an earlier comment of the same leading trivia), and the delimited comment behind an opening brace with code behind it on the brace's line, which brace placement carries onto the next line with that code:

```csharp
class C { /** d */ void M() { } }
```

```csharp
class C
{
    /** d */ void M()
    {
    }
}
```

## Decisions

### FMT-DOC-001 decides both forms, with the reach the code gives delimited comments today

The widened requirement names the delimited cases moved today as decided where FMT-DOC-001 already decides `///` — before type and member declarations — and states its reach in a coverage sentence everywhere else. A delimited comment that ends the line of an opening brace stays where it is, as FMT-INDENT-006 already shows; one behind a switch label and one behind an earlier comment of its own leading trivia stay too, but no requirement decides them, because `///` in the same places moves and the difference has no reason of its own. Whether those delimited comments should move like `///` is a follow-up question for the maintainer; this change keeps their output.

### Brace placement keeps carrying a delimited comment that has code behind it

When code follows a delimited comment on the line of an opening brace (`class C { /** d */ void M() { } }`, `if (a > 0) { /** d */ a++; }`), brace placement moves that code onto the next line, and the comment, which Roslyn files as the code's leading trivia, goes with it: `{` / `/** d */ void M()`. The documentation-comment phase does not move this comment, because FMT-DOC-001 leaves a delimited comment behind an opening brace undecided and moving it would also move one that ends the brace's line, against FMT-INDENT-006's example. Brace placement therefore keeps this case, and the issue's goal that brace placement no longer moves any documentation comment is dropped for it: keeping that goal would change the output. Whether FMT-BREAK-003's comment behind an opening brace includes a documentation comment is part of the follow-up question on delimited comments behind an opening brace. For `///` behind an opening brace, brace placement already moves nothing: the documentation-comment phase moves it first.

### Comments before a statement stay undecided for both forms

FMT-BLANK-012 asked the blank-line phase to move a delimited comment "when the comment precedes the code it documents". A documentation comment before a statement documents nothing — the compiler reports CS1587 there — so FMT-BLANK-012 did not decide that case, although the code moves it and tests pin the output. FMT-DOC-001 already leaves the same case open for `///`. The widened requirement keeps it open for both forms instead of deciding it for one, so one decision later covers both. The code keeps moving such comments, and the existing tests keep their outputs.

### A delimited comment in front of a using directive is moved, as today

A `/** */` comment that leads a using directive behind another using directive on its line is moved by the documentation-comment phase, exactly as a `///` comment there is moved today and as the blank-line phase moves the delimited one today. FMT-DOC-001 does not decide comments before a using directive for either form, and the blank lines inside a using block stay with SHARED-USING-008 and the open case of FMT-BLANK-011.

### The blank-line phase keeps only the blank line

FMT-BLANK-012 no longer moves anything: the blank-line phase inserts no line break in front of a documentation comment, single-line or delimited. It states that a moved comment gets FMT-BLANK-011's blank line in the same run, including the first-in-block exemption, and that a stranded comment gets none because it does not start its line.

## Alternatives considered

- **The blank-line and line-break phases own the move, and the documentation phase only normalizes `///` text** — rejected: the move would stay split by comment form and would still depend on the order of the blank-line and line-break phases, which is what this change removes.
- **Brace placement no longer moves a documentation comment; the documentation-comment phase also moves a delimited comment behind an opening brace** — rejected: it changes visible output (`class C { /** d */` at the end of the brace's line moves too, against FMT-INDENT-006), which this change does not.
- **Decide comments before a statement for delimited comments only** — rejected: it would decide one form and leave the other open for the same input, so the two forms could drift apart once the open case is decided.
- **Move a delimited comment behind an opening brace, a switch label, or an earlier comment of its own leading trivia, like `///`** — not taken here: it changes visible output, which this change does not, and contradicts the example of FMT-INDENT-006. It is a separate question for the maintainer.
