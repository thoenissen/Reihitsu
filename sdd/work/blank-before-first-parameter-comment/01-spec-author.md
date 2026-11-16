Role: spec author
Commit: 869c867
Verdict: change doc written

The prompt carried no intended fix or verdict beyond the issue text; the issue's own option A and its "Done when" are proposals (`agent-filed`), weighed below.

## Changed requirements

- FMT-BLANK-011 — A comment on its own line is preceded by exactly one blank line — changed: the exemption names the first content behind every opening token (any `{`, any `(`, any `[`, and the `<` of a type parameter list, type argument list, or function pointer parameter list) and the first content of a switch section; a comment behind a comma, an operator (`<` as operator included), or any other token keeps its blank line; the requirement neither inserts nor removes a blank line behind an opening token (FMT-BLANK-007 removes one behind `{`, one behind `(`/`[`/`<` is kept). Clarified in the same edit: the brace exemption covers every opening brace, not only blocks, type bodies, and switch sections — that is today's behavior (`BlockStartUtilities.IsFirstInBlock` checks `OpenBraceToken` only by kind; verified with an anonymous object and a collection initializer). Four new examples.
- RH5020-002 — The first comment behind an opening token, of a switch section, or of a file is not reported — changed (title, token list, why, two examples).
- RH8303-002 — The first documentation comment behind an opening token is not reported — changed (title, token list, why, two examples).
- No new ID; `requirement-ids.txt`, code owners, and `Uses` unchanged. `scripts/test.sh --project architecture`: 153 passed.
- Change doc: `sdd/changes/2026-11/first-comment-behind-opening-token.md` (Status: Draft).
- Rule page: `documentation/rules/RH5020.md`, *Exceptions* bullet extended. `documentation/rules/RH8303.md` left unchanged: a documentation comment on a parameter or type parameter is a corner case, and the page's exemption list is not meant to be complete.

## Examples for the tests

FMT-BLANK-011 (the anonymous-object example, the opening-token example, the documentation-comment example, the comma/author-blank-line example), RH5020-002 (opening-token example: not reported; comma example: reported), RH8303-002 (first parameter/type parameter: not reported; behind a comma: reported). The change doc's entries 3 and 7 add further formatter inputs (attribute argument list, indexer, tuple, array size, element access) and code-fix results (RH3004, RH5305).

How the examples were checked, all on 869c867:

- Every *before* in the change doc and every formatter example input was run through `scripts/format.sh`; the *before* blocks are that output verbatim. Every *after* is the *before* minus the blank line above the comment; indentation was checked unchanged where the author wrote no blank line (FMT-BLANK-011 example `N`: a written blank line behind `(` is kept today, also behind `[` and `<`).
- A second `scripts/format.sh` pass over the outputs changed nothing.
- `scripts/apply-fix.sh RH5020` reports and fixes the comment behind `(` of a record and of a union and behind `[` of a collection expression; not behind `{` of an array initializer.
- `scripts/apply-fix.sh RH8303` reports and fixes a `///` behind `(` of a record parameter list, behind `<` of a type parameter list, and behind a comma.
- Code fixes: RH3004 (anonymous method → lambda) and RH5305 (collection expression) write the blank line today (change-doc entry 7). RH3202, RH3208, RH5106 keep the comment-first parameter list as written (no blank line inserted). RH5109 offers no fix on a parameter list with a leading comment; RH5101, RH5102, RH5306, RH5307 did not report on the fixtures tried.
- Today the formatter also moves `/** first */` written behind `(` onto a line of its own and puts a blank line above it (change-doc entry 4). FMT-DOC-001 does not decide that position. `DocumentationCommentFormattingPhase.IsMovableDelimitedDocumentation` reads the same `BlockStartUtilities.IsFirstInBlock` fact as the blank-line phase; entry 4's *after* keeps the move and drops only the blank line.

## Output questions

**1 · Formatter, RH5020, RH8303 — a comment that is the first content behind an opening parenthesis, bracket, or `<`**

Input:
```csharp
public record R(
    // first
    int A,
    int B);

class C
{
    int[] F = [
        // first
        1,
        2
    ];
}
```

Before (`main`):
```csharp
public record R(

                // first
                int A,
                int B);

class C
{
    int[] F = [

                  // first
                  1,
                  2
              ];
}
```

After, A — parameter lists only (the issue's proposal: the `(` of a parameter list or a union's case-type list, the `[` of an indexer's parameter list):
```csharp
public record R(
                // first
                int A,
                int B);

class C
{
    int[] F = [

                  // first
                  1,
                  2
              ];
}
```

After, B — keep today's output: as *Before*.

After, C (recommended) — every opening parenthesis, bracket, and generic `<` (parameter, argument, attribute argument, type parameter and type argument lists, tuples, collection expressions, list patterns, element access, array sizes, statement headers, casts, parenthesized expressions and patterns):
```csharp
public record R(
                // first
                int A,
                int B);

class C
{
    int[] F = [
                  // first
                  1,
                  2
              ];
}
```

In A and C, a comment behind a comma keeps its blank line (`void M(int a,⏎⏎       // second⏎       int b)`), and an author's blank line behind the opening token stays. RH5035 keeps reporting the comment in the parameter list in every option.

Why C: the opening token already separates the comment from the code above, as an initializer's or a block's `{` does today; A changes output only for comments RH5035 asks the user to remove and leaves the collection expression, where such a comment is commonly written and allowed, with the blank line. What would change the pick: if the maintainer wants this run kept to the issue's construct, A — the collection-expression case then becomes a follow-up. B fits the earlier decision that an unwanted comment shape gets no RH5020 exemption and is fixed by hand (`sdd/changes/2026-11/blank-line-sole-owner.md`, "RH5020 keeps reporting a comment glued to `{`"), but it leaves the comment detached from the element it describes.

Neighbours the answer reads as a principle for: under C, all of the constructs listed; under A, only parameter lists, and the collection expression, list pattern, argument list, and tuple stay as *Before*; behind `{` nothing changes in any option. The documentation-comment cases (change doc entry 4) follow the same answer.

**2 · RH5020 · Comments should be preceded by a blank line, and RH8303 · Element documentation header must be preceded by blank line — the same comment, when the answer to 1 is A or C**

Input: the input of question 1.

Before (`main`): RH5020 reports `// first` in both places; its code fix inserts the blank line.

After, A (recommended): not reported, in exactly the places question 1 exempts. RH8303 likewise for a `///` first in a parameter or type parameter list.

After, B: still reported; the code fix inserts the blank line, and the formatter keeps the blank line the fix wrote.

Why A: otherwise the formatter's output is reported by a rule it mirrors, and for a collection expression the user gets a warning on a comment no rule forbids.

## Internal choices

- The spec diff is written on option C of question 1 and option A of question 2; for 1A the token lists of FMT-BLANK-011, RH5020-002, and RH8303-002 narrow to the parameter-list openers, for 1B the three requirements revert to `main` and only the brace clarification of FMT-BLANK-011 remains (a clarification of today's behavior, no change doc). Alternative: write no spec until the answer — rejected, the draft makes every option's diff mechanical.
- The exemption is an edit of FMT-BLANK-011, RH5020-002, and RH8303-002, not a new shared requirement: the SHARED-BLANK non-goals leave the comment boundary to RH5020 and FMT-BLANK. Alternative: a SHARED-BLANK requirement referenced by all three.
- The brace clarification (every `{`, not only block/type body/switch section) is folded into the FMT-BLANK-011 edit rather than written as its own requirement.
- FMT-BLANK-012 is unchanged: a documentation comment FMT-DOC-001 moves always precedes a type or member declaration, which never follows `(`, `[`, or `<`.
- The author's blank line behind `(`, `[`, `<` is kept (today's behavior, FMT-BLANK-019's decision for `[`); removing it would be a further output change no issue asks for, so it is not offered as an option.
- RH5020 rule page updated (the common collection-expression and argument-list cases change for the user under C); RH8303 page left alone. Under 1A the RH5020 page edit is reverted (a parameter-list comment is reported by RH5035 anyway).

## Follow-up candidates

**Formatter · indentation of the first element behind an opening token that no alignment rule covers** (outside the issue; the inputs are reported by RH5039, RH5044, RH5046, except the cast and the array size — the lead may drop it as rare by definition)

Input:
```csharp
class C
{
    void M<
        // first
        T>(object o, int a)
    {
        if (
            // first
            a > 0)
        {
            a = (
                // first
                int)o;
        }
    }
}
```

Before (`main`), blank lines aside:
```csharp
    void M<
    // first
    T>(object o, int a)
    {
        if (
        // first
        a > 0)
        {
            a = (
            // first
            int)o;
        }
    }
```

The comment and the element land in the column of the statement or member that holds the opening token, not indented under it or aligned behind it as an argument list's are (`M(⏎         // first⏎         a,`). The same holds for the opening bracket of an attribute list, `new int[` with its size, a parenthesized expression, and a parenthesized or positional pattern. Readings: A — indent one level deeper than the line of the opening token; B — align behind the opening token, as FMT-INDENT-012 aligns argument and parameter lists; C — keep. Recommendation: B for consistency with the lists FMT-INDENT-012 covers, or drop the candidate as rare.
