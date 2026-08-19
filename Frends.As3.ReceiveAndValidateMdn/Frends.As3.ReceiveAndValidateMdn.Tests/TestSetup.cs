using System;
using System.IO;
using System.Threading.Tasks;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Frends.As3.ReceiveAndValidateMdn.Definitions;

namespace Frends.As3.ReceiveAndValidateMdn.Tests
{
    public static class TestSetup
    {
        public const string ValidOriginalContentMIC = "dJiEB7oIV60bG1GIi5sEDCpAjbh+uOOq+MOxACs4eFQ=, sha-256";
        public const string ValidMdnOptions = "signed-receipt-protocol=optional, pkcs7-signature; signed-receipt-micalg=optional, sha-256";
        public const string ValidMdnFileName = "valid_mdn.txt";
        public const string InvalidMdnFileName = "invalid_mdn.txt";
        public const string FailedMdnFileName = "failed_mdn.txt";

        private const int FtpControlPort = 21;
        private const int PassivePortMin = 30000;
        private const int PassivePortMax = 30009;
        private const string FtpUser = "testuser";
        private const string FtpPass = "testpass";
        private const string FtpUserHomeAbsolute = "/home/ftpusers/testuser";
        private const string MdnAbsolute = "/home/ftpusers/testuser/mdn";

        public static async Task<IContainer> StartFtpContainerAsync()
        {
            var container = new ContainerBuilder("stilliard/pure-ftpd:latest")
                .WithEnvironment("FTP_USER_NAME", FtpUser)
                .WithEnvironment("FTP_USER_PASS", FtpPass)
                .WithEnvironment("FTP_USER_HOME", FtpUserHomeAbsolute)
                .WithEnvironment("PUBLICHOST", "localhost")
                .WithEnvironment("FTP_PASSIVE_PORTS", $"{PassivePortMin}:{PassivePortMax}")
                .WithPortBinding(FtpControlPort, FtpControlPort)
                .WithPortBinding(PassivePortMin + 0, PassivePortMin + 0)
                .WithPortBinding(PassivePortMin + 1, PassivePortMin + 1)
                .WithPortBinding(PassivePortMin + 2, PassivePortMin + 2)
                .WithPortBinding(PassivePortMin + 3, PassivePortMin + 3)
                .WithPortBinding(PassivePortMin + 4, PassivePortMin + 4)
                .WithPortBinding(PassivePortMin + 5, PassivePortMin + 5)
                .WithPortBinding(PassivePortMin + 6, PassivePortMin + 6)
                .WithPortBinding(PassivePortMin + 7, PassivePortMin + 7)
                .WithPortBinding(PassivePortMin + 8, PassivePortMin + 8)
                .WithPortBinding(PassivePortMin + 9, PassivePortMin + 9)
                .WithWaitStrategy(Wait.ForUnixContainer()
                    .UntilInternalTcpPortIsAvailable(FtpControlPort))
                .Build();

            await container.StartAsync();
            await container.ExecAsync(new[] { "mkdir", "-p", MdnAbsolute });
            await container.ExecAsync(new[] { "chmod", "-R", "777", FtpUserHomeAbsolute });

            return container;
        }

        public static Input Input(string mdnFileName = ValidMdnFileName) => new()
        {
            RemoteMdnPath = $"mdn/{mdnFileName}",
            OriginalContentMic = ValidOriginalContentMIC,
            MdnOptions = ValidMdnOptions,
        };

        public static Connection Connection() => new()
        {
            FtpHost = "localhost",
            FtpPort = FtpControlPort,
            FtpUser = FtpUser,
            FtpPassword = FtpPass,
            UsePassiveFtp = true,
            PartnerCertificatePath = Path.Combine(AppContext.BaseDirectory, "certs", "receiver.pem"),
        };

        public static Options Options() => new()
        {
            ThrowErrorOnFailure = false,
            ErrorMessageOnFailure = null,
            DeleteMdnAfterVerification = false,
        };

        public static async Task<bool> FileExistsOnFtp(IContainer container, string absolutePath)
        {
            var result = await container.ExecAsync(new[] { "ls", absolutePath });
            return result.ExitCode == 0;
        }

        public static string AbsoluteMdnPath(string fileName)
            => $"{MdnAbsolute}/{fileName}";

        public static async Task UploadMdnFileAsync(IContainer container, string fileName)
        {
            var localPath = Path.Combine(AppContext.BaseDirectory, "testData", fileName);
            var content = await File.ReadAllTextAsync(localPath);
            await container.ExecAsync(new[]
            {
                "sh", "-c",
                $"cat > {MdnAbsolute}/{fileName} << 'HEREDOC'\n{content}\nHEREDOC",
            });
        }
    }
}
