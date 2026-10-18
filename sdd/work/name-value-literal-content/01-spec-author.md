Role: spec author
Commit: 9245af936519544006a859d3a67677866c264bad
Verdict: open question

The spec is written in full, following the recommended answer to the decision below, so the run can go on as soon as the maintainer confirms. If the maintainer picks the other reading, one paragraph of SHARED-DIAG-003 changes (see Decisions needed). No change doc: this is a clarification, as the maintainer decided (`00-lead.md`). My prompt carried no intended fix or verdict.

## Changed requirements

- SHARED-DIAG-002 — A name is compared by its value — added
- SHARED-DIAG-003 — Only the text of a literal or a comment is content — added
- `sdd/specs/shared/diagnostics.md` gains two non-goals: (1) how a code fix spells a name it writes, `@` included (this keeps the casing-fix `@keyword` change, which P3 tracks elsewhere, out of scope); (2) whether whitespace next to a comment is reported (decided separately, see #1078).
- Both IDs are added to `sdd/specs/requirement-ids.txt` right after SHARED-DIAG-001. Header, code owners, and `Uses` are unchanged. The requirement text names rule IDs only, not other requirement IDs.

How I checked it: all spec examples ran as throwaway tests in a temporary worktree of this commit, and every one passes today. The worktree is deleted and nothing from it is committed. `scripts/test.sh --project architecture` filtered to `SpecConsistencyTests`, `SpecStructureTests`, and `RequirementCoverageTests`: 38 passed, 1 failed. The one failure is `EveryRequirementOfAnEnforcedSpecIsCovered`, and only because SHARED-DIAG-002 and -003 have no covering tests yet. That is the test implementer's job.

Choice of examples:
- The examples in the issue use RH4001, RH4009, RH5602, RH6009/RH6010, and blank lines inside literals (RH5025). Each of those rules breaks the requirement today, and the breaks are tracked in #1071, #1074, and #1075. In the spec, the issue's examples appear only as prose that classifies the input, with no rule named: `class @class`, `namespace A . B`, `"a";···`, `$"{new int[]{1}.Length}"`, and the empty line inside a raw string. **Do not turn the prose into tests in this run.** They fail today.
- The other spec specs that stay silent on these cases do not contradict the new requirements. Those carve-outs were written to leave them out:
  - RH4001-001/003 and RH4009-002 cover only names written without `@` or trivia.
  - RH5601-001 and RH5602-001 only require reports away from string literals.
  - RH6009-001 and RH6010-001 cover only braces outside interpolated strings.

  No other spec was changed.
- No rule compares a qualified name *syntactically* and gets it right today. The using-ordering rules get it wrong (see Open questions to file), and so does RH4009. For the qualified-name qualifier, the example uses RH3103. That rule finds `System.Nullable<T>` through the semantic model, so it reports `System . Nullable<int>` and `System./* c */Nullable<int>` the same way it reports `System.Nullable<int>`.
- SHARED-DIAG-003 says the text of an interpolation's format clause is literal text. Current behavior agrees: RH6021 does not report `$"{2: D3}"`. Also, the compiler rejects trailing whitespace in a format specifier (CS8088). The example covers this.
- `documentation/rules/` is unchanged. These are rule-wide guarantees that the rules named in #1071, #1074, and #1075 do not meet yet. Promising them on `general-notes.md` before those fixes land would document behavior that does not exist.

## Examples for the tests

Every block below passes today. Each block names its rule.

- SHARED-DIAG-002, block 1 — RH8101: `@class`, documented by `<param name="class">`, is not reported. `@event` has no `<param>` and is reported.
- SHARED-DIAG-002, block 2 — RH3103: `System.Nullable<int>`, `System . Nullable<int>`, and `System./* c */Nullable<int>` are each reported, and each span covers the whole qualified name. Diagnostics only.
- SHARED-DIAG-003, fix pair 1 — RH6002, inside an interpolation hole and before a literal:
  - The comma inside the hole is reported.
  - The missing space before `"d"` is reported.
  - The ` ,` inside the literal text and inside the `//` comment is not reported and is unchanged after the fix.
- SHARED-DIAG-003, fix pair 2 — RH6003: the space after `"a ;"` is reported and removed. The literal is unchanged.
- SHARED-DIAG-003, negative block — the empty lines inside a raw string literal and inside a block comment, plus `$"{1:N2}{2: D3}"`. RH5023 reports nothing in it, and neither does RH6021. I checked it against each analyzer separately.

The RH6002 and RH6003 providers offer Fix All. Choosing the test base is up to the test implementer.

## Open questions to file

**Decided by principle P3 (#912) — the using-directive rules compare the name's text, not its value (SHARED-DIAG-002).**
This is a new instance. #1071 does not list it. I confirmed each case with a throwaway test at this commit; the control is the same input written `System.IO`.
- RH7201:
  - Input: `using Microsoft.Win32;⏎using System . IO;`
  - Output today: not reported. The `System.IO` control is reported.
- RH7203:
  - Input: `using System . IO;⏎using System.Collections;`
  - Output today: not reported. The `System.IO` control is reported.
- RH7207:
  - Input: `using System.Collections;⏎using System . IO;`
  - Output today: reported as needing a blank line between groups. By value, both directives belong to the `System` group, so no blank line is needed.

Why it looks wrong: `System . IO` is the namespace `System.IO`. Its group, its `System` root, and its sort key should match the same directive written without spaces.

Requirements touched: SHARED-DIAG-002, SHARED-USING-004, SHARED-USING-005, SHARED-USING-007. I did not check the formatter's FMT-USING phase, which shares this policy.

Where to file it: this is shared behavior, so it belongs to the open-questions issue for `sdd/specs/shared/using-directive-ordering.md`. #916 (`SDD open questions: sdd/specs/formatter/using-directives.md and shared/using-directive-ordering.md`) exists but is closed. The lead has to choose: reopen #916 and add this there, or open a new issue. #994 (specs for RH7201–RH7311) is open and may be relevant.

## Decisions needed

**Q1 — Does SHARED-DIAG-003 restrict rules whose subject is the comment itself?**

- Input: a `//` comment with no text inside a class.
- Output today: RH3201 reports it, which I confirmed with a test. Other rules also read or change comment text on purpose:
  - the documentation rules (RH8###), for example RH8301, which reports `///text`;
  - RH7301, which reports a mismatched `#endregion` comment.
- Why it blocks: P5 says, word for word, that a rule "neither reports nor changes" comment text. Read literally, that makes all of these rules break the requirement.
- Reading A (literal): no rule reports anything inside comment text. RH3201, the documentation rules, and RH7301 become bugs against SHARED-DIAG-003.
- Reading B (recommended; this is what the spec says now): the requirement applies to rules that check code. A rule whose subject is a comment, such as RH3201 or a documentation rule, checks that comment's text as its subject. The spec says this in the paragraph that starts "A rule whose subject is a comment itself…".
- Recommendation: B. P5 was decided for spacing and blank-line rules that treated literal and comment text as code (#1074, #1075). Under reading A, RH3201 and every documentation rule would have nothing left to check.
- If A is chosen: delete that paragraph. RH3201, the RH8### rules, and RH7301 then need follow-up bug issues, which the maintainer approves.

## Notes

- SHARED-DIAG-003 forbids a code fix to change literal or comment text. That overlaps with SHARED-FIX-003, which the concurrent run for #1068 is writing. They do not contradict each other. SHARED-DIAG-003 does not name SHARED-FIX-003, because that ID does not exist on this branch.
- SHARED-DIAG-003 says that *only* literal and comment text is content. So directive text, such as a region name, is not content under this requirement. That matches today's rules: RH5601 reports tabs on directive lines, and RH7302 reports region names. Code in inactive `#if` branches is already a non-goal of this spec.
