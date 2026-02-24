using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro; // Necesario para TextMeshPro

public class DeckManagerTMP : MonoBehaviour
{
    [Header("API Settings")]
    [SerializeField] private string deckApiUrl = "https://tu-api-falsa.com/players"; // Cambiar por tu JSON

    [Header("UI References")]
    [SerializeField] private TMP_Text playerNameText;    // TextMeshPro para el nombre del jugador
    [SerializeField] private Transform cardsContainer;   // Contenedor de cartas
    [SerializeField] private GameObject cardPrefab;      // Prefab de carta con TMP_Text y opcional RawImage

    private Player[] allPlayers;
    private int currentPlayerIndex = 0;

    void Start()
    {
        StartCoroutine(GetPlayers());
    }

    IEnumerator GetPlayers()
    {
        UnityWebRequest www = UnityWebRequest.Get(deckApiUrl);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error al obtener jugadores: " + www.error);
        }
        else
        {
            // JsonUtility necesita un objeto raíz
            PlayerList playerList = JsonUtility.FromJson<PlayerList>("{\"players\":" + www.downloadHandler.text + "}");
            allPlayers = playerList.players;

            if (allPlayers.Length > 0)
                DisplayPlayer(allPlayers[currentPlayerIndex]);
        }
    }

    void DisplayPlayer(Player player)
    {
        playerNameText.text = player.name;

        // Limpiar cartas anteriores
        foreach (Transform child in cardsContainer)
            Destroy(child.gameObject);

        // Crear nuevas cartas
        foreach (Card card in player.cards)
        {
            GameObject cardObj = Instantiate(cardPrefab, cardsContainer);
            TMP_Text cardText = cardObj.GetComponentInChildren<TMP_Text>();
            if (cardText != null)
                cardText.text = card.name;

            // Opcional: mostrar imagen de carta
            //RawImage cardImage = cardObj.GetComponentInChildren<RawImage>();
            //StartCoroutine(GetCardImage(card.imageUrl, cardImage));
        }
    }

    public void NextPlayer()
    {
        if (allPlayers == null || allPlayers.Length == 0) return;

        currentPlayerIndex = (currentPlayerIndex + 1) % allPlayers.Length;
        DisplayPlayer(allPlayers[currentPlayerIndex]);
    }

    // Opcional: cargar imagen desde URL
    IEnumerator GetCardImage(string url, RawImage image)
    {
        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(url))
        {
            yield return uwr.SendWebRequest();
            if (uwr.result == UnityWebRequest.Result.Success)
                image.texture = DownloadHandlerTexture.GetContent(uwr);
        }
    }
}

[System.Serializable]
public class Card
{
    public int id;
    public string name;
    // public string imageUrl; // Usar si quieres mostrar imágenes
}

[System.Serializable]
public class Player
{
    public int id;
    public string name;
    public Card[] cards;
}

[System.Serializable]
public class PlayerList
{
    public Player[] players;
}