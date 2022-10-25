using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tetris_Server {
    internal class Game {
        private Player[] players;
        private int readyCounter = 0;
        public bool gameOver = false;

        /*
         * Game class is made this way to handle games with more than 2 people. 
         * (For future updates if any)
         */
        public Game(params Player[] players) {
            this.players = players;
            foreach (Player p in players) {
                p.GetSession().SetAttribute("Game", this);
                p.GetSession().Write("ts|Game is about to start...");
                p.GetSession().Write("ready|");
            }
        }

        public void SendToOpponents(string packet, Player player) {
            foreach (Player p in players) {
                if (p != player) {
                    p.GetSession().Write(packet);
                }
            }
        }

        public void SendAll(string packet) {
            foreach (Player p in players) {
                p.GetSession().Write(packet);
            }
        }

        public void Start() {
            foreach (Player p in players) {
                p.GetSession().Write("ts|Game started!");
                p.GetSession().SetAttribute("State", SessionHandler.State.Ingame);
                p.GetSession().Write("start|");
            }
        }

        public void Ready() {
            if (++readyCounter >= players.Length) {
                Start();
            }
        }

        public Player getBestPlayer() {
            int highest = 0;
            Player best = null;
            foreach (Player p in players) {
                if (p.Score > highest) {
                    highest = p.Score;
                    best = p;
                }
            }
            return best;
        }

        public void verifGameOver() {
            bool allDead = true;
            foreach (Player p in players) {
                if (p.Alive) {
                    allDead = false;
                    break;
                }
            }

            if (allDead) {
                gameOver = true;
                Player winner = getBestPlayer();
                if (winner != null) {
                    foreach (Player p in players) {
                        if (p == winner) {
                            p.GetSession().Write("win|" + "1");
                        } else {
                            p.GetSession().Write("win|" + "0");
                        }
                    }
                }
            }
        }
    }
}
