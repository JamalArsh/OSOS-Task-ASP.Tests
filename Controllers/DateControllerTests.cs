using System;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using OSOS_Task_ASP.Controllers;
using OSOS_Task_ASP.Dtos;
using OSOS_Task_ASP.Interfaces;
using Xunit;

namespace OSOS_Task_ASP.Tests.Controllers
{
    public class DateControllerTests
    {
        private readonly IDateService _fakeDateService;
        private readonly DateController _dateController;

        public DateControllerTests()
        {
            _fakeDateService = A.Fake<IDateService>();
            _dateController = new DateController(_fakeDateService);
        }

        [Fact]
        public async Task GetEndDate_ReturnsBadRequest_WhenRequestIsNull()
        {
            // Act
            var result = await _dateController.GetEndDate(null);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>()
                .Which.Value.Should().Be("Invalid inputs");
        }

        // [Fact]
        // public async Task GetEndDate_ReturnsBadRequest_WhenStartDateIsInvalid()
        // {
        //     // Arrange
        //     var request = new DateRequestDto { StartDate = null, WorkingDays = 5 };

        //     // Act
        //     var result = await _dateController.GetEndDate(request);

        //     // Assert
        //     result.Should().BeOfType<BadRequestObjectResult>()
        //         .Which.Value.Should().Be("Invalid input for start date");
        // }

        [Fact]
        public async Task GetEndDate_ReturnsBadRequest_WhenWorkingDaysIsInvalid()
        {
            // Arrange
            var request = new DateRequestDto { StartDate = new DateOnly(2024,6,12), WorkingDays = 0 };

            // Act
            var result = await _dateController.GetEndDate(request);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>()
                .Which.Value.Should().Be("Invalid input for working days");
        }

        [Fact]
        public async Task GetEndDate_ReturnsStatusCode500_WhenServiceReturnsNull()
        {
            // Arrange
            var request = new DateRequestDto { StartDate = new DateOnly(2024,6,12), WorkingDays = 5 };
            A.CallTo(() => _fakeDateService.CalculateEndDate(A<DateOnly>.Ignored, A<int>.Ignored)).Returns(Task.FromResult<DateResponseDto>(null));

            // Act
            var result = await _dateController.GetEndDate(request);

            // Assert
            result.Should().BeOfType<ObjectResult>()
                .Which.StatusCode.Should().Be(500);
        }

        [Fact]
        public async Task GetEndDate_ReturnsOk_WhenRequestIsValid()
        {
            // Arrange
            var request = new DateRequestDto { StartDate = new DateOnly(2024,6,12), WorkingDays = 5 };
            var expectedResponse = new DateResponseDto { EndDate = new DateOnly(2024,6,12).AddDays(5) };
            A.CallTo(() => _fakeDateService.CalculateEndDate(A<DateOnly>.Ignored, A<int>.Ignored)).Returns(Task.FromResult(expectedResponse));

            // Act
            var result = await _dateController.GetEndDate(request);

            // Assert
            result.Should().BeOfType<OkObjectResult>()
                .Which.Value.Should().Be(expectedResponse);
        }
    }
}