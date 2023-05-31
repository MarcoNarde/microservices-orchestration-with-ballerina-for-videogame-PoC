using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CanvasManager : MonoBehaviour
{
    public static CanvasManager Instance;
    [SerializeField] private TMP_Text playerTurnText;

    [SerializeField] private Button topLeft;
    [SerializeField] private Button topCenter;
    [SerializeField] private Button topRight;
    [SerializeField] private Button middleLeft;
    [SerializeField] private Button middleCenter;
    [SerializeField] private Button middleRight;
    [SerializeField] private Button bottomLeft;
    [SerializeField] private Button bottomCenter;
    [SerializeField] private Button bottomRight;
    [SerializeField] private GameObject gameOverScreen;
    [SerializeField] private Button backButton;

    private bool[] enableButtons = new bool[9] { true, true, true, true, true, true, true, true, true };

    private const string LABEL_YOU_TURN = "You";
    private const string LABEL_OPPPONENT_TURN = "Opponent";

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {

        DisableButtons();

        topLeft.onClick.AddListener(() => BtnClick(topLeft, 0));
        topCenter.onClick.AddListener(() => BtnClick(topCenter, 1));
        topRight.onClick.AddListener(() => BtnClick(topRight, 2));
        middleLeft.onClick.AddListener(() => BtnClick(middleLeft, 3));
        middleCenter.onClick.AddListener(() => BtnClick(middleCenter, 4));
        middleRight.onClick.AddListener(() => BtnClick(middleRight, 5));
        bottomLeft.onClick.AddListener(() => BtnClick(bottomLeft, 6));
        bottomCenter.onClick.AddListener(() => BtnClick(bottomCenter, 7));
        bottomRight.onClick.AddListener(() => BtnClick(bottomRight, 8));
        backButton.onClick.AddListener(() => BackBtnClick());
    }

    private void BackBtnClick()
    {
        PlayerPrefs.SetString("MatchData", null);

        SceneManager.LoadScene(0);
    }

    private void BtnClick(Button btn, int id)
    {
        Debug.Log("Cliccato bottone: " + id);
        TMP_Text text = btn.GetComponentInChildren<TMP_Text>();
        text.text = GameManager.Instance.symbol;
        enableButtons[id] = false;

        Move move = new Move();
        move.p = GameManager.Instance.myPlayer;
        move.pos = id;
        move.gameKey = GameManager.Instance.matchId;
        GameManager.Instance.SendMessage(JsonConvert.SerializeObject(move));
    }

    public void SetPlayerTurn(int turn)
    {
        if(turn == 0)
        {
            Debug.Log("OPPONENT!!!");
            playerTurnText.text = LABEL_OPPPONENT_TURN;
        }
        else
        {
            playerTurnText.text = LABEL_YOU_TURN;
        }
    }

    public void EnableButtons()
    {
        //TODO: Codice migliorabile
        if (enableButtons[0])
            topLeft.interactable = true;
        if (enableButtons[1])
            topCenter.interactable = true;
        if (enableButtons[2])
            topRight.interactable = true;
        if (enableButtons[3])
            middleLeft.interactable = true;
        if (enableButtons[4])
            middleCenter.interactable = true;
        if (enableButtons[5])
            middleRight.interactable = true;
        if (enableButtons[6])
            bottomLeft.interactable = true;
        if (enableButtons[7])
            bottomCenter.interactable = true;
        if (enableButtons[8])
            bottomRight.interactable = true;
    }

    public void DisableButtons()
    {
        topLeft.interactable = false;
        topCenter.interactable = false;
        topRight.interactable = false;
        middleLeft.interactable = false;
        middleCenter.interactable = false;
        middleRight.interactable = false;
        bottomLeft.interactable = false;
        bottomCenter.interactable = false;
        bottomRight.interactable = false;
    }

    public void ApplyMove(int pos, string symb)
    {
        try
        {
            switch (pos)
            {
                case 0:
                    TMP_Text TLT = topLeft.GetComponentInChildren<TMP_Text>();
                    TLT.text = symb;
                    break;
                case 1:
                    TMP_Text TCT = topCenter.GetComponentInChildren<TMP_Text>();
                    TCT.text = symb;
                    break;
                case 2:
                    TMP_Text TRT = topRight.GetComponentInChildren<TMP_Text>();
                    TRT.text = symb;
                    break;
                case 3:
                    TMP_Text MLT = middleLeft.GetComponentInChildren<TMP_Text>();
                    MLT.text = symb;
                    break;
                case 4:
                    TMP_Text MCT = middleCenter.GetComponentInChildren<TMP_Text>();
                    MCT.text = symb;
                    break;
                case 5:
                    TMP_Text MRT = middleRight.GetComponentInChildren<TMP_Text>();
                    MRT.text = symb;
                    break;
                case 6:
                    TMP_Text BLT = bottomLeft.GetComponentInChildren<TMP_Text>();
                    BLT.text = symb;
                    break;
                case 7:
                    TMP_Text BCT = bottomCenter.GetComponentInChildren<TMP_Text>();
                    BCT.text = symb;
                    break;
                case 8:
                    TMP_Text BRT = bottomRight.GetComponentInChildren<TMP_Text>();
                    BRT.text = symb;
                    break;
            }
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }

    private void OnDestroy()
    {
        topLeft.onClick.RemoveListener(() => BtnClick(topLeft, 0));
        topCenter.onClick.RemoveListener(() => BtnClick(topCenter, 1));
        topRight.onClick.RemoveListener(() => BtnClick(topRight, 2));
        middleLeft.onClick.RemoveListener(() => BtnClick(middleLeft, 3));
        middleCenter.onClick.RemoveListener(() => BtnClick(middleCenter, 4));
        middleRight.onClick.RemoveListener(() => BtnClick(middleRight, 5));
        bottomLeft.onClick.RemoveListener(() => BtnClick(bottomLeft, 6));
        bottomCenter.onClick.RemoveListener(() => BtnClick(bottomCenter, 7));
        bottomRight.onClick.RemoveListener(() => BtnClick(bottomRight, 8));
    }
    public void SetEnableButton(int pos, bool enable)
    {
        enableButtons[pos] = enable;
    }

    internal void ShowGameOverScreen(bool v, bool isTie)
    {
        if (isTie)
        {
            gameOverScreen.GetComponentInChildren<TMP_Text>().text = "GAME DRAWN";
        }
        else
        {
            gameOverScreen.GetComponentInChildren<TMP_Text>().text = v ? "YOU WIN" : "OPPONENT WINS";
        }

        
        gameOverScreen.SetActive(true);
    }
}
