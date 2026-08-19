using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DotNet.Testcontainers.Containers;
using NUnit.Framework;

namespace Frends.As3.ReceiveAndValidateMdn.Tests;

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
        await TestSetup.UploadMdnFileAsync(ftpContainer, TestSetup.ValidMdnFileName);
        await TestSetup.UploadMdnFileAsync(ftpContainer, TestSetup.FailedMdnFileName);
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        await ftpContainer.DisposeAsync();
    }

    [Test]
    public async Task ShouldVerifyValidMdn()
    {
        var result = await As3.ReceiveAndValidateMdn(
            TestSetup.Input(),
            TestSetup.Connection(),
            TestSetup.Options(),
            CancellationToken.None);

        Assert.That(result.Success, Is.True);
        Assert.That(result.IsProcessed, Is.True);
        Assert.That(result.Disposition, Does.Contain("processed"));
        Assert.That(result.OriginalMessageId, Is.Not.Null.And.Not.Empty);
    }

    [Test]
    public async Task ShouldFailForInvalidMic()
    {
        var input = TestSetup.Input();
        input.OriginalContentMic = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA==, sha256";

        var opt = TestSetup.Options();
        opt.ThrowErrorOnFailure = false;

        var result = await As3.ReceiveAndValidateMdn(
            input, TestSetup.Connection(), opt, CancellationToken.None);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Error, Is.Not.Null);
    }

    [Test]
    public async Task ShouldDeleteMdnAfterVerification()
    {
        var opt = TestSetup.Options();
        opt.DeleteMdnAfterVerification = true;

        var result = await As3.ReceiveAndValidateMdn(
            TestSetup.Input(),
            TestSetup.Connection(),
            opt,
            CancellationToken.None);

        Assert.That(result.Success, Is.True);
        Assert.That(
            await TestSetup.FileExistsOnFtp(
                ftpContainer,
                TestSetup.AbsoluteMdnPath(TestSetup.ValidMdnFileName)),
            Is.False);
    }

    [Test]
    public async Task ShouldFailWithInvalidFtpHost()
    {
        var con = TestSetup.Connection();
        con.FtpHost = "invalid-host-that-does-not-exist";

        var opt = TestSetup.Options();
        opt.ThrowErrorOnFailure = false;

        var result = await As3.ReceiveAndValidateMdn(
            TestSetup.Input(), con, opt, CancellationToken.None);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Error, Is.Not.Null);
    }

    [Test]
    public async Task ShouldFailWhenRemoteMdnPathIsMissing()
    {
        var input = TestSetup.Input();
        input.RemoteMdnPath = null;

        var opt = TestSetup.Options();
        opt.ThrowErrorOnFailure = false;

        var result = await As3.ReceiveAndValidateMdn(
            input, TestSetup.Connection(), opt, CancellationToken.None);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Error.Message, Does.Contain("RemoteMdnPath field is required"));
    }

    [Test]
    public async Task ShouldFailWhenOriginalContentMicIsMissing()
    {
        var input = TestSetup.Input();
        input.OriginalContentMic = null;

        var opt = TestSetup.Options();
        opt.ThrowErrorOnFailure = false;

        var result = await As3.ReceiveAndValidateMdn(
            input, TestSetup.Connection(), opt, CancellationToken.None);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Error.Message, Does.Contain("OriginalContentMIC field is required"));
    }

    [Test]
    public async Task ShouldFailWithWrongPartnerCertificate()
    {
        var con = TestSetup.Connection();
        con.PartnerCertificatePath = Path.Combine(
            AppContext.BaseDirectory, "certs", "wrong.pem");

        var opt = TestSetup.Options();
        opt.ThrowErrorOnFailure = false;

        var result = await As3.ReceiveAndValidateMdn(
            TestSetup.Input(), con, opt, CancellationToken.None);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Error, Is.Not.Null);
        Assert.That(result.Error.Message, Does.Contain("The certificate was not found in the specified store"));
    }

    [Test]
    public async Task ShouldFailWhenMdnHasNoMicDueToProcessingError()
    {
        var input = TestSetup.Input(TestSetup.FailedMdnFileName);
        input.MdnOptions = null;

        var con = TestSetup.Connection();
        con.PartnerCertificatePath = null;

        var result = await As3.ReceiveAndValidateMdn(
            input, con, TestSetup.Options(), CancellationToken.None);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Error.Message, Does.Contain("MDN Error"));
    }
}