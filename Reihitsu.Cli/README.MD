# Reihitsu.Cli

A command-line formatting tool for C# source files, powered by the Reihitsu formatter engine.

## Installation

```shell
dotnet tool install -g Reihitsu.Cli
```

## Usage

```shell
reihitsu-format [options] [<paths>...]
```

### Arguments

| Argument   | Description                                                              |
|------------|--------------------------------------------------------------------------|
| `<paths>`  | One or more files or directories to format. Defaults to the current directory if omitted. |

### Options

| Option      | Description                                                        |
|-------------|--------------------------------------------------------------------|
| `--check`   | Check if files are formatted correctly. Does not write changes. Returns exit code `1` if formatting is needed. |
| `--dry-run` | Show what would change without applying any modifications.         |
| `--verbose` | Show detailed output for each file.                                |
| `--help`    | Show help message.                                                 |

### Exit Codes

| Code | Meaning                                        |
|------|------------------------------------------------|
| `0`  | Success — all files are formatted correctly.   |
| `1`  | Formatting needed (when using `--check`).      |
| `2`  | Error occurred.                                |

## Examples

Format all C# files in the current directory:

```shell
reihitsu-format
```

Format a specific file:

```shell
reihitsu-format src/MyClass.cs
```

Check if files are properly formatted (useful for CI):

```shell
reihitsu-format --check src/
```

Preview changes without applying them:

```shell
reihitsu-format --dry-run src/
```

Show detailed output:

```shell
reihitsu-format --verbose src/