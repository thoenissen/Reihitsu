#:property TargetFramework=net11.0
#:property AssemblyName=Reihitsu.Benchmark
#:property PublishAot=false
#:property Nullable=enable

// Fixed-corpus benchmark of released reihitsu-format tool versions.
//
//   dotnet run scripts/benchmark/benchmark.cs -- [options]
//
// The corpus is a pinned commit's paths, extracted with `git archive`, so every run of
// every version formats the same bytes. Each timed repetition formats a fresh copy of the
// corpus in write mode, with --verbose, one file at a time. A tool that offers
// --max-parallelism is given `--max-parallelism 1`; released versions without the option
// already process files one after another. The time of one file is the gap between the
// tool's verbose line for that file and the line before it; the first file's gap starts
// at process start and therefore includes startup and the first JIT compilation.
//
// Versions run interleaved (A, B, A, B, ...) after one untimed warm-up run each, so a
// drift of the machine over time does not land on one version only. Each version runs on the
// runtime it targets, installed next to the SDK when it is missing; --roll-forward runs every
// version on the newest installed runtime instead, which separates the code from the runtime.

using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

return await Benchmark.RunAsync(args);

/// <summary>
/// The benchmark's options, as parsed from the command line
/// </summary>
internal sealed class Options
{
    public List<string> Versions { get; } = new();

    public string Commit { get; set; } = Benchmark.DefaultCommit;

    public List<string> Paths { get; } = new();

    public int Repetitions { get; set; } = 3;

    public int Warmups { get; set; } = 1;

    public bool Perturb { get; set; } = true;

    public bool RollForward { get; set; }

    public bool AllowInstall { get; set; } = true;

    public int Top { get; set; } = 15;

    public string? WorkDirectory { get; set; }
}

/// <summary>
/// One timed run of one tool version
/// </summary>
internal sealed record RunResult(string Version, int Repetition, double TotalMilliseconds, List<(string File, string Status, double Milliseconds)> Files);

/// <summary>
/// Fixed-corpus benchmark runner
/// </summary>
internal static class Benchmark
{
    /// <summary>
    /// The commit tagged v1.2.0: the source of the default corpus
    /// </summary>
    public const string DefaultCommit = "e56624d45ea91431552c960665c181fe04545213";

    private static readonly string[] DefaultPaths = ["src/Reihitsu.Formatter", "src/Reihitsu.Analyzer"];

    private static readonly string[] DefaultVersions = ["1.2.0", "1.3.0"];

    private static readonly Regex FileLine = new(@"^(?<status>[A-Z][A-Za-z ()-]*): (?<path>.+\.cs)$", RegexOptions.CultureInvariant);

    public static async Task<int> RunAsync(string[] args)
    {
        Options options;

        try
        {
            options = Parse(args);
        }
        catch (ArgumentException ex)
        {
            Console.Error.WriteLine($"benchmark: {ex.Message}");

            return 2;
        }

        try
        {
            return await ExecuteAsync(options);
        }
        catch (BenchmarkException ex)
        {
            Console.Error.WriteLine($"benchmark: {ex.Message}");

            return 2;
        }
    }

    private static Options Parse(string[] args)
    {
        var options = new Options();

        for (var index = 0; index < args.Length; index++)
        {
            var argument = args[index];

            string Next()
            {
                if (index + 1 >= args.Length)
                {
                    throw new ArgumentException($"{argument} needs a value.");
                }

                return args[++index];
            }

            int NextCount(int minimum)
            {
                var value = Next();

                if (int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var count) == false || count < minimum)
                {
                    throw new ArgumentException($"{argument} needs a whole number of at least {minimum}, not '{value}'.");
                }

                return count;
            }

            switch (argument)
            {
                case "--version":
                    options.Versions.Add(Next());
                    break;
                case "--commit":
                    options.Commit = Next();
                    break;
                case "--path":
                    options.Paths.Add(Next());
                    break;
                case "--repetitions":
                    options.Repetitions = NextCount(1);
                    break;
                case "--warmups":
                    options.Warmups = NextCount(0);
                    break;
                case "--top":
                    options.Top = NextCount(1);
                    break;
                case "--work":
                    options.WorkDirectory = Path.GetFullPath(Next());
                    break;
                case "--no-perturb":
                    options.Perturb = false;
                    break;
                case "--roll-forward":
                    options.RollForward = true;
                    break;
                case "--no-install":
                    options.AllowInstall = false;
                    break;
                default:
                    throw new ArgumentException($"unknown argument '{argument}'.");
            }
        }

        if (options.Versions.Count == 0)
        {
            options.Versions.AddRange(DefaultVersions);
        }

        if (options.Paths.Count == 0)
        {
            options.Paths.AddRange(DefaultPaths);
        }

        return options;
    }

    private static async Task<int> ExecuteAsync(Options options)
    {
        var repositoryRoot = Environment.GetEnvironmentVariable("REIHITSU_REPO_ROOT") ?? (await RunAsync("git", ["rev-parse", "--show-toplevel"], null)).Trim();
        var work = options.WorkDirectory ?? Path.Combine(Path.GetTempPath(), $"reihitsu-benchmark-{Environment.ProcessId}");

        Directory.CreateDirectory(work);

        var commit = await ResolveCommitAsync(repositoryRoot, options.Commit);
        var corpus = Path.Combine(work, "corpus");

        await ExtractCorpusAsync(repositoryRoot, commit, options.Paths, corpus, options.Perturb);

        var fileCount = Directory.EnumerateFiles(corpus, "*.cs", SearchOption.AllDirectories).Count();
        var tools = new Dictionary<string, ToolInstallation>();

        foreach (var version in options.Versions)
        {
            tools[version] = await InstallToolAsync(work, version, options);
        }

        Console.WriteLine($"Corpus: {string.Join(", ", options.Paths)} at {commit}, {fileCount} C# file(s), {(options.Perturb ? "every 'private ' turned into 'private  '" : "unchanged")}");
        Console.WriteLine($"Machine: {Environment.ProcessorCount} logical processor(s), {System.Runtime.InteropServices.RuntimeInformation.OSDescription}");

        foreach (var (version, tool) in tools)
        {
            Console.WriteLine($"Tool {version}: runtime {tool.Runtime}{(tool.RollForward ? " (rolled forward)" : string.Empty)}, arguments: {string.Join(' ', tool.Arguments)}");
        }

        var results = new List<RunResult>();

        for (var repetition = 1 - options.Warmups; repetition <= options.Repetitions; repetition++)
        {
            foreach (var version in options.Versions)
            {
                var run = await TimeRunAsync(tools[version], version, repetition, corpus, work);
                var label = repetition < 1 ? "warm-up" : $"repetition {repetition}";

                Console.WriteLine($"  {version} {label}: {run.TotalMilliseconds / 1000:F2} s, {run.Files.Count} file line(s)");

                if (repetition >= 1)
                {
                    results.Add(run);
                }
            }
        }

        var rawPath = Path.Combine(work, "per-file.csv");

        await WriteRawAsync(rawPath, results, corpus);
        Summarize(options, results, corpus);

        Console.WriteLine();
        Console.WriteLine($"Raw per-file timings: {rawPath}");

        return 0;
    }

    private static async Task<string> ResolveCommitAsync(string repositoryRoot, string commit)
    {
        try
        {
            return (await RunAsync("git", ["-C", repositoryRoot, "rev-parse", "--verify", $"{commit}^{{commit}}"], null)).Trim();
        }
        catch (BenchmarkException)
        {
            // A shallow clone lacks the pinned commit: fetch just that commit
            await RunAsync("git", ["-C", repositoryRoot, "fetch", "--quiet", "--depth", "1", "origin", commit], null);

            return (await RunAsync("git", ["-C", repositoryRoot, "rev-parse", "--verify", "FETCH_HEAD^{commit}"], null)).Trim();
        }
    }

    private static async Task ExtractCorpusAsync(string repositoryRoot, string commit, List<string> paths, string corpus, bool perturb)
    {
        if (Directory.Exists(corpus))
        {
            Directory.Delete(corpus, true);
        }

        Directory.CreateDirectory(corpus);

        var archive = Path.Combine(Path.GetDirectoryName(corpus)!, "corpus.tar");

        await RunAsync("git", ["-C", repositoryRoot, "archive", "--format=tar", "-o", archive, commit, "--", .. paths], null);
        await RunAsync("tar", ["-xf", archive, "-C", corpus], null);

        File.Delete(archive);

        foreach (var file in Directory.EnumerateFiles(corpus, "*", SearchOption.AllDirectories))
        {
            if (file.EndsWith(".cs", StringComparison.Ordinal) == false)
            {
                File.Delete(file);
            }
            else if (perturb)
            {
                var bytes = await File.ReadAllBytesAsync(file);
                var text = Encoding.UTF8.GetString(bytes);
                var perturbed = text.Replace("private ", "private  ", StringComparison.Ordinal);

                if (ReferenceEquals(text, perturbed) == false)
                {
                    var hasBom = bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF;
                    var content = hasBom ? perturbed.TrimStart('﻿') : perturbed;

                    await File.WriteAllTextAsync(file, content, new UTF8Encoding(hasBom));
                }
            }
        }
    }

    private sealed record ToolInstallation(string Executable, string Runtime, bool RollForward, string? DotnetRoot, string[] Arguments);

    private static async Task<ToolInstallation> InstallToolAsync(string work, string version, Options options)
    {
        var toolPath = Path.Combine(work, "tools", version);
        var executable = Path.Combine(toolPath, OperatingSystem.IsWindows() ? "reihitsu-format.exe" : "reihitsu-format");

        if (File.Exists(executable) == false)
        {
            await RunAsync("dotnet", ["tool", "install", "Reihitsu.Cli", "--version", version, "--tool-path", toolPath], null);
        }

        var runtimeConfig = Directory.EnumerateFiles(toolPath, "Reihitsu.Cli.runtimeconfig.json", SearchOption.AllDirectories).FirstOrDefault()
                            ?? throw new BenchmarkException($"Reihitsu.Cli {version} has no runtime configuration below {toolPath}.");

        using var document = JsonDocument.Parse(await File.ReadAllTextAsync(runtimeConfig));
        var runtime = document.RootElement.GetProperty("runtimeOptions").GetProperty("framework").GetProperty("version").GetString()!;
        var major = runtime.Split('.')[0];
        var runtimes = await RunAsync("dotnet", ["--list-runtimes"], null);
        var hasRuntime = runtimes.Split('\n').Any(line => line.StartsWith($"Microsoft.NETCore.App {major}.", StringComparison.Ordinal));
        var rollForward = false;
        string? dotnetRoot = null;

        if (options.RollForward)
        {
            rollForward = true;
        }
        else if (hasRuntime == false)
        {
            if (options.AllowInstall)
            {
                dotnetRoot = await InstallRuntimeAsync(major);
            }
            else
            {
                throw new BenchmarkException($"Reihitsu.Cli {version} needs the .NET {major} runtime, which is not installed and --no-install was requested; pass --roll-forward to run every version on the newest installed runtime.");
            }
        }

        var help = await RunToolAsync(executable, ["--help"], rollForward, dotnetRoot);
        var arguments = new List<string> { "--force", "--verbose" };

        if (help.Contains("--max-parallelism", StringComparison.Ordinal))
        {
            arguments.AddRange(["--max-parallelism", "1"]);
        }

        return new ToolInstallation(executable, rollForward ? $"{runtime} on {runtimes.Split('\n').Last(line => line.StartsWith("Microsoft.NETCore.App ", StringComparison.Ordinal)).Split(' ')[1]}" : runtime, rollForward, dotnetRoot, arguments.ToArray());
    }

    private static async Task<string> InstallRuntimeAsync(string major)
    {
        var root = Environment.GetEnvironmentVariable("REIHITSU_DOTNET_ROOT") ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".dotnet");
        var installer = Path.Combine(Path.GetTempPath(), $"dotnet-install-{Environment.ProcessId}.sh");

        Console.WriteLine($"Installing the .NET {major} runtime into {root}");

        try
        {
            await RunAsync("curl", ["-sSL", "--proto", "=https", "--proto-redir", "=https", "https://dot.net/v1/dotnet-install.sh", "-o", installer], null);
            await RunAsync("bash", [installer, "--runtime", "dotnet", "--channel", $"{major}.0", "--install-dir", root], null);
        }
        finally
        {
            File.Delete(installer);
        }

        return root;
    }

    private static async Task<RunResult> TimeRunAsync(ToolInstallation tool, string version, int repetition, string corpus, string work)
    {
        var copy = Path.Combine(work, "run");

        if (Directory.Exists(copy))
        {
            Directory.Delete(copy, true);
        }

        CopyDirectory(corpus, copy);

        var start = new ProcessStartInfo(tool.Executable)
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            RedirectStandardInput = true,
            UseShellExecute = false,
            WorkingDirectory = copy,
        };

        foreach (var argument in tool.Arguments)
        {
            start.ArgumentList.Add(argument);
        }

        start.ArgumentList.Add("--");
        start.ArgumentList.Add(copy);
        Configure(start, tool.RollForward, tool.DotnetRoot);

        var stamps = new List<(long Timestamp, string Line)>();
        var gate = new object();

        void Record(string? line)
        {
            var timestamp = Stopwatch.GetTimestamp();

            if (line != null)
            {
                lock (gate)
                {
                    stamps.Add((timestamp, line));
                }
            }
        }

        using var process = new Process { StartInfo = start };

        process.OutputDataReceived += (_, e) => Record(e.Data);
        process.ErrorDataReceived += (_, e) => Record(e.Data);

        var begin = Stopwatch.GetTimestamp();

        process.Start();
        process.StandardInput.Close();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        await process.WaitForExitAsync();

        var end = Stopwatch.GetTimestamp();

        if (process.ExitCode is not (0 or 1))
        {
            throw new BenchmarkException($"Reihitsu.Cli {version} exited with {process.ExitCode}:{Environment.NewLine}{string.Join(Environment.NewLine, stamps.TakeLast(10).Select(stamp => stamp.Line))}");
        }

        var files = new List<(string File, string Status, double Milliseconds)>();
        var previous = begin;

        foreach (var (timestamp, line) in stamps.OrderBy(stamp => stamp.Timestamp))
        {
            var match = FileLine.Match(line);

            if (match.Success == false || match.Groups["path"].Value.StartsWith(copy, StringComparison.Ordinal) == false)
            {
                continue;
            }

            var relative = Path.GetRelativePath(copy, match.Groups["path"].Value).Replace('\\', '/');

            files.Add((relative, match.Groups["status"].Value, Stopwatch.GetElapsedTime(previous, timestamp).TotalMilliseconds));
            previous = timestamp;
        }

        return new RunResult(version, repetition, Stopwatch.GetElapsedTime(begin, end).TotalMilliseconds, files);
    }

    private static void Summarize(Options options, List<RunResult> results, string corpus)
    {
        Console.WriteLine();
        Console.WriteLine($"Totals over {options.Repetitions} repetition(s), write mode, one file at a time (seconds; median [min-max]):");

        var medians = new Dictionary<string, Dictionary<string, double>>();

        foreach (var version in options.Versions)
        {
            var runs = results.Where(run => run.Version == version).ToList();
            var totals = runs.Select(run => run.TotalMilliseconds / 1000).ToList();
            var perFile = runs.SelectMany(run => run.Files.Skip(1))
                              .GroupBy(file => file.File)
                              .ToDictionary(group => group.Key, group => Median(group.Select(file => file.Milliseconds)));
            var first = Median(runs.Where(run => run.Files.Count > 0).Select(run => run.Files[0].Milliseconds));
            var statuses = runs[0].Files.GroupBy(file => file.Status).Select(group => $"{group.Key} {group.Count()}");

            medians[version] = perFile;

            Console.WriteLine($"  {version,-8} total {Median(totals),7:F2} [{totals.Min():F2}-{totals.Max():F2}]  start-up + first file {first / 1000:F2}  sum of per-file medians {perFile.Values.Sum() / 1000:F2}  per-file median {Median(perFile.Values):F1} ms  ({string.Join(", ", statuses)})");
        }

        var baseline = options.Versions[0];

        foreach (var version in options.Versions.Skip(1))
        {
            var shared = medians[baseline].Keys.Intersect(medians[version].Keys).ToList();
            var ratios = shared.Where(file => medians[baseline][file] > 0).Select(file => medians[version][file] / medians[baseline][file]).ToList();

            Console.WriteLine();
            Console.WriteLine($"{version} against {baseline}, per file over {shared.Count} shared file(s): median ratio {Median(ratios):F2}, sum {shared.Sum(file => medians[version][file]) / 1000:F2} s against {shared.Sum(file => medians[baseline][file]) / 1000:F2} s");
        }

        var last = options.Versions[^1];

        Console.WriteLine();
        Console.WriteLine($"Slowest {options.Top} file(s) by {last} median (ms; the first file of a run is left out, it carries start-up):");
        Console.WriteLine($"  {string.Join("  ", options.Versions.Select(version => $"{version,9}"))}  lines  file");

        foreach (var file in medians[last].OrderByDescending(pair => pair.Value).Take(options.Top).Select(pair => pair.Key))
        {
            var lines = File.ReadLines(Path.Combine(corpus, file)).Count();
            var cells = options.Versions.Select(version => medians[version].TryGetValue(file, out var value) ? $"{value,9:F1}" : $"{"-",9}");

            Console.WriteLine($"  {string.Join("  ", cells)}  {lines,5}  {file}");
        }
    }

    private static async Task WriteRawAsync(string path, List<RunResult> results, string corpus)
    {
        var builder = new StringBuilder("version,repetition,index,file,status,milliseconds").AppendLine();

        foreach (var run in results)
        {
            for (var index = 0; index < run.Files.Count; index++)
            {
                var (file, status, milliseconds) = run.Files[index];

                builder.Append(CultureInfo.InvariantCulture, $"{run.Version},{run.Repetition},{index},{file},{status},{milliseconds:F3}").AppendLine();
            }

            builder.Append(CultureInfo.InvariantCulture, $"{run.Version},{run.Repetition},total,,,{run.TotalMilliseconds:F3}").AppendLine();
        }

        await File.WriteAllTextAsync(path, builder.ToString());
    }

    private static double Median(IEnumerable<double> values)
    {
        var sorted = values.OrderBy(value => value).ToList();

        if (sorted.Count == 0)
        {
            return 0;
        }

        return sorted.Count % 2 == 1 ? sorted[sorted.Count / 2] : (sorted[(sorted.Count / 2) - 1] + sorted[sorted.Count / 2]) / 2;
    }

    private static void CopyDirectory(string source, string target)
    {
        foreach (var file in Directory.EnumerateFiles(source, "*", SearchOption.AllDirectories))
        {
            var destination = Path.Combine(target, Path.GetRelativePath(source, file));

            Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
            File.Copy(file, destination);
        }
    }

    private static void Configure(ProcessStartInfo start, bool rollForward, string? dotnetRoot)
    {
        if (rollForward)
        {
            start.Environment["DOTNET_ROLL_FORWARD"] = "LatestMajor";
        }

        if (dotnetRoot != null)
        {
            start.Environment["DOTNET_ROOT"] = dotnetRoot;
        }
    }

    private static Task<string> RunToolAsync(string executable, string[] arguments, bool rollForward, string? dotnetRoot)
    {
        return RunAsync(executable, arguments, start => Configure(start, rollForward, dotnetRoot));
    }

    private static async Task<string> RunAsync(string fileName, string[] arguments, Action<ProcessStartInfo>? configure)
    {
        var start = new ProcessStartInfo(fileName)
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };

        foreach (var argument in arguments)
        {
            start.ArgumentList.Add(argument);
        }

        configure?.Invoke(start);

        using var process = Process.Start(start) ?? throw new BenchmarkException($"could not start {fileName}.");
        var output = process.StandardOutput.ReadToEndAsync();
        var error = process.StandardError.ReadToEndAsync();

        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            throw new BenchmarkException($"{fileName} {string.Join(' ', arguments)} failed with {process.ExitCode}: {(await error).Trim()}");
        }

        return await output;
    }
}

/// <summary>
/// A failure that stops the benchmark with exit code 2
/// </summary>
internal sealed class BenchmarkException(string message) : Exception(message);
