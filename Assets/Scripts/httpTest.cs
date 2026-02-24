using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;

public class DeckManager : MonoBehaviour
{
    [Header("API Falsa")]
    [SerializeField] private string deckApiUrl = "https://my-json-server.typicode.com/TU_USUARIO/TU_REPO/players";

    [Header("API Tercero")]
    [SerializeField] private string rickMortyUrl = "https://rickandmortyapi.com/api/character";

    [Header("UI")]
    [SerializeField] private TMP_Text playerNameText;
    [SerializeField] private TMP_Text developerNameText;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private Transform cardsContainer;
    [SerializeField] private GameObject cardPrefab;

    private Player[] allPlayers;
    private int currentPlayerIndex = 0;

    void Start()
    {
        developerNameText.text = "Tu Nombre Completo";
        StartCoroutine(GetPlayers());
    }

    // ─────────────────────────────────────────────
    // PASO 1: Pedir jugadores a la API falsa
    // ─────────────────────────────────────────────
    IEnumerator GetPlayers()
    {
        SetStatus("Cargando jugadores...");

        UnityWebRequest www = UnityWebRequest.Get(deckApiUrl);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            SetStatus("Error: " + www.error);
            yield break;
        }

        // Wrapper necesario para JsonUtility
        string json = "{\"players\":" + www.downloadHandler.text + "}";
        PlayerList list = JsonUtility.FromJson<PlayerList>(json);
        allPlayers = list.players;

        SetStatus($"{allPlayers.Length} jugadores cargados.");
        DisplayPlayer(allPlayers[0]);
    }

    // ─────────────────────────────────────────────
    // PASO 2: Mostrar jugador y cargar sus cartas
    // ─────────────────────────────────────────────
    void DisplayPlayer(Player player)
    {
        playerNameText.text = "Jugador: " + player.name;

        // Limpiar cartas anteriores
        foreach (Transform child in cardsContainer)
            Destroy(child.gameObject);

        // Cargar cada carta con su ID
        StartCoroutine(LoadDeck(player.deck));
    }

    // ─────────────────────────────────────────────
    // PASO 3: Por cada ID, consultar Rick & Morty
    // ─────────────────────────────────────────────
    IEnumerator LoadDeck(int[] cardIds)
    {
        foreach (int cardId in cardIds)
        {
            SetStatus($"Cargando carta #{cardId}...");

            UnityWebRequest req = UnityWebRequest.Get(rickMortyUrl + "/" + cardId);
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning("Error carta " + cardId + ": " + req.error);
                continue;
            }

            // Parsear el personaje
            RickMortyCharacter character = JsonUtility.FromJson<RickMortyCharacter>(req.downloadHandler.text);

            // Crear la carta en la UI
            GameObject cardObj = Instantiate(cardPrefab, cardsContainer);
            CardDisplay display = cardObj.GetComponent<CardDisplay>();
            display.SetData(character);

            // Cargar imagen
            StartCoroutine(LoadImage(character.image, display));

            yield return new WaitForSeconds(0.1f);
        }

        SetStatus("Baraja lista.");
    }

    IEnumerator LoadImage(string url, CardDisplay display)
    {
        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(url))
        {
            yield return uwr.SendWebRequest();

            if (uwr.result == UnityWebRequest.Result.Success)
            {
                Texture2D tex = DownloadHandlerTexture.GetContent(uwr);
                display.SetImage(tex);
            }
        }
    }

    // ─────────────────────────────────────────────
    // NAVEGACIÓN
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
    public int[] deck;      // ← solo IDs, no objetos completos
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
    public string image;    // URL de la imagen
}