using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServerTTT
{
    public class Match
    {
        private const int MATRIX_DIM = 3;

        public Player playerX { get; set; }
        public Player playerO { get; set; }
        public Player starter { get; set; }
        public string matchId { get; set; }

        private Player nextPlayer;

        private string[,] myMatrix = { { "", "", "" }, { "", "", "" }, { "", "", "" } };

        public Match(Player playerX, Player playerO, string matchId)
        {
            this.playerX = playerX;
            this.playerO = playerO;
            this.starter = playerX;
            nextPlayer = playerX;
            this.matchId = matchId;
        }

        internal void UpdateMatrix(Move mov)
        {
            Console.WriteLine("Received move from: " + mov.p.username);
            (int, int) positions = GetFromPos(mov.pos);
            string symbol = mov.p.username == playerX.username ? "X" : "O";
            myMatrix[positions.Item1, positions.Item2] = symbol;
            Console.WriteLine("Add " + symbol + " in position: " + positions);
            nextPlayer = nextPlayer == playerX ? playerO : playerX;
        }

        internal Player CheckWinner()
        {
            // Check rows
            for (int i = 0; i < 3; i++)
            {
                if (myMatrix[i, 0] == "O" && myMatrix[i, 1] == "O" && myMatrix[i, 2] == "O")
                {
                    return playerO;
                }

                if (myMatrix[i, 0] == "X" && myMatrix[i, 1] == "X" && myMatrix[i, 2] == "X")
                {
                    return playerX;
                }
            }

            // Check columns
            for (int i = 0; i < 3; i++)
            {
                if (myMatrix[0, i] == "O" && myMatrix[1, i] == "O" && myMatrix[2, i] == "O")
                {
                    return playerO;
                }

                if (myMatrix[0, i] == "X" && myMatrix[1, i] == "X" && myMatrix[2, i] == "X")
                {
                    return playerX;
                }
            }

            // Check diagonals
            if (myMatrix[0, 0] == "O" && myMatrix[1, 1] == "O" && myMatrix[2, 2] == "O")
            {
                return playerO;
            }
            if (myMatrix[0, 2] == "O" && myMatrix[1, 1] == "O" && myMatrix[2, 0] == "O")
            {
                return playerO;
            }

            if (myMatrix[0, 0] == "X" && myMatrix[1, 1] == "X" && myMatrix[2, 2] == "X")
            {
                return playerX;
            }
            if (myMatrix[0, 2] == "X" && myMatrix[1, 1] == "X" && myMatrix[2, 0] == "X")
            {
                return playerX;
            }

            bool isMatrixFull = true;
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (myMatrix[i, j] == "")
                    {
                        isMatrixFull = false;
                        break;
                    }
                }
                if (!isMatrixFull)
                {
                    break;
                }
            }

            if (isMatrixFull)
                return new Player { id = "", username = ""};


            // No win
            return null;
        }

        internal static (int, int) GetFromPos(int pos)
        {
            int x = pos / MATRIX_DIM;
            int y = pos % MATRIX_DIM;

            return (x, y);
        }
    }
}
