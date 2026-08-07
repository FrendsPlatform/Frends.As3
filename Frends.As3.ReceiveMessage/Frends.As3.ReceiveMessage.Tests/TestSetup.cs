using System;
using System.IO;
using System.Threading.Tasks;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Frends.As3.ReceiveMessage.Definitions;

namespace Frends.As3.ReceiveMessage.Tests;

public static class TestSetup
{
    private const int FtpControlPort = 21;
    private const int PassivePortMin = 30000;
    private const int PassivePortMax = 30009;
    private const string FtpUser = "testuser";
    private const string FtpPass = "testpass";

    private const string FtpUserHomeAbsolute = "/home/ftpusers/testuser";
    private const string InboxAbsolute = "/home/ftpusers/testuser/inbox";
    private const string MdnAbsolute = "/home/ftpusers/testuser/mdn";

    private static readonly string TestDataDir =
        Path.Combine(AppContext.BaseDirectory, "testData");

    public static async Task<IContainer> StartFtpContainerAsync()
    {
        var builder = new ContainerBuilder("stilliard/pure-ftpd:latest")
            .WithEnvironment("FTP_USER_NAME", FtpUser)
            .WithEnvironment("FTP_USER_PASS", FtpPass)
            .WithEnvironment("FTP_USER_HOME", FtpUserHomeAbsolute)
            .WithEnvironment("PUBLICHOST", "localhost")
            .WithEnvironment("FTP_PASSIVE_PORTS", $"{PassivePortMin}:{PassivePortMax}")
            .WithPortBinding(FtpControlPort, FtpControlPort)
            .WithPortBinding(PassivePortMin, PassivePortMin)
            .WithPortBinding(PassivePortMin + 1, PassivePortMin + 1)
            .WithPortBinding(PassivePortMin + 2, PassivePortMin + 2)
            .WithPortBinding(PassivePortMin + 3, PassivePortMin + 3)
            .WithPortBinding(PassivePortMin + 4, PassivePortMin + 4)
            .WithPortBinding(PassivePortMin + 5, PassivePortMin + 5)
            .WithPortBinding(PassivePortMin + 6, PassivePortMin + 6)
            .WithPortBinding(PassivePortMin + 7, PassivePortMin + 7)
            .WithPortBinding(PassivePortMin + 8, PassivePortMin + 8)
            .WithPortBinding(PassivePortMin + 9, PassivePortMin + 9)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilInternalTcpPortIsAvailable(FtpControlPort));

        var container = builder.Build();
        await container.StartAsync();

        await container.ExecAsync(new[] { "mkdir", "-p", InboxAbsolute });
        await container.ExecAsync(new[] { "mkdir", "-p", MdnAbsolute });
        await container.ExecAsync(new[] { "chmod", "-R", "777", FtpUserHomeAbsolute });

        return container;
    }

    public static async Task UploadTestFileToContainer(IContainer container, string localFileName, string remoteFileName = null)
    {
        var localPath = Path.Combine(TestDataDir, localFileName);
        var targetFileName = remoteFileName ?? localFileName;
        var fileBytes = await File.ReadAllBytesAsync(localPath);
        await container.CopyAsync(fileBytes, $"{InboxAbsolute}/{targetFileName}");
    }

    public static Input Input(string messageFileName) => new()
    {
        RemoteMessagePath = $"inbox/{messageFileName}",
        RemoteMdnPath = "mdn",
    };

    public static Connection Connection(bool requireSigned = false, bool requireEncrypted = false) => new()
    {
        FtpHost = "localhost",
        FtpPort = FtpControlPort,
        FtpUser = FtpUser,
        FtpPassword = FtpPass,
        UsePassiveFtp = true,
        RequireSigned = requireSigned,
        RequireEncrypted = requireEncrypted,
        OwnCertificatePath = Path.Combine(AppContext.BaseDirectory, "certs", "receiver.pfx"),
        OwnCertificatePassword = "receiver123",
        PartnerCertificatePath = Path.Combine(AppContext.BaseDirectory, "certs", "sender.pem"),
    };

    public static Options Options() => new()
    {
        ThrowErrorOnFailure = false,
        ErrorMessageOnFailure = null,
        DeleteMessageAfterProcessing = false,
    };

    public static async Task<bool> FileExistsOnFtp(IContainer container, string absolutePath)
    {
        var result = await container.ExecAsync(new[] { "ls", absolutePath });
        return result.ExitCode == 0;
    }

    public static string AbsoluteInboxPath(string fileName)
        => $"{InboxAbsolute}/{fileName}";

    public static string AbsoluteMdnPath(string fileName)
        => $"{MdnAbsolute}/{fileName}";

    public static async Task<string> ReadFileFromContainer(IContainer container, string absolutePath)
    {
        var result = await container.ExecAsync(new[] { "cat", absolutePath });
        return result.Stdout;
    }
}