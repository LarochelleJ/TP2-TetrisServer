using Mina.Core.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tetris_Server {
    internal class Player {
        public int Level { get; set; }
        public int Score { get; set; }

        public string Name { get; set; }
        public bool Alive { get; set; }

        private IoSession session;

        public Player(IoSession session) {
            this.session = session;
            Alive = true;
        }

        public void SendPacket(string packet) {
            session.Write(packet);
        }

        public IoSession GetSession() {
            return session;
        }

    }
}
