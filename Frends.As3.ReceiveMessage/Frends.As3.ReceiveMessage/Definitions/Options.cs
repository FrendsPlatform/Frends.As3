using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.As3.ReceiveMessage.Definitions;

/// <summary>
/// Additional parameters.
/// </summary>
public class Options
{
    /// <summary>
    /// Whether to throw an error on failure.
    /// </summary>
    /// <example>true</example>
    [DefaultValue(true)]
    public bool ThrowErrorOnFailure { get; set; } = true;

    /// <summary>
    /// Overrides the error message on failure.
    /// </summary>
    /// <example>Custom error message</example>
    [DisplayFormat(DataFormatString = "Text")]
    [DefaultValue("")]
    public string ErrorMessageOnFailure { get; set; } = string.Empty;

    /// <summary>
    /// If true, the original AS3 message file will be deleted from the FTP server after successful processing.
    /// </summary>
    /// <example>true</example>
    public bool DeleteMessageAfterProcessing { get; set; }
}
