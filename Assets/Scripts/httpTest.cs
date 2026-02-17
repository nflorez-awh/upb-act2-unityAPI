using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class httpTest : MonoBehaviour
{
    [SerializeField]
    private int characterID = 1;

    [SerializeField]
    private string url = "https://rickandmortyapi.com/api/character";
    void Start()
    {
        StartCoroutine(GetText());
    }

    IEnumerator GetText()
    {
        UnityWebRequest www = UnityWebRequest.Get(url + "/" + characterID);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
        }
        else
        {
            // Show results as text
            Debug.Log(www.downloadHandler.text);

            // Or retrieve results as binary data
            byte[] results = www.downloadHandler.data;
        }
    }
}

class Character
{
    public int id;
    public string name;
    public string status;
    public string species;
    public string image;
}
