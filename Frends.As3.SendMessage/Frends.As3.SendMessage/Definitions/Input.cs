using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.As3.SendMessage.Definitions;

/// <summary>
/// Essential parameters.
/// </summary>
public class Input
{
    /// <summary>
    /// Id of the company that will send the message.
    /// </summary>
    /// <example>MyCompany</example>
    [DisplayFormat(DataFormatString = "Text")]
    public string SenderAs3Id { get; set; }

    /// <summary>
    /// Id of the company that will receive the message.
    /// </summary>
    /// <example>YourCompany</example>
    [DisplayFormat(DataFormatString = "Text")]
    public string ReceiverAs3Id { get; set; }

    /// <summary>
    /// Subject of the AS3 message.
    /// </summary>
    /// <example>Subject of the message</example>
    [DisplayFormat(DataFormatString = "Text")]
    public string Subject { get; set; }

    /// <summary>
    /// Path to the file that will be sent in the message.
    /// </summary>
    /// <example>C:\Document\message.txt</example>
    [DisplayFormat(DataFormatString = "Text")]
    public string MessageFilePath { get; set; }
}