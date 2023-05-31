using Newtonsoft.Json;
using System;
using System.Reflection;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Windows;
using WebSocketSharp;
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;


    [SerializeField]
    TextMeshProUGUI playerX;
    [SerializeField]
    TextMeshProUGUI playerO;

    public Player myPlayer = null;
    public string symbol = "";
    public string matchId = "";

    private WebSocket ws;
    private bool gameStartReceived = false;
    private bool turnAlreadyUpdated = false;
    private bool showWinnerScreen = false;
    private Match match = null;
    private Player playerTurn = null;
    private Player winnerPlayer = null;
    private Move lastOpponentMove = null;

    struct InitialMessage
    {
        public string matchId;
        public string username;
    }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        string jsonString = PlayerPrefs.GetString("MatchData");
        //string myUsername = PlayerPrefs.GetString("Username");
        string myUsername = StaticVariables.Username;
        match = JsonConvert.DeserializeObject<Match>(jsonString);

        myPlayer = myUsername == match.playerX.username ? match.playerX : match.playerO;
        playerTurn = match.starter;

        // Cambio la grafica per renderla compatibile tra i due partecipanti
        playerX.text = match.playerX.username;
        playerO.text = match.playerO.username;
        symbol = match.playerX.username == myPlayer.username ? "X" : "O";
        matchId = match.matchId;

        Debug.Log(matchId);
        ws = new WebSocket("ws://204.216.215.36:9095/orchestrator/game");
        //ws = new WebSocket("ws://127.0.0.1:9095/orchestrator/game");
        ws.OnMessage += OnMessage;
        ws.Connect();

        //Creo il messaggio iniziale
        InitialMessage message = new InitialMessage();
        message.matchId = matchId;
        message.username = myPlayer.username;

        Debug.Log("Sending IM username: " + message.username);
        SendMessage(JsonConvert.SerializeObject(message));
    }

    private void Update()
    {
        if (showWinnerScreen) 
        {
            if (winnerPlayer.username == "")
            {
                CanvasManager.Instance.ShowGameOverScreen(false, true);
            }
            else
            {
                CanvasManager.Instance.ShowGameOverScreen(winnerPlayer.username == myPlayer.username, false);
            }
        }
        else if(gameStartReceived && !turnAlreadyUpdated)
        {
            turnAlreadyUpdated = true;
            if (playerTurn.username == myPlayer.username)
            {
                CanvasManager.Instance.EnableButtons();
                CanvasManager.Instance.SetPlayerTurn(1);
                if(lastOpponentMove != null)
                {
                    CanvasManager.Instance.ApplyMove(lastOpponentMove.pos, symbol == "X" ? "O" : "X");
                    lastOpponentMove = null;
                }
            }
            else
            {
                CanvasManager.Instance.DisableButtons();
                CanvasManager.Instance.SetPlayerTurn(0);
            }
        }
    }

    void OnDestroy()
    {
        ws.Close();
    }

    void OnMessage(object sender, MessageEventArgs e)
    {
        
        Debug.Log("Received message: " + e.Data);
        string msg = e.Data.Trim().Normalize();
        string ok = "OK";
        string ct = "change turn";

        Player winner;

        winner = ExistsWinner(e.Data);

        if (msg.Equals(ok))
        {
            gameStartReceived = true;
        }
        else if (winner != null)
        {
            Debug.Log("VINCITORE: " + winner.ToString());
            winnerPlayer = winner;
            showWinnerScreen = true;
        }
        else if (msg.Equals(ct))
        {
            Debug.Log("Changing turn");
            playerTurn = playerTurn == match.playerO ? match.playerX : match.playerO;
            turnAlreadyUpdated = false;
        }
        else
        {
            Debug.Log("My Turn and put symbol");
            lastOpponentMove = JsonConvert.DeserializeObject<Move>(e.Data);
            CanvasManager.Instance.SetEnableButton(lastOpponentMove.pos, false);
            playerTurn = myPlayer;
            turnAlreadyUpdated = false;
        }
    }

    private Player ExistsWinner(string data)
    {
        try
        {
            string[] parts = data.Split('-');
            if (parts.Length == 2)
            { // O parts[0] == "winner";
                var winner = JsonConvert.DeserializeObject<Player>(parts[1]);
                return winner;
            }
            else
            {
                return null;
            }
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            return null;
        }
    }

    public void SendMessage(string message)
    {
        ws.Send(message);
    }
}
