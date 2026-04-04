using System;

namespace Reihitsu.Analyzer.Test.Design.Resources;

internal class RH0103
{
    public void ThrowNotImplemented()
    {
        throw new {|#0:NotImplementedException|}();
    }

    public void ThrowNotImplementedWithMessage()
    {
        throw new {|#1:NotImplementedException|}("Not yet implemented");
    }

    public int Property
    {
        get
        {
            throw new {|#2:NotImplementedException|}();
        }
    }

    public void ThrowArgumentException()
    {
        throw new ArgumentException("test");
    }

    public void ThrowInvalidOperationException()
    {
        throw new InvalidOperationException();
    }

    public void NoThrow()
    {
    }
}
