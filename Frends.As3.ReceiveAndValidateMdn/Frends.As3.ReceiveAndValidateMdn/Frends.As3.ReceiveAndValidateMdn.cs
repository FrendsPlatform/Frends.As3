using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Frends.As3.ReceiveAndValidateMdn.Definitions;
using Frends.As3.ReceiveAndValidateMdn.Helpers;
using nsoftware.async.IPWorksEDI;

namespace Frends.As3.ReceiveAndValidateMdn;

/// <summary>
/// Task Class for As3 operations.
/// </summary>
public static class As3
{
    /// <summary>
    /// Task to receive and validate MDN receipts for sent AS3 messages
    /// [Documentation](https://tasks.frends.com/tasks/frends-tasks/Frends-As3-ReceiveAndValidateMdn)
    /// </summary>
    /// <param name="input">Essential parameters.</param>
    /// <param name="connection">Connection parameters.</param>
    /// <param name="options">Additional parameters.</param>
    /// <param name="cancellationToken">A cancellation token provided by Frends Platform.</param>
    /// <returns>object { bool Success, string OriginalMessageId, string Disposition, bool IsProcessed, object Error { string Message, Exception AdditionalInfo } }</returns>
    public static async Task<Result> ReceiveAndValidateMdn(
     [PropertyTab] Input input,
     [PropertyTab] Connection connection,
     [PropertyTab] Options options,
     CancellationToken cancellationToken)
    {
        try
        {
            ValidationHandler.Run(input, connection, options);

            var as3 = NSoftware.Activation.NSoftware.ActivateAs3Sender();

            as3.RemoteHost = connection.FtpHost;
            as3.RemotePort = connection.FtpPort;
            as3.User = connection.FtpUser;
            as3.Password = connection.FtpPassword;
            as3.Passive = connection.UsePassiveFtp;
            as3.LogDirectory = "logs";

            as3.OriginalContentMIC = input.OriginalContentMIC;
            as3.MDNOptions = input.MdnOptions ?? string.Empty;

            if (!string.IsNullOrEmpty(connection.PartnerCertificatePath))
                as3.ReceiptSignerCert = new Certificate(connection.PartnerCertificatePath);

            await as3.Logon(cancellationToken);

            var mdnDirectory = Path.GetDirectoryName(input.RemoteMdnPath);
            var mdnFileName = Path.GetFileName(input.RemoteMdnPath);

            if (!string.IsNullOrEmpty(mdnDirectory))
                await as3.ChangeRemotePath(mdnDirectory, cancellationToken);

            await as3.ReadReceipt(mdnFileName, cancellationToken);

            await as3.VerifyReceipt(cancellationToken);

            var disposition = as3.MDNReceipt.MDN
                .Split("\r\n")
                .FirstOrDefault(l => l.StartsWith("Disposition:", StringComparison.OrdinalIgnoreCase))
                ?.Trim();

            var originalMessageId = as3.MDNReceipt.MDN
                .Split("\r\n")
                .FirstOrDefault(l => l.StartsWith("Original-Message-ID:", StringComparison.OrdinalIgnoreCase))
                ?.Replace("Original-Message-ID:", string.Empty)
                .Trim();

            if (options.DeleteMdnAfterVerification)
                await as3.DeleteFile(mdnFileName, cancellationToken);

            await as3.Logoff(cancellationToken);

            return new Result
            {
                Success = true,
                OriginalMessageId = originalMessageId,
                Disposition = disposition,
                IsProcessed = ParseIsProcessed(disposition),
            };
        }
        catch (Exception e)
        {
            return ErrorHandler.Handle(e, options);
        }
    }

    private static bool ParseIsProcessed(string disposition)
    {
        if (string.IsNullOrWhiteSpace(disposition))
            return false;

        var semicolonIndex = disposition.IndexOf(';');
        if (semicolonIndex < 0)
            return false;

        var afterSemicolon = disposition[(semicolonIndex + 1)..].Trim();
        var dispositionType = afterSemicolon.Split('/')[0].Trim();

        if (!dispositionType.Equals("processed", StringComparison.OrdinalIgnoreCase))
            return false;

        var hasError = afterSemicolon.Contains("/error", StringComparison.OrdinalIgnoreCase);
        return !hasError;
    }
}
