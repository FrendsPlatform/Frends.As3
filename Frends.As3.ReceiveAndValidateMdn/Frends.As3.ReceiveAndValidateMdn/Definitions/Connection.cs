using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.As3.ReceiveAndValidateMdn.Definitions;

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
    /// Path to the trading partner's public certificate file in .pem format, used to verify the MDN signature.
    /// </summary>
    /// <example>C:\Document\partner_cert.pem</example>
    [DisplayFormat(DataFormatString = "Text")]
    public string PartnerCertificatePath { get; set; }
}
