# Casing fixes keep acronyms, an existing prefix, and every identifier character

| | |
|---|---|
| Status | Accepted |
| Date | 2026-11 |
| Affected requirements | SHARED-CASING-001, SHARED-CASING-002, SHARED-CASING-003, SHARED-CASING-006, RH4005-002, RH4121-003 |

## What

The casing fixes derive a new name from the old one (SHARED-CASING-006). This change extends that derivation, and the styles it checks against, in three ways:

- **Leading acronyms in camelCase targets** (thoenissen/Reihitsu#971). For a camelCase or `_camelCase` target, the leading run of uppercase letters of the first word is written in lowercase as one word. When that run has two or more letters and a lowercase letter follows it directly, the run's last letter keeps its case, because it starts the next word: `HTTPClient` becomes `httpClient` and `_httpClient`, not `hTTPClient` and `_hTTPClient`. PascalCase targets are unchanged: `HTTPClient` stays as it is.
- **An existing prefix counts** (thoenissen/Reihitsu#1451). RH4005 and RH4121 treat an `i`/`I` or `t`/`T` that is followed by `_` as the prefix the rule requires, and drop the underscores: `i_document` becomes `IDocument`, `T_Key` becomes `TKey`, `T_1` becomes `T1`. RH4005 also treats an `i`/`I` in front of a digit as the prefix, and an `i` or `I` alone as a prefix with nothing behind it, for which no usable name remains, so the fix is withheld.
- **Every identifier character belongs to its word** (thoenissen/Reihitsu#1451). A combining mark (Unicode categories Mn, Mc), a letter number (Nl), and connector punctuation other than `_` (Pc) are no longer treated as separators that the fix drops. The styles of SHARED-CASING-001 to SHARED-CASING-003 accept them wherever they accept a letter or a digit after the first character, and the fix keeps them in their word. A combining mark counts as part of the character in front of it, so `CAFÉ_NAME`, with `É` written as `E` followed by U+0301, still counts as a word in uppercase letters.

## Why

- `_hTTPClient` is not what anyone writes for a field holding an HTTP client; the conversion lowercased one letter of an acronym because it treated a mixed-case word as a single word. The maintainer decided the conversion as a defect (thoenissen/Reihitsu#928 RH4106 Q1); triage reclassified it as an extension because SHARED-CASING-006 explicitly leaves names that start with two uppercase letters out of its coverage for camelCase targets.
- `IIDocument`, `TTKey`, and `TT1` double a prefix the author already wrote; the maintainer decided that an existing prefix, in either case and with or without `_`, is the prefix (thoenissen/Reihitsu#1010 Q1, thoenissen/Reihitsu#1020 Q1).
- `class Café` and `class ClassⅫ` are valid identifiers, yet they were reported and the fix dropped a character, which changes the name's letters, not their case. The maintainer decided that every character C# allows in an identifier part belongs to the word and that the fix never drops one (thoenissen/Reihitsu#1009 Q3).

## Visible output changes

The "before" outputs were measured with `scripts/apply-fix.sh` on `a8fa3fa`, under LF and CRLF alike. No rule page changes: each page shows the normal case, and none of its examples has an acronym, a prefix with `_`, or one of these characters.

### 1. RH4106 (and RH4107, RH2001) — `_camelCase` target with a leading acronym

```csharp
// input
public class C
{
    private int HTTPClient;
    private int XMLParser;
    private int IOStream;
    private int HTTP2Client;
    private int URLs;

    public int Sum => HTTPClient + XMLParser + IOStream + HTTP2Client + URLs;
}
```

```csharp
// before
public class C
{
    private int _hTTPClient;
    private int _xMLParser;
    private int _iOStream;
    private int _hTTP2Client;
    private int _uRLs;

    public int Sum => _hTTPClient + _xMLParser + _iOStream + _hTTP2Client + _uRLs;
}
```

```csharp
// after
public class C
{
    private int _httpClient;
    private int _xmlParser;
    private int _ioStream;
    private int _http2Client;
    private int _urLs;

    public int Sum => _httpClient + _xmlParser + _ioStream + _http2Client + _urLs;
}
```

Unchanged, because the run is taken from the first word only: `A_B` becomes `_aB`, `A_B_Cd` becomes `_aBCd`, `m_HTTPClient` becomes `_mHTTPClient`, and `X_Y_Z` becomes `_xYZ`, as before; for a camelCase target, `I_O_Stream` stays `iOStream`.

The same holds for RH4107, and for the backing field the RH2001 fix declares: `private int HTTPClient { get; set; }` becomes `private int _httpClient;` instead of `private int _hTTPClient;`.

### 2. RH4105 (and RH4115, RH4117) — camelCase target with a leading acronym

```csharp
// input
class C
{
    int M(int HTTPClient)
    {
        return HTTPClient;
    }
}
```

Before: `int M(int hTTPClient) { return hTTPClient; }`. After: `int M(int httpClient) { return httpClient; }`.

### 3. RH4005 — an interface name with an existing prefix

| Input | Before | After |
|---|---|---|
| `interface i_document` | `IIDocument` | `IDocument` |
| `interface I_reader` | `IIReader` | `IReader` |
| `interface I` | `II` | no fix offered |
| `interface i` | `II` | no fix offered |
| `interface i2Factory` | `II2Factory` | no fix offered |

`I2Factory` is still reported by RH4005-001, because the character after the `I` is not an uppercase letter, so SHARED-CASING-008 withholds the fix. Unchanged: `Item` becomes `IItem`, `index` becomes `IIndex`, `IOrder_Line` becomes `IOrderLine`.

### 4. RH4121 — a type parameter with an existing prefix and `_`

| Input | Before | After |
|---|---|---|
| `class C<T_Key>` | `TTKey` | `TKey` |
| `class C<t_key>` | `TTKey` | `TKey` |
| `class C<T__Key>` | `TTKey` | `TKey` |
| `class C<T_1>` | `TT1` | `T1` |
| `class C<T_>` | `TT` | `T` |

### 5. RH4002 (and every PascalCase, camelCase, and `_camelCase` rule) — a combining mark, a letter number, or connector punctuation

`Café` below is written as `Cafe` followed by U+0301 COMBINING ACUTE ACCENT, and `Ⅻ` is U+216B ROMAN NUMERAL TWELVE.

```csharp
// input
class Café
{
}

class ClassⅫ
{
}

class café_bar
{
}

class order‿line
{
}
```

```csharp
// before: all four are reported
class Cafe
{
}

class Class
{
}

class CafeBar
{
}

class OrderLine
{
}
```

```csharp
// after: Café and ClassⅫ are not reported; the other two are reported
class Café
{
}

class ClassⅫ
{
}

class CaféBar
{
}

class Order‿line
{
}
```

`‿` is U+203F UNDERTIE (category Pc): it belongs to its word, so `order‿line` is one word whose first letter is raised.

### 6. RH4110 (and every PascalCase target) — a word in uppercase letters with a combining mark

```csharp
// input
public class C
{
    private const int CAFÉ_NAME = 1;

    public int Get() => CAFÉ_NAME;
}
```

Before: `CafeName` (U+0301 dropped). After: `CaféName`.

## Decisions

### A combining mark counts as part of the character in front of it

A combining mark has no case of its own and modifies the letter in front of it. Counting it with that letter keeps a word such as `CAFÉ` a word in uppercase letters, and keeps `ÉTATClient` a leading run of uppercase letters, so these names convert exactly like their spelling without the mark. A letter number and connector punctuation other than `_` stand on their own: like a digit, they keep a word from consisting of uppercase letters only.

### The acronym rule applies to camelCase and `_camelCase` targets only

A PascalCase target keeps an acronym as written (`HTTPClient`), because SHARED-CASING-001 accepts it and normalizing it to `HttpClient` would rename a name the rule does not report.

### The acronym run is taken from the first word only

The maintainer chose the first word over the joined name (output batch 1, question 10, answer A). The run is measured on the first word as the word step wrote it, so a word boundary the author marked with `_` is never merged into an acronym: `A_B` stays `_aB` and `m_HTTPClient` stays `_mHTTPClient`, instead of `_ab` and `_mhttpClient`. `XML_Parser` becomes `xmlParser` and `X_YZ` becomes `xYz`, as before, in both readings.

### Maintainer answers to output batch 1

The maintainer chose the recommended option for every question of this change:

- **Question 1 — leading acronym in a camelCase or `_camelCase` target (RH4106, RH4105, RH4107, RH4115, RH4117, RH2001's backing field): A.** `HTTPClient` becomes `_httpClient`, `IOStream` `_ioStream`, `HTTP2Client` `_http2Client`, and the plural acronym `URLs` `_urLs`; PascalCase targets keep `HTTPClient`.
- **Question 2 — RH4005, an existing `I` prefix: A.** `i_document` becomes `IDocument`, `I_reader` `IReader`; no fix is offered for `I`, `i`, and `i2Factory`.
- **Question 3 — RH4121, an existing `T` prefix with `_`: A.** `T_Key` and `t_key` become `TKey`, `T_1` `T1`, and `T_` `T`.
- **Question 4 — combining mark, letter number, and connector punctuation: A.** `Café` and `ClassⅫ` are not reported; `café_bar` becomes `CaféBar`, `order‿line` `Order‿line`.
- **Question 5 — a combining mark in a word in uppercase letters: A.** `CAFÉ_NAME`, with U+0301, becomes `CaféName`.
- **Question 10 — the leading run of the first word, not of the joined name: A.** See the decision above.

### `_` stays the only separator

The maintainer decided that connector punctuation belongs to the word. `_` (U+005F) is connector punctuation too, but it is the separator of `FIRST_NAME` and `m_value`, which SHARED-CASING-006 already splits; only `_` splits a name.

### Prefix with `_` for RH4005 and RH4121

`i`/`I` or `t`/`T` followed by one or more `_` is the prefix, and the underscores are dropped. RH4005 additionally treats `i`/`I` followed by a digit, and `i` or `I` alone, as the prefix: the maintainer listed `i2Factory` → `II2Factory` and `I` → `II` among the doubled prefixes, and no name in RH4005's style can be built from what remains, so the fix is withheld (SHARED-CASING-008). For RH4121, `T` alone is a valid name, so `T_` becomes `T`, as `t` already becomes `T`.

## Alternatives considered

- **Normalize acronyms in PascalCase targets too** (`HTTPClient` → `HttpClient`) — rejected: SHARED-CASING-001 accepts the acronym, and the issue keeps PascalCase targets unchanged.
- **Keep reporting `Café` and withhold the fix** — rejected: the fix would derive the old name again, so the diagnostic would have no outcome; the characters have no case, and the style is about case.
- **Withhold the fix for a name with an existing prefix and `_`, or with one of these characters** — rejected: the decided names are known.
