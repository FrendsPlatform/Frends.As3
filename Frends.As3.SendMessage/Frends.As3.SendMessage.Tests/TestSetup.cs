using System;
using System.IO;
using System.Threading.Tasks;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Frends.As3.SendMessage.Definitions;

namespace Frends.As3.SendMessage.Tests;

public static class TestSetup
{
    public const string FtpSubdirRelative = "subdir";

    private const int FtpControlPort = 21;
    private const int PassivePortMin = 30000;
    private const int PassivePortMax = 30009;
    private const string FtpUser = "testuser";
    private const string FtpPass = "testpass";

    private const string FtpUserHomeAbsolute = "/home/ftpusers/testuser";
    private const string FtpSubdirAbsolute = "/home/ftpusers/testuser/subdir";

    private static readonly string TestFilePath =
        Path.Combine(AppContext.BaseDirectory, "testData", "mess.txt");

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

        await container.ExecAsync(new[] { "mkdir", "-p", FtpSubdirAbsolute });
        await container.ExecAsync(new[] { "chmod", "-R", "777", FtpUserHomeAbsolute });

        return container;
    }

    public static Input Input() => new()
    {
        SenderAs3Id = "Sender",
        ReceiverAs3Id = "Receiver",
        Subject = "Test AS3 Connection",
        MessageFilePath = TestFilePath,
    };

    public static Connection Connection() => new()
    {
        FtpHost = "localhost",
        FtpPort = FtpControlPort,
        FtpUser = FtpUser,
        FtpPassword = FtpPass,
        UsePassiveFtp = true,
        RemoteFilePath = null,
        SignMessage = false,
        EncryptMessage = false,
        SenderCertificatePassword = "sender123",
        SenderCertificatePath = Path.Combine(AppContext.BaseDirectory, "certs", "sender.pfx"),
        ReceiverCertificatePath = Path.Combine(AppContext.BaseDirectory, "certs", "receiver.pem"),
        ContentTypeHeader = "text/plain",
        MdnReceiver = "usr@example.com",
    };

    public static Options Options() => new()
    {
        ThrowErrorOnFailure = false,
        ErrorMessageOnFailure = null,
    };

    public static async Task<bool> FileExistsOnFtp(IContainer container, string remotePath)
    {
        var result = await container.ExecAsync(new[] { "ls", remotePath });
        return result.ExitCode == 0;
    }

    public static string AbsoluteFtpPath(string fileName)
        => $"{FtpUserHomeAbsolute}/{fileName}";

    public static string AbsoluteFtpSubdirPath(string fileName)
        => $"{FtpSubdirAbsolute}/{fileName}";

    public static async Task<string> ReadFileFromFtp(IContainer container, string remotePath)
    {
        var result = await container.ExecAsync(new[] { "cat", remotePath });
        return result.Stdout;
    }
}