namespace Frends.As3.SendMessage.Definitions;

/// <summary>
/// Result of the task.
/// </summary>
public class Result
{
    /// <summary>
    /// Indicates if the task completed successfully.
    /// </summary>
    /// <example>true</example>
    public bool Success { get; set; }

    /// <summary>
    /// Information about the FTP transport result or remote server response.
    /// </summary>
    /// <example>File 'message.txt' successfully uploaded to FTP directory '/inbox'.</example>
    public string PartnerResponse { get; set; }

    /// <summary>
    /// Generated Message-ID of the sent AS3 message.
    /// </summary>
    /// <example>&lt;123456789@ftp.example.com&gt;</example>
    public string MessageId { get; set; }

    /// <summary>
    /// The locally computed digital Message Integrity Check (MIC) code of the sent file (e.g., Base64-encoded hash and algorithm name).
    /// </summary>
    /// <example>7v7F+fQbH4lD8bKGJTbXzWWcUlI=, sha256</example>
    public string OriginalContentMIC { get; set; }

    /// <summary>
    /// The MIME options string that describes the MDN signature algorithm requested from the trading partner (e.g., signed-receipt-protocol and signed-receipt-micalg).
    /// </summary>
    /// <example>signed-receipt-protocol=optional, pkcs7-signature; signed-receipt-micalg=optional, sha256</example>
    public string MdnOptions { get; set; }

    /// <summary>
    /// Error that occurred during task execution.
    /// </summary>
    /// <example>object { string Message, Exception AdditionalInfo }</example>
    public Error Error { get; set; }
}
