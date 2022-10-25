using System.Net;
using System.Text;
using Mina.Core.Service;
using Mina.Filter.Codec;
using Mina.Filter.Codec.TextLine;
using Mina.Filter.Logging;
using Mina.Transport.Socket;

namespace Tetris_Server {
    class Program {
        private static readonly int port = 2121;
        private static bool isRunning = true;
        private static SessionHandler sHandler = new SessionHandler();
        private static IoAcceptor acceptor;
        public static bool debug = false;

        static void Main(string[] args) {
            acceptor = new AsyncSocketAcceptor();


            acceptor.FilterChain.AddLast("logger", new LoggingFilter());
            acceptor.FilterChain.AddLast("codec", new ProtocolCodecFilter(new TextLineCodecFactory(Encoding.UTF8)));

            acceptor.Handler = sHandler;

            acceptor.Bind(new IPEndPoint(IPAddress.Any, port));

            DisplayHomeMessage();

            while (isRunning) {
                string consoleMessage = Console.ReadLine();
                string[] messageArgs = consoleMessage.Split(" ");
                string cmd = messageArgs.Length > 0 ? messageArgs[0] : "";
                switch (cmd.ToLower()) {
                    case "shutdown":
                        isRunning = false;
                        break;
                    case "sendall":
                        string messageToSend = consoleMessage.Substring(cmd.Length+1);
                        Console.WriteLine("Sent {0} to everyone", messageToSend);
                        sHandler.SendAll(messageToSend);
                        break;
                    case "debug":
                        debug = !debug;
                        if (debug) Console.WriteLine("Debug mode enabled"); else Console.WriteLine("Debug mode disabled");
                        break;
                    case "resetscore":
                        sHandler.resetBestScore();
                        Console.WriteLine("Best score reseted!");
                        break;
                    case "clear":
                        Console.Clear();
                        DisplayHomeMessage();
                        break;
                    default:
                        break;
                }
            }
        }

        private static void DisplayHomeMessage() {
            Console.WriteLine("Listening on " + acceptor.LocalEndPoint + Environment.NewLine);
            Console.WriteLine("Available commands");
            Console.WriteLine("shutdown - Shutdown the server");
            Console.WriteLine("sendall - Send a message / packet to every sessions");
            Console.WriteLine("debug - Display debugging informations");
            Console.WriteLine("resetscore - Reset best score");
            Console.WriteLine("clear - Clear the console" + Environment.NewLine);
        }

    }
}