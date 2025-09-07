internal class RH0321
{
    public System.Collections.Generic.IEnumerable<int> YieldReturn()
    {
        yield return 1;
        {|#0:yield|} return 1;

        yield return 1;
        // Test
        yield return 1;
        /* Test */
        yield return 1;
    }
}