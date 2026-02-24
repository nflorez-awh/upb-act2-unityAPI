using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;

public class DeckManager : MonoBehaviour
{
    [Header("API Falsa - my-json-server")]
    [SerializeField] private string playersUrl = "https://my-json-server.typicode.com/nflorez-awh/upb-act2-unityAPI/players";

    [Header("API Tercero - Rick and Morty")]
    private string rickMortyUrl = "https://rickandmortyapi.com/api/character";

    [Header("UI - Jugador")]
    [SerializeField] private TMP_Text playerNameText;
    [SerializeField] private TMP_Text playerIndexText;
    [SerializeField] private TMP_Text developerNameText;
    [SerializeField] private TMP_Text statusText;

    [Header("UI - Baraja")]
    [SerializeField] private Transform cardsContainer;
    [SerializeField] private GameObject cardPrefab;

    [Header("UI - Navegacion")]
    [SerializeField] private Button prevButton;
    [SerializeField] private Button nextButton;

    [Header("Configuracion")]
    [SerializeField] private string developerFullName = "Tu Nombre Completo";

    private Player[] allPlayers;
    private int currentPlayerIndex = 0;

    void Start()
    {
        if (developerNameText != null)
            developerNameText.text = developerFullName;

        if (prevButton != null)
            prevButton.onClick.AddListener(PreviousPlayer);
        if (nextButton != null)
            nextButton.onClick.AddListener(NextPlayer);

        StartCoroutine(GetPlayers());
    }

    // ─────────────────────────────────────────────
    // PASO 1: Consultar API falsa
    // my-json-server devuelve un array directo
    // por eso necesitamos el wrapper
    // ─────────────────────────────────────────────
    IEnumerator GetPlayers()
    {
        SetStatus("Cargando jugadores...");

        UnityWebRequest www = UnityWebRequest.Get(playersUrl);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            SetStatus("Error: " + www.error);
            yield break;
        }

        // my-json-server devuelve un array [] — necesita wrapper
        string json = "{\"players\":" + www.downloadHandler.text + "}";
        PlayerList list = JsonUtility.FromJson<PlayerList>(json);

        if (list == null || list.players == null || list.players.Length == 0)
        {
            SetStatus("Error: no se encontraron jugadores.");
            yield break;
        }

        allPlayers = list.players;
        SetStatus(allPlayers.Length + " jugadores cargados.");
        DisplayPlayer(allPlayers[0]);
    }

    // ─────────────────────────────────────────────
    // PASO 2: Mostrar jugador y su baraja
    // ─────────────────────────────────────────────
    void DisplayPlayer(Player player)
    {
        if (playerNameText != null)
            playerNameText.text = player.name;

        if (playerIndexText != null)
            playerIndexText.text = (currentPlayerIndex + 1) + " / " + allPlayers.Length;

        foreach (Transform child in cardsContainer)
            Destroy(child.gameObject);

        StartCoroutine(LoadDeck(player.deck));
    }

    // ─────────────────────────────────────────────
    // PASO 3: Por cada ID consultar Rick and Morty
    // ─────────────────────────────────────────────
    IEnumerator LoadDeck(int[] cardIds)
    {
        SetStatus("Cargando baraja...");

        foreach (int cardId in cardIds)
        {
            SetStatus("Cargando carta #" + cardId + "...");

            UnityWebRequest req = UnityWebRequest.Get(rickMortyUrl + "/" + cardId);
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning("Error carta " + cardId + ": " + req.error);
                continue;
            }

            RickMortyCharacter character =
                JsonUtility.FromJson<RickMortyCharacter>(req.downloadHandler.text);

            SpawnCard(character);
            yield return new WaitForSeconds(0.1f);
        }

        SetStatus("Baraja lista.");
    }

    // ─────────────────────────────────────────────
    // Crear carta en la UI y cargar su imagen
    // ─────────────────────────────────────────────
    void SpawnCard(RickMortyCharacter character)
    {
        GameObject cardObj = Instantiate(cardPrefab, cardsContainer);
        CardDisplay display = cardObj.GetComponent<CardDisplay>();

        if (display != null)
            display.SetData(character);

        StartCoroutine(LoadImage(character.image, display));
    }

    IEnumerator LoadImage(string url, CardDisplay display)
    {
        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(url))
        {
            yield return uwr.SendWebRequest();

            if (uwr.result == UnityWebRequest.Result.Success)
            {
                Texture2D tex = DownloadHandlerTexture.GetContent(uwr);
                if (display != null)
                    display.SetImage(tex);
            }
            else
            {
                Debug.LogWarning("Error imagen: " + uwr.error);
            }
        }
    }

    // ─────────────────────────────────────────────
    // Navegacion entre jugadores
    // ─────────────────────────────────────────────
    public void NextPlayer()
    {
        if (allPlayers == null) return;
        currentPlayerIndex = (currentPlayerIndex + 1) % allPlayers.Length;
        DisplayPlayer(allPlayers[currentPlayerIndex]);
    }

    public void PreviousPlayer()
    {
        if (allPlayers == null) return;
        currentPlayerIndex = (currentPlayerIndex - 1 + allPlayers.Length) % allPlayers.Length;
        DisplayPlayer(allPlayers[currentPlayerIndex]);
    }

    void SetStatus(string msg)
    {
        if (statusText != null) statusText.text = msg;
        Debug.Log(msg);
    }
}

// ─────────────────────────────────────────────
// MODELOS DE DATOS
// ─────────────────────────────────────────────

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

[System.Serializable]
public class RickMortyCharacter
{
    public int id;
    public string name;
    public string species;
    public string status;
    public string image;
}