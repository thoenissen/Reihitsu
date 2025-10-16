internal class RH0307
{
    public async void Test()
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
        
        await using (var resource = new System.IO.MemoryStream())
        {
        }
    }
}