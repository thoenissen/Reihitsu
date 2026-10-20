# Round 7b: remaining open questions (26)

Copied from `round7-plan.md` section 3 (24 entries) and extended by two entries the maintainer did not decide in round 7a (#1185 Q1, #1175 Q4 third case). Each question issue keeps its decision comment of round 7a; the issues below stay open. Recommendations are the plan's.

## Group A. The rule page accepts a nested one-line form, the formatter expands it (2)

```
#1214 Q1 (RH5304 vs FMT-BREAK-021)
  var o = new C { L = { 1, 2 } };
  today: not reported (rule page: accepted); the formatter, and the fix of another assignment in the same initializer, expands it to a block.
  A) keep the analyzer, state in FMT-BREAK-021 that the formatter expands it (clarification; P11: formatter may do more)
  B) report it (behavior change, new diagnostics)
  Recommendation: A. Kind: clarification (SDD spec), and the fix expansion is gone anyway via P9 (#1214 Q3).

#1218 Q4 (RH5308 vs FMT-BREAK-018)
  var x = a ? 1 : b ? 2 : 3;
  today: not reported (rule page: one-line conditional never reported); the formatter always spreads a conditional containing a nested one.
  A) keep as is, state it in FMT-BREAK-018 (clarification)   B) report nested one-line conditionals (behavior change)
  Recommendation: A, same answer as #1214 Q1. Kind: clarification.
```

## Group B. Regions (4)

```
#1176 Q2, analyzer part (RH7304)
  class C { #region Outer  void M() { #region Inner ... #endregion } #endregion }   // Inner opened at type level, closed inside M
  today: the analyzer pairs only directives outside executable code (Inner with Outer's #endregion), reports both; the fix pairs like the compiler, edits the #endregion in M, the diagnostic stays. (Fix part decided: withheld.)
  A) analyzer pairs like the compiler and, when a pair has a directive in executable code, reports nothing for it (RH7303 already reports the stray #endregion)
  B) keep the analyzer pairing, state it in the spec (clarification)
  Recommendation: A. Kind: bug (analyzer), with #1262.

#1177 Q1 (RH7306)
  #region Implementation ... #endregion // Implementation
  today: both reported; the fix writes `#region` ... `#endregion //` (unnamed region, dangling `//`).
  A) withhold the fix when nothing remains of the name, keep the diagnostic (consistent with RH7301-005: choosing a name is the user's)
  B) remove the dangling `//` too   C) keep as is
  Recommendation: A. Kind: bug (fix) in #1262.

#1185 Q1 (RH7309), NOT decided in round 7a (plan had it closed by AGENTS.md "prefer no fix"; the maintainer override leaves it open)
  The RH7309 fix relocates region blocks but not the comments above them (`// The disposal logic` stays above the other region).
  A) withhold the fix when a comment stands directly above a region block (AGENTS.md "prefer no fix", SHARED-FIX-003; comments are not carried along)
  B) carry the comments with their block (extension of #1262; relates to #1136)
  Recommendation (plan R7): A. Kind: bug (fix). Decision wanted before #1262 is implemented.

#1175 Q4, third case (RH7303 / RH7310 fix, P14c vs P14a)
  if (a){}
  #region R
  ...
  #endregion
  M(a);
  After the removal `}` is directly followed by `M(a);`: P14c cleans blank lines the removal leaves, P14a forbids inserting one, the formatter wants a blank line after `}`.
  A) leave it: nothing is inserted (plan R4)   B) insert the one blank line the formatter wants
  Recommendation: A; confirm with the maintainer when #1263 runs. Kind: clarification of P14c/P14a.
```

## Group C. Documentation rules (4)

```
#1195 Q2 (RH8102/8103/8109/8110) and #1199 Q3 (RH8202): declaration-kind scope
  /// <param name="z"/>  on a class without primary ctor -> RH8102 silent; on `M(int a, int b)` surplus param reported; RH8103 reports <param> on class `C`, not on interface `I` or property `P`; RH8110 reports <typeparam/> on `Run()`, not on a ctor.
  /// <value> on a method -> RH8202 silent (only properties/indexers registered).
  A) report on every documentable declaration (the element documents something that does not exist; P11 own gap)
  B) keep the current scope per rule and write it into the specs (clarification)
  Recommendation: A for #1195 Q2 (behavior change, new diagnostics, one issue), A for #1199 Q3.

#1196 Q1 (RH8106 vs RH8107): one span, two rules
  /// <returns/>  on `void H()` -> RH8106 ("no text", advice: add text) and RH8107 (void must not document a return) report the same span.
  A) RH8107 owns void members, RH8106 skips them (behavior change)
  B) both report (documented overlap)
  Recommendation: A, otherwise RH8106's advice contradicts RH8107 (SHARED-FIX-004 spirit). Kind: behavior change.

#1200 Q2 (RH8203): constructor matching
  class B { private B(ref int a) {} }  class D : B { /// <inheritdoc/> D(int a) {} }
  today: signature compared by type only (ref/in/out/params, nullable annotations, base accessibility ignored), so D "inherits".
  A) match what the compiler resolves (ref kind, accessibility; nullability ignored)   B) keep type-only matching
  Recommendation: A (consistent with P8). Kind: bug.
```

## Group D. RH3001 (1)

```
#1245 Q3 (RH3001 fix output)
  B >>>= !x;  B ??= !x;   -> (x == false) with parentheses
  also named attribute args, `goto case`, `select`/`group`/`orderby` get parentheses; the same positions with `=`, `+=`, `>>=`, `where`, `let`, `join` get none.
  Partly decided: where RH3002 (widened by #1224 Q1) reports the produced pair, P7/SHARED-FIX-004 makes it a bug. Open: positions RH3002 does not check (`select`, `group`, `orderby`, `goto case`, named attribute args).
  A) parentheses only where binding needs them (bug)   B) accept the Simplifier result (clarification)
  Recommendation: A; decide after the RH3002 issue (#1278) exists. Kind: bug (guard), in #1285.
```

## Group E. Equality analyzers (RH1001/RH1002, SHARED-EQUAL) (9)

```
#1247 Q1
  struct S : IEquatable<S> { }  // no GetHashCode override
  HashSet<S> / items.Distinct()  -> not reported (rule pages: "IEquatable or override both"; tests pin it)
  A) keep ("has equality members" = IEquatable OR overrides)  B) require GetHashCode too, report this input
  Recommendation: A, clarification text in the spec (boxing ValueType.GetHashCode is cheaper than no equality at all and the pages already say so). Kind: clarification.

#1247 Q2
  Dictionary<S?, int>, Dictionary<(int, S), int> -> not reported; Dictionary<S, int> reported.
  A) look through Nullable<S> and tuples (new diagnostics)  B) outer type only, stated in the spec
  Recommendation: B for now (P4: nothing forces it; a specialized rule if wanted). Kind: clarification (A would be behavior change).

#1247 Q3
  ToImmutableDictionary(x => x, x => 0, null, valueComparer)  -> not reported (comparer for the int values taken as key comparer; keys hashed by default).
  A) judge the comparer's type argument against the key type (bug)  B) keep
  Recommendation: A. Kind: bug.

#1247 Q6
  Distinct(default(TC)) with TC : IEqualityComparer<S> -> reported; default(TV) with a struct constraint -> not reported.
  A) default(T) of a type parameter is a null comparer only when T is a reference type (constraint class); otherwise it is a real comparer, report neither struct nor unconstrained  B) report both
  Recommendation: A. Kind: bug (edge case, in #1290 or separate small).

#1248 Q2 / Q3 (collection expressions, RH1001)
  HashSet<S> set = [with(_comparer), _item];   -> declared type reported (but `new(_comparer) { .. }` not)
  Take([_item]);  (parameter HashSet<S>)        -> not reported; Take(new()) reported
  A) recognize collection expressions: `with(comparer)` counts as comparer; `[_item]` converted to HashSet<S> counts as a creation without comparer (new diagnostics)
  B) do not handle collection expressions (state it)  C) Q2 only
  Recommendation: A for both (the same creation, preview syntax already compiles). Kind: behavior change.

#1249 Q1 (RH1002 method list)
  items.CountBy(x => x), AggregateBy, LeftJoin, RightJoin, ImmutableHashSet.Create/CreateRange, ImmutableDictionary.CreateRange -> not reported (GroupBy, Join, ToImmutable* are).
  A) extend the fixed list (behavior change)   B) keep, list stated in the spec
  Recommendation: A. Kind: behavior change.

#1249 Q2 (RH1002 query clauses)
  from x in items group x by x;  join ... on ...;  join ... into ...;  -> not reported (compiler emits GroupBy/Join/GroupJoin).
  A) report at the clause keyword (`group`/`join`) (needs a location rule)   B) not covered, stated
  Recommendation: A, location = the clause keyword (P21). Kind: behavior change.

#1250 Q1 (RH2006/RH2007, SHARED-DEBUG)
  Debug.Assert(c, string.Empty);  Debug.Fail(string.Empty);  Debug.Assert(c, flag ? "" : null);  -> not reported; Debug.Assert(c, "") reported.
  A) `string.Empty` and conditionals whose every branch is empty are "no text"  B) only literal constants
  Recommendation: A for `string.Empty` and constant-folding (`const string E = ""`), B for conditionals with `null` branches? Simplest: A for both, using P16 transparency. Kind: behavior change.
```

## Group F. RH2001 (2)

```
#1251 Q4
  int P { get => field; set => field = value * 2; }   int Other { get => field; }   int Plain => field;   -> reported, no fix
  A) report only properties whose accessors are pure auto-implemented (compiler-created backing field AND no accessor logic)  B) keep the compiler definition (reported, no fix)  C) report none using `field`
  Recommendation: B (no fix is a supported end state), written into the spec. Kind: clarification.

#1251 Q5
  [field: NonSerialized] private int Cache { get; set; }   int Q { [ExcludeFromCodeCoverage] get; set; }   -> reported; rule page says "properties decorated with attributes are exempt".
  A) exempt them as the page says (bug)  B) report, correct the page (fix could move `[field:]`)
  Recommendation: A for accessor attributes; `[field: ...]` too (the fix cannot move it safely). Kind: bug.
```

## Group G. Explicit interface members (3)

```
#1252 Q1 (RH2002)
  async void I.M() {...}  -> not reported; public async void N() reported.
  A) report explicit implementations too (same hazard)  B) exempt (interface dictates void)
  Recommendation: A for async void: an explicit implementation can still return Task only if the interface does, so B is right only when the interface method returns void... The hazard is the same either way; A reports (no fix when the interface dictates void, P23). Kind: behavior change.

#1254 Q1 (RH2004 fix)
  static S operator -(S a) {...}   -> the fix adds `private`, still CS0558.
  A) fix adds `public` for operators (RH2004 asks for explicit accessibility; operators must be public static)  B) withhold the fix
  Recommendation: A. Kind: bug (fix).

#1254 Q2 (RH2004)
  static N IAdd<N>.operator +(N a, N b) {...}   and explicit conversion  -> reported; explicit implementations take no modifier; RH2004-003 exempts methods, properties, indexers, events but not operators.
  A) extend the exemption to operators and conversions (bug)  B) keep, withhold the fix only (#1254 Q3)
  Recommendation: A (then #1254 Q3 is moot). Kind: bug.
```

## Group H. RH2109 (1)

```
#1256 Q4
  @functions { int Count; }  -> not reported (older equivalent of @code)
  A) report @functions like @code (behavior change)  B) @code only
  Recommendation: A (same risk, same fix text if the rule has one). Kind: behavior change.
```

Count: A 2, B 4 (#1176 Q2 analyzer, #1177 Q1, #1185 Q1, #1175 Q4 case 3), C 4, D 1, E 9, F 2, G 3, H 1 = 26. Open question issues: 19 (#1175, #1176, #1177, #1185, #1214, #1218, #1195, #1196, #1199, #1200, #1245, #1247, #1248, #1249, #1250, #1251, #1252, #1254, #1256).
