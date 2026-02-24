using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardDisplay : MonoBehaviour
{
    [SerializeField] private RawImage cardImage;
    [SerializeField] private TMP_Text cardNameText;
    [SerializeField] private TMP_Text cardSuitText;

    public void SetData(CardData card)
    {
        if (cardNameText != null)
            cardNameText.text = card.value;

        if (cardSuitText != null)
            cardSuitText.text = card.suit;
    }

    public void SetImage(Texture2D texture)
    {
        if (cardImage != null)
            cardImage.texture = texture;
    }
}