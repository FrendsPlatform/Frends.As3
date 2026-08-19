using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Frends.As3.SendMessage.Tests;

[TestFixture]
internal class ErrorHandlerTest : TestBase
{
    private const string CustomErrorMessage = "CustomErrorMessage";

    [Test]
    public void Should_Throw_Error_When_ThrowErrorOnFailure_Is_True()
    {
        Func<Task> action = async () =>
        {
            await As3.SendMessage(
                DefaultInput(),
                DefaultConnection(),
                DefaultOptions(),
                CancellationToken.None);
        };

        var ex = Assert.ThrowsAsync<Exception>(action);
    }

    [Test]
    public async Task Should_Return_Failed_Result_When_ThrowErrorOnFailure_Is_False()
    {
        var options = DefaultOptions();
        options.ThrowErrorOnFailure = false;
        var result = await As3.SendMessage(DefaultInput(), DefaultConnection(), options, CancellationToken.None);
        Assert.That(result.Success, Is.False);
    }

    [Test]
    public void Should_Throw_Custom_Error_Message_When_ErrorMessageOnFailure_Is_Set()
    {
        var options = DefaultOptions();
        options.ErrorMessageOnFailure = CustomErrorMessage;

        Func<Task> action = async () =>
        {
            await As3.SendMessage(
                DefaultInput(),
                DefaultConnection(),
                options,
                CancellationToken.None);
        };

        var ex = Assert.ThrowsAsync<Exception>(action);

        Assert.That(ex, Is.Not.Null);
        Assert.That(ex.Message, Does.Contain(CustomErrorMessage));
    }
}
