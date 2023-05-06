using System.Collections.Generic;


public class Message
{
    public Dictionary<string, string> Headers { get; set; }
    public byte[] Payload { get; set; }

    public Message(Dictionary<string, string> headers, byte[] payload)
    {
        Headers = headers;
        Payload = payload;
    }
}