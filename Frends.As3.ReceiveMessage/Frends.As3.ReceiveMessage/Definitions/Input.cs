using System.ComponentModel.DataAnnotations;

namespace Frends.As3.ReceiveMessage.Definitions;

/// <summary>
/// Essential parameters.
/// </summary>
public class Input
{
    /// <summary>
    /// Path to the AS3 message file on the FTP server to be retrieved and processed.
    /// </summary>
    /// <example>/incoming/message.as3</example>
    [Required]
    [DisplayFormat(DataFormatString = "Text")]
    public string RemoteMessagePath { get; set; }

    /// <summary>
    /// Path to the directory on the FTP server where the generated MDN receipt will be stored.
    /// </summary>
    /// <example>/mdn</example>
    [DisplayFormat(DataFormatString = "Text")]
    public string RemoteMdnPath { get; set; }
}
