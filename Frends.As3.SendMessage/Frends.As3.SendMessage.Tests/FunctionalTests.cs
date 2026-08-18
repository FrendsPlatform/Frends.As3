using DotNet.Testcontainers.Containers;
using NUnit.Framework;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

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

        var content = await TestSetup.ReadFileFromFtp(ftpContainer, TestSetup.AbsoluteFtpPath(FileName));
        Assert.That(content, Does.Contain("multipart/signed").Or.Contain("pkcs7-signature"));
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

        var content = await TestSetup.ReadFileFromFtp(ftpContainer, TestSetup.AbsoluteFtpPath(FileName));
        Assert.That(content, Does.Contain("application/pkcs7-mime").Or.Contain("enveloped-data"));

        var plaintext = await File.ReadAllTextAsync(TestSetup.Input().MessageFilePath);
        Assert.That(content, Does.Not.Contain(plaintext));
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

        var content = await TestSetup.ReadFileFromFtp(ftpContainer, TestSetup.AbsoluteFtpPath(FileName));
        Assert.That(content, Does.Contain("application/pkcs7-mime").Or.Contain("enveloped-data"));

        var plaintext = await File.ReadAllTextAsync(TestSetup.Input().MessageFilePath);
        Assert.That(content, Does.Not.Contain(plaintext));
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

    [Test]
    public async Task ShouldWriteLogsToSpecifiedDirectory()
    {
        var logDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(logDir);

        try
        {
            var opt = TestSetup.Options();
            opt.LogDirectory = logDir;

            var result = await As3.SendMessage(
                TestSetup.Input(),
                TestSetup.Connection(),
                opt,
                CancellationToken.None);

            Assert.That(result.Success, Is.True);
            Assert.That(Directory.GetFiles(logDir), Is.Not.Empty, "Log files should be created in the specified directory");
        }
        finally
        {
            Directory.Delete(logDir, recursive: true);
        }
    }
}