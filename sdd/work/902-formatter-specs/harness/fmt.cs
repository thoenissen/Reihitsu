#:project /home/user/Reihitsu/src/Reihitsu.Formatter/Reihitsu.Formatter.csproj
#:property TargetFramework=net10.0
#:property NoWarn=RH0002;CS1591
#:property TreatWarningsAsErrors=false
#:property EnableNETAnalyzers=false
#:property RunAnalyzers=false

using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Reihitsu.Formatter;

// usage: fmt <dir-or-file>...  Formats every *.cs (not *.out.cs) and writes <name>.out.cs plus a report to stdout.
var asm = typeof(ReihitsuFormatter).Assembly;
var pipeline = asm.GetType("Reihitsu.Formatter.Pipeline.FormattingPipeline")!;
var execute = pipeline.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static)
                      .Single(m => m.Name == "Execute" && m.GetParameters().Length == 4);
var ctxType = asm.GetType("Reihitsu.Formatter.Data.FormattingContext")!;
var ctor = ctxType.GetConstructors().OrderByDescending(c => c.GetParameters().Length).First();

var files = args.SelectMany(a => Directory.Exists(a) ? Directory.GetFiles(a, "*.cs").OrderBy(x => x).ToArray() : new[] { a })
                .Where(f => !f.EndsWith(".out.cs"));
foreach (var file in files)
{
    var text = File.ReadAllText(file);
    var m = System.Text.RegularExpressions.Regex.Match(Path.GetFileName(file), @"\.lang(\d+)\.");
    var lv = m.Success ? Enum.Parse<LanguageVersion>("CSharp" + m.Groups[1].Value) : LanguageVersion.Preview;
    var tree = CSharpSyntaxTree.ParseText(text, new CSharpParseOptions(lv));
    string formatted;
    if (file.Contains(".node."))
    {
        // Node mode: format only the first member of the first type through FormatNodeInDocumentAsync.
        var ws = new AdhocWorkspace();
        var proj = ws.AddProject("p", LanguageNames.CSharp).WithParseOptions(new CSharpParseOptions(lv));
        var doc = proj.AddDocument("d.cs", text);
        var droot = doc.GetSyntaxRootAsync().Result!;
        var target = droot.DescendantNodes().OfType<Microsoft.CodeAnalysis.CSharp.Syntax.TypeDeclarationSyntax>().First().Members.First();
        formatted = ReihitsuFormatter.FormatNodeInDocumentAsync(doc, target).Result.GetTextAsync().Result.ToString();
    }
    else
    {
        formatted = ReihitsuFormatter.FormatSyntaxTree(tree).GetText().ToString();
    }
    var second = ReihitsuFormatter.FormatSyntaxTree(CSharpSyntaxTree.ParseText(formatted, new CSharpParseOptions(lv))).GetText().ToString();
    File.WriteAllText(Path.ChangeExtension(file, ".out.cs"), formatted);

    var phases = new List<string>();
    var root = tree.GetRoot();
    if (!tree.GetDiagnostics().Any(d => d.Severity == DiagnosticSeverity.Error))
    {
        var eol = text.Contains("\r\n") ? "\r\n" : "\n";
        var ps = ctor.GetParameters();
        var cargs = ps.Select((p, i) => i == 0 ? (object)eol : p.Name == "languageVersion" ? (lv == LanguageVersion.Preview ? LanguageVersion.CSharp14 : lv) : p.HasDefaultValue ? p.DefaultValue : (p.ParameterType.IsValueType ? Activator.CreateInstance(p.ParameterType) : null)).ToArray();
        var ctx = ctor.Invoke(cargs);
        Action<string, SyntaxNode, SyntaxNode> obs = (n, i, o) => { if (i.ToFullString() != o.ToFullString()) phases.Add(n.Replace("Phase", "")); };
        execute.Invoke(null, new object?[] { root, ctx, obs, CancellationToken.None });
    }
    var status = formatted == text ? "UNCHANGED" : "CHANGED";
    var idem = formatted == second ? "idempotent" : "NOT-IDEMPOTENT";
    Console.WriteLine($"=== {Path.GetFileName(file)}: {status}, {idem}; phases: {string.Join(", ", phases)}");
    if (formatted != text) { Console.WriteLine(formatted.Replace("\r\n", "␍\n").Replace("\t", "→")); Console.WriteLine(formatted.EndsWith("\n") ? "[ends with newline]" : "[no final newline]"); }
    if (formatted != second) { Console.WriteLine("--- second pass:"); Console.WriteLine(second); }
}
