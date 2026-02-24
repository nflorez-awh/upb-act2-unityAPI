using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardDisplay : MonoBehaviour
{
    [SerializeField] private RawImage cardImage;
    [SerializeField] private TMP_Text cardNameText;
    [SerializeField] private TMP_Text cardSpeciesText;

    public void SetData(RickMortyCharacter character)
    {
        if (cardNameText != null) cardNameText.text = character.name;
        if (cardSpeciesText != null) cardSpeciesText.text = character.species;
    }

    public void SetImage(Texture2D texture)
    {
        if (cardImage != null)
            cardImage.texture = texture;
    }
}