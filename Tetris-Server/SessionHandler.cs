using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mina.Core.Service;
using Mina.Core.Session;

namespace Tetris_Server {
    class SessionHandler : IoHandlerAdapter {
        public enum State {
            Waiting,
            Ingame
        }

        // Session pool
        LinkedList<IoSession> sessions = new LinkedList<IoSession>();
        List<Game> games = new List<Game>();

        public override void SessionCreated(IoSession session) {
            sessions.AddLast(session);
            session.Write("ts|Connected to server: Waiting for a player...");
            session.SetAttribute("Player", new Player(session));
            session.SetAttribute("State", State.Waiting);
            updateBestScore();
            FindOpponent(session);
        }

        private void FindOpponent(IoSession playerSession) {
            foreach (IoSession session in sessions){
                if (session != playerSession && session.GetAttribute("Game") == null) {
                    Game game = new Game((Player)playerSession.GetAttribute("Player"), (Player)session.GetAttribute("Player"));
                    games.Add(game);
                }
            }
        }

        public override void MessageReceived(IoSession session, Object message) {
            if (Program.debug) {
                Console.WriteLine($"Packet received: {message}");
            }
            if ((State)session.GetAttribute("State") == State.Ingame) {
                ParsePacket((string)message, session);
            } else { // Waiting
                ParseWaitPacket((string)message, session);
            }
        }

        public override void SessionClosed(IoSession session) {
            Game game = (Game)session.GetAttribute("Game");
            if (game != null) {
                Player p = (Player)session.GetAttribute("Player");
                if (p != null) {
                    p.Alive = false;
                    game.SendToOpponents("username|Player disconnected", p);
                }
            }
            sessions.Remove(session);
        }

        public void SendAll(string message) {
            foreach (IoSession session in sessions) {
                session.Write(message);
            }
        }


        public override void ExceptionCaught(IoSession session, Exception cause) {
            Console.WriteLine("Unexpected exception." + cause);
            session.Close(true);
        }

        private void ParseWaitPacket(string packet, IoSession session) {
            string[] args = packet.Split("|");
            if (args.Length > 0) {
                switch (args[0]) {
                    case "ready":
                        int shape_id = int.Parse(args[1]);
                        string playerName = args[2];
                        Player p = (Player)session.GetAttribute("Player");
                        p.Name = playerName;

                        Game game = (Game)session.GetAttribute("Game");
                        game.SendToOpponents("p|" + shape_id, p);
                        game.SendToOpponents("username|" + playerName, p);
                        game.Ready();
                        break;
                }
            }
        }

        private void ParsePacket(string packet, IoSession session) {
            Game game = (Game)session.GetAttribute("Game");
            Player player = (Player)session.GetAttribute("Player");

            string[] args = packet.Split("|");
            if (args.Length > 0) {
                switch (args[0]) {
                    case "p":
                        int shape_id = int.Parse(args[1]);
                        game.SendToOpponents("p|" + shape_id, player);
                        break;
                    case "r":
                        game.SendToOpponents("r|", player);
                        break;
                    case "l":
                        game.SendToOpponents("l|", player);
                        break;
                    case "d":
                        game.SendToOpponents("d|" + args[1], player);
                        break;
                    case "ro":
                        game.SendToOpponents("ro|", player);
                        break;
                    case "sd":
                        game.SendToOpponents("sd|", player);
                        break;
                    case "po":
                        player.Score = int.Parse(args[1]);
                        game.SendToOpponents("po|" + args[1], player);
                        updateBestScore();
                        break;
                    case "sb":
                        game.SendToOpponents("sb|", player);
                        break;
                    case "dead":
                        player.Alive = false;
                        game.verifGameOver();
                        break;
                }
            }
        }

        private void updateBestScore() {
            int highest = 0;
            Player bestPlayer = null;
            foreach (Game game in games) {
                Player bestFromGame = game.getBestPlayer();
                if (bestFromGame != null && bestFromGame.Score > highest) {
                    highest = bestFromGame.Score;
                    bestPlayer = bestFromGame;
                }
            }

            if (bestPlayer != null) {
                SendAll("best|" + bestPlayer.Name + "|" + bestPlayer.Score);
            }
        }

        public void resetBestScore() {
            List<Game> gamesToClear = new List<Game>();
            foreach (Game game in games) {
                if (game.gameOver) {
                    gamesToClear.Add(game);
                }
            }

            foreach (Game game in gamesToClear) {
                games.Remove(game);
            }

            updateBestScore();
            SendAll("best| |0");
        }
    }
}
