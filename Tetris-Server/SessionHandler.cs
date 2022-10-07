using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mina.Core.Service;
using Mina.Core.Session;

namespace Tetris_Server {
    class SessionHandler : IoHandlerAdapter {
        // Session pool
        Dictionary<IoSession, Boolean> sessions = new Dictionary<IoSession, Boolean>();

        public override void SessionCreated(IoSession session) {
            sessions[session] = true;
        }

        public override void MessageReceived(IoSession session, Object message) {
            if (Program.debug) {
                Console.WriteLine($"Packet received: {message}");
            }
            parsePacket((string)message);
        }

        public override void SessionClosed(IoSession session) {
            sessions.Remove(session);
        }

        public void SendAll(string message) {
            foreach (IoSession session in sessions.Keys) {
                session.Write(message);
            }
        }


        public override void ExceptionCaught(IoSession session, Exception cause) {
            Console.WriteLine("Unexpected exception." + cause);
            session.Close(true);
        }

        private void parsePacket(string packet) {
            string[] args = packet.Split("|");
            if (args.Length > 0) {
                switch (args[0]) {
                    case "p":
                        int shape_id = int.Parse(args[1]);
                        SendAll("p|" + shape_id);
                        break;
                    case "r":
                        SendAll("r|");
                        break;
                    case "l":
                        SendAll("l|");
                        break;
                    case "d":
                        SendAll("d|");
                        break;
                    case "ro":
                        SendAll("ro|");
                        break;
                    case "sd":
                        SendAll("sd|");
                        break;
                    case "sb":
                        SendAll("sb|");
                        break;
                }
            }
        }
    }
}
