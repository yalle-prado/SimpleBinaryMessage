using System;

class Program
{
    static void Main(string[] args)
    {
        // Create a message

        var message = new Message(
            new Dictionary<string, string>
            {
                { "From", "UserA" },
                { "To", "UserB" }
            },
            new byte[] { 0x01, 0x02, 0x03 }
        );

        // Create message codec
        var messageCodec = new SimpleMessageCodec();

        // Encode message
        var encodedMessage = messageCodec.Encode(message);

        // Decode message
        var decodedMessage = messageCodec.Decode(encodedMessage);

        // Print original and decoded message payloads
        Console.WriteLine("Original message payload:");
        Console.WriteLine(BitConverter.ToString(message.Payload));
        Console.WriteLine("Decoded message payload:");
        Console.WriteLine(BitConverter.ToString(decodedMessage.Payload ?? Array.Empty<byte>()));
    }
}