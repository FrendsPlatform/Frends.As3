using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.As3.SendMessage.Definitions;

/// <summary>
/// Additional parameters.
/// </summary>
public class Options
{
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
    /// <example>false</example>
    [DefaultValue(true)]
    public bool ThrowErrorOnFailure { get; set; } = true;

    /// <summary>
    /// Overrides the error message on failure.
    /// </summary>
    /// <example>Custom error message</example>
    [DisplayFormat(DataFormatString = "Text")]
    [DefaultValue("")]
    public string ErrorMessageOnFailure { get; set; }
}
