using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Frends.As3.SendMessage.Definitions;

namespace Frends.As3.SendMessage.Tests;

public static class TestSetup
{
    public const string FtpSubdirRelative = "subdir";

    private const int FtpControlPort = 21;
    private const int PassivePortCount = 3;
    private const string FtpUser = "testuser";
    private const string FtpPass = "testpass";

    private const string FtpUserHomeAbsolute = "/home/ftpusers/testuser";
    private const string FtpSubdirAbsolute = "/home/ftpusers/testuser/subdir";

    private static readonly string TestFilePath =
        Path.Combine(AppContext.BaseDirectory, "testData", "mess.txt");

    public static async Task<IContainer> StartFtpContainerAsync()
    {
        var passivePortMin = FindFreePortRange(PassivePortCount);
        var passivePortMax = passivePortMin + PassivePortCount - 1;

        var builder = new ContainerBuilder("stilliard/pure-ftpd:latest")
            .WithEnvironment("FTP_USER_NAME", FtpUser)
            .WithEnvironment("FTP_USER_PASS", FtpPass)
            .WithEnvironment("FTP_USER_HOME", FtpUserHomeAbsolute)
            .WithEnvironment("PUBLICHOST", "localhost")
            .WithEnvironment("FTP_PASSIVE_PORTS", $"{passivePortMin}:{passivePortMax}")
            .WithEnvironment("ADDED_FLAGS", "--tls=1")
            .WithEnvironment("TLS_CN", "localhost")
            .WithEnvironment("TLS_ORG", "Frends")
            .WithEnvironment("TLS_C", "FI")
            .WithPortBinding(FtpControlPort, true)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilInternalTcpPortIsAvailable(FtpControlPort));

        for (var port = passivePortMin; port <= passivePortMax; port++)
            builder = builder.WithPortBinding(port, port);

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

    public static Connection Connection(IContainer container) => new()
    {
        FtpHost = "localhost",
        FtpPort = container.GetMappedPublicPort(FtpControlPort),
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
        if (result.ExitCode != 0)
            throw new InvalidOperationException("The FTP file could not be read.");
        return result.Stdout;
    }

    public static async Task<string> GetFtpServerCertificateBase64Async(
        IContainer container)
    {
        const string command =
            "openssl x509 -in /etc/ssl/private/pure-ftpd.pem -outform DER | base64 -w 0";
        var result = await container.ExecAsync(["sh", "-c", command]);

        if (result.ExitCode != 0)
            throw new InvalidOperationException($"The FTP server certificate could not be read: {result.Stderr}");

        return result.Stdout.Trim();
    }

    public static string ConvertDerBase64ToPemBase64(string derBase64)
    {
        using var certificate = new X509Certificate2(Convert.FromBase64String(derBase64));

        return Convert.ToBase64String(Encoding.UTF8.GetBytes(certificate.ExportCertificatePem()));
    }

    public static string GetUntrustedCertificateBase64()
    {
        var certificatePath = Path.Combine(AppContext.BaseDirectory, "certs", "receiver.pem");
        using var certificate = new X509Certificate2(certificatePath);

        return Convert.ToBase64String(certificate.Export(X509ContentType.Cert));
    }

    public static string GetSenderCertificateBase64()
    {
        using var certificate = new X509Certificate2(
            Path.Combine(AppContext.BaseDirectory, "certs", "sender.pfx"),
            "sender123");

        return Convert.ToBase64String(certificate.Export(X509ContentType.Cert));
    }

    private static int FindFreePortRange(int count)
    {
        var random = new Random();
        for (var attempt = 0; attempt < 100; attempt++)
        {
            var start = random.Next(20000, 60000 - count);
            var listeners = new List<TcpListener>();
            try
            {
                for (var port = start; port < start + count; port++)
                {
                    var listener = new TcpListener(IPAddress.Any, port);
                    listener.Start();
                    listeners.Add(listener);
                }

                return start;
            }
            catch (SocketException)
            {
            }
            finally
            {
                foreach (var listener in listeners)
                    listener.Stop();
            }
        }

        throw new InvalidOperationException("No free passive port range found.");
    }
}
