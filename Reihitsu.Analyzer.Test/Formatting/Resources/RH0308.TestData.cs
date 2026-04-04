internal class RH0308
{
    private System.Collections.Generic.IAsyncEnumerable<int> _enumerable;
    
    public async void Test()
    {
        foreach (var item in new int[0])
        {
        }
        {|#0:foreach|} (var item in new int[0])
        {
        }

        foreach (var item in new int[0])
        {
        }
        // Test
        foreach (var item in new int[0])
        {
        }
        /* Test */
        foreach (var item in new int[0])
        {
        }

        await foreach (var item in _enumerable)
        {
        }
    }
}