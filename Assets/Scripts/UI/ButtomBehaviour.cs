using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public struct ButtomImageOriginalColor
{
    public Image image;
    public Color originalColor;

    public ButtomImageOriginalColor(Image img, Color color)
    {
        image = img;
        originalColor = color;
    }
}

namespace UI
{
    public class ButtomBehaviour : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public Color hoverColor = Color.yellow;
        private List<ButtomImageOriginalColor> _originalColors = new List<ButtomImageOriginalColor>();
        private Vector3 _originalScale;
        private Image[] _images;

        private void Awake()
        {
            _images = GetComponentsInChildren<Image>();
            if (_images.Length > 0)
            {
                foreach (var image in _images)
                {
                    if (image) _originalColors.Add(new ButtomImageOriginalColor(image, image.color));
                }
            }

            _originalScale = transform.localScale;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            foreach (var image in _images)
                image.color = hoverColor;
            transform.localScale = _originalScale * 1.2f;
            SoundManager.PlaySfx(GlobalSfxTypes.MENUCLICK, 0);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            foreach (var image in _images)
                image.color = _originalColors.Find(img => img.image == image).originalColor;
                
            transform.localScale = _originalScale;
        }

        private void OnDisable()
        {
            OnPointerExit(null);
        }

        public void OnClick()
        {
            SoundManager.PlaySfx(GlobalSfxTypes.MENUCLICK, 1);
        }
    }
}
