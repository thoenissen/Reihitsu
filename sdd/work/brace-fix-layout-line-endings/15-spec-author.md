Role: spec author
Commit: 6605b8ac712db915351ad7b7d513970e93d079ef (statements derived from this tree; the edits are in the commit that adds this handover)
Verdict: spec written

Repair round 2, decisions 17, 18, and 19 of `00-lead.md` (auditor 14, findings 1 to 3). The prompt carried no intended fix, suspected cause, or hoped-for verdict beyond the lead's recorded decisions. All four issues are `agent-filed`. The change doc stays `Status: Accepted`: every addition is a rare-class withhold or an unchanged requirement stated with an example.

## Changed requirements

### thoenissen/Reihitsu#1416 (decision 17, finding 1)

- SHARED-BRACE-002 — The fix wraps the body in braces — clarified, no change of intended behavior. The requirement already demanded the fix: "where SHARED-BRACE-003 does not withhold it", and SHARED-BRACE-003 names no end-of-file case. The case was not explicit, so it now has a paragraph and an example:
  - A body, with the comments behind it, that ends the document without a line break behind it gets the fix. The fix creates the line break in front of `}` and writes none behind it, so the document still ends without one.
  - Example: a top-level `if (args.Length > 0) {|RH5405:System.Console.WriteLine();|}` with no final line break. It becomes `if (args.Length > 0)` ⏎ `{` ⏎ `    System.Console.WriteLine();` ⏎ `}`, with nothing behind `}`.
  - Prose twin: the body below the `if` with `// c` behind it, at the end of the file.
  - Measured on `166e3dd` (`main`) with `scripts/apply-fix.sh`: both fixtures give exactly this output. The head withholds them, which is the repair.

### thoenissen/Reihitsu#1142 (decisions 18 and 19, findings 2 and 3)

- SHARED-ATTR-022 — retitled "The split fix is withheld on the line of the brace that opens the body holding the declaration" — changed (widened). The fix is withheld when the reported list starts on the line of the `{` that opens the body directly holding the list's declaration. That `{` belongs to one of four bodies:
  - the accessor list of an accessor;
  - the type declaration (class, struct, record, interface, enum) of a member, nested type, delegate, or enum member;
  - the namespace with braces of a type or delegate;
  - the block of a local function.

  What stands between that `{` and the list does not matter. The fix is still offered in three cases, and each has an example or a sentence:
  - the `{` is on a line above;
  - a `{` on the list's line opens another body (`void M() { } [A, B] void N() { }`), with its output;
  - no `{` holds the declaration directly (a parameter, a file-scoped namespace, a top-level local function).
- SHARED-ATTR-023 — The split fix is withheld for a line break it would copy or keep in another line ending — added. It has two bullets, each stated with both sides:
  - **Line break in the target specifier.** The list has a target specifier, and its first attribute starts on a later line than its `[`. The fix is still offered for a line break between attributes, after the last attribute, or in a list without a specifier. Example: RH5518 `[field:⏎A, B]` is withheld, and `[field: A,⏎B]` → `[field: A]⏎[field: B]`.
  - **Another line ending in the list.** The span from `[` through `]` holds a line break other than the detected ending. This includes a line break inside a literal, and one between attributes that the fix drops. Every line break in a list in an all-lone-`\r` document counts. A line break outside the list does not count, such as the one ending the list's line. Example: RH5514 `[A(1,\n      2), B]` in a CRLF document is withheld. In an all-CRLF document it is offered, and the inner break stays: `[A(1,⏎      2)]⏎    [B]`.
- SHARED-ATTR-017 — clarified. The exception sentence now points to the widened SHARED-ATTR-022 and to SHARED-ATTR-023.
- The withhold requirements of all 14 split rules now read "when SHARED-ATTR-019, SHARED-ATTR-022, or SHARED-ATTR-023 withholds it" — changed. Each has a new title and Why, and SHARED-ATTR-022 and -023 are added to `Uses` where they were missing. The 14 requirements are RH5502-004, RH5504-004, RH5506-004, RH5508-004, RH5510-004, RH5512-004, RH5514-005, RH5516-004, RH5518-004, RH5520-004, RH5522-004, RH5526-004, RH5528-004, and RH5531-006. RH5531 keeps "withholds the split". New withheld examples:
  - RH5506-004: `namespace N { [A, B] class C { } }`;
  - RH5514-005: `class C { [A, B] void M() { } }` and the local function `void N() { [A, B] void L() { } }`.
- `requirement-ids.txt`: SHARED-ATTR-023 added.

### Change doc `sdd/changes/2026-11/brace-fix-layout-line-endings.md`

- Affected requirements: SHARED-BRACE-002 (adds "a body that ends the document"), SHARED-ATTR-023 (added), and all 14 rule withhold requirements.
- What: the brace bullet states the end-of-file case. The SHARED-ATTR-022 bullet is widened. There is a new SHARED-ATTR-023 bullet.
- New visible output entries. The `main` outputs were measured on `166e3dd`, and the intro says so:
  - a list behind the `{` of a namespace, a class, or a block;
  - a line break in the target specifier;
  - another line ending inside the list. Its Before is "the fix is offered", because `apply-fix.sh` runs only uniform LF/CRLF.
- Unchanged list: the end-of-file body (same output on `main` and after the repair).
- Decisions:
  - the answer-7 entry now records both widenings, decisions 14 and 18;
  - new entries for decision 19 (SHARED-ATTR-023) and decision 17 (end of file).
- Alternatives considered: two rejected options are added, keeping the brace-line output and rewriting kept line breaks to the detected ending.
- Status stays `Accepted`.

No rule page changes: these are withheld fixes in corner cases, and the end-of-file case is the normal fix.

## Examples for the tests

- SHARED-BRACE-002: the new "body that ends the document" example, plus the `// c` prose twin. Each test needs a top-level-statement document without a final line break, under LF and CRLF.
- SHARED-ATTR-022:
  - the withheld example covers RH5506 namespace, RH5514 class, RH5514 local function, RH5518, and RH5531;
  - the offered example `void M() { } [A, B] void N() { }` → `void M() { } [A]⏎    [B] void N() { }`;
  - the existing `{`-above examples.
  - The new rule-spec examples in RH5506-004 and RH5514-005 can carry both tags.
- SHARED-ATTR-023 (no test is tagged yet; `EveryRequirementOfAnEnforcedSpecIsCovered` fails only for it):
  - RH5518 `[field:⏎A, B]` withheld; also `[⏎field: A, B]` and `[field⏎: A, B]` if wanted;
  - RH5518 `[field: A,⏎B]` offered, with its output;
  - RH5514 CRLF document with LF inside `A(1,⏎2)` withheld;
  - the same list in an all-CRLF document offered, with the inner CRLF kept;
  - a list holding a line break in an all-lone-`\r` document withheld;
  - a CRLF document with an LF ending the list's line (outside `[`…`]`) offered;
  - a foreign line break between two attributes withheld.

## Output questions

_None._ Each addition withholds a fix for a rare class (decisions 18, 19) or states an unchanged requirement (decision 17). The `main` outputs in the change doc show what the withholds replace.

## Internal choices

- The target-specifier bullet of SHARED-ATTR-023 is stated as "the list has a target specifier and its first attribute starts on a later line than its `[`". The alternative is "a line break in the target's trivia", the wording of decision 19. Roslyn puts a line break directly behind `[` in the trailing trivia of `[`, not of the target. The fix drops that break but copies the next line's indentation into every list (`[    field: A]`). The line comparison withholds that shape as well, is just as cheap, and can be tested from the outside. Implementer: compare the line of `[` with the line of the first attribute's start when `Target` is not null.
- The line-ending bullet inspects `[` through `]`, not the list's trailing trivia. The alternatives were the trailing trivia, which SHARED-FIX-008's "its trailing trivia included" suggests, and the narrower "only line breaks the fix keeps". The fix replaces only the list's span. SHARED-FIX-008's first paragraph puts the break that ends the line of a change that ends within the line outside the fix's scope. Including it would withhold every split in an all-lone-`\r` document and contradict SHARED-ATTR-017's lone-`\r` example. The span check is the cheap `LineEndingUtilities.HoldsOtherLineBreak` over `attributeList.Span`. It also counts a break between attributes that the fix drops: rarer still, and telling them apart costs more.
- The split fix is not added to the list in SHARED-FIX-008's second paragraph. The alternative was to name it there as a fix that moves code without the formatter. The split does not move code under SHARED-FIX-007 (*Moved code*), so SHARED-ATTR-023 states its own withhold and cites SHARED-FIX-008 in its Why, the same way SHARED-BRACE-003 states its fifth bullet.
- SHARED-ATTR-023 is a new requirement, not part of SHARED-ATTR-019. The alternative was a paragraph in SHARED-ATTR-019, whose title and Why are about comments and directives. A separate heading keeps the test tags precise, as decision 14's choice for SHARED-ATTR-022 did.
- All 14 split rules now cite SHARED-ATTR-022 and -023. The alternative was to cite only the rules where each shape is common. The fix is the shared `TargetAttributeListShapeCodeFixProviderBase`, so every split rule withholds alike. RH5502 and RH5504 keep SHARED-ATTR-022 (no `{` holds an assembly or module list, so it never applies) and gain SHARED-ATTR-023 (`[assembly:⏎A, B]`).

## Follow-up candidates

_None._ Checked: a list without a target specifier whose `[` ends its line, `[⏎    A, B]`, becomes `[A]⏎    [B]`. The fix copies no line break or indentation there, so it keeps the fix.

## Checks

- `scripts/test.sh --project architecture --filter "FullyQualifiedName~Sdd"`: 50 of 51 pass. The one failure is `EveryRequirementOfAnEnforcedSpecIsCovered`, for SHARED-ATTR-023 alone. It goes green once test implementer 16 tags a test with `[Requirement("SHARED-ATTR-023")]`. `UsesListsExactlyTheReferencedRequirements` passes.
- `scripts/apply-fix.sh` on a scratch worktree of `166e3dd`: RH5405 end-of-file (2 fixtures), RH5514 class and local function, RH5506 namespace, RH5518 `[field:⏎A, B]`. The outputs are quoted in the change doc. The worktree was removed.
