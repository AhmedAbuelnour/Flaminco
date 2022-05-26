namespace Flaminco.PeriodicAction;

public class PeriodicActionOption
{
    public TimeSpan InvokeForEach { get; set; } = TimeSpan.Zero;
    public Action PeridicAction { get; set; }
}
