using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using OSOS_Task_ASP.Dtos;
using OSOS_Task_ASP.Models;
using OSOS_Task_ASP.Interfaces;
using OSOS_Task_ASP.Services;
using Xunit;

namespace OSOS_Task_ASP.Tests.Services
{
    public class DateServiceTests
    {
        private readonly DateService _dateService;
        private readonly IFakeWrapper _fakeWrapper;

        public DateServiceTests()
        {
            _fakeWrapper = A.Fake<IFakeWrapper>();
            _dateService = new DateService(_fakeWrapper);
        }

        [Fact]
        public async Task CalculateEndDate_ReturnsCorrectEndDate_WithoutHolidays()
        {
            // Arrange
            var startDate = new DateOnly(2023, 6, 1); // A Thursday
            var workingDays = 5;
            var holidays = new List<DateOnly>(); // No holidays

            A.CallTo(() => _fakeWrapper.GetHolidaysAsync(A<int>.Ignored, A<int>.Ignored)).Returns(Task.FromResult(holidays));

            // Act
            var result = await _dateService.CalculateEndDate(startDate, workingDays);

            // Assert
            result.Should().NotBeNull();
            result.EndDate.Should().Be(new DateOnly(2023, 6, 7)); // 5 working days excluding weekends
        }

        [Fact]
        public async Task CalculateEndDate_ReturnsCorrectEndDate_WithHolidays()
        {
            // Arrange
            var startDate = new DateOnly(2023, 6, 1); // A Thursday
            var workingDays = 5;
            var holidays = new List<DateOnly> { new DateOnly(2023, 6, 6) }; // Holiday on a Tuesday

            A.CallTo(() => _fakeWrapper.GetHolidaysAsync(A<int>.Ignored, A<int>.Ignored)).Returns(Task.FromResult(holidays));

            // Act
            var result = await _dateService.CalculateEndDate(startDate, workingDays);

            // Assert
            result.Should().NotBeNull();
            result.EndDate.Should().Be(new DateOnly(2023, 6, 8)); // 5 working days excluding weekends and holiday
        }

        [Fact]
        public async Task CalculateEndDate_ReturnsNull_OnException()
        {
            // Arrange
            var startDate = new DateOnly(2023, 6, 1); // A Thursday
            var workingDays = 5;

            A.CallTo(() => _fakeWrapper.GetHolidaysAsync(A<int>.Ignored, A<int>.Ignored)).Throws<Exception>();

            // Act
            var result = await _dateService.CalculateEndDate(startDate, workingDays);

            // Assert
            result.Should().BeNull();
        }
    }

    public interface IFakeWrapper
    {
        Task<List<DateOnly>> GetHolidaysAsync(int startYear, int endYear);
    }

    public class DateService : IDateService
    {
        private readonly IFakeWrapper _fakeWrapper;

        public DateService(IFakeWrapper fakeWrapper)
        {
            _fakeWrapper = fakeWrapper;
        }

        public async Task<DateResponseDto?> CalculateEndDate(DateOnly startDate, int workingDays)
        {
            var currentDate = startDate;
            var daysCounted = 0;
            var daysDetails = new List<DayDetails>();
            List<DateOnly> holidays;

            try
            {
                holidays = await _fakeWrapper.GetHolidaysAsync(startDate.Year, startDate.AddDays(workingDays).Year);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                return null;
            }

            while (daysCounted < workingDays)
            {
                if (currentDate.DayOfWeek != DayOfWeek.Saturday &&
                    currentDate.DayOfWeek != DayOfWeek.Sunday &&
                    !holidays.Contains(currentDate))
                {
                    daysCounted++;
                }

                daysDetails.Add(new DayDetails
                {
                    Date = currentDate,
                    DayOfWeek = currentDate.DayOfWeek.ToString(),
                    IsHoliday = holidays.Contains(currentDate)
                });

                currentDate = currentDate.AddDays(1);
            }
            return new DateResponseDto
            {
                EndDate = currentDate.AddDays(-1),
                DayDetails = daysDetails
            };
        }
    }
}