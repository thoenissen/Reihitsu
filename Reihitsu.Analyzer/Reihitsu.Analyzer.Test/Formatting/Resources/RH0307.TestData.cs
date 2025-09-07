internal class RH0307
{
    public RH0307()
    {
        using (var resource = new System.IO.MemoryStream())
        {
        }
        {|#0:using|} (var resource = new System.IO.MemoryStream())
        {
        }

        using (var resource = new System.IO.MemoryStream())
        {
        }
        // Test
        using (var resource = new System.IO.MemoryStream())
        {
        }
        /* Test */
        using (var resource = new System.IO.MemoryStream())
        {
        }
    }
}