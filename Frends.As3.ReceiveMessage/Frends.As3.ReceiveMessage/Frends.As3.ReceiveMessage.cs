using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Frends.As3.ReceiveMessage.Definitions;
using Frends.As3.ReceiveMessage.Helpers;
using nsoftware.async.IPWorksEDI;

namespace Frends.As3.ReceiveMessage;

/// <summary>
/// Task Class for As3 operations.
/// </summary>
public static class As3
{
    /// <summary>
    /// Task to process incoming AS3 messages and generate MDN receipts
    /// [Documentation](https://tasks.frends.com/tasks/frends-tasks/Frends-As3-ReceiveMessage)
    /// </summary>
    /// <param name="input">Essential parameters.</param>
    /// <param name="connection">Connection parameters.</param>
    /// <param name="options">Additional parameters.</param>
    /// <param name="cancellationToken">A cancellation token provided by Frends Platform.</param>
    /// <returns>object { bool Success, string MessageId, string As3From, string As3To, string Payload, string MdnRemotePath, object Error { string Message, Exception AdditionalInfo } }</returns>
    public static async Task<Result> ReceiveMessage(
        [PropertyTab] Input input,
        [PropertyTab] Connection connection,
        [PropertyTab] Options options,
        CancellationToken cancellationToken)
    {
        try
        {
            ValidationHandler.Run(input, connection, options);

            var as3 = NSoftware.Activation.NSoftware.ActivateAs3Receiver();

            as3.RemoteHost = connection.FtpHost;
            as3.RemotePort = connection.FtpPort;
            as3.User = connection.FtpUser;
            as3.Password = connection.FtpPassword;
            as3.Passive = connection.UsePassiveFtp;
            as3.LogDirectory = "logs";

            if (connection.RequireEncrypted || connection.RequireSigned)
            {
                as3.Certificate = new Certificate(
                    CertStoreTypes.cstAuto,
                    connection.OwnCertificatePath,
                    connection.OwnCertificatePassword,
                    "*");
            }

            if (connection.RequireSigned)
            {
                as3.SignerCert = new Certificate(connection.PartnerCertificatePath);
            }

            await as3.Config($"RequireEncrypt={connection.RequireEncrypted}", cancellationToken);
            await as3.Config($"RequireSign={connection.RequireSigned}", cancellationToken);

            await as3.Logon(cancellationToken);
            try
            {
                var messageDirectory = Path.GetDirectoryName(input.RemoteMessagePath)?.Replace("\\", "/");
                var messageFileName = Path.GetFileName(input.RemoteMessagePath);

                if (!string.IsNullOrEmpty(messageDirectory))
                    await as3.ChangeRemotePath(messageDirectory, cancellationToken);

                await as3.ReadRequest(messageFileName, cancellationToken);

                Exception processingError = null;
                try
                {
                    await as3.ProcessRequest(cancellationToken);
                }
                catch (Exception ex)
                {
                    processingError = ex;
                }

                await as3.ChangeRemotePath("/", cancellationToken);

                string mdnRemotePath = null;

                if (as3.MDNReceipt != null)
                {
                    var messageId = string.IsNullOrWhiteSpace(as3.MessageId)
                        ? Guid.NewGuid().ToString("N")
                        : as3.MessageId.Trim('<', '>');

                    var mdnFileName = $"mdn-{messageId}.txt";

                    if (!string.IsNullOrEmpty(input.RemoteMdnPath))
                        await as3.ChangeRemotePath(input.RemoteMdnPath, cancellationToken);

                    await as3.SendResponse(mdnFileName, cancellationToken);

                    mdnRemotePath = string.IsNullOrEmpty(input.RemoteMdnPath)
                        ? $"/{mdnFileName}"
                        : $"{input.RemoteMdnPath.TrimEnd('/')}/{mdnFileName}";
                }

                if (processingError != null)
                {
                    var errorResult = ErrorHandler.Handle(processingError, options);
                    errorResult.MdnRemotePath = mdnRemotePath;
                    errorResult.MessageId = as3.MessageId;
                    errorResult.As3From = as3.AS3From;
                    errorResult.As3To = as3.AS3To;
                    return errorResult;
                }

                if (options.DeleteMessageAfterProcessing)
                {
                    if (!string.IsNullOrEmpty(input.RemoteMdnPath))
                        await as3.ChangeRemotePath("/", cancellationToken);
                    if (!string.IsNullOrEmpty(messageDirectory))
                        await as3.ChangeRemotePath(messageDirectory, cancellationToken);
                    await as3.DeleteFile(messageFileName, cancellationToken);
                }

                return new Result
                {
                    Success = true,
                    As3From = as3.AS3From,
                    As3To = as3.AS3To,
                    MessageId = as3.MessageId,
                    Payload = as3.EDIData?.Data,
                    MdnRemotePath = mdnRemotePath,
                };
            }
            finally
            {
                await as3.Logoff(cancellationToken);
            }
        }
        catch (Exception e)
        {
            return ErrorHandler.Handle(e, options);
        }
    }
}
