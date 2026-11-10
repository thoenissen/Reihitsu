Role: spec author
Commit: 478f89c
Verdict: #1269: no change; #1342: spec written; #1343: no change; #1459: spec written

The prompt carried no intended fix, suspected cause, or hoped-for verdict. This round follows `00-lead.md` decisions 20, 21, and 22. It specifies behavior only. The entry point's name is the one spec text the implementer has to follow (see *Whole run → Internal choices*).

## thoenissen/Reihitsu#1269

### Changed requirements

_None._ Decision 19 (the RH8201 test inputs) needs no spec text: RH8201-005 already decides it.

### Examples for the tests

_None._

### Output questions

_None._

### Internal choices

_None._

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1342

### Changed requirements

- RH8307-006 — The fix is withheld for an element of a `/** */` comment — added (decision 21). The fix is not offered for an element of a delimited documentation comment, whether the element is at the top level or nested. Whether such an element is reported is explicitly not decided, because RH8307-001 does not cover `/** */`. Why: FMT-DOC-012 means the formatter has no layout for the fix to produce (SHARED-FIX-007).
- RH8307-004 — clarified: the coverage sentence points to RH8307-006. The element's new text is the text FMT-PIPE-032 gives the element, the columns of its lines included (decision 22). `Uses` gains FMT-DOC-012, FMT-PIPE-032, SHARED-FIX-007.
- RH8305-004 — clarified the same way (FMT-PIPE-032). `Uses` gains FMT-PIPE-032.
- FMT-PIPE-032 — The documentation entry point lays out one documentation element — added (decision 22). It states what the entry point receives, what it does, and what callers pass:
  - **Receives:** a document and one XML element of a `///` comment.
  - **Lays out:** it replaces only the element's span with the text the documentation-comment phase gives that element (FMT-DOC-008, -009, -010, -011). Nested elements are rebuilt, and an element that is already canonical is left as it is.
  - **Leaves as it is:** everything outside the element, including the text before and after the element on its lines, the `///` separator in front of the start tag, the comment's other lines and elements, the other comments and blank lines, the code, and the rest of the document. It runs no other phase and no other documentation-phase requirement: no FMT-DOC-001 move, no FMT-DOC-005 outside the element, and no FMT-INDENT.
  - **Lays out in place**, not as a detached copy (FMT-PIPE-023 does not apply). Each added line starts with the spaces and tabs in front of the `///` on the start tag's line; a byte order mark is not repeated. Each line break is the document's line ending (FMT-PIPE-007).
  - **Does not check or refuse:** no syntax-error check, no generated-code check, no unsupported-syntax refusal. It returns the document, not a `FormattingResult`.
  - **Supported inputs:** it decides only what the RH8305 and RH8307 fixes pass. Behind-code comments, FMT-DOC-013 comments, `/** */` comments, and elements without their own end tag are explicitly not decided.
  - Two examples: a compilation-unit-level `<remarks>` (RH8307), where the summary and `///Text` stay; and a member summary whose line is indented differently from the comment's other line (RH8305).
- FMT-PIPE-009 — clarified: the entry-point list names `FormatDocumentationElementInDocumentAsync` (FMT-PIPE-032).
- FMT-UNSUP-001, FMT-UNSUP-002 — clarified: the new entry point refuses no input and returns the document. No shape FMT-UNSUP declares can stand inside a documentation element.
- SHARED-FIX-010 — clarified: a fix that formats only through `OrganizeUsingDirectivesInDocumentAsync` or `FormatDocumentationElementInDocumentAsync` asks `FindUnsupportedSyntax` nothing.
- `documentation-comments.md` intro — names the entry point as the RH8305/RH8307 fixes' path to the phase's element layout. `Uses` gains FMT-PIPE-032.
- `Uses` updates: `pipeline.md` gains FMT-DOC-003, -005, -008, -009, -010, -011, -012, -013; `unsupported-syntax.md` gains FMT-PIPE-032; `shared/code-fixes.md` gains FMT-PIPE-030, FMT-PIPE-032.
- `requirement-ids.txt`: FMT-PIPE-032 and RH8307-006 added.

### Examples for the tests

- RH8307-006: the `/** */` example (characterization of today's report; assert that no fix is offered). Also cover a nested element of a `/** */` comment.
- FMT-PIPE-032: both examples, as formatter unit tests that call the entry point directly with the element the fix would pass. Also add a CRLF variant and an element on a later line of the comment whose `///` lines have different indentation.
- RH8305-003 and RH8307-003: the positional-record examples, each reported once (decision 20; the existing compilation-unit rows may already pin this, so the test implementer only needs to tag them).

### Output questions

_None._ RH8307-006 withholds a fix for a rare class (decision 21). FMT-PIPE-032 changes no output (decision 22).

### Internal choices

- RH8307-006 does not decide whether a `/** */` element is reported, and RH8307-001 is left alone. The alternative was to widen RH8307-001 to cover it. I did not, because it looks wrong (see *Follow-up candidates*).
- FMT-PIPE-032 takes the indentation of added lines from the element's own start-tag line, not from the comment's first `///` line (which FMT-DOC-011 uses inside the phase). This matches today's fix output, and the two differ only when a comment's lines are indented differently. In document formatting the indentation phase decides those columns anyway.
- FMT-PIPE-032 does not decide the inputs that no caller passes (AGENTS.md *Supported inputs*). The entry point therefore needs no guards for them. The fixes keep their own registration checks under RH8305-005, RH8307-005, and RH8307-006.

### Follow-up candidates

- **RH8307 · Content after opening XML tag must be on same line as closing tag — element in a `/** */` comment.**
  - Input:
    ```csharp
    class C
    {
        /** <summary>
        Total
        </summary>
        <remarks>Sum
        more</remarks> */
        int A;
    }
    ```
  - Real output: `<remarks>` is reported. After this run, no fix is offered.
  - Why it looks wrong: RH8307-001 excludes `/** */`. RH8304, RH8305, and RH8308 report nothing in a `/** */` comment. The formatter never changes the comment's text (FMT-DOC-012), so the diagnostic can never go away except by hand.
  - Readings: A — reported, no fix (today). B — not reported, like the sibling rules.
  - Recommendation: B. Outside the issues.

## thoenissen/Reihitsu#1343

### Changed requirements

_None._

### Examples for the tests

_None._

### Output questions

_None._

### Internal choices

_None._

### Follow-up candidates

_None._

## thoenissen/Reihitsu#1459

### Changed requirements

- RH8305-003, RH8307-003 — clarified (decision 20): an element in the comment of a record, class, or struct with a primary constructor is reported once. Each has a new positional-record example. No new requirement is needed: "one diagnostic per reported element" already demanded this, so the duplicate was a bug against -003.

### Examples for the tests

RH8305-003, RH8307-003 (positional record).

### Output questions

_None._

### Internal choices

_None._

### Follow-up candidates

_None._ The 04-implementer candidate (other documentation analyzers reporting twice on primary-constructor types) is already in `00-lead.md`.

## Whole run

### Internal choices

- **Entry point name** `ReihitsuFormatter.FormatDocumentationElementInDocumentAsync`, after `FormatNodeInDocumentAsync` and `OrganizeUsingDirectivesInDocumentAsync`. The specs use this name in FMT-PIPE-009, FMT-PIPE-032, FMT-UNSUP-001/-002, SHARED-FIX-010, and the `documentation-comments.md` intro. If the implementer picks another name, those five places change with it.
- No change doc: decision 22 changes no output, and RH8307-006 is rare-class withholding (as decision 13).
- `scripts/test.sh --configuration Debug --project architecture --filter "FullyQualifiedName~SpecStructureTests|FullyQualifiedName~SpecConsistencyTests|FullyQualifiedName~RuleDocumentation"`: 26 passed. `RequirementCoverageTests` will fail on FMT-PIPE-032 and RH8307-006 until their tests exist.
- Seen in the build, not mine to fix: `src/Reihitsu.Analyzer/Core/DocumentationAnalysisUtilities.cs(636,26): warning RH7110` (member order of the new `IsPrimaryConstructorRun`). This is for the implementer or the cleaner.
- Rule pages are unchanged: a withheld fix and a no-duplicate clarification do not change the normal case.
