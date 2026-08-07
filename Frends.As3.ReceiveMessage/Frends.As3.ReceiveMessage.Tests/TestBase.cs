using Frends.As3.ReceiveMessage.Definitions;

namespace Frends.As3.ReceiveMessage.Tests;

internal abstract class TestBase
{
    protected static Input DefaultInput() => new();

    protected static Connection DefaultConnection() => new();

    protected static Options DefaultOptions() => new();
}
