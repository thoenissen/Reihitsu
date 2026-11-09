Role: spec author
Commit: 679baae0
Verdict: change doc written

Second amendment. The maintainer chose A for questions 9 to 11. These answers are now in FMT-UNSUP-004, RH2001-007, and the change doc. Nothing visible is open any more. The lead asked me to take the change doc out of Draft, so `sdd/changes/2026-11/unsupported-precheck.md` is now `Status: Accepted`. `sdd/README.md` gives that step to the lead, so the lead should confirm it.

The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Changed requirements

- FMT-UNSUP-004 — Comments inside a variable declaration are unsupported — changed.
  - Q10: the span of a `using` or `fixed` statement's declaration now runs through the `)` behind its last declarator. The keyword and `(` stay outside the span.
  - Q9: the inside of a value now runs from its first token's trailing trivia through its last token's leading trivia. A comment between the value and the `,`, `;`, or `)` behind it is refused. A value of one token has no inside.
  - The trailing trivia of the `)` is excepted, the same as the `;`'s. Before this round the `)` was outside the span, so this output does not change.
  - Q11: attribute arguments are named in the Why. I also added an example, `[Obsolete("old" /* why */)]`.
  - New refused example: `= 30 /* seconds */;`, `= 1 /* c */,`, `Open() /* c */)`, and `// c` on its own line in front of `)`.
  - The other-side example gains `using (var s = Open()) // c11`, which is not refused.
  - Both "not decided yet" sentences are removed.
- RH2001-007 — clarified. A comment behind the initializer value's last token, such as `= 5 /* five */;`, now withholds the fix, because the built field is refused (Q9 together with Q7). The "not decided yet" clause is gone.
- The change doc gets:
  - Status Accepted.
  - The What paragraph is extended.
  - Visible change 1 gains a Q9/Q10/Q11 block. Its input, before, and after outputs were produced with the CLI built from this branch, which carries no code change against `main`.
  - Visible change 2 gains `[Obsolete("old" /* why */)]` (was left unsplit) and `private int _a = 1 /* c */, _b;`, which used to be split into `_a = 1 /* c */;` and `_b;` and is now refused.
  - Visible change 3 gains the RH2001 `= 5 /* five */;` case.
  - The "Open" section is removed.
  - Three new Decisions (Q9, Q10, Q11) and three rejected alternatives. In the Q5 decision, the sentence "attribute arguments are no initializer value" moves to the Q11 decision.
  - "Written-back boundary trivia" now names the `)`.
  - The "Checked and kept" list now notes that a declarator's trailing trivia in the split carries no comment either.

No ID is added, so `requirement-ids.txt` and `sdd/specs/README.md` are unchanged. The architecture spec tests give 50 passed and 1 failed. The one failure is `EveryRequirementOfAnEnforcedSpecIsCovered`, for the same new or untested IDs as in round 03.

Spec examples were re-scanned with Roslyn (`sdd/` code blocks, both documentation modes) for the new shapes. Two kinds of hits exist, and none needs rewriting:

- The value-trailing comments in RH3204, RH5041 (both a `using` statement and a local) and the attribute-argument comments in RH5517 to RH5520 belong to rules whose fix does not use the formatter.
- RH7101.md:304 `[Obsolete /* why */]` already states that no fix is offered.

## Examples for the tests

- FMT-UNSUP-004: the new third example (Q9 and Q10), the `[Obsolete("old" /* why */)]` field in the second example (Q11), and `// c11` in the other-side example (not refused).
- RH2001-007: `private int Value { get; set; } = 5 /* five */;` is reported, and no code fix is offered.
- Change doc visible change 2: `private int _a = 1 /* c */, _b;` is now returned unchanged and reported as unsupported.

Test sources that hold the newly refused shapes. This is a Roslyn scan of string literals under `src/`; the test implementer decides whether each one is a whole-pipeline input or a formatting fix:

- Value-trailing comments:
  - `RH3104DoNotUseDefaultValueTypeConstructorAnalyzerTests` (the fix needs checking)
  - `RH5601UseTabsCorrectlyFormatterTests`
  - `RH5113DeclarationSemicolonMustStayOnDeclarationLineAnalyzerTests`
  - `RH6003SemicolonsMustBeSpacedCorrectlyAnalyzerTests`
  - `TabReplacementTests` (fragments at lines 48, 82, and 218)
  - `StrandedDocumentationTokenSpacingTests`
  - `RH3204` and `RH5041` tests (no formatting fix, so unaffected)
- Attribute arguments:
  - `FieldDeclarationSplitTests`
  - `ReihitsuFormatterSplitsFieldDeclarationTests`
  - `DocumentationCommentCollapseGuardTests`
  - `RH7101DoNotCombineFieldsAnalyzerTests`
  - RH5517 to RH5520 (unaffected)
- In front of a `using` statement's `)`: `RH7004UsingDeclarationsShouldNotBeUsedAnalyzerTests`, around lines 1317 and 1333. That input already holds a comment in front of the `;`, which was refused before this round.

## Output questions

_None._

## Internal choices

- The trailing trivia of a `using` or `fixed` statement's `)` is excepted, like the trailing trivia of a `;`. The alternative is to refuse it. Before this round the `)` was outside the span, so excepting it keeps today's output, and Q10 asked only about the trivia in front of the `)`.
- I set the change doc to `Accepted` on the lead's instruction. The alternative is to leave it `Draft` for the lead, because `sdd/README.md` says the lead sets it.
- For the implementer, not decided in the spec: in `FieldDeclarationSplitTransform.CreateSplitField`, `variable.WithoutTrivia().WithTrailingTrivia(variable.GetTrailingTrivia())` keeps the declarator's trailing trivia. Under Q9 that trivia now holds no comment, so this may simplify further. I listed it under "Checked and kept" only as whitespace carry-over, not as a deletion.

## Follow-up candidates

_None_ beyond those in 03-spec-author.md.
