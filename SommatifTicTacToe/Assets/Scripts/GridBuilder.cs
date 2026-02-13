using UnityEngine;
using UnityEngine.Serialization;

/// Construit une grille 3x3 de cellules jouables.
public class GridBuilder : MonoBehaviour
{
    [Header("Configuration Grille")]
    [Tooltip("Taille de chaque cellule (largeur/hauteur)")]
    [FormerlySerializedAs("cubeSize")]
    public float tailleCellule = 0.08f;

    [Tooltip("Epaisseur des cellules")]
    [FormerlySerializedAs("cubeDepth")]
    public float epaisseurCellule = 0.02f;

    [Tooltip("Espacement entre les cellules")]
    [FormerlySerializedAs("spacing")]
    public float espacement = 0.01f;

    [Header("Visuel")]
    [Tooltip("Couleur des cellules de la grille")]
    [FormerlySerializedAs("gridColor")]
    public Color couleurGrille = Color.green;

    [Header("Layer")]
    [Tooltip("Layer assignee aux cellules (6 = Cell)")]
    [FormerlySerializedAs("cellLayer")]
    public int coucheCellule = 6;

    private Material materiauGrille;

    private Vector3[] CalculerPositionsCellules()
    {
        Vector3[] positions = new Vector3[9];
        float pas = tailleCellule + espacement;
        float offset = pas;

        int index = 0;
        for (int ligne = 0; ligne < 3; ligne++)
        {
            for (int colonne = 0; colonne < 3; colonne++)
            {
                float x = -offset + (colonne * pas);
                float y = offset - (ligne * pas);
                positions[index] = new Vector3(x, y, 0f);
                index++;
            }
        }

        return positions;
    }

    public void GenerateGrid()
    {
        ClearGrid();
        CreerMateriauGrille();

        Vector3[] positions = CalculerPositionsCellules();
        for (int i = 0; i < positions.Length; i++)
            CreerCellule(i, positions[i]);

        Debug.Log("Grille generee (9 cellules).");
    }

    public void ClearGrid()
    {
        Transform[] enfants = GetComponentsInChildren<Transform>();
        foreach (Transform enfant in enfants)
        {
            if (enfant == transform || !enfant.name.StartsWith("Cell_"))
                continue;

#if UNITY_EDITOR
            DestroyImmediate(enfant.gameObject);
#else
            Destroy(enfant.gameObject);
#endif
        }
    }

    private void CreerCellule(int index, Vector3 positionLocale)
    {
        GameObject cellule = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cellule.name = $"Cell_{index}";
        cellule.transform.SetParent(transform);
        cellule.transform.localPosition = positionLocale;
        cellule.transform.localRotation = Quaternion.identity;
        cellule.transform.localScale = new Vector3(tailleCellule, tailleCellule, epaisseurCellule);
        cellule.layer = coucheCellule;

        Renderer rendu = cellule.GetComponent<Renderer>();
        if (rendu != null)
            rendu.material = materiauGrille;

        CellController controleur = cellule.AddComponent<CellController>();
        controleur.cellIndex = index;

        Debug.Log($"Cellule {index} -> {positionLocale}");
    }

    private void CreerMateriauGrille()
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
            shader = Shader.Find("Standard");
        if (shader == null)
            shader = Shader.Find("Diffuse");

        materiauGrille = new Material(shader);
        materiauGrille.color = couleurGrille;

        if (materiauGrille.HasProperty("_Metallic"))
            materiauGrille.SetFloat("_Metallic", 0.3f);
        if (materiauGrille.HasProperty("_Glossiness"))
            materiauGrille.SetFloat("_Glossiness", 0.5f);
        if (materiauGrille.HasProperty("_Smoothness"))
            materiauGrille.SetFloat("_Smoothness", 0.5f);

        if (materiauGrille.HasProperty("_BaseColor"))
            materiauGrille.SetColor("_BaseColor", couleurGrille);
        if (materiauGrille.HasProperty("_Color"))
            materiauGrille.SetColor("_Color", couleurGrille);
    }

    private void OnDrawGizmos()
    {
        Vector3[] positions = CalculerPositionsCellules();
        Gizmos.color = Color.green;

        foreach (Vector3 position in positions)
        {
            Vector3 positionMonde = transform.TransformPoint(position);
            Vector3 taille = new Vector3(tailleCellule, tailleCellule, epaisseurCellule);
            Gizmos.DrawWireCube(positionMonde, taille);
        }
    }
}

#if UNITY_EDITOR
[UnityEditor.CustomEditor(typeof(GridBuilder))]
public class GridBuilderEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GridBuilder constructeur = (GridBuilder)target;

        UnityEditor.EditorGUILayout.Space(10);
        UnityEditor.EditorGUILayout.LabelField("Actions", UnityEditor.EditorStyles.boldLabel);

        if (GUILayout.Button("Generer Grille", GUILayout.Height(40)))
            constructeur.GenerateGrid();

        if (GUILayout.Button("Vider Grille", GUILayout.Height(30)))
            constructeur.ClearGrid();
    }
}
#endif
