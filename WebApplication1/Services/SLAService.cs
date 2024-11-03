//using Flaminco.SLA.Models;
//using Flaminco.SLA.Services;

//namespace WebApplication1.Services
//{
//    public class SLAService(ILogger<SLAService> logger) : SLAServiceBase(logger)
//    {
//        public override ValueTask<CalendarInfo> GetCalendar()
//        {
//            // Inject your caching here if you want.

//            return ValueTask.FromResult(new CalendarInfo
//            {
//                WorkingDays =
//                [
//                    DayOfWeek.Saturday,
//                    DayOfWeek.Sunday,
//                    DayOfWeek.Monday,
//                    DayOfWeek.Tuesday,
//                    DayOfWeek.Thursday,
//                ],
//                Holidays =
//                [
//                    new Holiday(new DateOnly(2024,01,01),new DateOnly(2024,01,11))
//                ]
//            });
//        }
//    }
//}

