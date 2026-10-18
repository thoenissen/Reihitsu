Role: spec author
Commit: 349dc02
Verdict: spec written

Repair round on finding 1 of `08-spec-checker.md`. I re-derived it from the code and by running the inputs rather than taking over its suggested direction. The spec changes are in commit 8f02d0f. The open-question texts and the #1093 comment in `05-spec-author.md` are unchanged: the case below is already named in the posted #1093 comment.

My prompt carried no intended fix, suspected cause, or hoped-for verdict.

## The case, re-derived

Two predicates in `AttributeTargetUtilities` decide what happens to an accessor's lists:

- `IsAttributeListOnSingleLinePropertyAccessor` holds when the property, indexer, or event that owns the accessor is on one line, apart from its own attribute lists. RH5531 then uses merge mode (`ResolveListShapeMode`).
- `IsAttributeListOnInlineAutoPropertyAccessor` adds two conditions: no accessor has a body or an expression body, and no comment or directive lies inside the accessor list. That is the SHARED-ATTR-006 exemption. The split base (`TargetAttributeListShapeAnalyzerBase`) skips exactly these lists.

The two predicates differ on one ground: a one-line owner whose accessor list SHARED-ATTR-006 does not exempt. On that ground, RH5531 requires merged lists, while the split rules (RH5528, and RH5518 with its CS0657 placement) report and split a list with several attributes. That is the #1093 instance the posted comment names. I ran the following (scratch verifier over the shipped analyzers and fixes):

- RH5528 reports `[return: A, B]` in each of these:
  - `int Q { [return: A, B] get => 1; }`
  - `int S { [return: A, B] get; /* c */ set; }`
  - `int T { [return: A, B] /* c */ get; set; }`
  - `event System.Action E { [return: A, B] add { } remove { } }`
- RH5531 reports `[B]` in `int Q { [A] /* c */ [B] get; set; }` and offers no fix. It reports and merges `int R { [A] [B] get => 1; }`. It does not report `int S { [A, B] get => 1; }`.

The spec stated the split side of that ground in two places:

- SHARED-ATTR-013's normative sentence: "every attribute list of its target that holds more than one attribute … whatever comments surround it".
- SHARED-ATTR-014's second sentence and its `Q` example line.

The spec checker named only the second. SHARED-ATTR-013 alone already claimed the one-line `Q` and `S` lists are reported. Narrowing SHARED-ATTR-014 alone would have left that claim standing.

The spec also stated the merge side once: RH5531-006's example line `int Q { [A] /* c */ {|RH5531:[B]|} get; set; }`. Its accessor list holds a comment, so SHARED-ATTR-006 does not exempt it, and RH5531-002 (merge only for exempt lists) does not cover it. The example still showed a reported merge diagnostic there, together with "no code fix is offered".

I checked the other specs that reference SHARED-ATTR-014: RH5502-001, RH5504-001, RH5516-001, RH5518-001, RH5520-001, RH5528-001, and RH5531-002. None of their examples or sentences states the one-line, non-exempt case. RH5528-001's example has only the exempt `P` and the multi-line `Q`. SHARED-ATTR-006 (placement) also covers one-line accessors with an expression body or a comment. It is the decided reference of #1093 and outside list shape, so I left it alone.

## Changed requirements

- **SHARED-ATTR-013** — A list with several attributes is reported — clarified. The normative sentence now covers lists "not attached to an accessor", and adds: "A list attached to an accessor is reported as SHARED-ATTR-014 specifies." The example has no accessor and is unchanged.
- **SHARED-ATTR-014** — Accessor lists of an inline auto-property are not split — clarified.
  - The exemption sentence is unchanged.
  - The second sentence now reports only when the owner "spans more than one line, apart from its own attribute lists" (the wording of RH5531-001), instead of "any other accessor".
  - `Why:` gains one sentence for the multi-line case.
  - The example lost its `int Q { {|RH5528:[return: A, B]|} get => 1; }` line and keeps the exempt `P` and the multi-line `R`.

  The one-line, non-exempt accessor list is now stated by no requirement, neither as reported nor as not reported.
- **RH5531-001** — now cites "SHARED-ATTR-013 and SHARED-ATTR-014". Since the change to SHARED-ATTR-013, an accessor list reaches it only through SHARED-ATTR-014. Both IDs were already in `Uses`.
- **RH5531-006** — The fix is withheld for comments and directives — clarified.
  - The normative sentence keeps only the split half: "when SHARED-ATTR-019 withholds the split".
  - `Why:` now speaks of a comment inside the list.
  - The example lost its `int Q { [A] /* c */ {|RH5531:[B]|} get; set; }` line.

  The merge half could only apply to the case left out. A merge is specified only for lists SHARED-ATTR-006 exempts (RH5531-002). Any comment or directive around the merged group lies inside the accessor list, so the list is no longer exempt. SHARED-ATTR-020 can therefore never withhold an RH5531 merge on specified ground.
- **RH5531 header** — `SHARED-ATTR-020` was removed from `Uses`. RH5531 references it nowhere else.

No requirement was added or retired. The ledger, code owners, and `sdd/specs/README.md` are unchanged.

Checks:

- Scratch example verifier over `shared/attribute-targets.md` and the 16 analyzer specs: 128 checks, 0 failures under LF and under CRLF.
- `scripts/test.sh --project architecture`: 72 passed, including `SpecStructureTests` (`Uses`) and `RequirementCoverageTests`.

## Tagged tests that now assert behavior the spec no longer states

For the test implementer. The behavior is current and unchanged, so the tests still pass. They must stop tagging it as specified.

- **`RH5528ReturnValueAttributeListsMustFollowShapeRulesAnalyzerTests.VerifyDiagnosticForMultiAttributeAccessorListUnlessThePropertyIsAnInlineAutoProperty`** (tags RH5528-001, SHARED-ATTR-014)
  - Its `int Q { {|#0:[return: A, B]|} get => 1; }` line is the case left out.
  - `P` (exempt) and `R` (multi-line) stay specified. Remove the `Q` line, so the test expects one diagnostic, or move it to an untagged test.
- **`RH5528ReturnValueAttributeListsMustFollowShapeRulesAnalyzerTests.VerifyDiagnosticForMultiAttributeAccessorListWhenCommentSitsBetweenTheAccessors`** (tags RH5528-001, SHARED-ATTR-014)
  - The whole test, `int S { {|#0:[return: A, B]|} get; /* c */ set; }`, is the case left out.
  - Remove both tags. It may stay as an untagged characterization test.
- **`RH5531AccessorAttributeListsMustFollowShapeRulesAnalyzerTests.VerifyDiagnosticWithoutCodeFixWhenCommentIsInsideOrBetweenTheLists`** (tags RH5531-006, SHARED-ATTR-019, SHARED-ATTR-020)
  - Its `int Q { [A] /* c */ {|#1:[B]|} get; set; }` part is the case left out. The multi-line `P` with `[A, /* c */ B]` stays specified.
  - Remove the `Q` part from the tagged test, or move it to an untagged one.
  - Remove `[Requirement("SHARED-ATTR-020")]`: RH5531 no longer uses SHARED-ATTR-020.
  - After the change, the test name's "Or Between The Lists" no longer fits; rename at the test implementer's discretion.

Coverage after these changes:

- RH5528-001 and SHARED-ATTR-014 keep other tagged tests in the RH5528 and RH5531 classes.
- RH5531-006 keeps a second tagged test.
- SHARED-ATTR-020 stays covered by the RH5524 and RH5529 test classes.

The untagged tests the spec checker listed (the `…OnSingleLinePropertyAccessorWithExpressionBody` tests of RH5518 and RH5528) need no change.

## Examples for the tests

Unchanged from `01-spec-author.md`, with these changes:

- SHARED-ATTR-014 has lost its `Q` line, and RH5531-006 its `Q` line.
- No test of SHARED-ATTR-013, SHARED-ATTR-014, RH5528-001, or RH5531-006 may use a one-line property, indexer, or event whose accessor list SHARED-ATTR-006 does not exempt.

## Open questions to file

_None._ The case is the #1093 instance already named in the posted comment ("On a one-line property whose accessor list SHARED-ATTR-006 does not exempt, RH5531 requires merged lists while RH5528/RH5518 split"). For the lead: SHARED-ATTR-013 and RH5531-006 are now also affected requirements of that change. The posted comment does not list them, and a one-line addition to it would.
