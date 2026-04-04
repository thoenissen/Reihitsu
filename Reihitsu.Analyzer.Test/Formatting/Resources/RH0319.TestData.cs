internal class RH0319
{
    public unsafe RH0319()
    {
        fixed (byte* ptr = stackalloc byte[10])
        {
        }
        {|#0:fixed|} (byte* ptr = stackalloc byte[10])
        {
        }

        fixed (byte* ptr = stackalloc byte[10])
        {
        }
        // Test
        fixed (byte* ptr = stackalloc byte[10])
        {
        }
        /* Test */
        fixed (byte* ptr = stackalloc byte[10])
        {
        }
    }
}