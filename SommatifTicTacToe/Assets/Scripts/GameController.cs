using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    [Header("Prefabs Symboles")]
    [FormerlySerializedAs("player1SpherePrefab")]
    [SerializeField] private GameObject prefabSymboleJoueurX;
    [FormerlySerializedAs("player2SpherePrefab")]
    [SerializeField] private GameObject prefabSymboleJoueurO;
    [FormerlySerializedAs("useCustomSymbolPrefabs")]
    [SerializeField] private bool utiliserPrefabsPersonnalises;

    [Header("Interface")]
    [FormerlySerializedAs("turnText")]
    [SerializeField] private TMP_Text texteTour;
    [FormerlySerializedAs("gameOverPanel")]
    [SerializeField] private GameObject panneauFinPartie;
    [FormerlySerializedAs("gameOverText")]
    [SerializeField] private TMP_Text texteFinPartie;
    [FormerlySerializedAs("instructionText")]
    [SerializeField] private TMP_Text texteInstruction;
    [FormerlySerializedAs("introPanel")]
    [SerializeField] private GameObject panneauIntroduction;

    [TextArea(4, 8)]
    [FormerlySerializedAs("introInstructions")]
    [SerializeField] private string consignesIntroduction =
        "Comment jouer:\n" +
        "1. Touchez une case vide pour poser votre symbole.\n" +
        "2. Alignez 3 symboles pour gagner.\n" +
        "3. Utilisez Repositionner si vous voulez replacer la grille.";

    [FormerlySerializedAs("scanInstruction")]
    [SerializeField] private string messageScannerSurface = "Scannez une surface...";
    [FormerlySerializedAs("readyInstruction")]
    [SerializeField] private string messagePretAJouer = "Jouez !";

    [Header("Effets Victoire")]
    [FormerlySerializedAs("player1WinColor")]
    [SerializeField] private Color couleurVictoireX = new Color(1f, 0.46f, 0.34f, 1f);
    [FormerlySerializedAs("player2WinColor")]
    [SerializeField] private Color couleurVictoireO = new Color(0.30f, 0.85f, 1f, 1f);

    [Header("Etat Partie")]
    [FormerlySerializedAs("isPlayer1Turn")]
    [SerializeField] private bool tourDuJoueurX = true;

    public enum GameState { WaitingForPlacement, Playing, GameOver }

    [FormerlySerializedAs("currentState")]
    public GameState etatCourant = GameState.WaitingForPlacement;

    private readonly CellController.CellState[] plateau = new CellController.CellState[9];
    private CellController[] cellules;
    private ARPlacementManager gestionnairePlacementAR;
    private bool partieDemarree;
    private bool boutonsConfigures;

    private static readonly int[][] CombinaisonsGagnantes =
    {
        new[] { 0, 1, 2 }, new[] { 3, 4, 5 }, new[] { 6, 7, 8 },
        new[] { 0, 3, 6 }, new[] { 1, 4, 7 }, new[] { 2, 5, 8 },
        new[] { 0, 4, 8 }, new[] { 2, 4, 6 }
    };

    private void Start()
    {
        InitialiserPlateau();
        RafraichirCellulesActives();
        gestionnairePlacementAR = FindAnyObjectByType<ARPlacementManager>();

        if (gestionnairePlacementAR != null)
            gestionnairePlacementAR.BoardPlacementChanged += GererChangementPlacement;

        if (panneauFinPartie != null)
        {
            panneauFinPartie.SetActive(false);
            Image imagePanneauFin = panneauFinPartie.GetComponent<Image>();
            if (imagePanneauFin != null)
                imagePanneauFin.raycastTarget = false;
        }

        bool grilleDejaPlacee = gestionnairePlacementAR != null && gestionnairePlacementAR.IsBoardPlaced;
        etatCourant = grilleDejaPlacee ? GameState.Playing : GameState.WaitingForPlacement;
        partieDemarree = false;

        CreerPanneauIntroductionSiAbsent();
        if (panneauIntroduction != null)
            panneauIntroduction.SetActive(true);

        ConfigurerBoutonsInterface();
        MettreAJourInterfaceEtat();
    }

    private void OnDestroy()
    {
        if (gestionnairePlacementAR != null)
            gestionnairePlacementAR.BoardPlacementChanged -= GererChangementPlacement;
    }

    public void PlaceSymbol(CellController cellule)
    {
        if (etatCourant != GameState.Playing || cellule == null)
            return;

        if (cellule.currentState != CellController.CellState.Empty)
            return;

        GameObject prefabSymbole = DeterminerPrefabSymbole();
        CellController.CellState etatSymbole = tourDuJoueurX
            ? CellController.CellState.Player1
            : CellController.CellState.Player2;

        cellule.PlaceSymbol(prefabSymbole, etatSymbole);
        plateau[cellule.cellIndex] = etatSymbole;

        if (TrouverLigneGagnante(out CellController.CellState gagnant, out int[] ligneGagnante))
        {
            etatCourant = GameState.GameOver;
            MettreEnValeurLigneGagnante(ligneGagnante, gagnant);
            AfficherFinDePartie(gagnant == CellController.CellState.Player1 ? "X A GAGNE !" : "O A GAGNE !");
            return;
        }

        if (VerifierMatchNul())
        {
            etatCourant = GameState.GameOver;
            AfficherFinDePartie("MATCH NUL");
            return;
        }

        tourDuJoueurX = !tourDuJoueurX;
        MettreAJourTexteTour();
    }

    public void ResetGame()
    {
        RafraichirCellulesActives();

        foreach (CellController cellule in cellules)
        {
            if (cellule != null)
                cellule.ResetCell();
        }

        InitialiserPlateau();
        tourDuJoueurX = true;
        etatCourant = (gestionnairePlacementAR != null && !gestionnairePlacementAR.IsBoardPlaced)
            ? GameState.WaitingForPlacement
            : GameState.Playing;

        if (panneauFinPartie != null)
            panneauFinPartie.SetActive(false);

        MettreAJourInterfaceEtat();
    }

    public void ResetPlacement()
    {
        if (gestionnairePlacementAR == null)
            gestionnairePlacementAR = FindAnyObjectByType<ARPlacementManager>();

        if (gestionnairePlacementAR != null)
            gestionnairePlacementAR.ResetPlacement();

        ResetGame();

        if (partieDemarree)
            MettreAJourTexteInstruction(messageScannerSurface);
    }

    public void BeginGame()
    {
        partieDemarree = true;
        RafraichirCellulesActives();

        if (panneauIntroduction != null)
            panneauIntroduction.SetActive(false);

        etatCourant = (gestionnairePlacementAR != null && !gestionnairePlacementAR.IsBoardPlaced)
            ? GameState.WaitingForPlacement
            : GameState.Playing;

        MettreAJourInterfaceEtat();
    }

    private void InitialiserPlateau()
    {
        for (int i = 0; i < plateau.Length; i++)
            plateau[i] = CellController.CellState.Empty;
    }

    private GameObject DeterminerPrefabSymbole()
    {
        if (!utiliserPrefabsPersonnalises)
            return null;

        return tourDuJoueurX ? prefabSymboleJoueurX : prefabSymboleJoueurO;
    }

    private bool TrouverLigneGagnante(out CellController.CellState gagnant, out int[] ligneGagnante)
    {
        foreach (int[] combinaison in CombinaisonsGagnantes)
        {
            CellController.CellState a = plateau[combinaison[0]];
            CellController.CellState b = plateau[combinaison[1]];
            CellController.CellState c = plateau[combinaison[2]];

            if (a != CellController.CellState.Empty && a == b && b == c)
            {
                gagnant = a;
                ligneGagnante = combinaison;
                return true;
            }
        }

        gagnant = CellController.CellState.Empty;
        ligneGagnante = null;
        return false;
    }

    private bool VerifierMatchNul()
    {
        for (int i = 0; i < plateau.Length; i++)
        {
            if (plateau[i] == CellController.CellState.Empty)
                return false;
        }
        return true;
    }

    private void MettreEnValeurLigneGagnante(int[] ligne, CellController.CellState gagnant)
    {
        Color couleur = gagnant == CellController.CellState.Player1 ? couleurVictoireX : couleurVictoireO;

        foreach (int index in ligne)
        {
            CellController cellule = RecupererCelluleParIndex(index);
            if (cellule != null)
                cellule.SetWinningVisual(true, couleur);
        }
    }

    private CellController RecupererCelluleParIndex(int index)
    {
        if (cellules == null)
            return null;

        foreach (CellController cellule in cellules)
        {
            if (cellule != null && cellule.cellIndex == index)
                return cellule;
        }

        return null;
    }

    private void RafraichirCellulesActives()
    {
        cellules = FindObjectsByType<CellController>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
    }

    private void MettreAJourInterfaceEtat()
    {
        if (!partieDemarree)
        {
            if (texteTour != null)
            {
                texteTour.text = "Lisez les instructions";
                texteTour.color = Color.white;
            }

            MettreAJourTexteInstruction("Cliquez sur Jouer pour commencer.");
            return;
        }

        if (etatCourant == GameState.WaitingForPlacement)
        {
            if (texteTour != null)
            {
                texteTour.text = messageScannerSurface;
                texteTour.color = Color.white;
            }

            MettreAJourTexteInstruction(messageScannerSurface);
            return;
        }

        MettreAJourTexteInstruction(messagePretAJouer);
        MettreAJourTexteTour();
    }

    private void MettreAJourTexteTour()
    {
        if (texteTour == null)
            return;

        texteTour.text = tourDuJoueurX ? "Tour de X" : "Tour de O";
        texteTour.color = tourDuJoueurX
            ? new Color(0.9f, 0.2f, 0.2f)
            : new Color(0.2f, 0.45f, 0.95f);
    }

    private void MettreAJourTexteInstruction(string message)
    {
        if (texteInstruction != null)
            texteInstruction.text = message;
    }

    private void AfficherFinDePartie(string message)
    {
        if (texteTour != null)
            texteTour.text = "Partie terminee";

        if (panneauFinPartie != null)
        {
            Image imagePanneauFin = panneauFinPartie.GetComponent<Image>();
            if (imagePanneauFin != null)
                imagePanneauFin.raycastTarget = false;
            panneauFinPartie.SetActive(true);
        }

        if (texteFinPartie != null)
            texteFinPartie.text = message;
    }

    private void GererChangementPlacement(bool grillePlacee)
    {
        if (grillePlacee)
            RafraichirCellulesActives();

        etatCourant = grillePlacee ? GameState.Playing : GameState.WaitingForPlacement;

        if (!partieDemarree)
            return;

        MettreAJourTexteInstruction(grillePlacee ? messagePretAJouer : messageScannerSurface);

        if (panneauFinPartie != null && !grillePlacee)
            panneauFinPartie.SetActive(false);

        MettreAJourInterfaceEtat();
    }

    private void CreerPanneauIntroductionSiAbsent()
    {
        if (panneauIntroduction != null)
            return;

        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
            return;

        TMP_FontAsset policeReference = texteTour != null ? texteTour.font : null;

        GameObject panneau = new GameObject("IntroPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        RectTransform panneauRect = panneau.GetComponent<RectTransform>();
        panneau.transform.SetParent(canvas.transform, false);
        panneauRect.anchorMin = Vector2.zero;
        panneauRect.anchorMax = Vector2.one;
        panneauRect.offsetMin = Vector2.zero;
        panneauRect.offsetMax = Vector2.zero;
        panneau.GetComponent<Image>().color = new Color(0.02f, 0.03f, 0.07f, 0.85f);

        TMP_Text titre = CreerTexteTMP("Titre", panneau.transform, policeReference, "Bienvenue");
        RectTransform titreRect = titre.rectTransform;
        titreRect.anchorMin = new Vector2(0.5f, 0.5f);
        titreRect.anchorMax = new Vector2(0.5f, 0.5f);
        titreRect.anchoredPosition = new Vector2(0f, 280f);
        titreRect.sizeDelta = new Vector2(900f, 100f);
        titre.fontSize = 68f;
        titre.alignment = TextAlignmentOptions.Center;

        TMP_Text corps = CreerTexteTMP("Consignes", panneau.transform, policeReference, consignesIntroduction);
        RectTransform corpsRect = corps.rectTransform;
        corpsRect.anchorMin = new Vector2(0.5f, 0.5f);
        corpsRect.anchorMax = new Vector2(0.5f, 0.5f);
        corpsRect.anchoredPosition = new Vector2(0f, 40f);
        corpsRect.sizeDelta = new Vector2(920f, 380f);
        corps.fontSize = 34f;
        corps.alignment = TextAlignmentOptions.TopLeft;

        GameObject boutonDemarrer = new GameObject("StartButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        RectTransform boutonRect = boutonDemarrer.GetComponent<RectTransform>();
        boutonDemarrer.transform.SetParent(panneau.transform, false);
        boutonRect.anchorMin = new Vector2(0.5f, 0.5f);
        boutonRect.anchorMax = new Vector2(0.5f, 0.5f);
        boutonRect.anchoredPosition = new Vector2(0f, -300f);
        boutonRect.sizeDelta = new Vector2(320f, 96f);

        boutonDemarrer.GetComponent<Image>().color = new Color(0.1f, 0.57f, 0.37f, 1f);

        Button bouton = boutonDemarrer.GetComponent<Button>();
        bouton.onClick.AddListener(BeginGame);

        TMP_Text texteBouton = CreerTexteTMP("TexteBouton", boutonDemarrer.transform, policeReference, "Jouer");
        RectTransform texteBoutonRect = texteBouton.rectTransform;
        texteBoutonRect.anchorMin = Vector2.zero;
        texteBoutonRect.anchorMax = Vector2.one;
        texteBoutonRect.offsetMin = Vector2.zero;
        texteBoutonRect.offsetMax = Vector2.zero;
        texteBouton.fontSize = 38f;
        texteBouton.alignment = TextAlignmentOptions.Center;

        panneauIntroduction = panneau;
    }

    private void ConfigurerBoutonsInterface()
    {
        if (boutonsConfigures)
            return;

        AssocierBouton("ReplayButton", ResetGame);
        AssocierBouton("RepositionButton", ResetPlacement);
        boutonsConfigures = true;
    }

    private static void AssocierBouton(string nomBouton, UnityEngine.Events.UnityAction action)
    {
        GameObject objetBouton = GameObject.Find(nomBouton);
        if (objetBouton == null)
            return;

        Button bouton = objetBouton.GetComponent<Button>();
        if (bouton == null)
            return;

        bouton.onClick.RemoveListener(action);
        bouton.onClick.AddListener(action);
    }

    private static TMP_Text CreerTexteTMP(string nom, Transform parent, TMP_FontAsset police, string contenu)
    {
        GameObject objet = new GameObject(nom, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        objet.transform.SetParent(parent, false);

        TMP_Text texte = objet.GetComponent<TMP_Text>();
        texte.text = contenu;
        texte.color = Color.white;
        texte.enableWordWrapping = true;
        if (police != null)
            texte.font = police;

        return texte;
    }

    public GameState currentState => etatCourant;
}
