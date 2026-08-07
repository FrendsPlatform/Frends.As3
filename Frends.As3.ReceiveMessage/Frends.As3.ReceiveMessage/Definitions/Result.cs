namespace Frends.As3.ReceiveMessage.Definitions;

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
    /// The Message-ID extracted from the processed AS3 message, identifying the message sent by the trading partner.
    /// </summary>
    /// <example>&lt;123456789@ftp.example.com&gt;</example>
    public string MessageId { get; set; }

    /// <summary>
    /// The AS3-From identifier extracted from the processed message, identifying the sender.
    /// </summary>
    /// <example>PartnerCompany</example>
    public string As3From { get; set; }

    /// <summary>
    /// The AS3-To identifier extracted from the processed message, identifying the receiver.
    /// </summary>
    /// <example>MyCompany</example>
    public string As3To { get; set; }

    /// <summary>
    /// The decrypted and verified EDI payload extracted from the AS3 message.
    /// </summary>
    /// <example>ISA*00*...</example>
    public string Payload { get; set; }

    /// <summary>
    /// The full path on the FTP server where the generated MDN receipt was stored.
    /// </summary>
    /// <example>/mdn/message-mdn.txt</example>
    public string MdnRemotePath { get; set; }

    /// <summary>
    /// Error that occurred during task execution.
    /// </summary>
    /// <example>object { string Message, Exception AdditionalInfo }</example>
    public Error Error { get; set; }
}
