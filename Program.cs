using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TcpMultiClient
{
    internal class Program
    {
        // https://elysiatools.com/en/samples/windows-networking-csharp
        // TCP Server with Multiple Client Support
        static async Task Main()
        { var foo = Task.Run(() => MultiClientTcpServer()); Console.WriteLine("Main():  launched MultiClientTcpServer"); await foo; }
        public static async Task MultiClientTcpServer()
        {
            Console.WriteLine("\n=== Multi-Client TCP Server ===");

            int port = 8081;
            TcpListener server = null;
            ConcurrentDictionary<string, TcpClient> clients = new ConcurrentDictionary<string, TcpClient>();

            try
            {
                server = new TcpListener(IPAddress.Any, port);
                server.Start();
                Console.WriteLine($"Multi-client server started on port {port}");

                // Accept clients continuously
                while (true)
                {
                    TcpClient client = await server.AcceptTcpClientAsync();
                    string clientId = $"Client_{DateTime.Now:HHmmss}_{client.Client.RemoteEndPoint}";

                    clients[clientId] = client;
                    Console.WriteLine($"New client connected: {clientId}");

                    _ = Task.Run(() => HandleMultiClient(client, clientId, clients));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Server error: {ex.Message}");
            }
            finally
            {
                server?.Stop();
            }
        }

        static async Task HandleMultiClient(TcpClient client, string clientId, ConcurrentDictionary<string, TcpClient> clients)
        {
            try
            {
                using (client)
                using (NetworkStream stream = client.GetStream())
                {
                    // Send welcome message
                    string welcome = $"Welcome! You are {clientId}. Connected clients: {clients.Count}\n";
                    byte[] welcomeBytes = Encoding.UTF8.GetBytes(welcome);
                    await stream.WriteAsync(welcomeBytes, 0, welcomeBytes.Length);

                    while (true)
                    {
                        byte[] buffer = new byte[1024];
                        int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                        if (bytesRead == 0) break;

                        string message = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
                        Console.WriteLine($"{clientId}: {message}");

                        // Broadcast message to all clients (except sender)
                        string broadcastMsg = $"{clientId}: {message}\n";
                        byte[] broadcastBytes = Encoding.UTF8.GetBytes(broadcastMsg);

                        foreach (var kvp in clients)
                        {
                            try
                            {
                                if (kvp.Key != clientId && kvp.Value.Connected)
                                {
                                    NetworkStream clientStream = kvp.Value.GetStream();
                                    await clientStream.WriteAsync(broadcastBytes, 0, broadcastBytes.Length);
                                }
                            }
                            catch
                            {
                                // Remove disconnected client
                                clients.TryRemove(kvp.Key, out _);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling client {clientId}: {ex.Message}");
            }
            finally
            {
                clients.TryRemove(clientId, out _);
                Console.WriteLine($"{clientId} disconnected");
            }
        }
    }
}
