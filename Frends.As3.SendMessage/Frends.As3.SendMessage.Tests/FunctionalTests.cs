using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DotNet.Testcontainers.Containers;
using NUnit.Framework;

namespace Frends.As3.SendMessage.Tests;

[TestFixture]
public class FunctionalTests
{
    private static readonly string FileName = Path.GetFileName(TestSetup.Input().MessageFilePath);
    private IContainer ftpContainer;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        ftpContainer = await TestSetup.StartFtpContainerAsync();
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        await ftpContainer.DisposeAsync();
    }

    [Test]
    public async Task ShouldSendPlainMessage()
    {
        var result = await As3.SendMessage(
            TestSetup.Input(),
            TestSetup.Connection(),
            TestSetup.Options(),
            CancellationToken.None);

        Assert.That(result.Success, Is.True);
        Assert.That(result.MessageId, Is.Not.Null.And.Not.Empty);
        var fileName = Path.GetFileName(TestSetup.Input().MessageFilePath);
        Assert.That(await TestSetup.FileExistsOnFtp(ftpContainer, TestSetup.AbsoluteFtpPath(FileName)), Is.True);
    }

    [Test]
    public async Task ShouldSendSignedMessage()
    {
        var con = TestSetup.Connection();
        con.SignMessage = true;

        var result = await As3.SendMessage(
            TestSetup.Input(), con, TestSetup.Options(), CancellationToken.None);

        Assert.That(result.Success, Is.True);
        Assert.That(result.OriginalContentMIC, Is.Not.Null.And.Not.Empty);
        Assert.That(result.MdnOptions, Is.Not.Null.And.Not.Empty);
        Assert.That(await TestSetup.FileExistsOnFtp(ftpContainer, TestSetup.AbsoluteFtpPath(FileName)), Is.True);
    }

    [Test]
    public async Task ShouldSendEncryptedMessage()
    {
        var con = TestSetup.Connection();
        con.EncryptMessage = true;

        var result = await As3.SendMessage(
            TestSetup.Input(), con, TestSetup.Options(), CancellationToken.None);

        Assert.That(result.Success, Is.True);
        Assert.That(await TestSetup.FileExistsOnFtp(ftpContainer, TestSetup.AbsoluteFtpPath(FileName)), Is.True);
    }

    [Test]
    public async Task ShouldSendSignedAndEncryptedMessage()
    {
        var con = TestSetup.Connection();
        con.SignMessage = true;
        con.EncryptMessage = true;

        var result = await As3.SendMessage(
            TestSetup.Input(), con, TestSetup.Options(), CancellationToken.None);

        Assert.That(result.Success, Is.True);
        Assert.That(result.OriginalContentMIC, Is.Not.Null.And.Not.Empty);
        Assert.That(result.MdnOptions, Is.Not.Null.And.Not.Empty);
        Assert.That(await TestSetup.FileExistsOnFtp(ftpContainer, TestSetup.AbsoluteFtpPath(FileName)), Is.True);
    }

    [Test]
    public async Task ShouldUploadFileToRemoteDirectory()
    {
        var con = TestSetup.Connection();
        con.RemoteFilePath = TestSetup.FtpSubdirRelative;

        var result = await As3.SendMessage(
            TestSetup.Input(), con, TestSetup.Options(), CancellationToken.None);

        Assert.That(result.Success, Is.True);
        Assert.That(result.PartnerResponse, Does.Contain("subdir"));
        Assert.That(await TestSetup.FileExistsOnFtp(ftpContainer, TestSetup.AbsoluteFtpSubdirPath(FileName)), Is.True);
    }

    [Test]
    public async Task ShouldReturnCorrectMessageId()
    {
        var result = await As3.SendMessage(
            TestSetup.Input(),
            TestSetup.Connection(),
            TestSetup.Options(),
            CancellationToken.None);

        Assert.That(result.MessageId, Does.StartWith("<"));
        Assert.That(result.MessageId, Does.EndWith(">"));
        Assert.That(result.MessageId, Does.Contain("@"));
    }

    [Test]
    public async Task ShouldFailWithInvalidFtpHost()
    {
        var con = TestSetup.Connection();
        con.FtpHost = "invalid-host-that-does-not-exist";

        var opt = TestSetup.Options();
        opt.ThrowErrorOnFailure = false;

        var result = await As3.SendMessage(
            TestSetup.Input(), con, opt, CancellationToken.None);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Error, Is.Not.Null);
    }

    [Test]
    public async Task ShouldFailWithInvalidCredentials()
    {
        var con = TestSetup.Connection();
        con.FtpUser = "wronguser";
        con.FtpPassword = "wrongpassword";

        var opt = TestSetup.Options();
        opt.ThrowErrorOnFailure = false;

        var result = await As3.SendMessage(
            TestSetup.Input(), con, opt, CancellationToken.None);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Error, Is.Not.Null);
    }

    [Test]
    public async Task ShouldFailWithInvalidMessageFilePath()
    {
        var input = TestSetup.Input();
        input.MessageFilePath = "invalid/path/message.txt";

        var opt = TestSetup.Options();
        opt.ThrowErrorOnFailure = false;

        var result = await As3.SendMessage(
            input, TestSetup.Connection(), opt, CancellationToken.None);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Error.Message, Does.Contain("Could not find"));
    }

    [Test]
    public async Task ShouldPopulateResultFieldsOnSuccess()
    {
        var con = TestSetup.Connection();
        con.SignMessage = true;

        var result = await As3.SendMessage(
            TestSetup.Input(), con, TestSetup.Options(), CancellationToken.None);

        Assert.That(result.Success, Is.True);
        Assert.That(result.MessageId, Is.Not.Null.And.Not.Empty, "MessageId should be set");
        Assert.That(result.OriginalContentMIC, Is.Not.Null.And.Not.Empty, "MIC should be set when signing");
        Assert.That(result.MdnOptions, Is.Not.Null.And.Not.Empty, "MdnOptions should be set when signing");
        Assert.That(result.PartnerResponse, Is.Not.Null.And.Not.Empty, "PartnerResponse should be set");
    }
}