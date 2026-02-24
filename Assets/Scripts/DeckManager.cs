using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;

public class DeckManager : MonoBehaviour
{
    [SerializeField]
    private string playersURL = "https://my-json-server.typicode.com/nflorez-awh/upb-act2-unityAPI/players";
    [SerializeField]
    private string rickMortyURL = "https://rickandmortyapi.com/api/character";
    [SerializeField]

    private TMP_Text playerNameText;
    [SerializeField]
    private TMP_Text playerIndexText;
    [SerializeField]

    private TMP_Text statusText;

    [SerializeField]
    private Transform cardsContainer;
    [SerializeField]
    private GameObject cardPrefab;

    [SerializeField]
    private Button prevButton;
    [SerializeField]
    private Button nextButton;

    private Player[] players;
    private int currentIndex = 0;

    void Start()
    {
        prevButton.onClick.AddListener(PreviousPlayer);
        nextButton.onClick.AddListener(NextPlayer);
        StartCoroutine(GetPlayers());
    }

    IEnumerator GetPlayers()
    {
        statusText.text = "Cargando jugadores...";

        UnityWebRequest www = UnityWebRequest.Get(playersURL);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
            statusText.text = "Error: " + www.error;
        }
        else
        {
            string json = "{\"players\":" + www.downloadHandler.text + "}";
            PlayerList list = JsonUtility.FromJson<PlayerList>(json);
            players = list.players;

            Debug.Log("Jugadores cargados: " + players.Length);
            ShowPlayer(players[currentIndex]);
        }
    }

    void ShowPlayer(Player player)
    {
        playerNameText.text = player.name;
        playerIndexText.text = (currentIndex + 1) + " / " + players.Length;

        foreach (Transform child in cardsContainer)
            Destroy(child.gameObject);

        StartCoroutine(GetDeck(player.deck));
    }

    IEnumerator GetDeck(int[] deck)
    {
        statusText.text = "Cargando baraja...";

        foreach (int cardId in deck)
        {
            UnityWebRequest www = UnityWebRequest.Get(rickMortyURL + "/" + cardId);
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
                if (www.responseCode == 404)
                    Debug.Log("Carta no encontrada: " + cardId);
            }
            else
            {
                Character character = JsonUtility.FromJson<Character>(www.downloadHandler.text);
                Debug.Log(character.name + " is a " + character.species);

                GameObject cardObj = Instantiate(cardPrefab, cardsContainer);
                CardDisplay display = cardObj.GetComponent<CardDisplay>();
                display.SetData(character);

                StartCoroutine(GetTexture(character.image, display));
            }

            yield return new WaitForSeconds(0.1f);
        }

        statusText.text = "Baraja lista.";
    }

    IEnumerator GetTexture(string imageUrl, CardDisplay display)
    {
        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(imageUrl))
        {
            yield return uwr.SendWebRequest();

            if (uwr.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(uwr.error);
            }
            else
            {
                var texture = DownloadHandlerTexture.GetContent(uwr);
                display.SetImage(texture);
            }
        }
    }

    public void NextPlayer()
    {
        currentIndex = (currentIndex + 1) % players.Length;
        ShowPlayer(players[currentIndex]);
    }

    public void PreviousPlayer()
    {
        currentIndex = (currentIndex - 1 + players.Length) % players.Length;
        ShowPlayer(players[currentIndex]);
    }
}

[System.Serializable]
public class Character
{
    public int id;
    public string name;
    public string species;
    public string image;
}

[System.Serializable]
public class Player
{
    public int id;
    public string name;
    public int[] deck;
}

[System.Serializable]
public class PlayerList
{
    public Player[] players;
}