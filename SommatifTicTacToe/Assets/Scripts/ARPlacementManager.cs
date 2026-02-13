using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ARPlacementManager : MonoBehaviour
{
    [Header("Composants AR")]
    [FormerlySerializedAs("arRaycastManager")]
    [SerializeField] private ARRaycastManager gestionnaireRaycastAR;
    [FormerlySerializedAs("arAnchorManager")]
    [SerializeField] private ARAnchorManager gestionnaireAncrageAR;
    [FormerlySerializedAs("arPlaneManager")]
    [SerializeField] private ARPlaneManager gestionnairePlansAR;

    [Header("Grille")]
    [FormerlySerializedAs("boardPrefab")]
    [SerializeField] private GameObject prefabGrille;

    [Header("Placement")]
    [FormerlySerializedAs("wallOffset")]
    [SerializeField] private float decalageAvant = 0.02f;
    [FormerlySerializedAs("hideSourceBoardOnStart")]
    [SerializeField] private bool masquerGrilleSourceAuDemarrage = true;
    [FormerlySerializedAs("allowHorizontalOnly")]
    [SerializeField] private bool autoriserPlansHorizontauxSeulement = true;
    [FormerlySerializedAs("editorAutoPlaceBoard")]
    [SerializeField] private bool placementAutoDansEditeur = false;
    [SerializeField, Range(0.5f, 3f)] private float multiplicateurTailleGrille = 1.25f;

    private PlayerInputActions actionsJoueur;
    private readonly List<ARRaycastHit> resultatsRaycast = new List<ARRaycastHit>();

    private GameObject grillePlacee;
    private GameObject grilleSource;
    private ARAnchor ancreActive;
    private bool grilleEstPlacee;

    public bool IsBoardPlaced => grilleEstPlacee;
    public event Action<bool> BoardPlacementChanged;

    private void Awake()
    {
        actionsJoueur = new PlayerInputActions();
    }

    private void Start()
    {
        ConfigurerDetectionPlans();

        grilleSource = prefabGrille;
        bool masquerSource = masquerGrilleSourceAuDemarrage;

#if UNITY_EDITOR
        if (placementAutoDansEditeur)
        {
            grilleEstPlacee = true;
            masquerSource = false;
            BoardPlacementChanged?.Invoke(true);
        }
#endif

        if (grilleSource != null && grilleSource.scene.IsValid())
            grilleSource.SetActive(!masquerSource);
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

    private void GererTap(InputAction.CallbackContext _)
    {
        if (grilleEstPlacee)
            return;

        if (gestionnaireRaycastAR == null || gestionnairePlansAR == null)
            return;

        Vector2 positionTouch = actionsJoueur.AR.Point.ReadValue<Vector2>();
        if (!gestionnaireRaycastAR.Raycast(positionTouch, resultatsRaycast, TrackableType.PlaneWithinPolygon))
            return;

        ARRaycastHit impact = resultatsRaycast[0];
        if (!gestionnairePlansAR.trackables.TryGetTrackable(impact.trackableId, out ARPlane plan))
            return;

        if (autoriserPlansHorizontauxSeulement && plan.alignment != PlaneAlignment.HorizontalUp)
            return;

        PoserGrille(plan, impact.pose);
    }

    public void ResetPlacement()
    {
        if (grillePlacee != null)
            Destroy(grillePlacee);

        if (ancreActive != null)
            Destroy(ancreActive.gameObject);

        grillePlacee = null;
        ancreActive = null;
        grilleEstPlacee = false;

        ConfigurerDetectionPlans();
        BoardPlacementChanged?.Invoke(false);
    }

    private void PoserGrille(ARPlane plan, Pose poseImpact)
    {
        if (gestionnaireAncrageAR != null)
            ancreActive = gestionnaireAncrageAR.AttachAnchor(plan, poseImpact);

        Pose poseFinale = CalculerPoseFinale(plan, poseImpact);
        Transform parent = ancreActive != null ? ancreActive.transform : null;

        GameObject source = grilleSource != null ? grilleSource : prefabGrille;
        if (source == null)
        {
            Debug.LogError("ARPlacementManager: prefabGrille est manquant.");
            return;
        }

        grillePlacee = Instantiate(source, poseFinale.position, poseFinale.rotation, parent);
        grillePlacee.transform.localScale *= multiplicateurTailleGrille;
        if (!grillePlacee.activeSelf)
            grillePlacee.SetActive(true);

        grilleEstPlacee = true;
        BoardPlacementChanged?.Invoke(true);
        Debug.Log("Grille placee.");
    }

    private Pose CalculerPoseFinale(ARPlane plan, Pose poseImpact)
    {
        Vector3 normalePlan = plan != null ? plan.transform.up : poseImpact.up;
        normalePlan = normalePlan.sqrMagnitude > 0.0001f ? normalePlan.normalized : Vector3.up;

        Vector3 tangent = Vector3.ProjectOnPlane(Camera.main != null ? Camera.main.transform.forward : Vector3.forward, normalePlan);
        if (tangent.sqrMagnitude < 0.0001f)
            tangent = Vector3.Cross(normalePlan, Vector3.right);
        if (tangent.sqrMagnitude < 0.0001f)
            tangent = Vector3.Cross(normalePlan, Vector3.forward);
        tangent.Normalize();

        Quaternion rotationGrille = Quaternion.LookRotation(normalePlan, tangent);
        Vector3 positionGrille = poseImpact.position + (normalePlan * decalageAvant);

        Pose poseFinale = new Pose(positionGrille, rotationGrille);

        return poseFinale;
    }

    private void ConfigurerDetectionPlans()
    {
        if (gestionnairePlansAR == null)
            return;

        gestionnairePlansAR.requestedDetectionMode = autoriserPlansHorizontauxSeulement
            ? PlaneDetectionMode.Horizontal
            : PlaneDetectionMode.Horizontal | PlaneDetectionMode.Vertical;
    }
}
