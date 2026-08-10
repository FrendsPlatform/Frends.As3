using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Frends.As3.SendMessage.Definitions;
using Frends.As3.SendMessage.Helpers;
using nsoftware.async.IPWorksEDI;

namespace Frends.As3.SendMessage;

/// <summary>
/// Task Class for As3 operations.
/// </summary>
public static class As3
{
    /// <summary>
    /// Task to send messages with AS3 protocol
    /// [Documentation](https://tasks.frends.com/tasks/frends-tasks/Frends-As3-SendMessage)
    /// </summary>
    /// <param name="input">Essential parameters.</param>
    /// <param name="connection">Connection parameters.</param>
    /// <param name="options">Additional parameters.</param>
    /// <param name="cancellationToken">A cancellation token provided by Frends Platform.</param>
    /// <returns>object { bool Success, string PartnerResponse, string MessageId, string OriginalContentMIC, string MdnOptions, object Error { string Message, Exception AdditionalInfo } }</returns>
    public static async Task<Result> SendMessage(
    [PropertyTab] Input input,
    [PropertyTab] Connection connection,
    [PropertyTab] Options options,
    CancellationToken cancellationToken)
    {
        try
        {
            ValidationHandler.Run(input, connection, options);

            var as3 = NSoftware.Activation.NSoftware.ActivateAs3Sender();

            as3.AS3From = input.SenderAs3Id;
            as3.AS3To = input.ReceiverAs3Id;
            as3.Subject = input.Subject;

            as3.RemoteHost = connection.FtpHost;
            as3.RemotePort = connection.FtpPort;
            as3.User = connection.FtpUser;
            as3.Password = connection.FtpPassword;
            as3.Passive = connection.UsePassiveFtp;

            as3.MessageId = $"<{Guid.NewGuid()}@{connection.FtpHost}>";

            as3.MDNTo = connection.MdnReceiver;

            if (connection.EncryptMessage)
            {
                as3.RecipientCerts.Add(new Certificate(connection.ReceiverCertificatePath));
            }

            if (connection.SignMessage)
            {
                var password = connection.SenderCertificatePassword;
                as3.SigningCert =
                    new Certificate(CertStoreTypes.cstAuto, connection.SenderCertificatePath, password, "*");
            }
            else
            {
                as3.MDNOptions = string.Empty;
            }

            if (!connection.EncryptMessage)
            {
                as3.EncryptionAlgorithm = string.Empty;
            }

            as3.EDIData = new EDIData();
            as3.EDIData.EDIType = connection.ContentTypeHeader;
            as3.EDIData.Data = await File.ReadAllTextAsync(input.MessageFilePath, cancellationToken);

            as3.LogDirectory = "logs";

            var fileName = Path.GetFileName(input.MessageFilePath);
            await as3.Logon(cancellationToken);

            if (!string.IsNullOrEmpty(connection.RemoteFilePath))
                await as3.ChangeRemotePath(connection.RemoteFilePath, cancellationToken);

            await as3.Send(fileName, cancellationToken);

            await as3.Logoff(cancellationToken);

            var result = new Result
            {
                Success = true,
                MessageId = as3.MessageId,
                OriginalContentMIC = as3.OriginalContentMIC.Trim(),
                MdnOptions = as3.MDNOptions,
                PartnerResponse = $"File '{fileName}' successfully uploaded to FTP remote directory '{connection.RemoteFilePath ?? "/"}'.",
            };

            return result;
        }
        catch (Exception e)
        {
            return ErrorHandler.Handle(e, options);
        }
    }
}
