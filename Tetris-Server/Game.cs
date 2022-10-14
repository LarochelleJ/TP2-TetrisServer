using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tetris_Server {
    internal class Game {
        private Player[] players;
        private int readyCounter = 0;

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
            foreach(Player p in players) {
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
    }
}
