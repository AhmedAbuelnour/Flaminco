using Microsoft.Extensions.Options;

namespace Flaminco.PeriodicAction;

public class PeriodicActionHandler
{
    private readonly IOptions<PeriodicActionOption> _periodicActionOption;
    private readonly PeriodicTimer _currentTimer;
    private CancellationTokenSource _currentCancellationTokenSource;
    public PeriodicActionHandler(IOptions<PeriodicActionOption> periodicActionOption)
    {
        _periodicActionOption = periodicActionOption;
        _currentTimer = new PeriodicTimer(_periodicActionOption.Value.TimeInterval);
        _currentCancellationTokenSource = new CancellationTokenSource();
    }

    public async Task ActivateAsync()
    {
        if (_periodicActionOption.Value.PeridicAction == null)
        {
            throw new ArgumentNullException("PeriodicAction", "You have to give an implementation for the PeriodicAction in the AddPeriodicAction's options");
        }
        if (_periodicActionOption.Value.TimeInterval == TimeSpan.Zero)
        {
            throw new InvalidDataException("Your Action will not be invoked, because you didn't set a time interval in the AddPeriodicAction's options");
        }

        while (await _currentTimer.WaitForNextTickAsync(_currentCancellationTokenSource.Token))
        {
            _periodicActionOption.Value.PeridicAction.Invoke();
        }
    }

    public void Deactivate()
    {
        _currentCancellationTokenSource.Cancel();
    }
    public async Task ReactivateAsync()
    {
        if (_currentCancellationTokenSource.IsCancellationRequested)
        {
            _currentCancellationTokenSource = new CancellationTokenSource();
            await ActivateAsync();
        }
    }
}
