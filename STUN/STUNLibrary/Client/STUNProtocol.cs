using System;
using System.Net;
using System.Text;

namespace STUNLibrary.Client
{
    internal static class STUNProtocol
    {
        // Magic Cookie: 0x2112A442
        private static readonly byte[] MagicCookie = new byte[] { 0x21, 0x12, 0xA4, 0x42 };

        public static byte[] CreateBindingRequest()
        {
            byte[] message = new byte[20];

            // Header: Type (0x0001 - Binding Request)
            message[0] = 0x00; message[1] = 0x01;
            // Header: Length (0x0000)
            message[2] = 0x00; message[3] = 0x00;
            // Header: Magic Cookie
            Array.Copy(MagicCookie, 0, message, 4, 4);
            // Header: Transaction ID (Random 12 bytes)
            byte[] transId = Guid.NewGuid().ToByteArray(); // Quick way to get random bytes
            Array.Copy(transId, 0, message, 8, 12); // Use 12 bytes

            return message;
        }

        public static STUNNetworkInfo ParseResponse(byte[] response)
        {
            if (response.Length < 20) throw new Exception("Response too short");

            STUNNetworkInfo stunNetworkInfo = new STUNNetworkInfo();
            int offset = 20; // Skip header

            // Length from header
            int bodyLength = (response[2] << 8) | response[3];

            while (offset < 20 + bodyLength && offset < response.Length)
            {
                int attrType = (response[offset] << 8) | response[offset + 1];
                int attrLen = (response[offset + 2] << 8) | response[offset + 3];
                offset += 4;

                // MAPPED-ADDRESS (0x0001)
                if (attrType == 0x0001)
                {
                    ParseMappedAddr(response, offset, stunNetworkInfo);
                }
                // XOR-MAPPED-ADDRESS (0x0020)
                else if (attrType == 0x0020)
                {
                    ParseXorMappedAddr(response, offset, stunNetworkInfo);
                }
                // SOFTWARE (0x8022)
                else if (attrType == 0x8022)
                {
                    stunNetworkInfo.ServerSoftware = Encoding.UTF8.GetString(response, offset, attrLen);
                }

                offset += attrLen;
                // STUN attributes are padded to 4 bytes
                int padding = (4 - (attrLen % 4)) % 4;
                offset += padding;
            }

            return stunNetworkInfo;
        }

        private static void ParseMappedAddr(byte[] buffer, int offset, STUNNetworkInfo stunNetworkInfo)
        {
            int family = buffer[offset + 1];
            int port = (buffer[offset + 2] << 8) | buffer[offset + 3];

            if (family == 0x01) // IPv4
            {
                byte[] ipBytes = new byte[4];
                Array.Copy(buffer, offset + 4, ipBytes, 0, 4);
                stunNetworkInfo.PublicIPAddress = new IPAddress(ipBytes);
                stunNetworkInfo.PublicPort = port;
            }
        }

        private static void ParseXorMappedAddr(byte[] buffer, int offset, STUNNetworkInfo stunNetworkInfo)
        {
            int family = buffer[offset + 1];
            int xorPort = (buffer[offset + 2] << 8) | buffer[offset + 3];

            // Un-XOR Port (XOR with top 16 bits of Magic Cookie: 0x2112)
            int port = xorPort ^ 0x2112;

            if (family == 0x01) // IPv4
            {
                byte[] ipBytes = new byte[4];
                Array.Copy(buffer, offset + 4, ipBytes, 0, 4);

                // Un-XOR IP (XOR with full Magic Cookie)
                for (int i = 0; i < 4; i++)
                {
                    ipBytes[i] ^= MagicCookie[i];
                }

                stunNetworkInfo.PublicIPAddress = new IPAddress(ipBytes);
                stunNetworkInfo.PublicPort = port;
            }
        }
    }
}