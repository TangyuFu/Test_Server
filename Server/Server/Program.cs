using System;
using System.IO;
using System.Net;
using System.Net.Sockets;


namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            TcpListener server = null;
            try
            {
                server = new TcpListener(IPAddress.Parse("127.0.0.1"), 13000);
                server.Start();

                Byte[] header = new Byte[4];
                MemoryStream headerStream = new MemoryStream(header);
                BinaryReader headerReader = new BinaryReader(headerStream);
                Byte[] packet = new Byte[1024 * 8];
                while (true)
                {
                    TcpClient client = null;
                    try
                    {
                        Console.WriteLine("Waiting for a connection... ");
                        client = server.AcceptTcpClient();
                        Console.WriteLine("Connected!");
                        NetworkStream networkStream = client.GetStream();
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

                            headerStream.Position = 0L;
                            ushort packetLen = headerReader.ReadUInt16();
                            ushort packetId = headerReader.ReadUInt16();
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
                                networkStream.Write(header);
                                networkStream.Write(packet, 0, packetLen);
                                Console.WriteLine("Sent: header {0}, packet {1}", BitConverter.ToString(header),
                                    BitConverter.ToString(packet, 0, packetLen));
                            }

                            offset = 0;
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("TcpClient exception: {0}", e);
                    }
                    finally
                    {
                        // Shutdown and end connection
                        client?.Close();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("TcpListener exception: {0}", e);
            }
            finally
            {
                server?.Stop();
            }

            Console.WriteLine("Press Enter to continue...");
            Console.Read();
        }
    }
}