internal class RH0327
{
    public int P1 => 0;

    {|#0:public int P2
            => 2;|}

    {|#1:public int P3 => 0
                          + 3;|}

    public int P4 { get; set; }
    public int P5 { get; }
    public int P6 { get; } = 6;

    public int P7 
    { 
        get => 7;
    }

    public int P8
    {
        get => P4;
        set => P4 = value;
    }
}