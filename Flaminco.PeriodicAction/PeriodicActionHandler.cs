using Microsoft.Extensions.Options;

namespace Flaminco.PeriodicAction;

public class PeriodicActionHandler
{
    private readonly IOptions<PeriodicActionOption> _periodicActionOption;
    private PeriodicTimer _currentTimer;
    private CancellationTokenSource _currentCancellationTokenSource;
    public PeriodicActionHandler(IOptions<PeriodicActionOption> periodicActionOption)
    {
        _periodicActionOption = periodicActionOption;
        _currentTimer = new PeriodicTimer(_periodicActionOption.Value.InvokeForEach);
        _currentCancellationTokenSource = new CancellationTokenSource();
    }

    public async Task StartAsync()
    {
        if (_periodicActionOption.Value.PeridicAction == null)
        {
            throw new ArgumentNullException("PeriodicAction", "You have to give an implementation for the PeriodicAction in the AddPeriodicAction's options");
        }
        if (_periodicActionOption.Value.InvokeForEach == TimeSpan.Zero)
        {
            throw new InvalidDataException("Your Action will not be invoked, because you didn't set a time interval in the AddPeriodicAction's options");
        }
        if (_currentCancellationTokenSource.IsCancellationRequested)
        {
            _currentCancellationTokenSource = new CancellationTokenSource();
        }
        while (await _currentTimer.WaitForNextTickAsync(_currentCancellationTokenSource.Token))
        {
            _periodicActionOption.Value.PeridicAction.Invoke();
        }
    }

    public void Stop()
    {
        _currentCancellationTokenSource.Cancel();
    }
}
