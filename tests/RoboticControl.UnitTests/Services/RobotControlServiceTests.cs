using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RoboticControl.Application.DTOs;
using RoboticControl.Application.Mappings;
using RoboticControl.Application.Services;
using RoboticControl.Domain.Entities;
using RoboticControl.Domain.Enums;
using RoboticControl.Domain.Exceptions;
using RoboticControl.Domain.Interfaces;
using System.Reflection;
using Xunit;

namespace RoboticControl.UnitTests.Services;

public class RobotControlServiceTests
{
    private readonly IMapper _mapper;
    private readonly Mock<IRobotHardwareService> _hardwareService;
    private readonly Mock<ILogger<RobotControlService>> _logger;
    private readonly RobotControlService _service;

    public RobotControlServiceTests()
    {
        var mapperConfig = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        _mapper = mapperConfig.CreateMapper();
        _hardwareService = new Mock<IRobotHardwareService>();
        _logger = new Mock<ILogger<RobotControlService>>();
        _service = new RobotControlService(_logger.Object, _hardwareService.Object, _mapper);
    }

    [Fact]
    public async Task GetCurrentPositionAsync_ReturnsMappedDto()
    {
        var position = new RobotPosition(10, 20, 30, 1, 2, 3);
        _hardwareService
            .Setup(service => service.GetCurrentPositionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(position);

        var result = await _service.GetCurrentPositionAsync();

        result.X.Should().Be(10);
        result.Y.Should().Be(20);
        result.Z.Should().Be(30);
        result.RotationX.Should().Be(1);
        result.RotationY.Should().Be(2);
        result.RotationZ.Should().Be(3);
        _hardwareService.Verify(service => service.GetCurrentPositionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetCurrentPositionAsync_UsesCachedValueWithinWindow()
    {
        var position = new RobotPosition(10, 20, 30);
        _hardwareService
            .Setup(service => service.GetCurrentPositionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(position);

        var first = await _service.GetCurrentPositionAsync();
        var lastUpdateField = typeof(RobotControlService)
            .GetField("_lastPositionUpdate", BindingFlags.Instance | BindingFlags.NonPublic);
        lastUpdateField?.SetValue(_service, DateTime.UtcNow);
        var second = await _service.GetCurrentPositionAsync();

        first.X.Should().Be(second.X);
        _hardwareService.Verify(service => service.GetCurrentPositionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetSystemStatusAsync_WhenHardwareFails_ReturnsDisconnectedStatus()
    {
        _hardwareService
            .Setup(service => service.GetStatusAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HardwareConnectionException("Connection lost"));

        var result = await _service.GetSystemStatusAsync();

        result.IsConnected.Should().BeFalse();
        result.State.Should().Be(RobotState.Disconnected.ToString());
        result.ErrorCode.Should().Be((int)ErrorCode.ConnectionLost);
        result.ErrorMessage.Should().Be("Hardware connection lost");
    }

    [Fact]
    public async Task MoveToPositionAsync_WhenOutsideEnvelope_ThrowsValidationException()
    {
        var command = new MoveCommandDto
        {
            X = 2000,
            Y = 0,
            Z = 0
        };

        var act = async () => await _service.MoveToPositionAsync(command);

        await act.Should().ThrowAsync<CommandValidationException>();
        _hardwareService.Verify(
            service => service.MoveToPositionAsync(It.IsAny<RobotPosition>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task MoveToPositionAsync_WhenValid_InvokesHardwareMove()
    {
        var command = new MoveCommandDto
        {
            X = 100,
            Y = 50,
            Z = 200,
            RotationX = 1,
            RotationY = 2,
            RotationZ = 3
        };

        _hardwareService
            .Setup(service => service.MoveToPositionAsync(It.IsAny<RobotPosition>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _service.MoveToPositionAsync(command);

        result.Should().BeTrue();
        _hardwareService.Verify(service => service.MoveToPositionAsync(
            It.Is<RobotPosition>(pos =>
                pos.X == command.X &&
                pos.Y == command.Y &&
                pos.Z == command.Z &&
                pos.RotationX == command.RotationX &&
                pos.RotationY == command.RotationY &&
                pos.RotationZ == command.RotationZ),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task JogAsync_WhenResultOutsideEnvelope_ThrowsValidationException()
    {
        _hardwareService
            .Setup(service => service.GetCurrentPositionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RobotPosition(500, 0, 0));

        var command = new JogCommandDto
        {
            DeltaX = 10,
            DeltaY = 0,
            DeltaZ = 0
        };

        var act = async () => await _service.JogAsync(command);

        await act.Should().ThrowAsync<CommandValidationException>();
        _hardwareService.Verify(
            service => service.MoveRelativeAsync(It.IsAny<double>(), It.IsAny<double>(), It.IsAny<double>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task JogAsync_WhenValid_InvokesHardwareMove()
    {
        _hardwareService
            .Setup(service => service.GetCurrentPositionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RobotPosition(0, 0, 100));
        _hardwareService
            .Setup(service => service.MoveRelativeAsync(5, 0, 0, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _service.JogAsync(new JogCommandDto
        {
            DeltaX = 5,
            DeltaY = 0,
            DeltaZ = 0
        });

        result.Should().BeTrue();
        _hardwareService.Verify(
            service => service.MoveRelativeAsync(5, 0, 0, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteEmergencyStopAsync_WhenHardwareThrows_ReturnsFalse()
    {
        _hardwareService
            .Setup(service => service.EmergencyStopAsync())
            .ThrowsAsync(new InvalidOperationException("Stop failed"));

        var result = await _service.ExecuteEmergencyStopAsync();

        result.Should().BeFalse();
    }

    [Fact]
    public async Task HomeAsync_WhenHardwareFails_ThrowsException()
    {
        _hardwareService
            .Setup(service => service.HomeAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HardwareConnectionException("Disconnected"));

        var act = async () => await _service.HomeAsync();

        await act.Should().ThrowAsync<HardwareConnectionException>();
    }

    [Fact]
    public void GetWorkEnvelope_ReturnsDefaultEnvelope()
    {
        var result = _service.GetWorkEnvelope();
        var expected = WorkEnvelope.Default;

        result.XMin.Should().Be(expected.XMin);
        result.XMax.Should().Be(expected.XMax);
        result.YMin.Should().Be(expected.YMin);
        result.YMax.Should().Be(expected.YMax);
        result.ZMin.Should().Be(expected.ZMin);
        result.ZMax.Should().Be(expected.ZMax);
    }
}
