using System;
using System.Threading.Tasks;

namespace Reihitsu.Analyzer.Test.Design.Resources;

internal class RH0102
{
    public async Task ValidAsyncTask()
    {
        await Task.Delay(10);
    }

    public async void {|#0:InvalidAsyncVoid|}()
    {
        await Task.Delay(10);
    }

    private async void {|#1:OnClick|}(object sender, EventArgs e)
    {
        await Task.Delay(10);
    }

    public void NormalVoid()
    {
    }

    public async Task<int> ValidAsyncTaskWithReturn()
    {
        await Task.Delay(10);

        return 42;
    }
}

public class BaseClass
{
    public virtual async void {|#2:DoSomething|}() => await Task.Delay(10);
}

public class DerivedClass : BaseClass
{
    public override async void {|#3:DoSomething|}() => await Task.Delay(10);
}