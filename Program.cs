using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TcpMultiClient
{
	internal class SsClient
	{
		internal TcpClient Tc;
		internal bool Ht;
	}

	internal class Program
	{
		static byte[] ssefile, sseResponse;

		// https://elysiatools.com/en/samples/windows-networking-csharp
		// TCP Server with Multiple Client Support
		static async Task Main()
		{
			var foo = Task.Run(() => MultiClientTcpServer());
			Console.WriteLine("Main():  launched MultiClientTcpServer");
			string where = AppDomain.CurrentDomain.BaseDirectory;
			Console.WriteLine("AppDomain Base Directory: " + where);
			string swhere = where.Substring(0, where.LastIndexOf("bin")) + "wwwroot\\index.html";
            ssefile = File.ReadAllBytes(swhere);
		    Console.WriteLine("ssefile:  " + swhere + $"length: {ssefile.Length}");	
			sseResponse = Encoding.UTF8.GetBytes($"HTTP/1.1 200 OK\nContent-Length: {ssefile.Length}\nContent-Type: text/html\nServer: TcpMultiClient\n\n"
							 + Encoding.UTF8.GetString(ssefile));
			await foo;
		}

		public static async Task MultiClientTcpServer()
		{
			Console.WriteLine("\n=== Multi-Client TCP Server ===");

			int port = 8081;
			TcpListener server = null;
			ConcurrentDictionary<string, SsClient> clients = new ConcurrentDictionary<string, SsClient>();

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

					clients[clientId] = new SsClient() { Tc = client, Ht = false };
					Console.WriteLine($"\nNew client connected: {clientId}");

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

		static readonly byte[] ok = Encoding.UTF8.GetBytes("HTTP/1.1 200 OK\nContent-Type: text/event-stream; charset=UTF-8\n\n\n");
		static async Task HandleMultiClient(TcpClient client, string clientId, ConcurrentDictionary<string, SsClient> clients)
		{
			try
			{
				using (client)
				using (NetworkStream stream = client.GetStream())
				{
					// Test for HTTP
					using (StreamReader sr = new StreamReader(stream))
					{
						byte[] which;
						string first = sr.ReadLine();
						if (null != first)
						{
							Console.WriteLine($"{clientId} first: {first}");
							string[] actionLine = first?.Split(new char[] { ' ' }, 3);
							if (null != actionLine && "POST" == actionLine[0] || "GET" == actionLine[0])
							{
								which = ("/sse" == actionLine[1]) ? ok
										: ("/" == actionLine[1]) ? sseResponse
										: Encoding.UTF8.GetBytes("HTTP/1.1 404 NOT FOUND\n");
								clients[clientId].Ht = true;
								for (string line = sr.ReadLine(); null != line && 0 < line.Length; line = sr.ReadLine())
									Console.WriteLine(line);
							} else {
								// telnet welcome message
								string welcome = $"Welcome! You are {clientId}. Connected clients: {clients.Count}<br>\n";
								which = Encoding.UTF8.GetBytes(welcome);
							}
							await stream.WriteAsync(which, 0, which.Length);
						}

						Console.WriteLine($"\n---- {clientId} entering main loop ---");
						while (true)
						{
							byte[] buffer = new byte[1024];
							int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
							if (bytesRead == 0) break;

							string message = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
							Console.WriteLine($"{clientId}: {message}");
							if (message.StartsWith("GET /sse"))
								await stream.WriteAsync(ok, 0, ok.Length);

							// Broadcast HTTP message to all non-HTTP clients (except sender)
							string broadcastMsg = $"{clientId}: {message}<br>\n";
							byte[] broadcastBytes = Encoding.UTF8.GetBytes(broadcastMsg);

							foreach (var kvp in clients)
							{
								try
								{
									if (kvp.Key != clientId && kvp.Value.Tc.Connected && !(clients[clientId].Ht && kvp.Value.Ht))
									{
										NetworkStream clientStream = kvp.Value.Tc.GetStream();
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
