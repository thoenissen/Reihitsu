# Konzept: Korrektur der Einrückung logischer Operatoren in verschachtelten Kontexten

## 1. Problembeschreibung

Logische Operatoren (`&&`, `||`) werden in tief verschachtelten Kontexten zu weit rechts eingerückt.
Das Problem tritt auf, wenn logische Ausdrücke innerhalb von Expression-Lambdas stehen, die wiederum als Argumente in Method-Chains, Initializer-Zuweisungen oder Array-Initialisierern vorkommen.

### Beispiel

**Erwartet:**
```csharp
new ActionNode
{
    Execute = async value =>
              {
                  if (_serviceProvider.GetService<UserLedger>()
                                      .Refresh(current => records.Any(item => item.SessionId == current.Id
                                                                              && item.ActorId == Context.User.Id),
                                               current =>
                                               {
                                                   // ...
                                               }) == false)
                  {
                      throw _serviceProvider.LastProblem;
                  }
              }
}
```

**Aktuell (fehlerhaft):**
```csharp
new ActionNode
{
    Execute = async value =>
              {
                  if (_serviceProvider.GetService<UserLedger>()
                                      .Refresh(current => records.Any(item => item.SessionId == current.Id
                                                                                              && item.ActorId == Context.User.Id),
                                               // ...
```

Das `&&` wird um einen zusätzlichen Delta-Wert nach rechts verschoben, anstatt direkt unter dem linken Operanden (`item`) zu stehen.

---

## 2. Architektur-Überblick

### Formatting-Pipeline

Die Pipeline (`FormattingPipeline.Execute`) führt Regeln sequenziell in Phasen aus:

```
Phase 0: StructuralTransform  → Brace placement, expression body conversion
Phase 1: Spacing              → HorizontalSpacingRule
Phase 2: Indentation          → IndentationAndAlignmentRule  ← BETROFFEN
Phase 3: BlankLineManagement  → BlankLineBeforeStatementRule, BlankLineAfterStatementRule
Phase 4: RegionFormatting     → RegionFormattingRule
Phase 5: Cleanup              → TrailingTriviaCleanupRule
```

### IndentationAndAlignmentRule

Die Klasse erbt von `CSharpSyntaxRewriter` (via `FormattingRuleBase`) und verarbeitet den gesamten Syntaxbaum in einem einzigen Durchlauf. Die Verantwortung ist zweigeteilt:

1. **`VisitToken`** — Block-Einrückung für alle first-on-line Tokens (absolute Positionierung nach Baumtiefe)
2. **Node-Level-Visitors** — Überschreiben die Block-Einrückung für Fortsetzungszeilen (Argument-Alignment, Chain-Alignment, logische Operatoren, etc.)

---

## 3. Ursachenanalyse

### CSharpSyntaxRewriter: Depth-First-Reihenfolge

`CSharpSyntaxRewriter` traversiert den Syntaxbaum **depth-first**. Das bedeutet:

1. **Innere Visitors laufen zuerst** — `VisitBinaryExpression` positioniert `&&` korrekt relativ zum linken Operanden
2. **Äußere Visitors laufen danach** — `VisitArgumentList` → `ShiftLambdaBlockBodies` → `ShiftExpressionLambdaBody` wendet einen einheitlichen Shift auf alle Tokens an

### Der konkrete Fehler-Ablauf

Am Beispiel der Expression-Lambda `item => item.SessionId == current.Id && item.ActorId == Context.User.Id`:

```
Schritt 1: VisitBinaryExpression (innerer Visitor)
  → Findet BinaryExpression: (item.SessionId == current.Id) && (item.ActorId == Context.User.Id)
  → Positioniert && auf Spalte von "item.SessionId" (dem ersten Token des linken Operanden)
  → KORREKT: && steht unter item

Schritt 2: VisitArgumentList für .Refresh(...) (äußerer Visitor)
  → base.VisitArgumentList() gibt bereits transformierten Baum zurück (mit korrekt positioniertem &&)
  → AlignArgumentList() → ShiftLambdaBlockBodies() → ShiftExpressionLambdaBody()
  → Berechnet Shift-Delta D für die Expression-Lambda-Body
  → Iteriert über ALLE Tokens der Expression-Body:
      - "item.SessionId" → auf expressionStartLine → NICHT verschoben (korrekt)
      - "&&"             → nach expressionStartLine → VERSCHOBEN um D (FEHLERHAFT!)
      - "item.ActorId"   → nach expressionStartLine → VERSCHOBEN um D
  → ERGEBNIS: "item.SessionId" bleibt, aber && wird um D zusätzlich nach rechts verschoben
```

### Kern des Problems

`ShiftExpressionLambdaBody` wendet einen **nicht-uniformen Shift** an: Die erste Zeile der Expression-Body wird übersprungen, alle Folgezeilen werden verschoben. Da `&&` von `VisitBinaryExpression` **relativ zur ersten Zeile** positioniert wurde, entsteht durch den einseitigen Shift eine Fehlausrichtung.

### Betroffene Shift-Methoden

Das gleiche Muster existiert in allen Methoden, die descendant Tokens iterieren und verschieben:

| Methode | Aufgerufen von | Problem |
|---------|---------------|---------|
| `ShiftExpressionLambdaBody` | `ShiftLambdaBlockBodies` | Verschiebt `&&`/`\|\|` in Expression-Lambdas |
| `ShiftLambdaBlockBodies` (Block-Teil) | `AlignArgumentList` | Verschiebt Tokens in Block-Lambda-Bodies |
| `ShiftArmContinuationLines` | `VisitSwitchExpression` | Verschiebt Tokens in Switch-Arms (überspringt bereits Chain-Dots) |
| `ShiftNodeContinuationLines<T>` | `VisitCollectionExpression`, `AlignArrayInitializer` | Generisches Verschieben |
| `ShiftArgumentByColumn` | `AlignArgumentList` | Verschiebt alle Tokens in einem Argument |
| `BuildInvocationContinuationShiftPairs` | `AlignChain` | Verschiebt Tokens in Invocations |

---

## 4. Lösungsansatz

### Grundprinzip

Ein Continuation-Token (z.B. `&&`, `||`, `.`, `?.`, `or`, `and`, `is`) wurde von seinem zuständigen inneren Visitor **relativ zu einem Anker-Token** positioniert. Beim Verschieben von Continuation-Lines muss daher geprüft werden:

- **Anker auf der Start-Zeile (nicht verschoben)** → Continuation-Token **NICHT verschieben**
- **Anker auf einer Continuation-Zeile (wird verschoben)** → Continuation-Token **VERSCHIEBEN** (gleicher Delta, relative Position bleibt erhalten)

### Lösung: Anker-basierte Skip-Logik in Shift-Methoden

#### 4.1 Neue Helper-Methode: `TryGetContinuationAnchorLine`

Eine zentrale Methode, die für ein gegebenes Continuation-Token die Zeile seines Alignment-Ankers bestimmt:

```csharp
/// <summary>
/// Tries to determine the line of the alignment anchor for a continuation token.
/// The anchor is the reference token that the continuation token was aligned to
/// by an inner visitor (e.g., the left operand's first token for a binary operator).
/// </summary>
/// <param name="token">The continuation token.</param>
/// <param name="anchorLine">The line of the alignment anchor.</param>
/// <returns><c>true</c> if an anchor was found; otherwise, <c>false</c>.</returns>
private static bool TryGetContinuationAnchorLine(SyntaxToken token, out int anchorLine)
{
    anchorLine = -1;

    // Binary expression operators (&&, ||, ??, +, -, ==, !=, etc.)
    if (token.Parent is BinaryExpressionSyntax binary
        && binary.OperatorToken == token
        && IsLogicalOrNullCoalescingExpression(binary))
    {
        anchorLine = GetLine(binary.Left.GetFirstToken());
        return true;
    }

    // Binary pattern operators (or, and)
    if (token.Parent is BinaryPatternSyntax binaryPattern
        && binaryPattern.OperatorToken == token)
    {
        anchorLine = GetLine(binaryPattern.Left.GetFirstToken());
        return true;
    }

    // 'is' keyword in is-pattern expressions
    if (token.IsKind(SyntaxKind.IsKeyword)
        && token.Parent is IsPatternExpressionSyntax isPattern)
    {
        anchorLine = GetLine(isPattern.Expression.GetFirstToken());
        return true;
    }

    return false;
}
```

> **Hinweis**: Chain-Dots (`.`, `?.`) werden hier bewusst nicht behandelt, da `AlignChain` diese separat verwaltet und `ShiftArmContinuationLines` diese bereits über `CollectFluentChainDotSpanStarts` ausschließt. Die Logik kann bei Bedarf erweitert werden.

#### 4.2 Neue Helper-Methode: `ShouldSkipContinuationToken`

```csharp
/// <summary>
/// Determines whether a continuation token should be skipped during a
/// non-uniform shift operation (where the first line/start line is not shifted).
/// A continuation token should be skipped when its alignment anchor is on a
/// non-shifted line, because the token was already correctly positioned relative
/// to that anchor by an inner visitor.
/// </summary>
/// <param name="token">The token to check.</param>
/// <param name="nonShiftedLineThreshold">
/// Tokens on lines &lt;= this threshold are not shifted.
/// This is typically the expression start line or the first line of the construct.
/// </param>
/// <returns><c>true</c> if the token should be skipped; otherwise, <c>false</c>.</returns>
private static bool ShouldSkipContinuationToken(SyntaxToken token, int nonShiftedLineThreshold)
{
    if (TryGetContinuationAnchorLine(token, out var anchorLine) == false)
    {
        return false;
    }

    // If the anchor is on a non-shifted line, the token was positioned relative
    // to that anchor and should NOT be shifted (otherwise it would drift away).
    // If the anchor is on a shifted line, both anchor and token will be shifted
    // by the same delta, preserving their relative alignment.
    return anchorLine <= nonShiftedLineThreshold;
}
```

#### 4.3 Anpassung der Shift-Methoden

Jede betroffene Shift-Methode erhält eine zusätzliche Skip-Prüfung im Token-Iterations-Loop. Der `nonShiftedLineThreshold` ist dabei die Zeile, die nicht verschoben wird (typischerweise `expressionStartLine` oder `firstLine`).

**`ShiftExpressionLambdaBody`** (Zeilen 876–907):
```csharp
foreach (var token in expressionBody.DescendantTokens())
{
    if (IsFirstTokenOnLine(token) == false || GetLine(token) <= expressionStartLine)
    {
        continue;
    }

    // NEU: Skip tokens, die relativ zu einem nicht-verschobenen Anker positioniert wurden
    if (ShouldSkipContinuationToken(token, expressionStartLine))
    {
        continue;
    }

    // ... bestehende Shift-Logik
}
```

**Analog für die weiteren Shift-Methoden:**

- **`ShiftLambdaBlockBodies`** (Block-Lambda-Teil): Der Block-Lambda-Shift iteriert über `block.DescendantTokens()` und überspringt Tokens basierend auf `firstLine`. → `ShouldSkipContinuationToken(token, firstLine)` einfügen, wobei `firstLine` die `expressionStartLine` ist (Zeile des blockOpenBrace/Lambda-Arrows).

- **`ShiftArmContinuationLines`**: Überspringt bereits Chain-Dots. → Zusätzlich `ShouldSkipContinuationToken(token, firstLine)` einfügen.

- **`ShiftNodeContinuationLines<T>`**: Generischer Shift. → `ShouldSkipContinuationToken(token, firstLine)` einfügen.

- **`ShiftArgumentByColumn`**: Verschiebt ALLE first-on-line Tokens. → `ShouldSkipContinuationToken(token, -1)` einfügen (da es keinen nonShiftedLineThreshold gibt: hier müssten alle Continuation-Tokens verschoben werden, weil ALLES verschoben wird). **Achtung**: Bei `ShiftArgumentByColumn` ist der Shift uniform (alle Zeilen werden verschoben), daher sollten Continuation-Tokens NICHT übersprungen werden.

- **`BuildInvocationContinuationShiftPairs`**: Ähnlich wie `ShiftArmContinuationLines`. → `ShouldSkipContinuationToken(token, originalLinkLine)` einfügen.

### 4.4 Entscheidungsmatrix: Wann skippen, wann nicht?

| Shift-Methode | Shift-Art | Skip-Verhalten |
|---|---|---|
| `ShiftExpressionLambdaBody` | Nicht-uniform (1. Zeile bleibt) | Skippen wenn Anker auf 1. Zeile |
| `ShiftLambdaBlockBodies` (Block) | Nicht-uniform | Skippen wenn Anker auf 1. Zeile |
| `ShiftArmContinuationLines` | Nicht-uniform (1. Zeile bleibt) | Skippen wenn Anker auf 1. Zeile |
| `ShiftNodeContinuationLines` | Nicht-uniform (1. Zeile bleibt) | Skippen wenn Anker auf 1. Zeile |
| `ShiftArgumentByColumn` | **Uniform** (alle Zeilen) | **NICHT skippen** (relative Position bleibt erhalten) |
| `BuildInvocationContinuationShiftPairs` | Nicht-uniform (Link-Zeile bleibt) | Skippen wenn Anker auf Link-Zeile |

**Regel**: Continuation-Tokens nur skippen bei **nicht-uniformen** Shifts, und nur wenn der Anker auf der nicht-verschobenen Zeile liegt.

---

## 5. Warum diese Lösung nachhaltig ist

### 5.1 Funktioniert für beliebige Verschachtelungstiefe

Die Lösung ist **anker-basiert**, nicht verschachtelungstiefe-basiert. Egal wie tief verschachtelt ein logischer Ausdruck vorkommt:
- Sein `&&`-Operator hat immer einen Anker (`Left.GetFirstToken()`)
- Die Frage ist immer nur: Wird der Anker verschoben oder nicht?
- Das ist eine lokale Eigenschaft, die unabhängig von der Verschachtelungstiefe bestimmbar ist

### 5.2 Kompatibel mit der CSharpSyntaxRewriter-Architektur

Die Lösung arbeitet MIT dem Depth-First-Muster, nicht dagegen:
- Innere Visitors positionieren Tokens weiterhin korrekt (keine Änderung an `VisitBinaryExpression`)
- Äußere Visitors respektieren die bereits korrekte Positionierung
- Kein zweiter Pass nötig, kein Token-Tracking, keine globalen State-Änderungen

### 5.3 Erweiterbar für neue Continuation-Token-Typen

Sollten neue Continuation-Token-Typen hinzukommen (z.B. neue Pattern-Typen in zukünftigen C#-Versionen), müssen nur zwei Stellen erweitert werden:
1. `IsContinuationToken` (bereits bestehend)
2. `TryGetContinuationAnchorLine` (neu)

### 5.4 Minimal-invasive Änderung

- Keine Änderung der Visitor-Reihenfolge
- Keine Änderung der Pipeline-Architektur
- Keine neuen Felder/State
- Änderungen beschränkt auf: 2 neue Helper-Methoden + jeweils 4 zusätzliche Zeilen in 5 Shift-Methoden

---

## 6. Risiken und Mitigation

### 6.1 Anker-Bestimmung für komplexe Szenarien

Für verschachtelte Binary-Expressions wie `a && b || c`:
- Baum: `(a && b) || c`
- `||` hat Anker `a` (= `Left.GetFirstToken()` von `(a && b)`)
- `&&` hat Anker `a` (= `Left.GetFirstToken()` von `a`)

Beide haben denselben Anker → konsistentes Verhalten. ✓

### 6.2 Anker auf Continuation-Zeile

```csharp
lambda => firstLine
          .Method(x => innerExpr
                       && innerRight)
```

Hier:
- `innerExpr` ist auf einer Continuation-Zeile der äußeren Lambda
- `&&` hat Anker `innerExpr`
- Anker-Zeile > expressionStartLine → `&&` wird NICHT geskippt → wird verschoben wie `innerExpr` → ✓

### 6.3 Chain-Dots

Chain-Dots werden von `AlignChain` separat verwaltet. `ShiftArmContinuationLines` schließt Chain-Dots bereits über `CollectFluentChainDotSpanStarts` aus. Die neue Logik ergänzt dies für logische Operatoren. Kein Konflikt. ✓

### 6.4 Idempotenz

Die bestehende Test-Infrastruktur (`AssertRuleResult`) prüft automatisch, dass ein zweiter Durchlauf das gleiche Ergebnis liefert. Da die Lösung nur das Überspringen von bereits korrekt positionierten Tokens implementiert, bleibt die Idempotenz gewahrt. ✓

---

## 7. Implementierungsplan

### Phase 1: Helper-Methoden
1. `TryGetContinuationAnchorLine` implementieren
2. `ShouldSkipContinuationToken` implementieren

### Phase 2: Shift-Methoden anpassen
3. `ShiftExpressionLambdaBody` — Skip-Check einfügen
4. `ShiftLambdaBlockBodies` (Block-Lambda-Pfad) — Skip-Check einfügen
5. `ShiftArmContinuationLines` — Skip-Check einfügen
6. `ShiftNodeContinuationLines` — Skip-Check einfügen
7. `BuildInvocationContinuationShiftPairs` — Skip-Check einfügen

### Phase 3: Tests
8. Bestehende Tests ausführen und Stabilität verifizieren
9. Neuen Test für das Ursprungsszenario hinzufügen (Expression-Lambda mit `&&` in Argument-Liste innerhalb Initializer-Zuweisung in Array-Initializer)
10. Weitere Verschachtelungsszenarien als Tests ergänzen:
    - `&&` in Expression-Lambda in Collection-Expression-Element
    - `||` in Expression-Lambda in Switch-Expression-Arm
    - Verschachtelte Expression-Lambdas mit logischen Operatoren auf verschiedenen Ebenen
    - `or`/`and`-Pattern in verschachteltem Kontext

### Phase 4: Regression
11. Alle bestehenden Formatter-Tests ausführen
12. Idempotenz-Tests prüfen

---

## 8. Betroffene Dateien

| Datei | Änderung |
|-------|----------|
| `Reihitsu.Formatter/Rules/Indentation/IndentationAndAlignmentRule.cs` | Neue Helper + Anpassung der Shift-Methoden |
| `Reihitsu.Formatter.Test/Unit/Rules/Indentation/LogicalExpressionAlignmentTests.cs` | Neue Testfälle |
| `Reihitsu.Formatter.Test/Integration/Rules/Indentation/LogicalExpressionAlignmentIntegrationTests.cs` | Neue Integrationstests |
