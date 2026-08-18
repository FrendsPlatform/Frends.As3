using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Frends.As3.ReceiveMessage.Attributes;

namespace Frends.As3.ReceiveMessage.Definitions;

/// <summary>
/// Connection parameters.
/// </summary>
public class Connection
{
    /// <summary>
    /// FTP server host name or IP address for the AS3 connection.
    /// </summary>
    /// <example>ftp.example.com</example>
    [Required]
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
    [Required]
    [DisplayFormat(DataFormatString = "Text")]
    public string FtpUser { get; set; }

    /// <summary>
    /// Password for FTP authentication.
    /// </summary>
    /// <example>myFtpPassword123</example>
    [Required]
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
    /// If true, the incoming AS3 message is expected to be encrypted and the receiver's own certificate will be used to decrypt it.
    /// </summary>
    /// <example>true</example>
    public bool RequireEncrypted { get; set; }

    /// <summary>
    /// If true, the incoming AS3 message is expected to be signed and the partner's certificate will be used to verify the signature.
    /// </summary>
    /// <example>true</example>
    public bool RequireSigned { get; set; }

    /// <summary>
    /// Path to the receiver's private key certificate (.pfx), used to decrypt incoming AS3 messages and sign outgoing MDN receipts.
    /// </summary>
    /// <example>C:\Document\own_cert.pfx</example>
    [DisplayFormat(DataFormatString = "Text")]
    [UIHint(nameof(RequireEncrypted), "", true)]
    [RequiredIfAny(nameof(RequireEncrypted), nameof(RequireSigned))]
    public string OwnCertificatePath { get; set; }

    /// <summary>
    /// Password for the receiver's own private key certificate used for decryption and signing outgoing MDN receipts.
    /// </summary>
    /// <example>mySecurePassword123</example>
    [DisplayFormat(DataFormatString = "Text")]
    [PasswordPropertyText]
    [UIHint(nameof(RequireEncrypted), "", true)]
    [RequiredIfAny(nameof(RequireEncrypted), nameof(RequireSigned))]
    public string OwnCertificatePassword { get; set; }

    /// <summary>
    /// Path to the trading partner's public certificate file in .pem format, used to verify the signature of the incoming AS3 message.
    /// </summary>
    /// <example>C:\Document\partner_cert.pem</example>
    [DisplayFormat(DataFormatString = "Text")]
    [UIHint(nameof(RequireSigned), "", true)]
    [RequiredIf(nameof(RequireSigned), true)]
    public string PartnerCertificatePath { get; set; }
}
