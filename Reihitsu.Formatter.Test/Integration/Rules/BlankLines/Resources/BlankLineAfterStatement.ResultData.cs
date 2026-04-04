internal class BlankLineAfterStatementTestData
{
    public void SwitchWithBreakFollowedByCase()
    {
        switch (1)
        {
            case 1:
                System.Console.WriteLine();
                break;

            case 2:
                System.Console.WriteLine();
                break;

            default:
                break;
        }
    }

    public void BreakInLoopFollowedByStatement()
    {
        for (var i = 0; i < 10; i++)
        {
            break;

            var x = 1;
        }
    }

    // --- Cases that should NOT be modified ---

    public void BreakLastInBlock()
    {
        while (true)
        {
            break;
        }
    }

    public void BreakAlreadyFollowedByBlankLine()
    {
        for (var i = 0; i < 10; i++)
        {
            break;

            var x = 1;
        }
    }

    public void SwitchBreakLastInSection()
    {
        switch (1)
        {
            case 1:
                break;
        }
    }
}