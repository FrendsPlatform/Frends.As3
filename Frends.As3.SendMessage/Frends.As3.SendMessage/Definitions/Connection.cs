using Frends.As3.SendMessage.Attributes;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.As3.SendMessage.Definitions;

/// <summary>
/// Connection parameters.
/// </summary>
public class Connection
{
    /// <summary>
    /// FTP server host name or IP address for the AS3 connection.
    /// </summary>
    /// <example>ftp.example.com</example>
    [DisplayFormat(DataFormatString = "Text")]
    public string FtpHost { get; set; }

    /// <summary>
    /// FTP server port.
    /// </summary>
    /// <example>21</example>
    [DefaultValue(21)]
    public int FtpPort { get; set; } = 21;

    /// <summary>
    /// Username for FTP authentication.
    /// </summary>
    /// <example>myFtpUser</example>
    [DisplayFormat(DataFormatString = "Text")]
    public string FtpUser { get; set; }

    /// <summary>
    /// Password for FTP authentication.
    /// </summary>
    /// <example>myFtpPassword123</example>
    [DisplayFormat(DataFormatString = "Text")]
    [PasswordPropertyText]
    public string FtpPassword { get; set; }

    /// <summary>
    /// Defines whether to use Passive mode for the FTP data connection.
    /// </summary>
    /// <example>true</example>
    [DefaultValue(true)]
    public bool UsePassiveFtp { get; set; } = true;

    /// <summary>
    /// Target directory path on the remote FTP server where the message will be uploaded.
    /// The connection will change the working directory to this path before sending the file.
    /// </summary>
    /// <example>/inbox/messages</example>
    [DisplayFormat(DataFormatString = "Text")]
    public string RemoteFilePath { get; set; }

    /// <summary>
    /// Defines whether to sign the message.
    /// </summary>
    /// <example>true</example>
    [DefaultValue(false)]
    public bool SignMessage { get; set; }

    /// <summary>
    /// Defines whether to encrypt the message.
    /// </summary>
    /// <example>true</example>
    [DefaultValue(false)]
    public bool EncryptMessage { get; set; }

    /// <summary>
    /// Password for the sender certificate.
    /// </summary>
    /// <example>mySecurePassword123</example>
    [DisplayFormat(DataFormatString = "Text")]
    [PasswordPropertyText]
    [RequiredIf(nameof(SignMessage), true)]
    [UIHint(nameof(SignMessage), "", true)]
    public string SenderCertificatePassword { get; set; }

    /// <summary>
    /// Path to the sender certificate file in .pfx format.
    /// </summary>
    /// <example>C:\Document\sender_cert.pfx</example>
    [DisplayFormat(DataFormatString = "Text")]
    [UIHint(nameof(SignMessage), "", true)]
    [RequiredIf(nameof(SignMessage), true)]
    public string SenderCertificatePath { get; set; }

    /// <summary>
    /// Path to the receiver certificate file in .pfx format.
    /// </summary>
    /// <example>C:\Document\receiver_cert.pfx</example>
    [DisplayFormat(DataFormatString = "Text")]
    [UIHint(nameof(EncryptMessage), "", true)]
    [RequiredIf(nameof(EncryptMessage), true)]
    public string ReceiverCertificatePath { get; set; }

    /// <summary>
    /// Email where to send the MDN (Message Disposition Notification).
    /// </summary>
    /// <example>user@example.com</example>
    [DisplayFormat(DataFormatString = "Text")]
    public string MdnReceiver { get; set; }

    /// <summary>
    /// Specify content type header for the message if it's neither encrypted nor signed.
    /// </summary>
    /// <example>application/zip</example>
    [DisplayFormat(DataFormatString = "Text")]
    [DefaultValue("text/plain")]
    public string ContentTypeHeader { get; set; } = "text/plain";
}
