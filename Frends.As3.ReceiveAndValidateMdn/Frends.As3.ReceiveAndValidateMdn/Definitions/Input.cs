using System.ComponentModel.DataAnnotations;

namespace Frends.As3.ReceiveAndValidateMdn.Definitions;

/// <summary>
/// Essential parameters.
/// </summary>
public class Input
{
    /// <summary>
    /// Path to the MDN file on the FTP server to be retrieved and verified.
    /// </summary>
    /// <example>mdn/mdn-60f3099a-1a5c-470d-9d46-0d2778906a6e@localhost.txt</example>
    [Required]
    [DisplayFormat(DataFormatString = "Text")]
    public string RemoteMdnPath { get; set; }

    /// <summary>
    /// The locally computed MIC of the original sent message, used to verify file integrity.
    /// </summary>
    /// <example>7v7F+fQbH4lD8bKGJTbXzWWcUlI=, sha256</example>
    [Required]
    [DisplayFormat(DataFormatString = "Text")]
    public string OriginalContentMic { get; set; }

    /// <summary>
    /// The MDN options string from the original sent message, describing the requested signature algorithm.
    /// </summary>
    /// <example>signed-receipt-protocol=optional, pkcs7-signature; signed-receipt-micalg=optional, sha256</example>
    [DisplayFormat(DataFormatString = "Text")]
    public string MdnOptions { get; set; }
}