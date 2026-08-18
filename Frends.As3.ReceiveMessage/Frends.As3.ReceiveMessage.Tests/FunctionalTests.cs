using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DotNet.Testcontainers.Containers;
using NUnit.Framework;

namespace Frends.As3.ReceiveMessage.Tests;

[TestFixture]
public class FunctionalTests
{
    private IContainer ftpContainer;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        ftpContainer = await TestSetup.StartFtpContainerAsync();
    }

    [SetUp]
    public async Task SetUp()
    {
        await TestSetup.UploadTestFileToContainer(ftpContainer, "plain.txt");
        await TestSetup.UploadTestFileToContainer(ftpContainer, "signed_only.txt");
        await TestSetup.UploadTestFileToContainer(ftpContainer, "encrypted_only.txt");
        await TestSetup.UploadTestFileToContainer(ftpContainer, "signed_encrypted.txt");
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        await ftpContainer.DisposeAsync();
    }

    [Test]
    public async Task ShouldReceivePlainMessage()
    {
        var result = await As3.ReceiveMessage(
            TestSetup.Input("plain.txt"),
            TestSetup.Connection(requireSigned: false, requireEncrypted: false),
            TestSetup.Options(),
            CancellationToken.None);

        Assert.That(result.Success, Is.True, result.Error?.Message);
        Assert.That(result.Payload, Is.Not.Null.And.Not.Empty);
        Assert.That(result.MessageId, Is.Not.Null.And.Not.Empty);
        Assert.That(result.As3From, Is.Not.Null.And.Not.Empty);
        Assert.That(result.As3To, Is.Not.Null.And.Not.Empty);
        Assert.That(result.MdnRemotePath, Is.Not.Null.And.Not.Empty);
    }

    [Test]
    public async Task ShouldReceiveSignedMessage()
    {
        var result = await As3.ReceiveMessage(
            TestSetup.Input("signed_only.txt"),
            TestSetup.Connection(requireSigned: true, requireEncrypted: false),
            TestSetup.Options(),
            CancellationToken.None);

        Assert.That(result.Success, Is.True);
        Assert.That(result.Payload, Is.Not.Null.And.Not.Empty);
        Assert.That(result.MessageId, Is.Not.Null.And.Not.Empty);
    }

    [Test]
    public async Task ShouldReceiveEncryptedMessage()
    {
        var result = await As3.ReceiveMessage(
            TestSetup.Input("encrypted_only.txt"),
            TestSetup.Connection(requireSigned: false, requireEncrypted: true),
            TestSetup.Options(),
            CancellationToken.None);

        Assert.That(result.Success, Is.True);
        Assert.That(result.Payload, Is.Not.Null.And.Not.Empty);
        Assert.That(result.MessageId, Is.Not.Null.And.Not.Empty);
    }

    [Test]
    public async Task ShouldReceiveSignedAndEncryptedMessage()
    {
        var result = await As3.ReceiveMessage(
            TestSetup.Input("signed_encrypted.txt"),
            TestSetup.Connection(requireSigned: true, requireEncrypted: true),
            TestSetup.Options(),
            CancellationToken.None);

        Assert.That(result.Success, Is.True);
        Assert.That(result.Payload, Is.Not.Null.And.Not.Empty);
        Assert.That(result.MessageId, Is.Not.Null.And.Not.Empty);
        Assert.That(result.As3From, Is.EqualTo("Sender"));
        Assert.That(result.As3To, Is.EqualTo("Receiver"));
    }

    [Test]
    public async Task ShouldFailWhenSignerCertIsWrong()
    {
        var con = TestSetup.Connection(requireSigned: true);
        con.PartnerCertificatePath = Path.Combine(
            AppContext.BaseDirectory, "certs", "wrong.pem");

        var opt = TestSetup.Options();
        opt.ThrowErrorOnFailure = false;

        var result = await As3.ReceiveMessage(
            TestSetup.Input("signed_only.txt"), con, opt, CancellationToken.None);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Error, Is.Not.Null);
        Assert.That(result.Error.Message, Does.Contain("Unable to authenticate signature"));
    }

    [Test]
    public async Task ShouldGenerateMdn()
    {
        var result = await As3.ReceiveMessage(
            TestSetup.Input("plain.txt"),
            TestSetup.Connection(requireSigned: false, requireEncrypted: false),
            TestSetup.Options(),
            CancellationToken.None);

        Assert.That(result.Success, Is.True);
        Assert.That(result.MdnRemotePath, Is.Not.Null.And.Not.Empty);

        var mdnFileName = Path.GetFileName(result.MdnRemotePath);
        Assert.That(await TestSetup.FileExistsOnFtp(ftpContainer, TestSetup.AbsoluteMdnPath(mdnFileName)), Is.True);
        var mdnContent = await TestSetup.ReadFileFromContainer(ftpContainer, TestSetup.AbsoluteMdnPath(mdnFileName));
        Assert.That(mdnContent, Does.Contain("was received successfully"));
    }

    [Test]
    public async Task ShouldFailWithInvalidCredentials()
    {
        var con = TestSetup.Connection();
        con.FtpUser = "wronguser";
        con.FtpPassword = "wrongpass";

        var result = await As3.ReceiveMessage(
            TestSetup.Input("plain.txt"),
            con,
            TestSetup.Options(),
            CancellationToken.None);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Error, Is.Not.Null);
    }

    [Test]
    public async Task ShouldDeleteMessageAfterProcessing()
    {
        await TestSetup.UploadTestFileToContainer(ftpContainer, "plain.txt", "to_delete.txt");

        var opt = TestSetup.Options();
        opt.DeleteMessageAfterProcessing = true;

        var result = await As3.ReceiveMessage(
            TestSetup.Input("to_delete.txt"),
            TestSetup.Connection(requireSigned: false, requireEncrypted: false),
            opt,
            CancellationToken.None);

        Assert.That(result.Success, Is.True);
        Assert.That(await TestSetup.FileExistsOnFtp(ftpContainer, TestSetup.AbsoluteInboxPath("to_delete.txt")), Is.False);
    }

    [Test]
    public async Task ShouldGenerateNegativeMdn_WhenSignatureValidationFails()
    {
        var result = await As3.ReceiveMessage(
            TestSetup.Input("plain.txt"),
            TestSetup.Connection(requireSigned: true, requireEncrypted: false),
            TestSetup.Options(),
            CancellationToken.None);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Error, Is.Not.Null);

        Assert.That(result.MdnRemotePath, Is.Not.Null.And.Not.Empty);

        var mdnFileName = Path.GetFileName(result.MdnRemotePath);
        Assert.That(await TestSetup.FileExistsOnFtp(ftpContainer, TestSetup.AbsoluteMdnPath(mdnFileName)), Is.True);
        var mdnContent = await TestSetup.ReadFileFromContainer(ftpContainer, TestSetup.AbsoluteMdnPath(mdnFileName));
        Assert.That(mdnContent, Does.Contain("unexpected-processing-error"));
    }

    [Test]
    public async Task ShouldGenerateNegativeMdn_WhenDecryptionFails()
    {
        var result = await As3.ReceiveMessage(
            TestSetup.Input("plain.txt"),
            TestSetup.Connection(requireSigned: false, requireEncrypted: true),
            TestSetup.Options(),
            CancellationToken.None);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Error, Is.Not.Null);
        Assert.That(result.MdnRemotePath, Is.Not.Null.And.Not.Empty);
        var mdnFileName = Path.GetFileName(result.MdnRemotePath);
        Assert.That(await TestSetup.FileExistsOnFtp(ftpContainer, TestSetup.AbsoluteMdnPath(mdnFileName)), Is.True);
        var mdnContent = await TestSetup.ReadFileFromContainer(ftpContainer, TestSetup.AbsoluteMdnPath(mdnFileName));
        Assert.That(mdnContent, Does.Contain("unexpected-processing-error"));
    }

    [Test]
    public async Task ShouldFailWhenOnlyRequireSignedTrueAndOwnCertificatePathMissing()
    {
        var con = TestSetup.Connection(requireSigned: true, requireEncrypted: false);
        con.OwnCertificatePath = null;
        con.OwnCertificatePassword = null;

        var opt = TestSetup.Options();
        opt.ThrowErrorOnFailure = false;

        var result = await As3.ReceiveMessage(
            TestSetup.Input("plain.txt"), con, opt, CancellationToken.None);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Error.Message, Does.Contain("OwnCertificatePath is required"));
    }

    [Test]
    public async Task ShouldFailWhenOnlyRequireEncryptedTrueAndOwnCertificatePathMissing()
    {
        var con = TestSetup.Connection(requireSigned: false, requireEncrypted: true);
        con.OwnCertificatePath = null;
        con.OwnCertificatePassword = null;

        var opt = TestSetup.Options();
        opt.ThrowErrorOnFailure = false;

        var result = await As3.ReceiveMessage(
            TestSetup.Input("plain.txt"), con, opt, CancellationToken.None);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Error.Message, Does.Contain("OwnCertificatePath is required"));
    }

    [Test]
    public async Task DiagnosticCheckFiles()
    {
        var check = await ftpContainer.ExecAsync(new[] { "ls", "-la", "/home/ftpusers/testuser/inbox/" });
        TestContext.WriteLine("Directory listing:");
        TestContext.WriteLine(check.Stdout);

        var content = await ftpContainer.ExecAsync(new[] { "od", "-c", "/home/ftpusers/testuser/inbox/plain.txt" });
        TestContext.WriteLine("plain.txt content:");
        TestContext.WriteLine(content.Stdout?[..Math.Min(500, content.Stdout.Length)] ?? "NULL");
    }
}
