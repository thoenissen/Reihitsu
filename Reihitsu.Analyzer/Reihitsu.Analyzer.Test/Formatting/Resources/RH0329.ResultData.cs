using System;

internal class RH0329
{
    // Valid: operator on same line
    void ValidSameLine()
    {
        var a = true && false;
        var b = true || false;
        var c = true && false || true;
    }

    // Valid: operator aligned with first expression on next line
    void ValidMultiLine()
    {
        var a = true
                && false;

        var b = true
                || false;

        var c = true
                && false
                && true;

        var d = true
                || false
                || true;
    }

    // Invalid: operator not aligned with first expression
    void InvalidMultiLine()
    {
        var a = true
                && false;

        var b = true
                || false;

        var c = true
                && false
                && true;
    }
}
