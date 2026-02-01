using UnityEngine;
using UnityEngine.UI;

public class VictoryScreen : MonoBehaviour
{
    private GameObject panel;
    private Text masksText;

    public void Initialize(Transform canvasTransform)
    {
        panel = RuntimeUIBuilder.CreatePanel(canvasTransform, "VictoryPanel",
            new Color(0f, 0f, 0f, 0.85f),
            Vector2.zero, Vector2.one);

        var title = RuntimeUIBuilder.CreateText(panel.transform, "Title", "VICTORY!",
            48, TextAnchor.MiddleCenter, Color.yellow);
        RuntimeUIBuilder.SetAnchors(title.rectTransform,
            new Vector2(0.1f, 0.6f), new Vector2(0.9f, 0.85f));

        masksText = RuntimeUIBuilder.CreateText(panel.transform, "MasksText", "",
            20, TextAnchor.MiddleCenter, Color.white);
        RuntimeUIBuilder.SetAnchors(masksText.rectTransform,
            new Vector2(0.15f, 0.3f), new Vector2(0.85f, 0.55f));

        var prompt = RuntimeUIBuilder.CreateText(panel.transform, "Prompt",
            "Press any key to quit",
            18, TextAnchor.MiddleCenter, new Color(0.7f, 0.7f, 0.7f));
        RuntimeUIBuilder.SetAnchors(prompt.rectTransform,
            new Vector2(0.2f, 0.1f), new Vector2(0.8f, 0.25f));

        panel.SetActive(false);
    }

    public void Show(MaskInventory inventory)
    {
        string text = "Masks Collected:\n\n";
        if (inventory != null)
        {
            foreach (var mask in inventory.CollectedMasks)
                text += $"  {mask.maskName} ({mask.element})\n";
        }
        masksText.text = text;
        panel.SetActive(true);
    }

    void Update()
    {
        if (panel != null && panel.activeSelf && Input.anyKeyDown)
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
