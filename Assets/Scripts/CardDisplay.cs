using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardDisplay : MonoBehaviour
{
    [SerializeField]
    private RawImage cardImage;
    [SerializeField]
    private TMP_Text cardNameText;
    [SerializeField]
    private TMP_Text cardSpeciesText;

    public void SetData(Character character)
    {
        cardNameText.text = character.name;
        cardSpeciesText.text = character.species;
    }

    public void SetImage(Texture2D texture)
    {
        cardImage.texture = texture;
    }
}