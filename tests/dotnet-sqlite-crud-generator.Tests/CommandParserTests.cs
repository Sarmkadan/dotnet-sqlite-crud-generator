#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Threading.Tasks;
using DotNet.SQLite.CrudGenerator.CLI;
using DotNet.SQLite.CrudGenerator.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace DotNet.SQLite.CrudGenerator.Tests;

/// <summary>
/// Tests for the <see cref="CommandParser"/> class, ensuring that
/// command-line argument parsing and execution works correctly.
/// </summary>
public sealed class CommandParserTests
{
    private readonly CommandParser _parser;

    public CommandParserTests()
    {
        _parser = new CommandParser(NullLogger<CommandParser>.Instance);
    }

    [Fact]
    public async Task ParseAndExecuteAsync_WithEmptyArgs_PrintsHelpAndReturnsZero()
    {
        // Arrange
        var args = Array.Empty<string>();

        // Act
        var result = await _parser.ParseAndExecuteAsync(args);

        // Assert
        result.Should().Be(0);
    }

    [Theory]
    [InlineData("-h")]
    [InlineData("--help")]
    [InlineData("help")]
    public async Task ParseAndExecuteAsync_WithHelpArgs_PrintsHelpAndReturnsZero(string arg)
    {
        // Act
        var result = await _parser.ParseAndExecuteAsync(new[] { arg });

        // Assert
        result.Should().Be(0);
    }

    [Theory]
    [InlineData("-v")]
    [InlineData("--version")]
    [InlineData("version")]
    public async Task ParseAndExecuteAsync_WithVersionArgs_PrintsVersionAndReturnsZero(string arg)
    {
        // Act
        var result = await _parser.ParseAndExecuteAsync(new[] { arg });

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public async Task ParseAndExecuteAsync_WithUnknownCommand_ReturnsNonZero()
    {
        // Arrange
        var args = new[] { "unknowncommand" };

        // Act
        var result = await _parser.ParseAndExecuteAsync(args);

        // Assert
        result.Should().NotBe(0);
    }

    [Fact]
    public async Task ParseAndExecuteAsync_WithRegisteredCommand_ExecutesAndPropagatesExitCode()
    {
        // Arrange
        var fakeCommand = new FakeCommand(returnCode: 42);
        _parser.RegisterCommand<FakeCommand>("fake");

        var args = new[] { "fake", "--some-option" };

        // Act
        var result = await _parser.ParseAndExecuteAsync(args);

        // Assert
        result.Should().Be(42);
        fakeCommand.ReceivedArgs.Should().Equal(new[] { "--some-option" });
    }

    [Theory]
    [InlineData("FAKE")]
    [InlineData("Fake")]
    [InlineData("fake")]
    public async Task ParseAndExecuteAsync_CommandLookup_IsCaseInsensitive(string commandName)
    {
        // Arrange
        var fakeCommand = new FakeCommand(returnCode: 0);
        _parser.RegisterCommand<FakeCommand>("fake");

        var args = new[] { commandName };

        // Act
        var result = await _parser.ParseAndExecuteAsync(args);

        // Assert
        result.Should().Be(0);
        fakeCommand.ReceivedArgs.Should().BeEmpty();
    }

    private sealed class FakeCommand : ICommand
    {
        public int ReturnCode { get; }
        public string[]? ReceivedArgs { get; private set; }

        public FakeCommand(int returnCode)
        {
            ReturnCode = returnCode;
        }

        public Task<int> ExecuteAsync(string[] args)
        {
            ReceivedArgs = args;
            return Task.FromResult(ReturnCode);
        }
    }
}