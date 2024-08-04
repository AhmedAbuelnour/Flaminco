using Flaminco.SLA.Models;
using Microsoft.Extensions.Logging;

namespace Flaminco.SLA.Services
{
    /// <summary>
    /// Base class for SLA (Service Level Agreement) service.
    /// Provides methods to calculate delivery dates, remaining days, and check working days and holidays.
    /// </summary>
    public abstract class SLAServiceBase(ILogger<SLAServiceBase> _logger)
    {
        /// <summary>
        /// Gets the calendar information.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains the calendar information.</returns>
        public abstract ValueTask<CalendarInfo> GetCalendar();

        /// <summary>
        /// Calculates the remaining days until the delivery date.
        /// </summary>
        /// <param name="deliveryDate">The delivery date.</param>
        /// <param name="currentDate">The current date.</param>
        /// <param name="addOneDayForPosVal">If set to true, adds one day if the result is positive.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the remaining days.</returns>
        public async ValueTask<int> CalculateRemainingDays(DateOnly deliveryDate, DateOnly currentDate, bool addOneDayForPosVal = false)
        {
            _logger.LogInformation("Calculating remaining days. DeliveryDate: {DeliveryDate}, CurrentDate: {CurrentDate}, AddOneDayForPosVal: {AddOneDayForPosVal}", deliveryDate, currentDate, addOneDayForPosVal);

            int totalDaysRemaining = deliveryDate.DayNumber - currentDate.DayNumber;

            int sign = totalDaysRemaining < 0 ? -1 : 1;

            int result = await GetTotalRemainingDays(Math.Abs(totalDaysRemaining), currentDate) * sign;

            if (addOneDayForPosVal && result >= 0)
            {
                result++;
            }

            _logger.LogInformation("Remaining days calculated. Result: {Result}", result);

            return result;
        }

        /// <summary>
        /// Calculates the delivery date based on the given task duration and spent days.
        /// </summary>
        /// <param name="asOfDate">The starting date for the calculation.</param>
        /// <param name="taskDuration">The total duration of the task in days.</param>
        /// <param name="spentDays">The number of days already spent on the task.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the calculated delivery date.</returns>
        public async ValueTask<DateOnly> CalculateDeliveryDate(DateOnly asOfDate, int taskDuration, int spentDays = 0)
        {
            _logger.LogInformation("Calculating delivery date. AsOfDate: {AsOfDate}, TaskDuration: {TaskDuration}, SpentDays: {SpentDays}", asOfDate, taskDuration, spentDays);

            //getting calendars from cache
            CalendarInfo calendarInfo = await GetCalendar();

            DateOnly deliveryDate = asOfDate;

            taskDuration -= spentDays;

            int sign = taskDuration < 0 ? -1 : 1;

            // Loop while the taskDuration isn't over
            while (Math.Abs(taskDuration) > 0)
            {
                // Add a day to the delivery date
                deliveryDate = deliveryDate.AddDays(1 * sign);

                //// If the delivery date is not a working day or it's a holiday, skip it
                if (!IsWorkingDay(deliveryDate, calendarInfo) || IsHoliday(deliveryDate, calendarInfo))
                {
                    continue;
                }

                //if the date is a working date , work on the task
                if (IsWorkingDay(deliveryDate, calendarInfo) && !IsHoliday(deliveryDate, calendarInfo))
                {
                    // we can add a call to DB to add a log in the task logs table for this user on this submission Id
                    taskDuration += -sign;
                }
            }

            while (!IsWorkingDay(deliveryDate, calendarInfo) || IsHoliday(deliveryDate, calendarInfo))
            {
                deliveryDate = deliveryDate.AddDays(1);
            }

            _logger.LogInformation("Delivery date calculated. DeliveryDate: {DeliveryDate}", deliveryDate);

            return deliveryDate;
        }

        /// <summary>
        /// Checks if the specified date is a working day.
        /// </summary>
        /// <param name="date">The date to check.</param>
        /// <returns>A task that represents the asynchronous operation. The task result indicates whether the specified date is a working day.</returns>
        public async ValueTask<bool> CheckIfIsWorkingDay(DateOnly date)
        {
            _logger.LogInformation("Checking if date is a working day. Date: {Date}", date);

            CalendarInfo calendarInfo = await GetCalendar();

            bool result = IsWorkingDay(date, calendarInfo);

            _logger.LogInformation("Check if date is a working day completed. Result: {Result}", result);

            return result;
        }

        /// <summary>
        /// Checks if the specified date is a holiday.
        /// </summary>
        /// <param name="date">The date to check.</param>
        /// <returns>A task that represents the asynchronous operation. The task result indicates whether the specified date is a holiday.</returns>
        public async ValueTask<bool> CheckIfIsHoliday(DateOnly date)
        {
            _logger.LogInformation("Checking if date is a holiday. Date: {Date}", date);

            CalendarInfo calendarInfo = await GetCalendar();

            bool result = IsHoliday(date, calendarInfo);

            _logger.LogInformation("Check if date is a holiday completed. Result: {Result}", result);

            return result;
        }

        /// <summary>
        /// Calculates the number of spent days between the start date and end date.
        /// </summary>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date. If not specified, the current date is used.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the number of spent days.</returns>
        public async ValueTask<int> CalculateSpentDays(DateOnly startDate, DateOnly? endDate = default)
        {
            _logger.LogInformation("Calculating spent days. StartDate: {StartDate}, EndDate: {EndDate}", startDate, endDate);

            CalendarInfo calendarInfo = await GetCalendar();

            endDate ??= DateOnly.FromDateTime(DateTime.UtcNow);

            int totalSpentDays = Math.Max(0, endDate.Value.DayNumber - startDate.DayNumber);

            for (int i = 0; i <= totalSpentDays; i++)
            {
                if (!IsWorkingDay(startDate, calendarInfo) || IsHoliday(startDate, calendarInfo))
                {
                    totalSpentDays--;
                }

                startDate = startDate.AddDays(1);
            }

            _logger.LogInformation("Spent days calculated. TotalSpentDays: {TotalSpentDays}", totalSpentDays);

            return totalSpentDays;
        }

        private static bool IsWorkingDay(DateOnly date, CalendarInfo calendarInfo)
             => calendarInfo.WorkingDays.Contains(date.DayOfWeek);

        private static bool IsHoliday(DateOnly date, CalendarInfo calendarInfo)
            => calendarInfo.Holidays.Any(vacation => date >= vacation.StartDate && date < vacation.EndDate);

        private async ValueTask<int> GetTotalRemainingDays(int initialDaysRemaining, DateOnly currentDay)
        {
            _logger.LogInformation("Getting total remaining days. InitialDaysRemaining: {InitialDaysRemaining}, CurrentDay: {CurrentDay}", initialDaysRemaining, currentDay);

            CalendarInfo calendarInfo = await GetCalendar();

            int totalRemainingDays = !IsWorkingDay(currentDay, calendarInfo) || IsHoliday(currentDay, calendarInfo) ? 1 : 0;

            for (int i = 0; i < initialDaysRemaining; i++)
            {
                if (IsWorkingDay(currentDay, calendarInfo) && !IsHoliday(currentDay, calendarInfo))
                {
                    totalRemainingDays++;
                }
                currentDay = currentDay.AddDays(1);
            }

            _logger.LogInformation("Total remaining days calculated. TotalRemainingDays: {TotalRemainingDays}", totalRemainingDays);

            return totalRemainingDays;
        }
    }
}
