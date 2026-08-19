namespace Frends.As3.ReceiveAndValidateMdn.Definitions;

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
    /// The Message-ID of the original AS3 message confirmed by the MDN.
    /// </summary>
    /// <example>&lt;60f3099a-1a5c-470d-9d46-0d2778906a6e@localhost&gt;</example>
    public string OriginalMessageId { get; set; }

    /// <summary>
    /// The Disposition value extracted from the MDN, indicating whether the message was processed successfully by the trading partner.
    /// </summary>
    /// <example>automatic-action/MDN-sent-automatically; processed</example>
    public string Disposition { get; set; }

    /// <summary>
    /// Indicates whether the trading partner confirmed successful processing of the original message.
    /// </summary>
    /// <example>true</example>
    public bool IsProcessed { get; set; }

    /// <summary>
    /// Error that occurred during task execution.
    /// </summary>
    /// <example>object { string Message, Exception AdditionalInfo }</example>
    public Error Error { get; set; }
}