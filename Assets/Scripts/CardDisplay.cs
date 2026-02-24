using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text speciesText;
    [SerializeField] private TMP_Text idText;
    [SerializeField] private RawImage characterImage;

    public void SetData(RickMortyCharacter character)
    {
        nameText.text = character.name;
        speciesText.text = character.species + " · " + character.status;
        idText.text = "#" + character.id;
    }

    public void SetImage(Texture2D texture)
    {
        if (characterImage != null)
            characterImage.texture = texture;
    }
}