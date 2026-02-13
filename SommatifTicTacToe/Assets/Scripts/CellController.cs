using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public class CellController : MonoBehaviour
{
    public int cellIndex;

    public enum CellState { Empty, Player1, Player2 }
    public CellState currentState = CellState.Empty;

    private const float DECALAGE_SYMBOLE = 0.03f;

    [Header("Effets Victoire")]
    [FormerlySerializedAs("winScaleMultiplier")]
    [SerializeField] private float multiplicateurEchelleVictoire = 1.15f;
    [FormerlySerializedAs("fallbackDefaultColor")]
    [SerializeField] private Color couleurDefautSecours = new Color(0.10f, 0.28f, 0.32f, 1f);
    [FormerlySerializedAs("player1Color")]
    [SerializeField] private Color couleurJoueurX = new Color(0.20f, 0.85f, 0.30f, 1f);
    [FormerlySerializedAs("player2Color")]
    [SerializeField] private Color couleurJoueurO = new Color(0.15f, 0.78f, 1f, 1f);
    [SerializeField] private bool forcerPaletteContraste = true;

    [Header("Effets Placement")]
    [FormerlySerializedAs("symbolScaleInDuration")]
    [SerializeField] private float dureeAnimationApparition = 0.12f;
    private const float RATIO_TAILLE_SYMBOLE_PAR_TUILE = 0.5f;

    [Header("Effets Tap")]
    [FormerlySerializedAs("tapFlashColor")]
    [SerializeField] private Color couleurFlashTap = new Color(1f, 0.95f, 0.55f);
    [FormerlySerializedAs("tapFlashDuration")]
    [SerializeField] private float dureeFlashTap = 0.08f;

    private GameObject symbolePlace;
    private GameController controleurJeu;
    private Renderer renduCellule;
    private Vector3 echelleInitiale;
    private Color couleurDefaut;
    private Coroutine coroutineFlashTap;

    private void Awake()
    {
        echelleInitiale = transform.localScale;
        renduCellule = GetComponent<Renderer>();

        couleurDefaut = couleurDefautSecours;
        if (renduCellule != null)
        {
            Material materiau = renduCellule.material;
            if (forcerPaletteContraste)
            {
                AppliquerCouleurMateriau(materiau, couleurDefautSecours);
                couleurDefaut = couleurDefautSecours;
            }
            else
            {
                if (materiau.HasProperty("_BaseColor"))
                    couleurDefaut = materiau.GetColor("_BaseColor");
                else if (materiau.HasProperty("_Color"))
                    couleurDefaut = materiau.color;
            }
        }
    }

    private void Start()
    {
        controleurJeu = FindAnyObjectByType<GameController>();
        if (controleurJeu == null)
            Debug.LogError("CellController: GameController non trouve.");
    }

    public void OnCellClicked()
    {
        if (controleurJeu == null || currentState != CellState.Empty)
            return;

        JouerFlashTap();
        controleurJeu.PlaceSymbol(this);
    }

    public void PlaceSymbol(GameObject prefabSymbole, CellState etat)
    {
        if (currentState != CellState.Empty)
            return;

        Vector3 positionSymbole = transform.position + transform.forward * DECALAGE_SYMBOLE;
        symbolePlace = prefabSymbole != null
            ? Instantiate(prefabSymbole, positionSymbole, transform.rotation, transform)
            : CreerSymboleParDefaut(etat, positionSymbole);

        if (symbolePlace != null)
            StartCoroutine(AnimerApparitionSymbole(symbolePlace.transform));

        currentState = etat;
    }

    public void SetWinningVisual(bool actif, Color couleurVictoire)
    {
        if (renduCellule != null)
        {
            Material materiau = renduCellule.material;
            Color couleurCible = actif ? couleurVictoire : couleurDefaut;

            if (materiau.HasProperty("_BaseColor"))
                materiau.SetColor("_BaseColor", couleurCible);
            if (materiau.HasProperty("_Color"))
                materiau.SetColor("_Color", couleurCible);
        }

        transform.localScale = actif ? echelleInitiale * multiplicateurEchelleVictoire : echelleInitiale;
    }

    public void ResetCell()
    {
        if (coroutineFlashTap != null)
        {
            StopCoroutine(coroutineFlashTap);
            coroutineFlashTap = null;
        }

        if (symbolePlace != null)
        {
            Destroy(symbolePlace);
            symbolePlace = null;
        }

        currentState = CellState.Empty;
        SetWinningVisual(false, couleurDefaut);
    }

    private GameObject CreerSymboleParDefaut(CellState etat, Vector3 positionMonde)
    {
        if (etat == CellState.Empty)
            return null;

        GameObject racine = new GameObject(etat == CellState.Player1 ? "XSymbol" : "OSymbol");
        racine.transform.SetParent(transform, true);
        racine.transform.position = positionMonde;
        racine.transform.rotation = transform.rotation;

        float envergure = CalculerEnvergureSymboleCible();
        if (etat == CellState.Player1)
            ConstruireSymboleX(racine.transform, couleurJoueurX, envergure);
        else
            ConstruireSymboleO(racine.transform, couleurJoueurO, envergure);

        return racine;
    }

    private void ConstruireSymboleX(Transform parent, Color couleur, float envergure)
    {
        float longueurBarre = envergure * 1.35f;
        float epaisseurBarre = envergure * 0.22f;

        CreerPartieSymbole(parent, Vector3.zero, Quaternion.Euler(0f, 0f, 45f), new Vector3(longueurBarre, epaisseurBarre, epaisseurBarre), couleur);
        CreerPartieSymbole(parent, Vector3.zero, Quaternion.Euler(0f, 0f, -45f), new Vector3(longueurBarre, epaisseurBarre, epaisseurBarre), couleur);
    }

    private void ConstruireSymboleO(Transform parent, Color couleur, float envergure)
    {
        const int nbSegments = 12;
        float rayon = envergure * 0.5f;
        float longueurSegment = (2f * Mathf.PI * rayon / nbSegments) * 1.15f;
        float epaisseurSegment = envergure * 0.2f;

        for (int i = 0; i < nbSegments; i++)
        {
            float angle = i * (360f / nbSegments);
            float radians = angle * Mathf.Deg2Rad;
            Vector3 positionLocale = new Vector3(Mathf.Cos(radians) * rayon, Mathf.Sin(radians) * rayon, 0f);
            Quaternion rotationLocale = Quaternion.Euler(0f, 0f, angle);

            CreerPartieSymbole(parent, positionLocale, rotationLocale, new Vector3(longueurSegment, epaisseurSegment, epaisseurSegment), couleur);
        }
    }

    private void CreerPartieSymbole(Transform parent, Vector3 positionLocale, Quaternion rotationLocale, Vector3 echelleLocale, Color couleur)
    {
        GameObject partie = GameObject.CreatePrimitive(PrimitiveType.Cube);
        partie.transform.SetParent(parent, false);
        partie.transform.localPosition = positionLocale;
        partie.transform.localRotation = rotationLocale;
        partie.transform.localScale = echelleLocale;

        Collider collision = partie.GetComponent<Collider>();
        if (collision != null)
            Destroy(collision);

        Renderer rendu = partie.GetComponent<Renderer>();
        if (rendu == null)
            return;

        rendu.material = new Material(rendu.material);
        Material materiau = rendu.material;
        if (materiau.HasProperty("_BaseColor"))
            materiau.SetColor("_BaseColor", couleur);
        if (materiau.HasProperty("_Color"))
            materiau.SetColor("_Color", couleur);
    }

    private IEnumerator AnimerApparitionSymbole(Transform transformSymbole)
    {
        Vector3 echelleFinale = transformSymbole.localScale;
        transformSymbole.localScale = Vector3.zero;

        float temps = 0f;
        while (temps < dureeAnimationApparition)
        {
            temps += Time.deltaTime;
            float t = Mathf.Clamp01(temps / dureeAnimationApparition);
            t = 1f - Mathf.Pow(1f - t, 3f);
            transformSymbole.localScale = Vector3.LerpUnclamped(Vector3.zero, echelleFinale, t);
            yield return null;
        }

        transformSymbole.localScale = echelleFinale;
    }

    private void JouerFlashTap()
    {
        if (renduCellule == null)
            return;

        if (coroutineFlashTap != null)
            StopCoroutine(coroutineFlashTap);

        coroutineFlashTap = StartCoroutine(CoroutineFlashTap());
    }

    private IEnumerator CoroutineFlashTap()
    {
        Material materiau = renduCellule.material;
        AppliquerCouleurMateriau(materiau, couleurFlashTap);
        yield return new WaitForSeconds(dureeFlashTap);
        AppliquerCouleurMateriau(materiau, couleurDefaut);
        coroutineFlashTap = null;
    }

    private static void AppliquerCouleurMateriau(Material materiau, Color couleur)
    {
        if (materiau == null)
            return;

        if (materiau.HasProperty("_BaseColor"))
            materiau.SetColor("_BaseColor", couleur);
        if (materiau.HasProperty("_Color"))
            materiau.SetColor("_Color", couleur);
    }

    private float CalculerEnvergureSymboleCible()
    {
        float tailleCellule = 0.08f;

        if (renduCellule != null)
        {
            Vector3 taille = renduCellule.bounds.size;
            tailleCellule = Mathf.Min(taille.x, taille.y);
        }
        else
        {
            Vector3 echelleMonde = transform.lossyScale;
            tailleCellule = Mathf.Min(echelleMonde.x, echelleMonde.y);
        }

        return tailleCellule * RATIO_TAILLE_SYMBOLE_PAR_TUILE;
    }
}
