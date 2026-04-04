internal class MethodChainAlignmentTestData
{
    // --- Multi-line method chain with misaligned dots ---

    public void MultiLineChainMisaligned()
    {
        var result = new System.Collections.Generic.List<int> { 1, 2, 3 }.Where(x => x > 0)
                                                                         .Select(x => x * 2)
                                                                         .ToList();
    }

    // --- Single-line chain (should stay unchanged) ---

    public void SingleLineChain()
    {
        var result = new System.Collections.Generic.List<int> { 1, 2, 3 }.Where(x => x > 0).ToList();
    }

    // --- Chain with conditional access (?.) ---

    public string ConditionalAccessChain(string input)
    {
        var result = input?.Trim()
                          .ToUpper();

        return result;
    }

    // --- Short chain with only one link (should stay unchanged) ---

    public void ShortChain()
    {
        var result = "hello".ToUpper();
    }

    // --- Multi-line chain starting at various indentation levels ---

    public void ChainWithIndentation()
    {
        var result = System.Linq.Enumerable.Range(0, 10)
                                           .Where(x => x > 2)
                                           .Select(x => x.ToString())
                                           .ToList();
    }
}