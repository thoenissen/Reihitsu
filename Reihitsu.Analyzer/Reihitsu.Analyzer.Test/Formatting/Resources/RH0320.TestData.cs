internal class RH0320
{
    private static object _lock = new object();
    
    public RH0320()
    {
        lock (_lock)
        {
        }
        {|#0:lock|} (_lock)
        {
        }

        lock (_lock)
        {
        }
        // Test
        lock (_lock)
        {
        }
        /* Test */
        lock (_lock)
        {
        }
    }
}