namespace XoClock
{
    public delegate void TickEventHandler(object sender, TickEventArgs e);

    internal interface IClock
    {
        event TickEventHandler Tick;
        int Period { get; set; }
    }
}