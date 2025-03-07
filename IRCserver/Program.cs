using System.Net.Sockets;
using System.Net;
namespace IRCserver
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");
            var server = new TcpListener(IPAddress.Any, 1234);
            server.Start();

            Thread serverThread = new Thread(AcceptClients);
            serverThread.Start(server);
            serverThread.IsBackground = true;

            while (true)
            {
                var line = Console.ReadLine();
                if (line == "exit")
                    break;
                if (line == "message")
                {
                    var message = Console.ReadLine();
                    lock (streamWriters)
                    {
                        foreach (var item in streamWriters)
                        {
                            item.WriteLine($"server admin -> {message}");
                            item.Flush();
                        }
                    }
                    Console.WriteLine(message);
                }
            }

        }
        static void AcceptClients(object tcpServer)
        {
            var server = tcpServer as TcpListener;

            while (true)
            {
                var client = server.AcceptTcpClient();
                var th = new Thread(ProcessClient);
                th.IsBackground = true;
                th.Start(client);
            }
        }
        static List<StreamWriter> streamWriters = new List<StreamWriter>();
        static void ProcessClient(object tcpClient)
        {
            var client = tcpClient as TcpClient;
            using (var ns = client.GetStream())
            using (var sr = new StreamReader(ns))
            using (var sw = new StreamWriter(ns))
            {
                lock (streamWriters)
                    streamWriters.Add(sw);
                try
                {
                    Console.WriteLine("Hello from server");
                    sw.Flush();
                    while (true)
                    {
                        var line = sr.ReadLine();
                        if (line == null)
                            break;
                        Console.WriteLine(line);
                        lock (streamWriters)
                            foreach (var item in streamWriters)
                            {
                                item.WriteLine($"{client.Client.RemoteEndPoint}-> {line}");
                                item.Flush();
                            }
                        sw.Flush();
                    }
                }
                catch (IOException)
                {
                    //client closed
                }
                lock (streamWriters)
                    streamWriters.Remove(sw);
            }
        }
    }
}