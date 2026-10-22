using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using BlokChein.Models;

namespace BlokChein.Services
{
    public class NetworkService
    {
        private readonly int _port;
        private List<Node> _nodes;

        public NetworkService(int port, List<Node> nodes)
        {
            _port = port;
            _nodes = nodes;
        }

        public void Start(Func<string, string, Task> onMessage)
        {
            var listener = new TcpListener(IPAddress.Any, _port);
            listener.Start();
            _ = Task.Run(async () =>
            {
                while (true)
                {
                    var client = await listener.AcceptTcpClientAsync();
                    using var reader = new StreamReader(client.GetStream());

                    string? message = await reader.ReadLineAsync();
                    if (message != null)
                    {
                        var parts = message.Split('|');
                        if (parts.Length == 2)
                        {
                            await onMessage(parts[0], parts[1]);
                        }
                    }
                }
            });
        }

        public async Task BroadCastAsync<T>(string type, T data)
        {
            var json = JsonSerializer.Serialize(data);
            foreach (var node in _nodes)
            {
                try
                {
                    using var client = new TcpClient();
                    await client.ConnectAsync(node.IP, node.Port);
                    using var writer = new StreamWriter(client.GetStream()) { AutoFlush = true };
                    await writer.WriteLineAsync($"{type}|{json}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to send message to peer {node.IP}: {ex.Message}");
                }
            }
        }
    }
}
