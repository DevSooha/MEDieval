using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class UIManager : Singleton<UIManager>
{
    [Header("Fade Settings")]
    public Image fadeImage;

    [Header("Message UI Settings")]
    public GameObject messagePanel;
    public TextMeshProUGUI messageText;

    private Coroutine messageRoutine;

    protected override void Awake()
    {
        base.Awake();
        // 초기 상태: UI 모두 비활성화
        if (messagePanel != null) messagePanel.SetActive(false);
        if (fadeImage != null) fadeImage.color = new Color(0, 0, 0, 0);
    }

    #region Fade 기능
    public IEnumerator FadeIn(float duration)
    {
        float t = 0;
        while (t < duration)
        {
            t += Time.deltaTime;
            fadeImage.color = new Color(0, 0, 0, 1f - (t / duration));
            yield return null;
        }
        fadeImage.color = new Color(0, 0, 0, 0);
    }

    public IEnumerator FadeOut(float duration)
    {
        float t = 0;
        while (t < duration)
        {
            t += Time.deltaTime;
            fadeImage.color = new Color(0, 0, 0, t / duration);
            yield return null;
        }
        fadeImage.color = new Color(0, 0, 0, 1);
    }
    #endregion

    #region 메시지 기능 (Warning vs Ending)

    // [Warning] 2초 뒤에 자동으로 사라짐
    public void ShowWarning(string message)
    {
        if (messageRoutine != null) StopCoroutine(messageRoutine);

        messagePanel.SetActive(true);
        messageText.text = message;

        messageRoutine = StartCoroutine(HideMessageRoutine());
    }

    // [Ending] 자동으로 사라지지 않음 (EndingEvent에서 사용)
    public void ShowEnding(string message)
    {
        if (messageRoutine != null) StopCoroutine(messageRoutine);

        messagePanel.SetActive(true);
        messageText.text = message;
    }

    // UI를 수동으로 닫고 싶을 때 호출
    public void HideMessage()
    {
        if (messageRoutine != null) StopCoroutine(messageRoutine);
        messagePanel.SetActive(false);
    }

    IEnumerator HideMessageRoutine()
    {
        yield return new WaitForSeconds(2.0f);
        messagePanel.SetActive(false);
    }
    #endregion
}