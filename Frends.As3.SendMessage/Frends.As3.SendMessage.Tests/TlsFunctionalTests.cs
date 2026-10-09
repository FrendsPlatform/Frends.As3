using System.Threading;
using System.Threading.Tasks;
using DotNet.Testcontainers.Containers;
using NUnit.Framework;

namespace Frends.As3.SendMessage.Tests;

[TestFixture]
public class TlsFunctionalTests
{
    private IContainer ftpContainer;

    private string ServerCertificateDerBase64 { get; set; }

    private string ServerCertificatePemBase64 { get; set; }

    [OneTimeSetUp]
    public async Task SetUp()
    {
        ftpContainer = await TestSetup.StartFtpContainerAsync();

        ServerCertificateDerBase64 = await TestSetup.GetFtpServerCertificateBase64Async(ftpContainer);
        ServerCertificatePemBase64 = TestSetup.ConvertDerBase64ToPemBase64(ServerCertificateDerBase64);
    }

    [OneTimeTearDown]
    public async Task TearDown()
    {
        await ftpContainer.DisposeAsync();
        Thread.Sleep(5000);
    }

    [Test]
    public async Task ShouldFailWithUntrustedCertificateOverHttpsByDefault()
    {
        var con = TestSetup.Connection(ftpContainer);
        var opt = TestSetup.Options();
        opt.TrustedCertificateBase64 = TestSetup.GetUntrustedCertificateBase64();

        var result = await As3.SendMessage(TestSetup.Input(), con, opt, CancellationToken.None);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Error.Message, Does.Contain("certificate").IgnoreCase);
    }

    [Test]
    public async Task ShouldSendMessageOverHttpsWhenAllowInvalidCertificateIsTrue()
    {
        var con = TestSetup.Connection(ftpContainer);
        var opt = TestSetup.Options();
        opt.AllowInvalidCertificate = true;

        var result = await As3.SendMessage(TestSetup.Input(), con, opt, CancellationToken.None);

        Assert.That(result.Success, Is.True, result.Error?.Message);
    }

    [Test]
    public async Task ShouldSendMessageOverHttpsWhenTrustedCertificateBase64MatchesServerCertificate()
    {
        var con = TestSetup.Connection(ftpContainer);
        var opt = TestSetup.Options();
        opt.TrustedCertificateBase64 = ServerCertificateDerBase64;

        var result = await As3.SendMessage(TestSetup.Input(), con, opt, CancellationToken.None);

        Assert.That(result.Success, Is.True, result.Error?.Message);
    }

    [Test]
    public async Task ShouldSendMessageOverHttpsWhenTrustedCertificateBase64ContainsPemEncodedServerCertificate()
    {
        var con = TestSetup.Connection(ftpContainer);
        var opt = TestSetup.Options();
        opt.TrustedCertificateBase64 = ServerCertificatePemBase64;

        var result = await As3.SendMessage(TestSetup.Input(), con, opt, CancellationToken.None);

        Assert.That(result.Success, Is.True, result.Error?.Message);
    }

    [Test]
    public async Task ShouldFailWhenTrustedCertificateBase64ContainsDifferentValidCertificate()
    {
        var con = TestSetup.Connection(ftpContainer);
        var opt = TestSetup.Options();
        opt.TrustedCertificateBase64 = TestSetup.GetSenderCertificateBase64();

        var result = await As3.SendMessage(TestSetup.Input(), con, opt, CancellationToken.None);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Error.Message, Does.Contain("certificate").IgnoreCase);
    }

    [Test]
    public async Task ShouldFailWithInvalidTrustedCertificateBase64Format()
    {
        var con = TestSetup.Connection(ftpContainer);
        var opt = TestSetup.Options();
        opt.TrustedCertificateBase64 = "not-a-valid-base64-certificate!!";

        var result = await As3.SendMessage(TestSetup.Input(), con, opt, CancellationToken.None);

        Assert.That(result.Success, Is.False);
        Assert.That(
            result.Error.Message,
            Does.Contain("TrustedCertificateBase64 is not a valid base64-encoded string."));
    }
}
