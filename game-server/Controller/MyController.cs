using GameServerTTT;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;

namespace ConsoleAppToWebAPI.Controllers
{
    [ApiController]
    //[Route("/[action]")]
    public class NewController : ControllerBase
    {

        // Crea un dizionario di partite, in cui la chiave è un identificatore univoco per ogni partita
        static volatile private Dictionary<string, Match> matches = new Dictionary<string, Match>();

        [HttpPost("/findstarter")]
        public IActionResult FindStarter([FromBody] Lobby lob)
        {
            // Leggi le proprietà "opponent" e "you".
            Player opponent = lob.opponent;
            Player you = lob.you;

            // Crea la chiave univoca della partita
            string gameKey = GetGameKey(you.username, opponent.username);

            // Verifica se la partita esiste già
            Match match;
            if (matches.TryGetValue(gameKey, out match))
            {
                return Ok(match);
            }

            // Crea una nuova partita e aggiungila al dizionario delle partite
            match = new Match(you, opponent, gameKey);
            matches.Add(gameKey, match);

            // Restituisci l'identificatore della partita e il giocatore iniziale
            return Ok(match);
        }

        [HttpPost("/addmove")]
        public IActionResult AddMove([FromBody] Move mov)
        {
            // Leggi l'identificatore della partita dalla richiesta
            string gameKey = mov.gameKey;

            // Cerca la partita corrispondente
            Match match;
            if (matches.TryGetValue(gameKey, out match))
            {
                // Aggiorna la matrice di gioco e controlla se c'è un vincitore
                match.UpdateMatrix(mov);

                Player winner = match.CheckWinner();

                if (winner != null)
                {
                    // Rimuovi la partita dal dizionario e restituisci il vincitore
                    matches.Remove(gameKey);
                    return Ok(winner);
                }
                else
                {
                    // Restituisci un messaggio "change turn"
                    return Ok("change turn");
                }
            }
            else
            {
                // La partita non esiste
                return NotFound();
            }
        }

        // Restituisce la chiave univoca della partita dati i nomi dei giocatori
        private string GetGameKey(string player1, string player2)
        {
            // Ordina i nomi in modo alfabetico
            string[] players = new[] { player1, player2 };
            Array.Sort(players);

            // Concatena i nomi
            return string.Join("&", players);
        }
    }
}
