using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// Ajuste l'UI au runtime (safe area + lisibilite mobile).
public class UIRuntimePolish : MonoBehaviour
{
    private const string NomRacineSafeArea = "SafeAreaRoot";
    private static UIRuntimePolish instance;

    private Rect derniereSafeArea;
    private bool styleApplique;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void InitialiserAuChargement()
    {
        if (instance != null)
            return;

        UIRuntimePolish existant = FindFirstObjectByType<UIRuntimePolish>();
        if (existant != null)
        {
            instance = existant;
            return;
        }

        GameObject objet = new GameObject(nameof(UIRuntimePolish));
        DontDestroyOnLoad(objet);
        instance = objet.AddComponent<UIRuntimePolish>();
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    private void Update()
    {
        if (!styleApplique || Screen.safeArea != derniereSafeArea)
            AppliquerStyleInterface();
    }

    private void AppliquerStyleInterface()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null || canvas.renderMode == RenderMode.WorldSpace)
            return;

        RectTransform racineSafeArea = ObtenirOuCreerRacineSafeArea(canvas);
        AppliquerAncragesSafeArea(racineSafeArea);

        TMP_Text texteTour = TrouverParNom<TMP_Text>("TurnText");
        TMP_Text texteInstruction = TrouverParNom<TMP_Text>("InstructionText");
        Button boutonNouvellePartie = TrouverParNom<Button>("ReplayButton");
        Button boutonRepositionner = TrouverParNom<Button>("RepositionButton");
        Image imagePanneauFin = TrouverParNom<Image>("GameOverPanel");
        TMP_Text texteFin = TrouverParNom<TMP_Text>("GameOverText");

        if (texteTour != null)
        {
            StyliserTexteHaut(texteTour, new Vector2(0f, -76f), new Vector2(880f, 86f), 52f);
            CreerOuMettreAJourBadge(texteTour, new Color(0.03f, 0.08f, 0.18f, 0.72f), new Vector2(36f, 18f));
        }

        if (texteInstruction != null)
        {
            StyliserTexteHaut(texteInstruction, new Vector2(0f, -152f), new Vector2(920f, 64f), 30f);
            texteInstruction.color = new Color(0.91f, 0.95f, 1f, 1f);
            CreerOuMettreAJourBadge(texteInstruction, new Color(0.02f, 0.05f, 0.11f, 0.62f), new Vector2(34f, 14f));
        }

        if (boutonRepositionner != null)
            StyliserBouton(boutonRepositionner, new Color(0.10f, 0.57f, 0.37f, 1f), new Vector2(320f, 92f));

        if (boutonNouvellePartie != null)
            StyliserBouton(boutonNouvellePartie, new Color(0.89f, 0.25f, 0.27f, 1f), new Vector2(340f, 96f));

        if (imagePanneauFin != null)
            imagePanneauFin.color = new Color(0.02f, 0.03f, 0.07f, 0.76f);

        if (texteFin != null)
        {
            texteFin.fontSize = 62f;
            texteFin.alignment = TextAlignmentOptions.Center;
            RectTransform rect = texteFin.rectTransform;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(0f, 220f);
            rect.sizeDelta = new Vector2(920f, 220f);
        }

        styleApplique = true;
        derniereSafeArea = Screen.safeArea;
    }

    private static void StyliserTexteHaut(TMP_Text texte, Vector2 position, Vector2 taille, float taillePolice)
    {
        RectTransform rect = texte.rectTransform;
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.anchoredPosition = position;
        rect.sizeDelta = taille;

        texte.fontSize = taillePolice;
        texte.alignment = TextAlignmentOptions.Center;
    }

    private static void StyliserBouton(Button bouton, Color couleurFond, Vector2 taille)
    {
        RectTransform rect = bouton.GetComponent<RectTransform>();
        rect.sizeDelta = taille;

        Image image = bouton.GetComponent<Image>();
        if (image != null)
            image.color = couleurFond;

        ColorBlock couleurs = bouton.colors;
        couleurs.normalColor = Color.white;
        couleurs.highlightedColor = new Color(0.95f, 0.95f, 0.95f, 1f);
        couleurs.selectedColor = new Color(0.95f, 0.95f, 0.95f, 1f);
        couleurs.pressedColor = new Color(0.79f, 0.79f, 0.79f, 1f);
        couleurs.disabledColor = new Color(0.55f, 0.55f, 0.55f, 0.5f);
        bouton.colors = couleurs;

        TMP_Text texte = bouton.GetComponentInChildren<TMP_Text>(true);
        if (texte == null)
            return;

        texte.fontSize = 32f;
        texte.alignment = TextAlignmentOptions.Center;
        texte.color = Color.white;
    }

    private static void CreerOuMettreAJourBadge(TMP_Text texte, Color couleur, Vector2 margeSupplementaire)
    {
        Transform parent = texte.transform.parent;
        string nomBadge = texte.name + "Badge";
        Image badge = parent.Find(nomBadge)?.GetComponent<Image>();

        if (badge == null)
        {
            GameObject objetBadge = new GameObject(nomBadge, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            objetBadge.transform.SetParent(parent, false);
            badge = objetBadge.GetComponent<Image>();
        }

        RectTransform rectTexte = texte.rectTransform;
        RectTransform rectBadge = badge.rectTransform;

        rectBadge.anchorMin = rectTexte.anchorMin;
        rectBadge.anchorMax = rectTexte.anchorMax;
        rectBadge.pivot = rectTexte.pivot;
        rectBadge.anchoredPosition = rectTexte.anchoredPosition;
        rectBadge.sizeDelta = rectTexte.sizeDelta + margeSupplementaire;

        badge.color = couleur;
        badge.raycastTarget = false;
        badge.transform.SetSiblingIndex(texte.transform.GetSiblingIndex());
    }

    private RectTransform ObtenirOuCreerRacineSafeArea(Canvas canvas)
    {
        RectTransform rectCanvas = canvas.GetComponent<RectTransform>();
        Transform existant = rectCanvas.Find(NomRacineSafeArea);
        if (existant != null)
            return (RectTransform)existant;

        GameObject racine = new GameObject(NomRacineSafeArea, typeof(RectTransform));
        RectTransform rectRacine = racine.GetComponent<RectTransform>();
        rectRacine.SetParent(rectCanvas, false);

        for (int i = rectCanvas.childCount - 1; i >= 0; i--)
        {
            Transform enfant = rectCanvas.GetChild(i);
            if (enfant != rectRacine)
                enfant.SetParent(rectRacine, false);
        }

        rectRacine.SetAsFirstSibling();
        return rectRacine;
    }

    private static void AppliquerAncragesSafeArea(RectTransform racineSafeArea)
    {
        Rect safeArea = Screen.safeArea;

        Vector2 min = safeArea.position;
        Vector2 max = safeArea.position + safeArea.size;

        min.x /= Screen.width;
        min.y /= Screen.height;
        max.x /= Screen.width;
        max.y /= Screen.height;

        racineSafeArea.anchorMin = min;
        racineSafeArea.anchorMax = max;
        racineSafeArea.offsetMin = Vector2.zero;
        racineSafeArea.offsetMax = Vector2.zero;
    }

    private static T TrouverParNom<T>(string nomObjet) where T : Component
    {
        GameObject objet = GameObject.Find(nomObjet);
        return objet != null ? objet.GetComponent<T>() : null;
    }
}
