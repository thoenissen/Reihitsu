Role: spec author
Commit: 5359fea
Verdict: spec written

Repair round 2 after the audit (`14-auditor.md` findings 1–3 and the architecture finding), on the maintainer's answers 1A and 2A and the maintainer's guidance as clarified in `00-lead.md` (Decisions taken rows 17, 18, 21; the coordinator's mid-task message about the clarified guidance). The prompt named the changes it asked for. It carried no suspected cause or hoped-for verdict. I re-derived each change from the handovers, the specs, and formatter runs, and none of the named changes was used as given without that check.

I checked the outputs with the CLI built from the branch (4313e44 production code) and from `main` (741157c), two passes each, in the session scratchpad. "Before" in the change doc is `main`'s output, and it equals the branch's for every new entry except 13 (2A, where the branch already removes the line). I compared every FMT-BLANK-018 example and the new FMT-BLANK-020 example against the branch output: they differ only in the lines this round changes.

## Changed requirements

- FMT-BLANK-018 — renamed "A comment glued to a token belongs to the token's line" — changed. The token list is gone; the new rule follows the maintainer's guidance:
  - A glued comment never gets the FMT-BLANK-011 blank line.
  - The blank lines above a glued comment are decided as the blank lines directly before its token:
    - removed where FMT-BLANK-007/008/019 remove them: every brace (except the `{` of a block statement after a statement), the chained keywords, `]`, and the pattern `)`;
    - decided by the statement rules, and by FMT-BLANK-017 for a block statement's brace;
    - kept as written elsewhere: a member, the `)` of an argument list.
  - What goes:
    - the complex-element layout qualifier (answer 1A, auditor finding 1, architecture finding);
    - the assigned-collection-initializer `}` exclusion;
    - the documentation-comment condition (a documentation comment *between* the glued comment and the token);
    - the array-initializer, extension-block, interpolation, and property-pattern brace exclusions;
    - the implicit "any other token gets FMT-BLANK-011".
  - The directive-in-gap clause stays: answer 5 and SHARED-BLANK-006 decide it.
  - "Glued" now allows any comments between the comment and the token, and still requires an *ordinary* comment to start the line.
  - New examples replace the array/argument-list, complex-element, assigned-`}`, and documentation-comment examples.
- FMT-BLANK-011 — changed. The glued exception now reads "glued to a token as FMT-BLANK-018 states it, whatever that token is"; it no longer names FMT-BLANK-018's tokens or the FMT-BLANK-017 brace.
- FMT-BLANK-020 — changed:
  - It states its own token list, which used to be FMT-BLANK-018's set:
    - all complex-element `{` (1A);
    - the assigned collection initializer `{` (2A);
    - the assigned collection initializer `}` stays excluded, and so do the array-initializer, extension-block, interpolation, and property-pattern braces.
  - The trigger is widened (auditor finding 2): "the last content is an ordinary comment that ends its line; what stands in front of it on its line does not matter". This restores `main`'s output for `/* a */ /* b */`, `/* a */ // b`, `/* x⏎ y */ // b`, and `/** d */ // c`.
  - The Why is rewritten, and a new example is added.
- Change doc `sdd/changes/2026-11/blank-line-sole-owner.md`:
  - Status goes back to **Draft**, because entry 18 has no recorded answer (see *Output questions*).
  - FMT-BLANK-020 is now "(added)", no longer a clarification.
  - Rewritten: *What* (FMT-BLANK-018 and FMT-BLANK-020), *Precedence* (FMT-BLANK-011 bullet), *Why* (non-idempotency), and *Unchanged*. *Unchanged* now covers finding 2 and adds an entry for the exclusions FMT-BLANK-020 keeps.
  - Visible output changes 12–18 are added:
    - 12 — a comment glued to a complex-element `{`, whatever its layout (1A);
    - 13 — an own-line comment plus a blank line above an assigned collection initializer's `{` (2A);
    - 14 — a comment glued to an assigned collection initializer's `}` (guidance);
    - 15 — a documentation comment between a glued comment and the brace (guidance);
    - 16 — a comment glued to an array-initializer, extension-block, interpolation, or property-pattern brace, or to a chained keyword: no blank line, the author's removed (guidance);
    - 17 — a comment glued to a statement, a member, or an argument list's `)`: none inserted, the author's kept (guidance);
    - 18 — an own-line comment plus a blank line above a one-line complex element's `{`: the blank line is removed (follows from 1A; not glued).
  - *Decisions*:
    - answers 1A and 2A and the guidance (including its clarification) are recorded;
    - "FMT-BLANK-018 keeps today's output" is replaced by "FMT-BLANK-018 decides a glued comment as part of its token's line";
    - "FMT-BLANK-020 is a clarification" is replaced by "FMT-BLANK-020 names its own tokens".
  - *Alternatives*: the earlier token-list draft, and "a documentation comment that starts its line counts as glued", each with its reason.

No new requirement ID. SDD architecture tests (`--filter FullyQualifiedName~Sdd`): 51/51 passed. No rule page changes.

## Examples for the tests

From FMT-BLANK-018, FMT-BLANK-020 (new example), and change-doc entries 12–18.

**Tests whose expected output changes** (`src/Reihitsu.Formatter.Test/Regression/BlankLines/GluedCommentBlankLineTests.cs`):
- `SeparatesCommentGluedToArrayInitializerBraceAndArgumentListParenthesis`: no blank line above `/* b */` or `/* e */`.
- `SeparatesCommentGluedToComplexElementInitializerBrace` and `SeparatesCommentGluedToOneLineComplexElementInitializerOnly`: no blank line above the comment in any element, and the author's blank line is removed.
- `SeparatesCommentsGluedToExtensionBlockBraces`: no blank line above either comment.
- `SeparatesCommentGluedToInterpolationClosingBrace`: expects `false`.
- `SeparatesCommentGluedToClosingBraceOfAssignedCollectionInitializerOnly`: no blank line in either initializer.
- `RemovesAuthorBlankLineAboveCommentGluedToClosingBraceOfUnassignedCollectionInitializerOnly`: the assigned initializer loses the blank line too.
- `SeparatesCommentInFrontOfDocumentationCommentOnBraceLineOnly`: `/* x */ /** Doc */ {` gets none; `/** Doc */ {` keeps its blank line.

Rename tests whose names no longer fit. Search the formatter and analyzer test projects for other expectations of a blank line above a comment glued to a statement, a member, a chained keyword, a property pattern, or an argument-list `)`. FMT-BLANK-011 used to insert those.

**New tests:**
1. Finding 1 (two-pass, LF and CRLF): a comment glued to a complex element whose contents the line-break phase expands, with no blank line in pass 1 or pass 2. Cover a lambda block body, an object initializer, a collection initializer, and an anonymous object. Also cover the author's blank line above such a glued comment, and FMT-BLANK-020 with `// c⏎⏎{ 2, new P { A = 2 } }` (the blank line is removed in one pass).
2. Finding 2: `/* a */ /* b */`, `/* a */ // b`, `/* x⏎ y */ // b`, and `/** d */ // c` followed by a blank line before a method-block `}`, a type-member-body `{`, an `if`-body `{`, an object-initializer `}`, a type-body `}`, and a collection-expression `]`. The blank line is removed. Also cover the parenthesized-pattern `)`, which keeps it on `main` too (the auditor's "Holds").
3. Entry 13 (2A): `L =⏎// c⏎⏎{`; the blank line is removed.
4. Entry 17: a glued comment above a statement (none inserted, the author's kept), above a member, and above an argument-list `)`. Add `}⏎/* s */ a++;`, which gets the SHARED-BLANK-003 blank line above the comment.
5. Entry 16: a glued comment above `else`/`catch`/`finally` and above a property-pattern `{` (removed or none inserted).
6. Entry 18: an own-line comment plus a blank line above a one-line complex element `{` (removed). Hold this test until the lead settles the output question below.
7. Unchanged, to pin: an own-line comment plus a blank line above an assigned collection initializer's `}` (kept, `Unchanged` entry), and above an array-initializer `{` (kept).

The maintainer's answers 3, 4, 5, and 7 keep their outputs under the new FMT-BLANK-018. The existing tests for them must pass unchanged.

**Notes for the implementer** (not for the spec):
- FMT-BLANK-011's skip needs no token check any more.
- The removal above a glued comment applies wherever the cleanup rewriter applies FMT-BLANK-007/008/019 to the token.
- `BraceBlankLinePolicy.KeepsGluedCommentWithoutBlankLine` becomes FMT-BLANK-020's set only. It still reads `IsAssignedCollectionInitializer` for the `}` exclusion, so auditor finding 5 (the `RootConstruct` summaries) still applies.
- Finding 4 (one glued predicate) fits the new rule: "comment glued to the token" is a single predicate.

## Output questions

**1 · Formatter · own-line comment and blank line above a complex element initializer that stays on one line** (change-doc entry 18; FMT-BLANK-020)

The comment stands on its own line between two elements and shares no line with code, so the guidance does not cover it. Answer 1A ("every complex element `{` is treated like the other initializer braces") decides the glued case. Applied to FMT-BLANK-020 as well, it gives A. No rule that reads no layout keeps `main`'s output for both one-line and multi-line elements.

Input:
```csharp
class C
{
    Dictionary<int, int> D = new()
                             {
                                 { 1, 2 },

                                 // c

                                 { 3, 4 }
                             };
}
```

Before (`main`): input unchanged. Before a multi-line element, `main` removes the blank line. For an element that the line-break phase expands, `main` decides differently in two passes.

After, A (recommended): the blank line between `// c` and `{ 3, 4 }` is removed, as before every other initializer brace and before a multi-line element.

After, B: the blank line is kept before every complex element, so a multi-line element (`// c⏎⏎{⏎5,⏎6⏎}`) now keeps it where `main` removed it.

Why A: it is answer 1A's "like the other initializer braces" applied to the second rule that reads the same token set. RH5025 reports the blank line that B keeps.

The lead may treat this as covered by 1A. In that case, set the change doc back to `Accepted` and record it under *Decisions*.

## Internal choices

- **FMT-BLANK-018 as "a glued comment belongs to its token's line"**:
  - Alternative: drop only the named special cases (the complex-element qualifier, the assigned `}` exclusion, the documentation-comment condition) and keep the token list.
  - I chose the rule without a list because the clarified guidance covers every comment that shares a line with code, and this rule names no construct.
  - It makes the FMT-BLANK-017 exception follow from FMT-BLANK-017 instead of being stated.
  - It keeps answers 3, 4, 5, and 7.
- **FMT-BLANK-020 states its own token list** instead of sharing FMT-BLANK-018's, which no longer has one.
- **Glued still requires an ordinary comment to start the line.** A documentation comment that starts its line stays under FMT-BLANK-011/012:
  - RH8303 asks for a blank line above documentation that follows a member;
  - the guidance leaves documentation comments' own blank lines alone.
  - Alternative: any comment. That would remove a blank line RH8303 asks for.

## What I did not simplify, and why

- **FMT-BLANK-020's exclusions are kept:**
  - the array-initializer, extension-block, and interpolation braces;
  - the property-pattern `{`;
  - the assigned collection initializer's `}`.

  They decide the blank line between a comment on its own line and the brace. That comment shares no line with code, so the guidance does not reach it. Removing them would change outputs for comments that are not glued. The `=` part of the guidance touches one member: `int[] G =⏎// c⏎⏎{ 1, 2 };` keeps the blank line today. But the array exclusion does not exist only for that case (`new int[]⏎// c⏎⏎{`, and before an array's `}`), so I left it. Dropping all of them would also settle the existing follow-up candidate: RH5024/RH5025 report several of those kept outputs (03-spec-author.md). I recommend deciding it there, not in this run.
- **FMT-BLANK-018's directive clause** (removes nothing when a directive stands in the gap). The maintainer decided it in answer 5, and SHARED-BLANK-006 requires the line below a region directive.
- **A documentation comment that starts its line** is not made glued (see *Internal choices*).

## Follow-up candidates

_None new._ The existing candidate about RH5024/RH5025 reporting the formatter's kept blank line behind an own-line comment before array-initializer, extension-block, and property-pattern braces now also covers the assigned collection initializer's `}` and the interpolation braces. These are the exclusions FMT-BLANK-020 keeps.

## Correction after the lead's decision (row 22)

The lead decided the output question above as covered by maintainer answer 1A (`00-lead.md`, *Decisions taken*, row 22). The change doc now marks visible output change 18 as decided by 1A, in its heading note, in the numbering paragraph of *Visible output changes*, and in answer 1 under *Decisions*, and its status is back to `Accepted`. The change doc held no separate list of open questions; entry 18 was open only through the `Draft` status and its "follows from entry 12" framing, both now replaced. No requirement, example, or output changed. The prompt carried no intended fix or verdict beyond restating row 22.
