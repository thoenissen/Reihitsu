# The `--dry-run` diff header names the file relative to the current directory

| | |
|---|---|
| Status | Draft |
| Date | 2026-11 |
| Affected requirements | CLI-013, CLI-024 (new) |

## What

The `---` and `+++` lines of the diff that `reihitsu-format --dry-run` prints (CLI-013) name a file inside the current directory by its path relative to that directory, written with `/` as the directory separator on every platform, after the `a/` and `b/` prefixes. A file outside the current directory is named by its full path, without the prefixes (CLI-024). The `Would format:` line, and every other line that names a file, keeps the full path. The example of CLI-013, which showed today's header `--- a//work/src/A.cs`, shows the new header.

## Why

Today the header is `a/` or `b/` followed by the file's full path: `--- a//work/src/A.cs` on Linux, `--- a/C:\work\src\A.cs` on Windows. The `a/` and `b/` prefixes are the git convention for a path relative to the root of the tree a patch applies to; `git apply` and `patch -p1` strip the first segment and look for the rest below the directory they run in, so a full path behind the prefix names no file they can find. The maintainer decided in thoenissen/Reihitsu#933 (`reihitsu-format.md` Q1) that the header is a bug to be fixed and how it reads; thoenissen/Reihitsu#975 carries the decision. Because the example of CLI-013 pins today's header, bringing the header in line with the decision changes a specified output and needs this change doc.

## Visible output changes

In every case, the run's other lines — `Would format:`, the hunks, the summary — and the exit code are unchanged.

### 1. A file inside the current directory

Run in `/work`, with `src/A.cs` unformatted:

```shell
reihitsu-format --dry-run src/A.cs
```

Before:

```text
Would format: /work/src/A.cs
--- a//work/src/A.cs
+++ b//work/src/A.cs
@@ -1,4 +1,4 @@
```

After:

```text
Would format: /work/src/A.cs
--- a/src/A.cs
+++ b/src/A.cs
@@ -1,4 +1,4 @@
```

The header is the same when the file is given by its full path (`reihitsu-format --dry-run /work/src/A.cs`) or through a directory (`reihitsu-format --dry-run src/`): it depends on where the file is, not on how it was given.

### 2. A file inside the current directory, on Windows

Run in `C:\work`, with `src\A.cs` unformatted:

```shell
reihitsu-format --dry-run src\A.cs
```

Before:

```text
Would format: C:\work\src\A.cs
--- a/C:\work\src\A.cs
+++ b/C:\work\src\A.cs
```

After:

```text
Would format: C:\work\src\A.cs
--- a/src/A.cs
+++ b/src/A.cs
```

### 3. A file outside the current directory

Run in `/work/src`, with `/work/other/B.cs` unformatted:

```shell
reihitsu-format --dry-run ../other/B.cs
```

Before:

```text
Would format: /work/other/B.cs
--- a//work/other/B.cs
+++ b//work/other/B.cs
```

After:

```text
Would format: /work/other/B.cs
--- /work/other/B.cs
+++ /work/other/B.cs
```

### 4. A file outside the current directory, on Windows

Run in `C:\work`, with `D:\other\B.cs` unformatted:

```shell
reihitsu-format --dry-run D:\other\B.cs
```

Before:

```text
Would format: D:\other\B.cs
--- a/D:\other\B.cs
+++ b/D:\other\B.cs
```

After, as the full path is written in the `Would format:` line:

```text
Would format: D:\other\B.cs
--- D:\other\B.cs
+++ D:\other\B.cs
```

The maintainer's decision does not say whether a full path in the header keeps the platform's separator or is written with `/`; this case is open until the maintainer answers it.

## Decisions

### The header path follows the maintainer's decision in thoenissen/Reihitsu#933

Maintainer's answer (`reihitsu-format.md` Q1): the header names the file relative to the current directory, with `/` on every platform, behind `a/` and `b/`; a file outside the current directory is named by its full path without the prefixes; the `Would format:` line is unchanged. This covers visible output changes 1 to 3.

### Inside means below the current directory, by path

A file is inside the current directory when that directory is one of the directories its full path passes through. The decision is made on the full path the CLI selected (CLI-006), with the platform's path case sensitivity and without resolving symbolic links, so the header agrees with the `Would format:` line and with how the CLI already compares paths. A name that only starts with the directory's name, such as `/work-old/A.cs` for `/work`, is not inside it.

### The header gets a requirement of its own

The file name in the header is a decision with its own reason, separate from what CLI-013 decides (that `--dry-run` writes nothing and prints a diff); it is CLI-024, and CLI-013 only refers to it.

## Alternatives considered

- **A relative path with `..` segments for a file outside the current directory** (`--- a/../other/B.cs`) — rejected: the maintainer decided on the full path, and `git apply` refuses a patch whose path leaves the directory it runs in, so the prefix would promise a use the header cannot serve.
- **The path relative to the path given on the command line** — rejected: the same file would get different headers depending on how it was named, and a path given as a full path would keep today's broken header.
- **The path relative to the root of the git repository that holds the file** — rejected: the CLI knows nothing about git, a file need not be in a repository, and `patch -p1` resolves the path against the directory it runs in, not against a repository root.
