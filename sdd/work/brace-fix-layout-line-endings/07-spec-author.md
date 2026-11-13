Role: spec author
Commit: f7d0fdd (statements derived from this tree; the edits are in the commit that adds this handover)
Verdict: spec written

Amendment for answer 7 and lead decisions 11 and 13 of `00-lead.md`. The prompt carried no intended fix, suspected cause, or hoped-for verdict. All four issues are `agent-filed`.

## Changed requirements

### thoenissen/Reihitsu#1142 (answer 7)

- SHARED-ATTR-022 — The split fix is withheld for an accessor list on the line of its opening brace — added. The split fix is withheld when the reported list is attached to an accessor and the `{` of the accessor list stands on the line where the reported list starts. When that `{` stands on a line above, the fix is offered as SHARED-ATTR-017 states. It has a withheld example and an offered example:
  - withheld: RH5518 `public int Value { [field: First, Second] get => 1; }`, and RH5531 with a two-line accessor list whose `{` shares the list's line;
  - offered: a multi-line accessor list with `{` on its own line, and one with `{` behind the property name.
- SHARED-ATTR-017 — clarified. The "shares its line with other code" clause now names SHARED-ATTR-022 as its exception.
- RH5502-004, RH5504-004, RH5516-004, RH5518-004, RH5520-004, RH5528-004, RH5531-006 — clarified:
  - each requirement is retitled and now withholds when SHARED-ATTR-019 or SHARED-ATTR-022 withholds the fix;
  - each Why gains one sentence;
  - SHARED-ATTR-022 is added to each `Uses`;
  - RH5516, RH5518, RH5520, RH5528, and RH5531 each gain a withheld example.
  These are the split rules whose specs cite SHARED-ATTR-014, which means they can report an accessor's list. RH5502 and RH5504 get the reference only, with no example, because an `assembly:` or `module:` list on an accessor is not a shape anyone writes.
- `requirement-ids.txt`: SHARED-ATTR-022 added.
- Rule pages are unchanged: this is a withheld fix in a corner case (`documentation/rules/general-notes.md` covers diagnostics without a fix).

### thoenissen/Reihitsu#1388 (decision 11)

- RH5011-004 — clarified, with no change to behavior.
  - The normative sentence says "the detected line ending". That is the document's own line ending when all its breaks are CRLF or all LF, and `\n` when all are a lone `\r`. It replaces "the line break of that line", which a mixed document contradicted.
  - The lone-`\r` paragraph now states the accepted output. In a lone-`\r` document, `break;\r            default:` becomes `break;\n\r            default:`, and it says why the `\n` goes in front of the `\r`.
  - A new sentence states the mixed case that `FixInsertsTheDetectedLineEndingWhenTheBreakLineEndsInAnotherOne` already asserts: `break;\n` becomes `break;\n\r\n` in a CRLF document.
  - `Uses` is unchanged.

### thoenissen/Reihitsu#1416 (decision 13)

- SHARED-BRACE-002 — clarified, with no change to behavior. Where the body starts is now decided by the header's last line, the line of `)` or `else`, not by the line of the `if` keyword.
  - The body is below that line when a line break outside a comment stands between the header's last token and the body. A line break inside a delimited comment does not count, so in `if (a) /* x⏎y */ M(a, b);` the body is on the header's last line. This matches the implementer's line-break-trivia check in 06.
  - The braces still take the column of the line where the `if` or `else` starts.
  - The two bullet headings and the blank-line sentence use the new term.
  - A new example, "a header that spans several lines", shows `if (a &&⏎    b) M(a, b);` and the same header with the body below `b)`.
- SHARED-BRACE-003 — clarified. The first bullet ("a body on the header's last line that spans several lines") and its other side ("a body below the header's last line keeps the fix … also when the header itself spans several lines") use the same term.

## Examples for the tests

- SHARED-ATTR-022: both examples. The two existing tests `RH5518…VerifyDiagnosticForMergedFieldSpecifierListOnSingleLinePropertyAccessorWithExpressionBody` and `RH5528…VerifyDiagnosticForMergedReturnSpecifierListOnSingleLinePropertyAccessorWithExpressionBody` become "no code fix is offered" tests. The offered twins need new tests. The new withheld examples in RH5516-004, RH5518-004, RH5520-004, RH5528-004, and RH5531-006 can be tagged with both the rule requirement and SHARED-ATTR-022.
- RH5011-004: no new tests. The two existing line-ending tests already assert the reworded text.
- SHARED-BRACE-002: the new multi-line header example, both statements, for RH5405. The implementer noted no test covers it. The comment case `if (a) /* x⏎y */ M(a, b);` is stated in prose and may get a test.

## Output questions

_None._

## Internal choices

- Answer 7 is a new requirement, SHARED-ATTR-022, instead of a paragraph in SHARED-ATTR-017 or SHARED-ATTR-019. The alternatives were:
  - a paragraph in SHARED-ATTR-017, which states what the fix produces, not when it is withheld;
  - a paragraph in SHARED-ATTR-019, whose title and Why are about comments inside the list.
  A heading of its own keeps test tags precise.
- The withhold condition is "`{` of the accessor list on the line where the reported list starts", not "accessor list on one line". The alternative was the maintainer's literal shape, a one-line accessor list. The broader condition is chosen for three reasons:
  - The odd output (a further list at the owner's indentation, in front of the rest of the accessor list) comes from the `{` on the list's line, whatever line the `}` is on.
  - It is the cheaper check: compare the line of the `{` with the list's line.
  - It is the only shape in which RH5531 can meet the case. RH5531 reports splits only for owners that span several lines (RH5531-001).
  It also covers a list behind an earlier accessor on the `{` line, such as `{ get => 1; [return: A, B] set { } }`. All of these are a rare class (`AGENTS.md`, Code-fix coverage). In every one, the fix is withheld and the diagnostic stays, which matches the output the maintainer picked.
- SHARED-ATTR-022 is referenced from the seven split rules whose specs cite SHARED-ATTR-014, including RH5502 and RH5504. The alternative was to reference it only from the rules where an accessor list is common. The fix is the shared `TargetAttributeListShapeCodeFixProviderBase`, so the withhold applies to every split rule that can report an accessor's list.
- The SHARED-BRACE-002 wording follows the implementer's reading, including the multi-line-comment case. The alternative was to decide by the body's first-token line. That would put `{` at the end of the `)` line, inside `/* x`. The SHARED-BRACE-003 line-ending check already covers a line break inside that comment, because it spans from the header's last token.

## Follow-up candidates

_None._

## Checks

- `scripts/test.sh --project architecture --filter "FullyQualifiedName~Sdd"`: 50 of 51 pass. The one failure is `EveryRequirementOfAnEnforcedSpecIsCovered`, for the new SHARED-ATTR-022. It goes green once test implementer 08 tags a test with `[Requirement("SHARED-ATTR-022")]`. `UsesListsExactlyTheReferencedRequirements` passes.
