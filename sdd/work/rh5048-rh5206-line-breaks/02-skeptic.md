Role: skeptic
Commit: 8f4e3ed2b929c2dfbc591d1b7a00e73016655b7f
Verdict: proceed narrower

My prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Premise

Lane: specs and tests for existing behavior. I checked every requirement of the 21 new specs, SHARED-CHAIN-008/009 and SHARED-WS-003 against the analyzers, the code-fix providers, `FluentChain`, `FluentChainAnalyzerBase`, `FluentChainCodeFixHelper`, `CollapseTokenGapCodeFixProviderBase`, `CommentSafeSpanReplacementCodeFixProviderBase`, `ParameterListParentPolicy`, `SyntaxTriviaUtilities.WouldJoinAcrossUnjoinableTrivia`, and `SyntaxNodeUtilities.LeadingAndInteriorContainsCommentOrDirective`. I settled the open questions with `scripts/apply-fix.sh` (LF and CRLF; no line-ending drift anywhere), `scripts/format.sh`, and `scripts/trace.sh` on fixtures outside the repository, which I deleted afterwards. RH5048 has no code fix, so `apply-fix` cannot run it. Its statements below come from reading `AnalyzeChain`, which is short enough to decide them.

Most requirements state what the code does. The exceptions fall into four groups: requirements that are false for some inputs (A), cases that already have an open question but are still written into a spec (B), shared behavior restated instead of referenced (C), and new open questions (D). Header rows, the corrected pages RH5106, RH5107 and RH5206, and the `Uses` lists match the code.

### A. Requirements that are false for some inputs (spec must change)

1. **RH5101-005, RH5102-004, RH5109-004: "in front of the opening parenthesis".** The guard reads only the `(` token's leading trivia, from the list's `FullSpan.Start` to its `Span.End`. A comment on the same line in front of `(` is trailing trivia of the token before it and does not withhold the fix. `return M /* note */ (` / `1,` / `2);` is fixed by RH5101 to `return M /* note */ (1,` / `2);`. `return M /* note */ (1, 2,` / `3);` is fixed by RH5102. RH5109 uses the same predicate. Only a comment or directive between a line break and `(` withholds the fix, which is the case the examples show. The wording needs to say that.
2. **RH5048-001 (and RH5112-002, through the existing wording of SHARED-CHAIN-004): "between the root of a chain and its first link".** RH5048 checks only `chain.RootLastToken.TrailingTrivia` plus `FirstLink.OperatorToken.LeadingTrivia`, and `IsFirstLinkBlocked` checks the same gap. That gap is the one directly in front of the first link's operator. In SHARED-CHAIN-001's model, an attached part between the root and the first link is not part of the root. So a comment in front of or inside such a part sits "between the root and the first link" but is not reported, and it does not block RH5112. Examples: `Get(/* c */ x)` / `.Where(…)`, and `a // c` / `?[0]` / `.B()`. The requirement needs to say "directly in front of the first link's operator".
3. **RH5104-001: the comments it reports.**
   - (a) A comment in the trivia of the start or end token of a multi-line interpolated string is not reported, although it is outside every interpolation. `M($"""` / `{x}` / `""" /* c */);` gives no diagnostic. The exemption covers every comment whose token has a multi-line `InterpolatedStringExpressionSyntax` ancestor, not only comments inside a hole.
   - (b) "Code" means any text that is not whitespace, another comment included. `/* a */ /* b */` on a line of its own reports `/* a */`, and the fix splits the two comments.
4. **RH5104-005: when a blank line is inserted.**
   - (a) A line above that holds only a `/* note */` comment gets a blank line, although it holds no code. `M();` / `/* note */` / `M(); // c` becomes `M();` / `/* note */` / (blank) / `// c` / `M();`. The code exempts only lines that start with `//`.
   - (b) No blank line is inserted below a `case 1:` line, although that line holds code. `BlankLineEditor.IsFirstInBlock` also counts a switch-label colon as the start of a block.
5. **RH5111-005: "anywhere in the trivia of the reported … parameter, or enum member".** Only a variable declarator is checked with its full trivia. Parameters and enum members use `InteriorContainsCommentOrDirective`. `void M(int b,` / `// c` / `int a` / `= 1)` is fixed to `int a = 1)` and the comment is kept. An enum member with a leading `/// <summary>A</summary>` is fixed too, and the doc comment is kept.
6. **RH5111-001/-002: "a simple assignment".** This includes the input of the planned RH5111 Q2: an assignment whose parent is an `= value` clause. The analyzer skips that assignment, so the sentence contradicts the code for that input. Reword it so it neither claims nor denies the case.
7. **RH5110-001: "a record".** Only `SyntaxKind.RecordDeclaration` is registered. Write "a record class", because a record struct is the input of the planned RH5110 question.
8. **RH5203-003: "MUST NOT change anything else".** The fix removes every whitespace trivia in the argument's leading trivia and appends the alignment at the end. Whenever a comment is in that trivia, the fix changes more than the indentation (see D6 and the planned RH5203 question). Limit the requirement to an argument with only whitespace in front of it since the line break.
9. **SHARED-CHAIN-009: "the statement or member that starts the chain's line".** `FormatChainAsync` takes the innermost enclosing `StatementSyntax` or `MemberDeclarationSyntax` whose first token starts a line. That node can start on an earlier line than the chain, and for a chain in an `if` header it includes the body. `if (x==1 && a` / `.B())` / `{` / `int  y = 1;` / `}` is fixed by RH5112 to `if (x == 1 && a.B())` and `int y = 1;`, so the body was formatted. With `x == 1` already spaced, only the chain changes. The fallback is triggered when the chain-only result differs from the context-formatted result in text or start column, not only by spacing in front of the chain. This belongs in the shared FC-030 question (D9).

### B. Cases with an open question that are still written into a spec

`sdd/README.md` (*Open questions*) requires these to be left out, as a requirement and as a non-goal alike.

1. The RH5101 and RH5102 non-goals list "attribute argument lists". Both specs have a planned P4 question about exactly that.
2. RH5108-001 says "MUST NOT be reported" for class and struct primary constructors, anonymous methods, and lambdas, and its example contains `class P(` / `int a)` and `(` / `int x) => x`. These are the inputs of the planned RH5108 question. Another instance for that question: `extension(` / `int value)` is not reported by RH5108, and `format.sh` joins it to `extension(int value)`.
3. RH5111-001 asserts the report of an enum member whose `=` is below its target. That is the input of the planned RH5111 Q1 (P7 and P4).
4. The RH5104-002 example contains `void M() { } /// tail`, which is the input of the planned RH5104 Q2. Under the default documentation mode, the lexer moves a trailing `///` or `/**` into the next token's leading trivia, so whether it counts as a documentation comment is exactly what Q1 and Q2 ask. Keep only a documentation comment that documents a declaration in the example.
5. Lower priority: RH5103-001 defines the predecessor by its start line, which decides the planned RH5103 Q1 (a multi-line predecessor). This is acceptable if the lead prefers precision, but it pins Q1's current outcome.

### C. Shared behavior restated instead of referenced

1. RH5048-002 restates SHARED-COMMENT-003 almost word for word, Why included. RH5048 already uses SHARED-COMMENT-001 and SHARED-COMMENT-002, so it should reference SHARED-COMMENT-003 instead. The ID is new on this branch and can be dropped.
2. RH5105-003 and RH5106-003 are the same requirement with the same Why: a file with a syntax error is not analyzed. Both analyzers use `SyntaxTreeErrorVerdictCache`, and no other analyzer does. Two specs rely on it, so it belongs under `shared/`.
3. RH5101-002, RH5105-002, RH5106-002, RH5107-002, RH5108-002, and RH5113-002 are one policy with one Why: a comment, directive, or disabled text in the join gap suppresses the diagnostic, because FMT-BREAK-001 refuses that join. All six analyzers call `WouldJoinAcrossUnjoinableTrivia`, and the Why of SHARED-WS-003 already relies on it. This is a candidate for one shared requirement.
4. Optional: the directive-comment clause of RH5104-002 restates SHARED-COMMENT-002.

### D. New open questions (not in 01-spec-author.md)

Each item gives the input, the real output, and what it touches. Leave each case out of the spec.

1. **RH5104: the fix inserts the comment inside a literal or a comment. Decided by principle P1 (#912); violates SHARED-FIX-003 (thoenissen/Reihitsu#1068).** The fix inserts the comment at the start of the line on which it starts, even when that line starts inside a multi-line literal or comment:
   - `var s = @"a` / `b" + /* c */ x;` becomes `@"a` / (blank) / `/* c */` / `b" + x;`, so the string's value changes.
   - `var s = """` / `a` / `""" + /* c */ x;` inserts a blank line and `/* c */` into the raw string's content.
   - `M("""` / `a` / `""" /* c */);` does the same.
   - `$"""` / `{x}` / `""" + /* c */ x;` does the same.
   - `/** <summary>` / `M</summary> */ void M() { } // c` inserts `// c` into the documentation comment.

   Touches RH5104-004. **Highest severity: the value of the string changes silently.**
2. **RH5101/RH5102: the fix misaligns a multi-line raw string argument (FC-028).**
   - RH5101: `M(` / `"""` / `a` / `""",` / `2);` (each on its own line) becomes `M("""` with `a` and the closing `"""` at column 9, while the opening quotes are at column 10. Formatting the whole document gives column 10.
   - RH5102: `M(1, """` / `a` / `""");` gives the same off-by-one, and RH5202 reports the result.

   The fixes call `FormatNodeInDocumentAsync` on the argument list, and node-level formatting differs from document formatting here. Touches RH5101-004 and RH5102-003, whose Why claims the fixed code is identical to formatted code. Not covered by #951. This could go to the `shared/code-fixes.md` issue or the `formatter/pipeline.md` issue.
3. **RH5103: top-level statements (P4).** `Console.WriteLine(1); Console.WriteLine(2);` at file level is not reported, because only `Block` and `SwitchSection` are registered. `format.sh` splits the two statements. Touches RH5103-001 and FMT-BREAK-006.
4. **RH5103: the fix on a block written on one line.**
   - `void N() { M(); M(); }` becomes `void N() { M();` / `M(); }`, with the moved `M();` at the method's column.
   - `Action a = () => { M(); M(); };` behaves the same way.

   The indentation of the predecessor's line is the header's indentation, not the block's. The requirement's own Why ("on a label's line, that column belongs to the label") argues against this output. Touches RH5103-003.
5. **RH5110: a block comment in front of `where`. P7 (SHARED-FIX-001, SHARED-FIX-005) and P4.** In `void N<T>()` / `/* c */ where T : class` the clause is reported. The fix inserts a blank line between the header and the comment and never clears the diagnostic (`no-progress`). `format.sh` gives the same output, so RH5110 reports formatted code, and the formatter puts a blank line inside a declaration header. Touches RH5110-001, RH5110-003, FMT-BREAK-023, and FMT-BLANK.
6. **RH5203: a block comment in front of the argument on its line. P7 and P4; belongs to the planned RH5203 question.** `M(1,` / `/* two */ 2);` (comment indented 12) is reported. The fix writes `/* two */` at column 0 followed by 10 spaces and `2`, and then makes no progress. `format.sh` gives `M(1,` / (blank) / `/* two */ 2);` at column 10, which RH5203 still reports, with a blank line inserted inside an argument list. Touches RH5203-001 and RH5203-003.
7. **RH5206: a block comment in front of an arm. P7 (SHARED-FIX-005) and P4.** In `status switch` / `{` / `/* idle */ 0 => "Idle",` / `_ => "Other"` / `}`, with every line correctly aligned, the expression is reported. The fix changes nothing (`no-progress`), and the formatter leaves the code as it is. Touches RH5206-001 and RH5206-003.
8. **RH5111: a construct nested in the value of an assignment (FC-027, FC-001).** `GetFormattingNode` looks for `AssignmentExpressionSyntax` first. A diagnostic on `var x` / `= 1;` inside `a = () => { … };` therefore formats and guards the whole outer assignment:
   - With `int  y = x;` in the same body, the fix also respaces that statement and moves the lambda's braces.
   - With a `// other` comment anywhere in the lambda body, no fix is offered.

   Touches RH5111-004 and RH5111-005.
9. **Additions to the planned `shared/code-fixes.md` question about fixes that format more than their diagnostic.**
   - The SHARED-CHAIN-009 fallback (A9): an `if` body gets formatted.
   - The RH5101 and RH5102 fixes also format inside arguments: `M(` / `1+2,` / `3)` becomes `M(1 + 2,` / `3)`.
   - D8.
   - The RH5204 `else` case, in which the rule's own diagnostic count rises from 4 to 8, is an FC-007 instance and not only an FC-030 one.

   If the maintainer rules that a fix must not change code its diagnostic is not about, RH5101-004, RH5102-003, RH5110-003, RH5111-004, RH5113-004, RH5204-006, RH5206-003, and SHARED-CHAIN-009 all change. The question should list them.
10. **RH5104-005: the `/* */` case (A4a).** If the behavior is not simply reworded into the spec, it is a question. The inserted blank line separates `/* note */` from the code it stood above.
11. **Follow-up candidate, formatter (FMT-BREAK-027 / FMT-INDENT-028).** For `Func<int, int> c = [Obsolete] (x) =>` / `{…}`, the formatter breaks the line after the lambda's attribute list, although FMT-BREAK-027 is about declarations. The RH5205 fix puts `(x) =>` and `{` at column 27, while formatting the document puts them at column 8 (FC-028). This touches RH5205-003 only incidentally.

### Notes for the lead (decided follow-ups)

- **thoenissen/Reihitsu#956** (one switch-expression arm per line): the RH5206-003 example output keeps `0 => "Idle", _ => "Other"` on one line. That is the formatter behavior #956 changes, and the RH5206 fix delegates to the formatter. Better use an example whose arms are already on separate lines, and post a one-line comment on #956 saying that RH5206-003 and its tests follow the change.
- **thoenissen/Reihitsu#1097** states as its premise that RH5048 "keeps reporting a comment before the first call of a wrapped chain, so the two rules do not report the same comment". The code reports the gap before the first link, not the first call. That is SHARED-CHAIN-004, pinned by `VerifyCommentBeforeWrappedCallAfterPropertyPrefixIsNotReported` and set by #887. Consequences:
  - (a) `M(items` / `// c` / `.Where(…))` is reported by RH5048 and by RH5038, today and after #1097.
  - (b) `M(a.P` / `// c` / `.B())` is reported by neither rule after #1097.

  The RH5048 spec correctly specifies today's behavior, and the lead was told not to anticipate #1097. This is a premise for the maintainer to recheck on #1097, not a blocker for this run.

## Mechanism and candidates

_N/A._ This is not a bug fix. The candidate sets I derived from the dispatch code are under Breaking inputs.

## Breaking inputs

Candidates derived from the registrations and type hierarchies. The test implementer should cover them, or avoid them where they are open questions (marked "do not pin").

- **ArgumentListSyntax owners** (RH5101/RH5102/RH5203 register `SyntaxKind.ArgumentList`): invocation, object creation, implicit object creation, constructor initializer, primary-constructor base type, and **`WithElementSyntax`**. `List<int> l = [with(` / `10), 1];` is reported and fixed by RH5101. RH5101-001's list omits it, so add it or leave it untested on purpose.
- **ParameterListSyntax parents** (`ParameterListParentPolicy` for RH5105/RH5106; every parent for RH5107/RH5109): method, constructor, destructor, local function, operator, conversion operator, delegate, record (class and struct), class, struct and interface primary constructor, extension block, parenthesized lambda, anonymous method.
- **RH5108**: only the registered kinds. Do not pin the primary-constructor, lambda, anonymous-method, and extension cases (B2).
- **RH5110**: class, struct, interface, record class, method, delegate, local function. Do not pin record struct, `/* c */ where` (D5), or extension blocks with constraints. Neither surface touches the last one.
- **RH5111**: simple assignment (also deconstruction and `with` initializers), declarators (field, local, `for`, `using`, `fixed`, `const`, event field), property initializer, parameter default (lambda parameters included), enum member. FC-029: all four combinations of single-line or multi-line target and value. FC-028: the declarator, parameter, and enum-member paths use `FormatNodeInDocumentWithContextAsync`, the assignment and property paths use `FormatNodeInDocumentAsync`, so test each path. Do not pin D8, Q1, or Q2.
- **RH5112/RH5201 chain spine**: links `.`, `?.`, `!.`, `!?.`; attached parts (argument list, `[ ]`, `?[ ]`, `!`); a null-conditional assignment target; a link behind a multi-line raw string literal; a multi-line root.
- **RH5204**: `ShouldAnalyzeToken` also checks the identifier of a labeled statement (`L:`), which is neither a keyword nor a modifier; a line that starts with an attribute list is never checked. Comment lines are checked only in front of checked tokens.
- **FC-002/FC-003**: the RH5101/RH5102/RH5109 fix guard with a comment in the `(` leading trivia, on the same line in front of `(`, inside the list, and behind `)`. These are four tests (A1).
- **FC-019/FC-020/FC-021**: for each join rule (RH5101, RH5105–RH5108, RH5112, RH5113), a comment, a directive, and an `#if false` block in each half of the gap (trailing trivia of the token before, leading trivia of the moved token), plus a `/* */` that ends mid-line.
- **FC-008, Fix All with overlapping scopes**:
  - RH5204: two misindented lines in one class, where both fixes format the class.
  - RH5112/RH5201: a chain inside an argument of a reported chain.
  - RH5101/RH5102: a nested argument list reported together with its outer list.
  - RH5110: the class's fix formats a method that carries its own diagnostic.
- **FC-026**: three statements on one line (RH5103) and three directives on one line (RH5114), under Fix All.
- **FC-010/FC-017**: CRLF variants for every fix that inserts a line break (RH5103, RH5104, RH5107, RH5109, RH5114). `apply-fix` showed no drift.
- **FC-033**: run RH5104 tests under the default documentation mode only; `DocumentationMode.None` is Q2 (do not pin).
- **FC-022 (do not pin)**: raw-string arguments for RH5101/RH5102 (D2), and literal or comment lines for RH5104 (D1).

## Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| Analyzers of the 21 rules | Must stay (lane: no production change) |
| Code-fix providers, Fix All (BatchFixer for all) | Must stay |
| `Reihitsu.Core` (`FluentChain`, `SyntaxTriviaUtilities`, `SyntaxNodeUtilities`) | Not involved |
| Formatter | Not involved; only open questions D2, D3, D5, D6, D7, D11 |
| Specs (`analyzer/RH5048`–`RH5206`, `shared/fluent-chain-layout.md`, `shared/whitespace-removal-fixes.md`) | Must change: A1–A9, B1–B4, C1–C3 |
| Rule pages | RH5106, RH5107 and RH5206 corrections verified against the code. Optional: the RH5101 page says the fix "removes the line break", while it lays out the whole list (RH5101-004). Pages that describe the RH5104 fix must not promise the D1 inputs |
| Tests | Test implementer: tags and focused tests, avoiding the "do not pin" inputs |

## Simpler option

A shared requirement for "a comment, directive, or disabled text in the join gap suppresses the diagnostic" (C3) and one for "a file with a syntax error is not analyzed" (C2). Together they replace eight near-identical rule requirements with references.

## Questions for the maintainer

None that blocks this run. The #1097 premise (Notes for the lead) is for the maintainer to recheck on that issue. Today's RH5048 behavior is already specified in merged SHARED-CHAIN-004, so this spec has no competing reading to choose between.
