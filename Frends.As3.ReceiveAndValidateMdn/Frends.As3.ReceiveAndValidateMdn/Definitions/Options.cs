using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.As3.ReceiveAndValidateMdn.Definitions;

/// <summary>
/// Additional parameters.
/// </summary>
public class Options
{
    /// <summary>
    /// If true, the MDN file will be deleted from the FTP server after successful verification.
    /// </summary>
    /// <example>false</example>
    [DefaultValue(false)]
    public bool DeleteMdnAfterVerification { get; set; } = false;

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
