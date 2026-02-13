using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class ARInteractionManager : MonoBehaviour
{
    [Header("Raycast")]
    [FormerlySerializedAs("arCamera")]
    [SerializeField] private Camera cameraAR;
    [FormerlySerializedAs("cellLayer")]
    [SerializeField] private LayerMask masqueCoucheCellules;
    [FormerlySerializedAs("maxRaycastDistance")]
    [SerializeField] private float distanceMaxRaycast = 10f;

    private PlayerInputActions actionsJoueur;
    private GameController controleurJeu;

    private void Awake()
    {
        actionsJoueur = new PlayerInputActions();
    }

    private void OnEnable()
    {
        actionsJoueur.Enable();
        actionsJoueur.AR.Tap.performed += GererTap;
    }

    private void OnDisable()
    {
        actionsJoueur.AR.Tap.performed -= GererTap;
        actionsJoueur.Disable();
    }

    private void Start()
    {
        if (cameraAR == null)
            cameraAR = Camera.main;

        controleurJeu = FindAnyObjectByType<GameController>();
        if (controleurJeu == null)
            Debug.LogError("ARInteractionManager: GameController introuvable.");
    }

    private void GererTap(InputAction.CallbackContext _)
    {
        if (cameraAR == null || controleurJeu == null)
            return;

        if (controleurJeu.currentState != GameController.GameState.Playing)
            return;

        Vector2 positionTap = actionsJoueur.AR.Point.ReadValue<Vector2>();
        Ray rayon = cameraAR.ScreenPointToRay(positionTap);

        if (Physics.Raycast(rayon, out RaycastHit impact, distanceMaxRaycast, masqueCoucheCellules))
        {
            CellController cellule = impact.collider.GetComponent<CellController>();
            if (cellule != null)
                cellule.OnCellClicked();
        }
    }
}
