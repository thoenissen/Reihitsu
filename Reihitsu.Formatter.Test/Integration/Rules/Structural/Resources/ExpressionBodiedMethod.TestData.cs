internal class ExpressionBodiedMethodTestData
{
    public int GetValue() => 42;

    public void DoWork() => System.Console.WriteLine("hello");

    public string GetName() => "test";

    // Already block body — should not change
    public int GetOther()
    {
        return 1;
    }
}
