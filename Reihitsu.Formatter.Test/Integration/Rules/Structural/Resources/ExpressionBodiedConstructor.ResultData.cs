internal class ExpressionBodiedConstructorTestData
{
    private int _value;

    public ExpressionBodiedConstructorTestData()
    {
        _value = 0;
    }

    public ExpressionBodiedConstructorTestData(int value)
    {
        _value = value;
    }

    // Already block body — should not change
    public ExpressionBodiedConstructorTestData(string text)
    {
        _value = text.Length;
    }
}