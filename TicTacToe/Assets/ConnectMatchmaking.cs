using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ConnectMatchmaking : MonoBehaviour
{
    [SerializeField]
    private TMP_InputField usernameField;
    IEnumerator SendRequest()
    {
        string username = usernameField.text;
        // L'URL dell'orchestratore a cui inviare la richiesta
        string url = "http://204.216.215.36:9092/orchestrator/find/getmatch/" + username;
        //string url = "localhost:9092/orchestrator/find/getmatch/" + username;
        Debug.Log(url);

        // Creare una richiesta POST
        UnityWebRequest request = UnityWebRequest.Get(url);

        // Aggiungere i dati della richiesta (se necessario)
        //request.SetRequestHeader("Content-Type", "application/json");
        //request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(jsonString));

        // Attendere la risposta dell'orchestratore
        yield return request.SendWebRequest();

        // Gestire la risposta
        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Richiesta inviata con successo per " + username + "!");
            string response = request.downloadHandler.text;
            Debug.LogWarning(response);

            try
            {
                //TODO: Controllare che sia convertibile in un match
                Match match = JsonConvert.DeserializeObject<Match>(response);

                Debug.LogWarning("playerX ID: " + match.playerX.id);
                Debug.LogWarning("playerX Name: " + match.playerX.username);
                Debug.LogWarning("playerO ID: " + match.playerO.id);
                Debug.LogWarning("playerO Name: " + match.playerO.username);
                Debug.LogWarning("Player Start: " + match.starter.username);

                PlayerPrefs.SetString("MatchData", JsonConvert.SerializeObject(match));
                //PlayerPrefs.SetString("Username", username);
                StaticVariables.Username = username;

                SceneManager.LoadScene(1);

            }
            catch (JsonReaderException ex)
            {
                // La stringa non è convertibile in JSON, gestisci l'eccezione qui
                Debug.LogError(ex.Message.ToString());
            }
        }
        else
        {
            Debug.LogError("Errore durante l'invio della richiesta: " + request.error);
        }
    }

    public void OnClickPlay()
    {
        StartCoroutine(SendRequest());
    }

}
