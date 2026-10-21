Role: skeptic
Commit: 287c622
Verdict: proceed narrower

Inputs: thoenissen/Reihitsu#1139 and its comment, `00-lead.md`, `01-spec-author.md`, `squad/decisions/2026-10-21-1139-shared-comment-001-contradicting-tests.md`, `git diff origin/main -- sdd/specs`, and the analyzers, shared helpers, and tests named below. My prompt carried no intended fix or hoped-for verdict. It did fix one boundary: the texts of P12 and P13 are decided. I did not reopen them.

## Premise

The premise holds. P12 and P13 are decided principles (thoenissen/Reihitsu#912), and this run writes them down. These code facts back the spec claims:

- **P13 (SHARED-COMMENT-001).** `CommentPositionUtilities.IsOrdinaryComment` excludes documentation trivia by kind. It also excludes any `//` comment with exactly three slashes and any `/**` comment other than `/**/`, by text. Both hold in every documentation mode and at every position. So RH5033–RH5049 never report a `///` or `/** */`, including one that documents nothing. This is the non-conformance the decisions entry records; thoenissen/Reihitsu#1362 Q1 tracks it.
- **Compiler check of the CS1587 definition.** I built a scratch `net10.0` library on SDK 10.0.402 with `GenerateDocumentationFile`, outside the repository, and deleted it afterwards.
  - CS1587 is reported for `[Obsolete]⏎/// <summary>X</summary>⏎void X()`.
  - It is reported for the first `///` of `/// <summary>Y</summary>⏎// note⏎/// <remarks>Y</remarks>⏎void Y()`.
  - It is reported for `void Z() { } /// tail` when Z is the last member.
  - No CS1587 for these shapes, and each member is documented:
    - `/// <summary>M1</summary>⏎// note⏎void M1()`
    - `/// …⏎// note⏎[Obsolete]⏎void M2()`
    - `/// …⏎#region R⏎// note⏎void M3()`
    - `/// <summary>M5</summary>⏎/** <remarks>M5</remarks> */⏎void M5()`
    - `/// …⏎/* note */⏎class C1`
    - `void A() { } /// <summary>B</summary>⏎void B()`, which documents B.
  - Consequences:
    - The definition keeps RH5034's subject intact, and RH5034-002's list of positions matches SHARED-COMMENT-001's list.
    - The spec's "behind the last member of a type" is correct.
    - The issue's "behind code: `void M() { } /// tail`" holds only when no member follows.
- **P12 (SHARED-COMMENT-011).** How each rule judges a line today:
  - RH5204 judges a comment-led line by the whitespace in front of the comment and reports on the token. `TryGetActualIndentation` reads the line text, and `AlignCommentIndentation` skips a comment that is on the token's own line.
  - RH5201 measures the line start with `GetLineStartColumn`, the first character that is not whitespace.
  - RH5110, RH5203, RH5205, RH5206, RH5302, and RH5308 compare the token's own column once the token is on a later line than the token in front of it.
  - `SyntaxTokenPositionUtilities.IsFirstOnLine` and `FluentChainLink.StartsLine` count a token behind a line-leading comment as first on its line.
- **Lane.** The issue's "no change doc" instruction is what keeps this run in the tests lane, with thoenissen/Reihitsu#1068 as the precedent. `sdd/README.md`, *When a change doc is required*, would otherwise ask for a change doc, because SHARED-COMMENT-001 is changed in a way the code does not yet meet. This is not a blocker, only a note for the lead.

### The spec diff against the code

**F1 — SHARED-COMMENT-011's third sentence contradicts SHARED-CHAIN-004 and SHARED-CHAIN-007.** The sentence is "A comment that does not start the line … MUST NOT change how the line is judged". P12 says nothing about comments that do not start a line, so the spec author added this sentence. RH5112 and RH5201 now list both SHARED-CHAIN-004 and SHARED-COMMENT-011, so their specs contradict themselves.
- RH5112: `return items // c⏎               .Where(x => x > 0);` is not reported. Without `// c` it is reported and fixed.
- RH5201: `return items // c⏎            .Where(x => x > 0);` is reported, because the anchor falls back to the root. Without `// c` it is not reported.
- Existing tests pin both behaviors:
  - RH5112 `VerifyNoDiagnosticWhenCommentFollowsRootOnItsLine`, tagged SHARED-CHAIN-004.
  - RH5112 `VerifyNoDiagnosticWhenCommentIsDirectlyAboveFirstWrappedCall`, tagged SHARED-CHAIN-004. This is the "on a line of its own above" case of the sentence.
  - RH5201 `VerifyLinkBehindCommentedRootIsAlignedToTheRoot`, tagged SHARED-CHAIN-004.
- The sentence would also contradict RH5305-001 and RH5306-001 if those rules joined (see F5). Each exempts a comment anywhere in front of the bracket on its line.
- Its own wording does not name the shape its RH5203 example uses. The list says "one behind a token on the line", but the example's comment is at the end of the line above.

**F2 — The per-rule sentences answer open question 2 (thoenissen/Reihitsu#1362 Q2), which the shared requirement explicitly leaves open.**
- RH5203-001 and RH5205-001 say an argument or brace "behind a comment that starts its line … MUST NOT be reported". That is reading (b), whatever column the comment is in.
- RH5201-002 and RH5204-001 judge the comment's column. That is reading (a), and the code agrees.
- RH5110-001 ("for its position") and RH5206-001 ("for its column") mirror the shared wording.
- `sdd/README.md` says an open question is not written into a spec. Each rule sentence must carry the same bound as SHARED-COMMENT-011, or be cut down to a reference to it.

**F3 — The carve-out sentence of SHARED-COMMENT-011 is a note on an open question.** The sentence is "This requirement does not cover a line whose leading comment is itself not where the rule asks the line to start". `sdd/README.md` allows "no requirement, no non-goal, no note" for an open question. Scoping the normative sentence to the case it decides achieves the same thing without the note: a line whose leading comment is where the rule asks the line to start.

**F4 — "A comment that starts the line" restates a predicate that SHARED-BLANK-002 already defines.** SHARED-BLANK-002 is "Trivia starts its line only after a line break that ends the previous content". It applies to both the analyzer and the formatter. It also excludes trivia behind a multi-line comment that ends on its line. Because the restatement does not reference it, one case stays undecided, and the rules disagree on it today: a line that starts with the tail of a multi-line `/* */` comment (FC-021).
- RH5204 judges the tail: `    /* a⏎    b */       private int _a;` is not reported.
- RH5203 judges the token: `P(1,⏎          /* a⏎          b */ 2);` is reported, with the tail in the alignment column.
- This run must not decide it. It goes into thoenissen/Reihitsu#1362 as an open question.

**F5 — The candidate set is wider than the specs that list SHARED-COMMENT-011.** Two texts claim every rule that checks where a line starts: the requirement ("A rule that checks where a line starts"), and the issue ("every rule that judges where a line starts (line breaks, alignment, indentation)"). The spec author's own scope test brings in rules beyond RH51xx/RH52xx; see the table below.
- RH5302 and RH5308 are new P12 instances that no issue tracks.
- Either `Uses` widens and RH5302/RH5308 are filed as "Decided by principle P12 (#912)" (clarification 7), or the lead confirms that the RH51xx/RH52xx limit in clarification 4 came from the maintainer and the requirement text is bounded to the rules that list it.
- The intro sentence "two things every rule shares" also claims more than `Uses` allows. Per `sdd/README.md`, only `shared/diagnostics.md` and `shared/code-fixes.md` apply without being listed.
  - No rule outside RH5033–RH5049 lists SHARED-COMMENT-001.
  - P13's `/* */`-line clause binds none of these rules. None of them tells comment lines from code lines, and RH5104 lists only SHARED-COMMENT-002 (thoenissen/Reihitsu#1146).

**F6 — A further instance of thoenissen/Reihitsu#1147, not yet commented there.** RH5206 reports the opening brace in `return status switch⏎               /* map */ {⏎                   0 => "Idle",⏎                   _ => "Other",⏎               };`, where the comment is in the anchor column of `status`. The amended RH5206-001 says this must not be reported. The lead's comment on #1147 covers RH5205 only.

**F7 — Minor.**
- The `Why:` of SHARED-COMMENT-011 says "a diagnostic on it would have no fix that clears it". That argues from fix availability, which `AGENTS.md` *Code-fix coverage* rejects as a reason on its own. The decided reason is already in P12: the first element is what is judged, and other rules forbid the comment.
- The sentence added to RH5112-001 restates SHARED-CHAIN-004, which already exempts any comment in the gap.
- The non-goal "Moving a comment. These rules have no code fix" now sits in a spec whose SHARED-COMMENT-011 users all have Fix All, so "these rules" is ambiguous.

**Verified, no change needed:**
- SHARED-COMMENT-001 examples. RH5033 reports `/**/` in both modes and `//// note` under `DocumentationMode.None`. RH5034 does not report `///` + `/** */` in front of `M`. The code paths are `IsOrdinaryComment` and `IsMultiLineDocumentationText`.
- SHARED-COMMENT-011 examples. RH5204 does not report `    /* a */   private int _a;`. RH5201 does not report `/* all */   .ToList()`, and the existing RH5201 test `VerifyBlockCommentStartingTheLinkLineIsAlignedWithTheLink` (RH5201-002) pins the same shape. RH5203 reports both counterparts.
- A `/** … */` documentation block leading a line in the scope column is not reported by RH5204. This is the "whatever kind" qualifier.
- The ledger is sorted. `scripts/test.sh --project architecture --filter "FullyQualifiedName~Spec|FullyQualifiedName~RequirementCoverage"` gives 38 passed and 1 failed. The failure is the expected `SHARED-COMMENT-011: no test covers it (shared/comment-placement.md:418)`.
- The decisions entry follows `squad/decisions/README.md`.
- After the decided removals, SHARED-COMMENT-001 is still covered:
  - RH5034 `VerifyCommentAfterDocumentationWithoutDocumentationParsingIsReported`
  - RH5034 `VerifyCommentAfterBlockDocumentationWithoutDocumentationParsingIsReported`
  - RH5033 `VerifyQuadrupleSlashCommentAfterAttributeWithoutDocumentationParsingIsReported`
  - the `/**/` halves of the two mixed tests
- No RH5034 test pins the boundary shape with a comment between two `///` blocks.
- No test of the seven rules pins a comment-led line as reported, so SHARED-COMMENT-011 raises no second escalation-4 contradiction.

## Mechanism and candidates

_N/A_ as a bug. The candidate set for SHARED-COMMENT-011 comes from each analyzer's reporting check, using the spec author's scope test: does the rule decide whether a token starts its line, or the column a line starts in?

| Rule | Defining check | Lists SHARED-COMMENT-011 | Comment-led line in the expected place, today |
|---|---|---|---|
| RH5110 | `where` column once on a later line | yes | reported (thoenissen/Reihitsu#1147) |
| RH5112 | `FirstLink.StartsLine` && !`IsFirstLinkBlocked` | yes | not reported, via SHARED-CHAIN-004 |
| RH5201 | `StartsLine`, `GetLineStartColumn` | yes | not reported |
| RH5203 | `IsFirstOnLine` + argument column | yes | reported (#1147) |
| RH5204 | line text indentation | yes | not reported |
| RH5205 | brace column once on a later line | yes | reported (#1147 comment) |
| RH5206 | brace column always; arm column via `IsFirstTokenOnLine` | yes | arm reported (#1147); brace reported (F6) |
| RH5301 | brace column at `new` (SHARED-INIT-001) | no | not reported (probe) |
| RH5302 | `&&`/`||` column once on a later line | no | reported (probe), untracked |
| RH5303 | brace column at `new` (SHARED-INIT-001) | no | not probed |
| RH5305 | closing bracket column, exempt with a comment in front on its line | no | exempt by RH5305-001 |
| RH5306 | closing bracket column, exempt with any comment around | no | exempt by RH5306-001 |
| RH5308 | `?`/`:` start-of-line and column | no | reported (probe), untracked |

Out of scope by the same test: RH5101, RH5105–RH5108, RH5111, RH5113, and RH5402 check a later line than the previous token. RH5102, RH5103, RH5109, RH5114, RH5417, and RH55xx check whether two elements share a line. RH5104 reports the comment itself. RH5202 is raw strings.

## Breaking inputs

- **FC-003 (both sides of "starts the line").** Cover three positions: a line-leading block comment (conforming RH5204 and RH5201 shapes), a comment at the end of the line above (RH5203 reported), and a comment on its own line above (RH5203 reported).
- **FC-002 ("whatever kind of comment it is").** A `/** … */` documentation block leading a line, which RH5204 does not report.
- **FC-021 (`/* */` ending mid-line).** Do not pin the tail-led line as SHARED-COMMENT-011. It is undecided (F4), and RH5203 and RH5204 differ on it.
- **FC-033 (green for another reason).** An RH5112 comment-led first link passes because of SHARED-CHAIN-004, so it cannot falsify SHARED-COMMENT-011. Tag only tests whose verdict depends on the requirement (`sdd/README.md`: a test that verifies a requirement only incidentally does not tag it).
- **FC-033 (harness normalization) for SHARED-COMMENT-001.** Run each kept shape with documentation comments parsed and with `DocumentationMode.None`, as the requirement states.
- **FC-013.** Do not pin these shapes; they are observations only. `scripts/format.sh`, run twice, leaves the comment-led lines in four fixtures (RH5203, RH5206 brace, RH5302, RH5308) in their columns. It only inserts a blank line above them, and the second pass is stable. RH5203, RH5206, RH5302, and RH5308 then flag the formatter's output.
- **Existing coverage to reuse.** RH5201 `VerifyBlockCommentStartingTheLinkLineIsAlignedWithTheLink` directly verifies the RH5201 shape of SHARED-COMMENT-011.

## Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `sdd/specs/shared/comment-placement.md` | must change: F1, F2 (shared side), F3, F4, F5 intro, F7 |
| RH5110, RH5112, RH5201, RH5203, RH5204, RH5205, RH5206 specs | must change: F2 bounds; RH5112 per F7; `Uses` per F5 |
| RH5301, RH5302, RH5303, RH5305, RH5306, RH5308 specs | open: depends on the F5 scope answer |
| Analyzers RH5033–RH5049, RH51xx, RH52xx, RH53xx | must stay (no production change in this lane) |
| Code fixes | must stay; follow-up candidates below |
| Formatter | not involved; FC-013 observations above |
| `Reihitsu.Core` (`IsFirstOnLine`, `FluentChain`) | not involved |
| Rule documentation, `general-notes.md` | not involved, which I agree with: the behavior does not ship yet |
| Tests | must change: the decided removals (decisions entry) and SHARED-COMMENT-011 covering tests |
| Decisions entry | must stay as written |

## Simpler option

- **F1.** Cut the third sentence of SHARED-COMMENT-011 back to what P12 decides. A comment that does not start the line does not make the line comment-led. Leave everything else to each rule's own requirement. The RH5203 counterpart example still holds under RH5203-001 alone.
- **F2 and F3.** Scope the shared normative sentence positively to lines whose leading comment is where the rule asks the line to start. Let each rule spec reference it without restating it, so no rule text decides open question 2.
- **F4.** Reference SHARED-BLANK-002 for "starts its line" instead of defining the predicate again.
- **RH5112.** Its added sentence can go. If F1 is narrowed, SHARED-COMMENT-011 can stay in `Uses` through a reference only.

## Questions for the maintainer

_None blocking._ These go to the lead:

1. **F5 scope.** Was the RH51xx/RH52xx limit in clarification 4 the maintainer's? If not, the issue text puts RH5302 and RH5308 in scope.
   - Input: `return a⏎               /* and */ && b;`, with the comment in the operator column.
   - In scope: RH5302 must not report it, and today it does, so it is filed as a P12 instance.
   - Out of scope: it stays reported, and SHARED-COMMENT-011's text names its rules.
   - The same holds for RH5308 with `return a⏎                   /* yes */ ? 1⏎                   : 2;`.
2. **Open question to file in thoenissen/Reihitsu#1362 (found in passing): the tail-led line from F4.**
   - Input: `    /* a⏎    b */       private int _a;` (RH5204), and `P(1,⏎          /* a⏎          b */ 2);` (RH5203).
   - Real output: RH5204 does not report its shape, because it judges the tail. RH5203 reports its shape, because it judges the token.
   - Readings: (a) the tail is the line's first element, so neither is reported; (b) the comment started an earlier line, so the token is judged and both are reported.
   - Touches: SHARED-COMMENT-011, RH5203-001, RH5204-001, SHARED-BLANK-002.
3. **Comment for thoenissen/Reihitsu#1147: the RH5206 brace shape from F6.**

Follow-up candidates, out of scope. All of them reproduce on `main` code through `scripts/apply-fix.sh`:
- **RH5302 and RH5308 fixes.** On a comment-led operator, the fix only inserts a blank line inside the expression and never clears the diagnostic (`no-progress`). The blank line comes from FMT-BLANK-011, because SHARED-BLANK-002 counts the line-leading comment as starting its line.
- **RH5203 fix.** On the tail-led argument, the fix moves the start of the user's comment to column 0, pads the argument with spaces, and does not converge. This is a comment relocation (FC-004) and is SHARED-CODEFIX-003 territory.
- **RH5204 and RH5206 fixes.** They offer an action that changes nothing on the tail-led line (RH5204) and on the comment-led brace (RH5206). This is FC-009.
