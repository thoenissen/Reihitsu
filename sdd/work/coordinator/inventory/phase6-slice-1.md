# Phase 6 slice 1 (issues #122–#1446)

| # | Milestone | Area | Theme candidate | Verdict | Evidence | Size | User-visible | Depends on | Output question | Note |
|---|---|---|---|---|---|---|---|---|---|---|
| 122 | vNext | new project (VS extension) + CLI orchestration layer | IDE integration | open | — | L | yes | — | yes | No VS project exists in `src/`; a design-level proposal, not a squad-sized issue. |
| 862 | Maintenance | code fixes (shared)/Reihitsu.Core + analyzer RH5047/RH5408 + formatter/LineBreakPhase | internal refactor | open | — | S | no | — | no | Copies still in RH5047:47, RH5408:48, `LineBreakDetection.cs:53`, `AttributeTargetUtilities.cs:49`; `OrderingDeclarationUtilities.cs:1024` is a fifth per-accessor copy the acceptance `rg` would hit. |
| 895 | 1.4.0 | formatter/LineBreakPhase | chain layout | open | — | M | yes | — | no | FMT-BREAK-020 still limits the join to chains with a link; #1373 LB-Q10 instances (incl. primary-ctor base args) folded in. |
| 896 | 1.4.0 | formatter/IndentationPhase + analyzer RH5201 (`FluentChain.GetAnchorLink`) | chain layout | open | — | S | yes | — | yes | Call-less chains (`order.Customer.Address⏎.City`) left for the maintainer to decide. |
| 950 | Maintenance | formatter/SwitchCaseBracePhase, IndentationPhase | internal refactor | open | — | S | no | — | no | `RemoveBraces` still at `SwitchCaseBraceRewriter.cs:530`; `SwitchExpressionContributor` summary still says `switch` keyword. |
| 953 | vNext | formatter/IndentationPhase | continuation indentation | open | — | L | yes | — | no | Rule decided; instance list keeps growing (≈20 shapes in comments); `indentation.md` non-goal line 19 still tracks it. |
| 955 | vNext | formatter/LineBreakPhase | line-break layout | partly done | Part 2 (wrap after `where T :`) covered by FMT-BREAK-037 (colon/first-constraint join); part 1 (colon-ended ctor initializer) remains, FMT-BREAK-010 still "shares the line" | S | yes | — | no | Remaining part's continuation column interacts with #953. |
| 956 | vNext | formatter/LineBreakPhase | line-break layout | partly done | Leading-comma half of part 2 done by #1635 (FMT-BREAK-012/-038); remain: `)` on own line in argument lists, one switch-expression arm per line (+ leading comma between arms) | M | yes | — | yes | Closing-paren target ("follow last argument" vs. RH5102 stops reporting) and arm-comma shape are open output choices; analyzer side split to #1739. |
| 957 | vNext | formatter/HorizontalSpacingPhase | formatter spacing | partly done | Part 1 `fixed (` done (B2 #1510, `OneSpaceAfterFixedKeyword`); part 2 (`int _a  /** x */;`) remains, forbidden by FMT-SPACE-014 | S | yes | 1077 | no | Part 2 disappears once #1077 makes a non-documenting `/** */` an ordinary comment; consider closing into #1077. |
| 959 | vNext | formatter/CleanupPhase, DocumentationCommentFormattingPhase, PreparationPhase/LineEndings | line ends & cleanup | open | — | L | yes | — | no | Three ownerships, splittable; needs FMT-PIPE-002 exception; also fixes the RH8305 empty-summary trailing space. |
| 960 | vNext | formatter/CleanupPhase | line ends & cleanup | open | — | S | yes | — | no | Tab half now handled by `PreparationPhase/TabReplacement`; trailing spaces on directive lines remain (cleanup.md non-goal l.19); EOF part split to #1747 (closed). |
| 961 | vNext | formatter/DocumentationCommentFormattingPhase | documentation-comment formatting | open | — | M | yes | — | no | FMT-DOC-010 still keeps case 1; FMT-DOC-009 still undecided on inline elements. |
| 962 | vNext | formatter/DocumentationCommentFormattingPhase + BlankLinePhase | documentation-comment formatting | open | — | M | yes | 1077 | yes | FMT-DOC-001 says other positions are "not yet decided"; many added instances (attribute, accessor, type param, list commas) need reading A/B confirmed. |
| 975 | vNext | CLI | CLI feature | open | — | S | yes | — | no | `DefaultDiffGenerator` still passes the full path; CLI spec still pins `a//work/src/A.cs`. |
| 1077 | vNext | analyzer/RH6xxx spacing + formatter | documentation-comment formatting | partly done | Comment-placement rules done (#1386 closed, SHARED-COMMENT-001 "documents nothing"); RH3201 instance done (#1280); spacing rules RH6001/6005/6009/6011/6023/6024 and the formatter instances remain | M | yes | — | no | Formatter side overlaps #962 and #957 part 2. |
| 1081 | Maintenance | specs only (RH5025) | spec clarification | partly done | Item 2 (lone `\r`) in FMT-PIPE-007; standalone-block part superseded by C3/O-10 (RH5025-005); remain: sibling braces (initializer element, switch arm, braced section) in RH5025-001, U+0085/U+2028 counting | S | no | — | no | RH5025-001 still lists only braces that continue code above. |
| 1082 | vNext | formatter/CleanupPhase + analyzer RH5603 spec | line ends & cleanup | open | — | S | yes | — | yes | FMT-CLEAN-007 unchanged; `///`-last-line case (#1720) and directive-only file option A/B added as open choices. |
| 1083 | vNext | analyzer RH5601 + formatter/PreparationPhase (FMT-TAB-002) | region handling | open | — | S | yes | — | no | FMT-TAB-002 still keeps a tab inside a region description; no SHARED-FIX-003 exception needed (2026-10-18 update). |
| 1095 | vNext | analyzer RH5029/RH5021 + formatter/BlankLinePhase | blank-line scope | open | — | S | yes | — | no | No spec mentions deconstruction declarations. |
| 1098 | vNext | analyzer RH50xx region comment rules + formatter/LineBreakPhase (FMT-BREAK-015) | comment rule scope | open | — | M | yes | — | no | SHARED-COMMENT-006 today exempts a comment before an anonymous object's `{` and before `switch` (its RH5038 example), the opposite of this decision. |
| 1131 | Maintenance | specs only (formatter/line-breaks.md) | spec clarification | partly done | #1111 Q1 done (FMT-BREAK-012 joins a list wrapped only behind `(`); `Uses` lists only SHARED-ATTR-014; lambda attribute list (Q2) not in FMT-BREAK-027 | S | no | — | no | Re-scope after C6 moved split/merge to FMT-STRUCT-028/-029. |
| 1148 | vNext | formatter/BlankLinePhase + analyzer RH5020/RH8303 | blank-line scope | open | — | M | yes | — | no | FMT-BLANK-011 still applies in any token's leading trivia; original two inputs fixed by C3 (FMT-BLANK-018). |
| 1291 | Maintenance | specs only (FMT-BREAK-018/-021, shared/struct-equality) | spec clarification | partly done | Struct-equality definition stated (`shared/struct-equality.md:22`); FMT-BREAK-018 still says "RH5308 reports the same shapes" and FMT-BREAK-021 has no P11 statement for RH5304 | S | no | — | no | — |
| 1354 | vNext | analyzer RH7302 + formatter/RegionFormattingPhase | region handling | open | — | M | yes | — | no | FMT-REGION-001 still capitalizes; related #1334 (which name a region must carry). |
| 1355 | vNext | formatter/DocumentationCommentFormattingPhase | documentation-comment formatting | open | — | S | yes | — | no | `DocCommentElementNormalizer.cs:165` still `OrdinalIgnoreCase`; companion new rule #1356. |
| 1399 | vNext | formatter/IndentationPhase | continuation indentation | open | — | M | yes | — | no | FMT-INDENT-010 unchanged; switch-section `#endregion` drift instance unchecked against the spec. |
| 1400 | Maintenance | specs only (FMT-INDENT-010/-028, FMT-SPACE-001) | spec clarification | partly done | Item 24 stated in FMT-SPACE-001 ("a line break inside the text of a multi-line comment is not a line break of the gap"); items 22 (inactive directive "not re-indented") and 23 (explicit-return-type lambda anchor) remain | S | no | — | no | — |
| 1432 | vNext | formatter/IndentationPhase (raw strings) | continuation indentation | open | — | S | yes | — | no | `raw-string-alignment.md` non-goal l.16 still tracks it. |
| 1439 | vNext | formatter/StructuralTransformPhase | expression-body conversion | open | — | S | yes | — | no | FMT-STRUCT-006 says where a comment at the arrow belongs "is a separate decision"; should run with #1515. |
| 1440 | vNext | formatter/HorizontalSpacingPhase | formatter spacing | partly done | Done: `?:` (FMT-SPACE-018), name colons (-019), `when` (-020), constraint colon (-009), relational/modifier gaps (-023/-024); remain per non-goal l.17: `[`/`(` before element access/argument list, `typeof (`, `::`, inside of one-line initializer braces (+ `new[]{1,2}`) | M | yes | — | no | Remaining pairs are all in the decided list. |
| 1442 | vNext | formatter/LineBreakPhase | line-break layout | open | — | M | yes | — | no | FMT-BREAK-018/-022/-025 unchanged; bug part #1397 closed. |
| 1446 | vNext | analyzer RH5404 + formatter/LineBreakPhase | line-break layout | open | — | M | yes | — | no | RH5404 non-goal l.21 still tracks it; bug part #1276 closed. |

## Closure candidates

- #955 — partly done: the constraint-clause half is FMT-BREAK-037; narrow the issue to the colon-ended constructor initializer.
- #956 — partly done: leading commas in argument lists done by #1635; narrow to `)` placement and switch-expression arms.
- #957 — partly done: `fixed (` done (#1510); part 2 can be closed into #1077, which removes it as a consequence.
- #1077 — partly done: comment-placement rules done (#1386, #1280); narrow to spacing rules and the formatter.
- #1081 — partly done: lone `\r` in FMT-PIPE-007, standalone block superseded by C3; narrow to RH5025 sibling braces and U+2028/U+0085.
- #1131 — partly done: FMT-BREAK-012 boundary join stated; narrow to `Uses` and the lambda attribute case.
- #1291 — partly done: struct-equality definition stated; narrow to FMT-BREAK-018/-021 wording.
- #1400 — partly done: item 24 stated; narrow to items 22 and 23.
- #1440 — partly done: `?:`, name colons, `when`, constraint colons, relational and modifier gaps specified; narrow to element access/invocation/`typeof`/`::`/initializer braces.

## Overlaps

- #1077 + #962 + #957 (part 2): one P6 run for "a documentation-style comment that documents nothing" in the formatter (relocation, spacing, blank line).
- #962 + #1148: both put a blank line inside a construct before a comment (FMT-BLANK-011/-012); decide scope together.
- #1439 + #1515 (slice 2): comments at the arrow of an expression body; #1439 is the decided subcase.
- #953 + #955 (part 1) + #1399 + #1633 (slice 3): continuation/region columns; #953 is the general rule the others land on.
- #956 + #1739 (slice 8) + #895: argument-list layout (formatter) vs. its new analyzer rule; attached parts outside chains.
- #959 + #960 + #1082: line-end and end-of-file cleanup (FMT-CLEAN, FMT-EOL).
- #1355 + #1356 (slice 6): case-sensitive documentation tags (formatter) and the new rule.
- #1354 + #1334: region names prescribed by RH7401–RH7412.
- #1083 + #1354: region-description text normalization (tabs, casing).
- #1442 + #862: accessor-list predicate (#1442 item 15 changes who counts as joinable).
- #1131 + #1291 + #1400 + #1081: Maintenance spec-clarification batch (docs lane, one PR possible).
- #896 + #895: chain layout (1.4.0).
