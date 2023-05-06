using System.Collections.Generic;
using System.IO;
using System.Text;

public class SimpleMessageCodec : IMessageCodec
{
    private const int MAX_PAYLOAD = 256 * 1024;
    private const int MAX_HEADER = 1023;
    private const int MAX_NUM_HEADERS = 4;

    public byte[] Encode(Message message)
        {
            byte[] headerBytes = EncodeHeaders(message.Headers);
            byte[] payloadBytes = message.Payload;
            int totalLength = MAX_NUM_HEADERS + headerBytes.Length + payloadBytes.Length;
            if (totalLength > MAX_PAYLOAD + MAX_HEADER + MAX_NUM_HEADERS)
            {
                throw new ArgumentException("Message too large");
            }
            MemoryStream stream = new MemoryStream(totalLength);
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                writer.Write((ushort)headerBytes.Length);
                writer.Write(headerBytes);
                writer.Write(payloadBytes);
            }
            return stream.ToArray();
        }

        public Message Decode(byte[] data)
        {
            using (MemoryStream stream = new MemoryStream(data))
            using (BinaryReader reader = new BinaryReader(stream))
            {
                ushort headerLength = reader.ReadUInt16();
                if (headerLength > MAX_HEADER)
                {
                    throw new ArgumentException("Header too long");
                }
                byte[] headerBytes = reader.ReadBytes(headerLength);
                Dictionary<string, string> headers = DecodeHeaders(headerBytes);
                byte[] payloadBytes = reader.ReadBytes(data.Length - MAX_NUM_HEADERS - headerBytes.Length);
                return new Message(headers, payloadBytes);
            }
        }


        private byte[] EncodeHeaders(Dictionary<string, string> headers)
        {

            MemoryStream stream = new MemoryStream();
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                foreach (KeyValuePair<string, string> entry in headers)
                {
                    byte[] nameBytes = Encoding.ASCII.GetBytes(entry.Key);
                    byte[] valueBytes = Encoding.ASCII.GetBytes(entry.Value);
                    if (nameBytes.Length > MAX_HEADER || valueBytes.Length > MAX_HEADER)
                    {
                        throw new ArgumentException("Header too long");
                    }
                    writer.Write((ushort)nameBytes.Length);
                    writer.Write(nameBytes);
                    writer.Write((ushort)valueBytes.Length);
                    writer.Write(valueBytes);
                }
            }
            return stream.ToArray();
        }

        private Dictionary<string, string> DecodeHeaders(byte[] headerBytes)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>();
            using (MemoryStream stream = new MemoryStream(headerBytes))
            using (BinaryReader reader = new BinaryReader(stream))
            {
                while (stream.Position < stream.Length)
                {
                    ushort nameLength = reader.ReadUInt16();
                    byte[] nameBytes = reader.ReadBytes(nameLength);
                    ushort valueLength = reader.ReadUInt16();
                    byte[] valueBytes = reader.ReadBytes(valueLength);
                    string name = Encoding.ASCII.GetString(nameBytes);
                    string value = Encoding.ASCII.GetString(valueBytes);
                    headers.Add(name, value);
                }
            }
            return headers;
        }


    private int WriteHeaders(BinaryWriter dataOutputStream, IDictionary<string, string> headers)
    {
        
	    if (headers == null || headers.Count == 0)
	    {
		dataOutputStream.Write((ushort) 0); // write 0 for headers length
		return 0;
	    }

	    if (headers.Count > MAX_NUM_HEADERS)
	    {
		throw new ArgumentException("Too many headers");
	    }

	    using (var headersOutputStream = new MemoryStream())
	    using (var headersDataOutputStream = new BinaryWriter(headersOutputStream))
	    {
		foreach (var entry in headers)
		{
		    var nameBytes = System.Text.Encoding.ASCII.GetBytes(entry.Key);
		    var valueBytes = System.Text.Encoding.ASCII.GetBytes(entry.Value);

		    if (nameBytes.Length > MAX_HEADER|| valueBytes.Length > MAX_HEADER)
		    {
		        throw new ArgumentException("Header is too large");
		    }

		    headersDataOutputStream.Write((ushort) nameBytes.Length);
		    headersDataOutputStream.Write(nameBytes);
		    headersDataOutputStream.Write((ushort) valueBytes.Length);
		    headersDataOutputStream.Write(valueBytes);
		}

		var headersBytes = headersOutputStream.ToArray();
		if (headersBytes.Length > MAX_PAYLOAD)
		{
		    throw new ArgumentException("Headers are too large");
		}

		dataOutputStream.Write((ushort) headersBytes.Length);
		dataOutputStream.Write(headersBytes);

		return headersBytes.Length;
	    }
	}

    private IDictionary<string, string> ReadHeaders(BinaryReader dataInputStream)
    {
        int headersLength = (int)dataInputStream.ReadUInt32();
        if (headersLength == 0)
        {
            return new Dictionary<string, string>();
        }

        byte[] headersBytes = dataInputStream.ReadBytes(headersLength);
        using (MemoryStream stream = new MemoryStream(headersLength))
        using (BinaryReader reader = new BinaryReader(stream))
        {
            var headers = new Dictionary<string, string>();

            while (stream.Position < stream.Length)
            {
                ushort nameLength = reader.ReadUInt16();
                byte[] nameBytes = reader.ReadBytes(nameLength);
                ushort valueLength = reader.ReadUInt16();
                byte[] valueBytes = reader.ReadBytes(valueLength);

                if (nameBytes.Length > MAX_HEADER || valueBytes.Length > MAX_HEADER)
                {
                    throw new ArgumentException("Header is too large");
                }

                var name = System.Text.Encoding.ASCII.GetString(nameBytes);
                var value = System.Text.Encoding.ASCII.GetString(valueBytes);
                headers[name] = value;
            }

            return headers;
        }
    }
}