using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;

public class DeckManager : MonoBehaviour
{
    [Header("API Jugadores — JSONPlaceholder")]
    [SerializeField] private string usersApiUrl = "https://jsonplaceholder.typicode.com/users";

    [Header("API Baraja — Deck of Cards")]
    private string deckApiBase = "https://deckofcardsapi.com/api/deck";

    [Header("UI - Info Jugador")]
    [SerializeField] private TMP_Text playerNameText;
    [SerializeField] private TMP_Text playerIdText;
    [SerializeField] private TMP_Text playerIndexText;
    [SerializeField] private TMP_Text developerNameText;
    [SerializeField] private TMP_Text statusText;

    [Header("UI - Baraja")]
    [SerializeField] private Transform cardsContainer;
    [SerializeField] private GameObject cardPrefab;

    [Header("UI - Navegación")]
    [SerializeField] private Button prevButton;
    [SerializeField] private Button nextButton;

    [Header("UI - Robar carta")]
    [SerializeField] private Button drawCardButton;
    [SerializeField] private TMP_Text remainingCardsText;

    [Header("Configuración")]
    [SerializeField] private int cardsPerPlayer = 5;
    [SerializeField] private string developerFullName = "Tu Nombre Completo";

    private Player[] allPlayers;
    private int currentPlayerIndex = 0;
    private string currentDeckId = "";

    void Start()
    {
        if (developerNameText != null)
            developerNameText.text = developerFullName;

        if (prevButton != null)
            prevButton.onClick.AddListener(PreviousPlayer);
        if (nextButton != null)
            nextButton.onClick.AddListener(NextPlayer);
        if (drawCardButton != null)
            drawCardButton.onClick.AddListener(OnDrawCardPressed);

        SetDrawButtonState(false);
        StartCoroutine(GetPlayers());
    }

    IEnumerator GetPlayers()
    {
        SetStatus("Cargando jugadores...");

        UnityWebRequest www = UnityWebRequest.Get(usersApiUrl);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            SetStatus("Error: " + www.error);
            yield break;
        }

        string json = "{\"players\":" + www.downloadHandler.text + "}";
        PlayerList list = JsonUtility.FromJson<PlayerList>(json);

        if (list == null || list.players == null || list.players.Length == 0)
        {
            SetStatus("Error: no se encontraron jugadores.");
            yield break;
        }

        allPlayers = list.players;
        SetStatus($"{allPlayers.Length} jugadores encontrados.");
        DisplayPlayer(allPlayers[0]);
    }

    void DisplayPlayer(Player player)
    {
        // Solo ID y nombre
        if (playerIdText != null) playerIdText.text = "#" + player.id;
        if (playerNameText != null) playerNameText.text = player.name;
        if (playerIndexText != null) playerIndexText.text = $"{currentPlayerIndex + 1} / {allPlayers.Length}";

        foreach (Transform child in cardsContainer)
            Destroy(child.gameObject);

        SetDrawButtonState(false);
        currentDeckId = "";

        StartCoroutine(ShuffleAndDraw());
    }

    IEnumerator ShuffleAndDraw()
    {
        SetStatus("Barajando mazo...");

        UnityWebRequest req = UnityWebRequest.Get($"{deckApiBase}/new/shuffle/?deck_count=1");
        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            SetStatus("Error creando mazo: " + req.error);
            yield break;
        }

        DeckResponse deck = JsonUtility.FromJson<DeckResponse>(req.downloadHandler.text);
        currentDeckId = deck.deck_id;

        yield return StartCoroutine(DrawCards(cardsPerPlayer));
        SetDrawButtonState(true);
    }

    IEnumerator DrawCards(int count)
    {
        SetStatus($"Repartiendo {count} carta(s)...");

        UnityWebRequest req = UnityWebRequest.Get($"{deckApiBase}/{currentDeckId}/draw/?count={count}");
        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            SetStatus("Error robando cartas: " + req.error);
            yield break;
        }

        DrawResponse response = JsonUtility.FromJson<DrawResponse>(req.downloadHandler.text);

        if (!response.success)
        {
            SetStatus("No quedan cartas en el mazo.");
            SetDrawButtonState(false);
            yield break;
        }

        foreach (CardData card in response.cards)
        {
            SpawnCard(card);
            yield return new WaitForSeconds(0.05f);
        }

        if (remainingCardsText != null)
            remainingCardsText.text = $"Quedan {response.remaining} cartas";

        SetStatus($"✅ Listo. Quedan {response.remaining} cartas.");

        if (response.remaining == 0)
            SetDrawButtonState(false);
    }

    public void OnDrawCardPressed()
    {
        if (string.IsNullOrEmpty(currentDeckId)) return;
        StartCoroutine(DrawCards(1));
    }

    void SpawnCard(CardData card)
    {
        GameObject cardObj = Instantiate(cardPrefab, cardsContainer);
        CardDisplay display = cardObj.GetComponent<CardDisplay>();
        if (display != null) display.SetData(card);
        StartCoroutine(LoadImage(card.image, display));
    }

    IEnumerator LoadImage(string url, CardDisplay display)
    {
        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(url))
        {
            yield return uwr.SendWebRequest();
            if (uwr.result == UnityWebRequest.Result.Success)
            {
                Texture2D tex = DownloadHandlerTexture.GetContent(uwr);
                if (display != null) display.SetImage(tex);
            }
        }
    }

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

    void SetDrawButtonState(bool state)
    {
        if (drawCardButton != null)
            drawCardButton.interactable = state;
    }

    void SetStatus(string msg)
    {
        if (statusText != null) statusText.text = msg;
        Debug.Log(msg);
    }
}

// ─────────────────────────────────────────────
// MODELOS
// ─────────────────────────────────────────────

[System.Serializable]
public class Player
{
    public int id;
    public string name;
}

[System.Serializable]
public class PlayerList
{
    public Player[] players;
}

[System.Serializable]
public class DeckResponse
{
    public bool success;
    public string deck_id;
    public int remaining;
}

[System.Serializable]
public class DrawResponse
{
    public bool success;
    public string deck_id;
    public CardData[] cards;
    public int remaining;
}

[System.Serializable]
public class CardData
{
    public string code;
    public string image;
    public string value;
    public string suit;
}