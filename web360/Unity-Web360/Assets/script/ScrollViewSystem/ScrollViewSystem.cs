using UnityEngine;
using UnityEngine.UI;

public class ScrollViewSystem : MonoBehaviour
{
    private ScrollRect _scrollRect;
    [SerializeField] ScrollButton _leftButton;
    [SerializeField] ScrollButton _rightButton;
    [SerializeField] private float scrollSpeed = 0.01f;

    // Start is called before the first frame update
    void Start()
    {
        _scrollRect = GetComponent<ScrollRect>();
    }

    // Update is called once per frame
    void Update()
    {
        if (_leftButton != null)
        {
            if (_leftButton.isDown)
            {
                ScrollLeft();
            }

        }

        if (_rightButton != null)
        {
            if (_rightButton.isDown)
            {
                ScrollRight();
            }

        }
    }

    private void ScrollLeft()
    {
        if (_scrollRect != null)
        {
            if (_scrollRect.horizontalNormalizedPosition >= 0f)
            {
                _scrollRect.horizontalNormalizedPosition -= scrollSpeed;
            }
        }
    }

    private void ScrollRight()
    {
        if (_scrollRect != null)
        {
            if (_scrollRect.horizontalNormalizedPosition <= 1f)
            {
                _scrollRect.horizontalNormalizedPosition += scrollSpeed;
            }
        }
    }



}
