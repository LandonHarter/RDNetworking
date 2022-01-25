using System;
using System.Net;
using System.Net.Sockets;

namespace Networking.Server
{

    public class ServerClient
    {

        public static int BufferSize = 4096;
        
        public int ID { get; private set; }
        public ClientRole Role { get; set; } = ClientRole.User;

        public TCP tcp;
        public UDP udp;

        public ServerClient(int id) {
            ID = id;

            tcp = new TCP(ID);
            udp = new UDP(ID);
        }

        public void Disconnect() {
            Console.WriteLine(tcp.socket.Client.RemoteEndPoint + " has disconnected");
            
            tcp.Disconnect();
            udp.Disconnect();
        }

        public class TCP
        {

            public TcpClient socket;
            private readonly int id;

            private NetworkStream stream;
            private Packet receivedData;
            private byte[] receiveBuffer;

            public TCP(int id) {
                this.id = id;
            }

            public void Connect(TcpClient socket) {
                this.socket = socket;

                this.socket.ReceiveBufferSize = BufferSize;
                this.socket.SendBufferSize = BufferSize;

                stream = this.socket.GetStream();
                receivedData = new Packet();
                receiveBuffer = new byte[BufferSize];

                stream.BeginRead(receiveBuffer, 0, BufferSize, ReceiveCallback, null);

                using (Packet packet = new Packet((int)ServerPackets.AssignID)) {
                    packet.Write(id);
                    Server.SendTCP(id, packet);
                }
            }

            public void Disconnect() {
                socket.Close();

                stream = null;
                receivedData = null;
                receiveBuffer = null;
                socket = null;
            }

            public void SendData(Packet packet) {
                try {
                    if (socket != null) {
                        stream.BeginWrite(packet.ToArray(), 0, packet.Length(), null, null);
                    }
                }
                catch (Exception ex) {
                    Console.WriteLine($"Failed to send data via TCP: {ex.Message}");
                }
            }

            private bool HandleData(byte[] data) {
                int packetLength = 0;
                receivedData.SetBytes(data);

                if (receivedData.UnreadLength() >= 4) {
                    packetLength = receivedData.ReadInt();
                    if (packetLength <= 0) return true;
                }

                while (packetLength > 0 && packetLength <= receivedData.UnreadLength()) {
                    byte[] packetBytes = receivedData.ReadBytes(packetLength);
                    
                    ThreadManager.ExecuteOnMainThread(() => {
                        using (Packet packet = new Packet(packetBytes)) {
                            int packetID = packet.ReadInt();
                            Server.PacketHandlers[packetID](id, packet);
                        }
                    });

                    packetLength = 0;
                    if (receivedData.UnreadLength() >= 4) {
                        packetLength = receivedData.ReadInt();
                        if (packetLength <= 0) return true;
                    }
                }

                if (packetLength <= 1) {
                    return true;
                }

                return false;
            }

            private void ReceiveCallback(IAsyncResult result) {
                try {
                    int byteLength = stream.EndRead(result);
                    if (byteLength <= 0) {
                        Disconnect();
                        return;
                    }

                    byte[] data = new byte[BufferSize];
                    Array.Copy(receiveBuffer, data, byteLength);
                    
                    receivedData.Reset(HandleData(data));
                    stream.BeginRead(receiveBuffer, 0, BufferSize, ReceiveCallback, null);
                }
                catch (Exception ex) {
                    Console.WriteLine($"Failed to receive TCP data: {ex.Message}");
                    Disconnect();
                }
            }

        }

        public class UDP
        {

            public IPEndPoint endPoint;
            private int id;
            
            public UDP(int id) {
                this.id = id;
            }

            public void Connect(IPEndPoint endPoint) {
                this.endPoint = endPoint;
            }

            public void Disconnect() {
                endPoint = null;
            }

            public void SendData(Packet packet) {
                Server.SendUDP(endPoint, packet);
            }

            public void HandleData(Packet packet) {
                int packetLength = packet.ReadInt();
                byte[] packetData = packet.ReadBytes(packetLength);
                
                ThreadManager.ExecuteOnMainThread(() => {
                    using (Packet newPacket = new Packet(packetData)) {
                        int packetID = newPacket.ReadInt();
                        Server.PacketHandlers[packetID](packetID, packet);
                    }
                });
            }
            
        }

    }

}