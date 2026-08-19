using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.As3.ReceiveMessage.Definitions;

/// <summary>
/// Additional parameters.
/// </summary>
public class Options
{
    /// <summary>
    /// If true, the original AS3 message file will be deleted from the FTP server after successful processing.
    /// </summary>
    /// <example>false</example>
    [DefaultValue(false)]
    public bool DeleteMessageAfterProcessing { get; set; } = false;

    /// <summary>
    /// Directory path where AS3 communication logs will be stored. Leave empty to disable logging.
    /// The caller is responsible for log rotation and cleanup of files in this directory.
    /// </summary>
    /// <example>C:\Logs\AS3</example>
    [DisplayFormat(DataFormatString = "Text")]
    public string LogDirectory { get; set; }

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
}
