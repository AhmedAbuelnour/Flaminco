

## How to use it?

You need to inject the service locator into the DI system, which must be located in the same library that your services are located at.

```csharp
builder.Services.AddHangfireServiceLocator<IHangfireServicesScanner>();
```

We have 4 service types each one of them, there is a contract represent each type 
* IQueueServiceJob for Queue based services
* IScheduleServiceJob for delayed based services
* IContinueServiceJob for continues based services
* IRecurringServiceJob for recurring based services.

```csharp
// Example for IQueueServiceJob with provided value
       public class SendConfirmationService : IQueueServiceJob
    {
        private readonly INotificationService _notificationService;

        public SendConfirmationService(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        public async ValueTask Execute(object? value = default, CancellationToken cancellationToken = default)
        {
            if (value is IEnumerable<JToken> jTokens)
            {
                JArray jArray = new JArray(jTokens);

                if (jArray.ToObject<SendConfirmationValue[]>() is SendConfirmationValue[] sendEmailValues
                    && sendEmailValues.FirstOrDefault() is SendConfirmationValue sendEmailValue)
                {
                    if (sendEmailValue.IsEmail)
                    {
                        bool result = await _notificationService.SendEmailAsync(new EmailNotificationModel
                        {
                            MailFrom = "noreply@selaheltelmeez.com",
                            MailTo = sendEmailValue.Email,
                            MailSubject = sendEmailValue.MailSubject,
                            IsBodyHtml = true,
                            DisplayName = "سلاح التلميذ",
                            MailToName = sendEmailValue.MailToName,
                            MailBody = sendEmailValue.OTP
                        }, cancellationToken);

                        if (result == false)
                        {
                            throw new Exception($"Can't send an email for {sendEmailValue.Email}");
                        }
                    }

                    if (sendEmailValue.IsMobile)
                    {
                        bool result = await _notificationService.SendSMSAsync(new SMSNotificationModel
                        {
                            Mobile = sendEmailValue.MobileNumber,
                            OTP = sendEmailValue.OTP
                        }, cancellationToken);

                        if (result == false)
                        {
                            throw new Exception($"Can't send a SMS for {sendEmailValue.MobileNumber}");
                        }
                    }
                }
            }
        }
    }

    public class SendConfirmationValue
    {
        public string? Email { get; set; }
        public string? MobileNumber { get; set; }
        public string OTP { get; set; }
        public string MailToName { get; set; }
        public bool IsEmail { get; set; }
        public bool IsMobile { get; set; }
    }
```
```csharp
// Example for IScheduleServiceJob
public class ScheduleEmailSend : IScheduleServiceJob
{
    private readonly INotification _notification;
    public QueueEmailSend(INotification notification)
    {
        _notification = notification;
    }
    // Here we define when this service will be invoked.
    public TimeSpan Offset { get; set; } = TimeSpan.FromHours(1);

    public async ValueTask Execute(object? value = default, CancellationToken cancellationToken = default)
    {
        string email = "example@outlook.com";

        await _notification.SendAsync(email);
    }
}
```
```csharp
// Example for IContinueServiceJob 
public class ContinueEmailSend : IContinueServiceJob
{
    private readonly INotification _notification;
    public ContinueEmailSend(INotification notification)
    {
        _notification = notification;
    }

    public async ValueTask Execute(object? value = default, CancellationToken cancellationToken = default)
    {
        string email = "example@outlook.com";

        await _notification.SendAsync(email);
    }
}

```
```csharp
// Example for IRecurringServiceJob
// Each midnight send email notification 
public class RecurringEmailNotification : IRecurringServiceJob
{
    // Each Midnight 
    public string RecurringCron { get; init; } = Cron.Daily(23,59);
    public string Key { get; init; } = nameof(RecurringEmailNotification );

    private readonly INotification _notification;
    public RecurringEmailNotification(INotification notification)
    {

        _notification = notification;
    }

    public async ValueTask Execute(object? value = default, CancellationToken cancellationToken = default)
    {
        string email = "example@outlook.com";

        await _notification.SendAsync(email);
    }
}
```

Example for the service locator

```csharp
app.MapGet("/hangfireRecurring", (IHangfireServiceLocator _serviceLocator) =>
{
  _serviceLocator.Execute<SendConfirmationService>(new SendConfirmationValue
            {
                IsEmail = isEmailUsed,
                IsMobile = !isEmailUsed,
                MailToName = user.FullName,
                OTP = user.Activations.FirstOrDefault()?.Code,
                Email = user.Email,
                MobileNumber = user.MobileNumber
            }, cancellationToken: cancellationToken); 
 });
 
app.MapGet("/hangfireContinuation", (IHangfireServiceLocator _serviceLocator) =>
{
    // Send an email after sign up for example.
    string signUpJobId = _serviceLocator.Execute<SignupService>();

    _serviceLocator.Execute<ContinueEmailSend>(signUpJobId);
 });
 
 app.MapGet("/hangfireSchedule", (IHangfireServiceLocator _serviceLocator) =>
{
     _serviceLocator.Execute<ScheduleEmailSend>();
});

app.MapGet("/hangfireQueue", (IHangfireServiceLocator _serviceLocator) =>
{
     _serviceLocator.Execute<QueueEmailSend>();
});
```


