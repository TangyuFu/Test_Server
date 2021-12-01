using System;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            TcpClient tcpClient = null;
            try
            {
                tcpClient = new TcpClient("127.0.0.1", 13000);
                NetworkStream networkStream = tcpClient.GetStream();
                Byte[] header = new Byte[2];
                Byte[] packet = new Byte[1024 * 8];
                MemoryStream headerMemoryStream = new MemoryStream(header);
                BinaryWriter headerBinaryWriter = new BinaryWriter(headerMemoryStream);
                BinaryReader headerBinaryReader = new BinaryReader(headerMemoryStream);
                string input;
                // Send console input string.
                while ((input = Console.ReadLine()) != "")
                {
                    ushort packetLen = (ushort) Encoding.ASCII.GetBytes(input, packet);
                    headerMemoryStream.Position = 0L;
                    // Use BinaryWriter for little-endian, not BitConverter.
                    headerBinaryWriter.Write(packetLen);
                    // Write 2 byte for packet length.
                    networkStream.Write(header);
                    // Write packet.
                    networkStream.Write(packet, 0, packetLen);
                    Console.WriteLine("Sent: header {0}, packet {1}", BitConverter.ToString(header), BitConverter.ToString(packet, 0, packetLen));
                    
                    int offset = 0;
                    int count;
                    // Read header, 2 bytes for packet length.
                    while ((count = networkStream.Read(header, offset, header.Length - offset)) != 0)
                    {
                        offset += count;
                        if (offset < header.Length)
                        {
                            continue;
                        }
                    
                        headerMemoryStream.Position = 0L;
                        packetLen = headerBinaryReader.ReadUInt16();
                        Console.WriteLine("Received header: {0} packet len {1}", BitConverter.ToString(header),
                            packetLen);
                        offset = 0;
                        // Read packet, packet length is packetLen.
                        while ((count = networkStream.Read(packet, offset, packetLen - offset)) != 0)
                        {
                            offset += count;
                            if (offset < packetLen)
                            {
                                continue;
                            }

                            Console.WriteLine("Received packet: {0}", BitConverter.ToString(packet, 0, packetLen));
                            // Break for next input.
                            break;
                        }
                        // Break for next input.
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("TcpClient exception: {0}", e);
            }
            finally
            {
                tcpClient?.Close();
            }
            
            Console.WriteLine("Press Enter to continue...");
            Console.Read();
        }
    }
}