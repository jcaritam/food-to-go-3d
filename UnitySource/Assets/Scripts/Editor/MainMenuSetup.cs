using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public static class MainMenuSetup
{
    private static readonly Color ColorOcre      = HexColor("#C4873A");
    private static readonly Color ColorTerracota = HexColor("#B5451B");
    private static readonly Color ColorCrema     = HexColor("#F5E6C8");
    private static readonly Color ColorCremaOsc  = HexColor("#E8D0A0");
    private static readonly Color ColorBlanco    = HexColor("#FFFFFF");
    private static readonly Color ColorTextoOsc  = HexColor("#3B1A06");

    [MenuItem("Tools/Setup Main Menu")]
    public static void Build()
    {
        string scenePath = "Assets/Scenes/MenuScene.unity";
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        var oldCanvas = GameObject.Find("MainMenuCanvas");
        if (oldCanvas != null)
        {
            Undo.DestroyObjectImmediate(oldCanvas);
        }

        var canvasGO = new GameObject("MainMenuCanvas");
        Undo.RegisterCreatedObjectUndo(canvasGO, "Create Main Menu");

        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 0;

        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        canvasGO.AddComponent<GraphicRaycaster>();

        var bgGO = CreatePanel(canvasGO.transform, "Background",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            Vector2.zero, Vector2.zero);
        var bgImg = bgGO.GetComponent<Image>();
        bgImg.color = ColorCrema;
        StretchFull(bgGO.GetComponent<RectTransform>());

        var overlayGO = CreatePanel(bgGO.transform, "TopOverlay",
            new Vector2(0f, 1f), new Vector2(1f, 1f),
            new Vector2(0f, -300f), new Vector2(0f, 0f));
        var overlayImg = overlayGO.GetComponent<Image>();
        overlayImg.color = new Color(ColorTerracota.r, ColorTerracota.g, ColorTerracota.b, 0.85f);
        var overlayRT = overlayGO.GetComponent<RectTransform>();
        overlayRT.anchorMin = new Vector2(0f, 0.65f);
        overlayRT.anchorMax = new Vector2(1f, 1f);
        overlayRT.offsetMin = Vector2.zero;
        overlayRT.offsetMax = Vector2.zero;

        var panelGO = CreatePanel(bgGO.transform, "CenterPanel",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(600f, 700f), Vector2.zero);
        var panelImg = panelGO.GetComponent<Image>();
        panelImg.color = new Color(ColorOcre.r, ColorOcre.g, ColorOcre.b, 0.15f);

        var titleGO = new GameObject("TitleText");
        titleGO.transform.SetParent(panelGO.transform, false);
        var titleTMP = titleGO.AddComponent<TextMeshProUGUI>();
        titleTMP.text = "FOOD TO GO";
        titleTMP.fontSize = 96;
        titleTMP.fontStyle = FontStyles.Bold;
        titleTMP.color = ColorBlanco;
        titleTMP.alignment = TextAlignmentOptions.Center;
        titleTMP.enableWordWrapping = false;
        var titleRT = titleGO.GetComponent<RectTransform>();
        titleRT.anchorMin = new Vector2(0.5f, 1f);
        titleRT.anchorMax = new Vector2(0.5f, 1f);
        titleRT.pivot = new Vector2(0.5f, 1f);
        titleRT.anchoredPosition = new Vector2(0f, -60f);
        titleRT.sizeDelta = new Vector2(560f, 120f);

        var subtitleGO = new GameObject("SubtitleText");
        subtitleGO.transform.SetParent(panelGO.transform, false);
        var subtitleTMP = subtitleGO.AddComponent<TextMeshProUGUI>();
        subtitleTMP.text = "SABOR PERUANO";
        subtitleTMP.fontSize = 36;
        subtitleTMP.color = ColorCremaOsc;
        subtitleTMP.alignment = TextAlignmentOptions.Center;
        subtitleTMP.enableWordWrapping = false;
        var subtitleRT = subtitleGO.GetComponent<RectTransform>();
        subtitleRT.anchorMin = new Vector2(0.5f, 1f);
        subtitleRT.anchorMax = new Vector2(0.5f, 1f);
        subtitleRT.pivot = new Vector2(0.5f, 1f);
        subtitleRT.anchoredPosition = new Vector2(0f, -190f);
        subtitleRT.sizeDelta = new Vector2(560f, 50f);

        var sepGO = CreatePanel(panelGO.transform, "Separator",
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(400f, 4f), new Vector2(0f, -255f));
        sepGO.GetComponent<Image>().color = ColorTerracota;

        var playGO = CreateButton(panelGO.transform, "PlayButton",
            "JUGAR",
            new Vector2(0f, -300f),
            new Vector2(320f, 72f),
            ColorTerracota,
            ColorBlanco,
            fontSize: 40);

        var quitGO = CreateButton(panelGO.transform, "QuitButton",
            "SALIR",
            new Vector2(0f, -400f),
            new Vector2(320f, 64f),
            ColorOcre,
            ColorTextoOsc,
            fontSize: 34);

        var bottomGO = CreatePanel(bgGO.transform, "BottomStripe",
            new Vector2(0f, 0f), new Vector2(1f, 0f),
            Vector2.zero, Vector2.zero);
        var bottomImg = bottomGO.GetComponent<Image>();
        bottomImg.color = ColorTerracota;
        var bottomRT = bottomGO.GetComponent<RectTransform>();
        bottomRT.anchorMin = new Vector2(0f, 0f);
        bottomRT.anchorMax = new Vector2(1f, 0f);
        bottomRT.offsetMin = Vector2.zero;
        bottomRT.offsetMax = new Vector2(0f, 60f);

        var creditGO = new GameObject("CreditText");
        creditGO.transform.SetParent(bottomGO.transform, false);
        var creditTMP = creditGO.AddComponent<TextMeshProUGUI>();
        creditTMP.text = "Cocina peruana en cada entrega";
        creditTMP.fontSize = 22;
        creditTMP.color = ColorCrema;
        creditTMP.alignment = TextAlignmentOptions.Center;
        var creditRT = creditGO.GetComponent<RectTransform>();
        creditRT.anchorMin = Vector2.zero;
        creditRT.anchorMax = Vector2.one;
        creditRT.offsetMin = Vector2.zero;
        creditRT.offsetMax = Vector2.zero;

        var mainMenuUI = canvasGO.GetComponentInChildren<MainMenuUI>();
        if (mainMenuUI == null)
        {
            var uiHolder = new GameObject("MainMenuUI");
            uiHolder.transform.SetParent(canvasGO.transform, false);
            mainMenuUI = uiHolder.AddComponent<MainMenuUI>();
        }

        var playButtonComp = playGO.GetComponent<Button>();
        var quitButtonComp = quitGO.GetComponent<Button>();
        var logoRT = panelGO.GetComponent<RectTransform>();

        SerializedObject so = new SerializedObject(mainMenuUI);
        so.FindProperty("playButton").objectReferenceValue = playButtonComp;
        so.FindProperty("quitButton").objectReferenceValue = quitButtonComp;
        so.FindProperty("logoTransform").objectReferenceValue = logoRT;
        so.ApplyModifiedProperties();

        if (GameObject.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var esGO = new GameObject("EventSystem");
            esGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        Debug.Log("[MainMenuSetup] MenuScene construida con identidad visual peruana.");
    }

    private static GameObject CreatePanel(Transform parent, string name,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 sizeDelta, Vector2 anchoredPosition)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.color = Color.white;
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.sizeDelta = sizeDelta;
        rt.anchoredPosition = anchoredPosition;
        return go;
    }

    private static GameObject CreateButton(Transform parent, string name,
        string label, Vector2 anchoredPosition, Vector2 sizeDelta,
        Color bgColor, Color textColor, int fontSize)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);

        var img = go.AddComponent<Image>();
        img.color = bgColor;

        var btn = go.AddComponent<Button>();
        var colors = btn.colors;
        colors.highlightedColor = new Color(
            Mathf.Min(bgColor.r + 0.1f, 1f),
            Mathf.Min(bgColor.g + 0.1f, 1f),
            Mathf.Min(bgColor.b + 0.1f, 1f));
        colors.pressedColor = new Color(
            Mathf.Max(bgColor.r - 0.15f, 0f),
            Mathf.Max(bgColor.g - 0.15f, 0f),
            Mathf.Max(bgColor.b - 0.15f, 0f));
        btn.colors = colors;

        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 1f);
        rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.anchoredPosition = anchoredPosition;
        rt.sizeDelta = sizeDelta;

        var labelGO = new GameObject("Label");
        labelGO.transform.SetParent(go.transform, false);
        var tmp = labelGO.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = fontSize;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color = textColor;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.enableWordWrapping = false;
        var labelRT = labelGO.GetComponent<RectTransform>();
        labelRT.anchorMin = Vector2.zero;
        labelRT.anchorMax = Vector2.one;
        labelRT.offsetMin = Vector2.zero;
        labelRT.offsetMax = Vector2.zero;

        return go;
    }

    private static void StretchFull(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private static Color HexColor(string hex)
    {
        ColorUtility.TryParseHtmlString(hex, out Color c);
        return c;
    }
}
