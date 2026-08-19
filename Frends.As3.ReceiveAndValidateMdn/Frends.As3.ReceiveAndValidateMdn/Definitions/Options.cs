using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.As3.ReceiveAndValidateMdn.Definitions;

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
    /// If true, the MDN file will be deleted from the FTP server after successful verification.
    /// </summary>
    /// <example>false</example>
    [DefaultValue(false)]
    public bool DeleteMdnAfterVerification { get; set; } = false;

    /// <summary>
    /// Number of times to retry retrieving the MDN file from the FTP server if it is not yet available.
    /// Set to 0 to disable retries.
    /// </summary>
    /// <example>3</example>
    [DefaultValue(0)]
    public int RetryCount { get; set; } = 0;

    /// <summary>
    /// Time in seconds to wait between retry attempts when the MDN file is not yet available on the FTP server.
    /// </summary>
    /// <example>30</example>
    [DefaultValue(30)]
    public int RetryIntervalSeconds { get; set; } = 30;

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
