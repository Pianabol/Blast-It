using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LevelData))]
public class LevelDataEditor : Editor
{
    private LevelData levelData;
    
    // İki farklı fırça tutacağız
    private CellType selectedCellBrush = CellType.Normal;
    private GoalType selectedGoalBrush = GoalType.None;
    
    // Boyama modu
    private enum PaintMode { BaseLayer, GoalLayer }
    private PaintMode currentMode = PaintMode.BaseLayer;

    private readonly Color colorNormal = new Color(0.8f, 0.8f, 0.8f);
    private readonly Color colorObstacle = new Color(0.4f, 0.2f, 0.0f);
    private readonly Color colorVoid = new Color(0.1f, 0.1f, 0.1f);
    
    // Hedefler için dikkat çekici bir renk
    private readonly Color colorGoal = new Color(1f, 0.8f, 0.2f); 

    private void OnEnable()
    {
        levelData = (LevelData)target;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // Boyut ve Temel ayarlar kısmı (Kendi kodundan aynen kalıyor)
        EditorGUI.BeginChangeCheck();
        int newWidth = EditorGUILayout.IntSlider("Width", levelData.width, 3, 12);
        int newHeight = EditorGUILayout.IntSlider("Height", levelData.height, 3, 12);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(levelData, "Resize Grid");
            levelData.width = newWidth;
            levelData.height = newHeight;
            ResizeGrid();
        }

        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("totalMoves"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("levelGoals"), true);

        if (levelData.gridLayout == null || levelData.gridLayout.Length != levelData.width * levelData.height || 
            levelData.goalLayout == null || levelData.goalLayout.Length != levelData.width * levelData.height)
        {
            ResizeGrid();
        }

        EditorGUILayout.Space(15);
        EditorGUILayout.LabelField("--- LEVEL DESIGNER (GRID PAINTER) ---", EditorStyles.boldLabel);
        
        // Mod Seçici (Zemin mi boyayacağız, Üstüne eşya mı koyacağız?)
        currentMode = (PaintMode)GUILayout.Toolbar((int)currentMode, new string[] { "Base Layer (Void/Obstacle)", "Goal Layer (Items)" });
        EditorGUILayout.Space();

        DrawBrushToolbar();
        DrawGridPainter();

        serializedObject.ApplyModifiedProperties();
        if (GUI.changed) EditorUtility.SetDirty(levelData);
    }

    private void DrawBrushToolbar()
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label("Selected Brush:", GUILayout.Width(100));

        if (currentMode == PaintMode.BaseLayer)
        {
            foreach (CellType type in System.Enum.GetValues(typeof(CellType)))
            {
                GUI.backgroundColor = (selectedCellBrush == type) ? Color.green : Color.white;
                if (GUILayout.Button(type.ToString(), GUILayout.Height(30))) selectedCellBrush = type;
            }
        }
        else // Goal Layer
        {
            foreach (GoalType type in System.Enum.GetValues(typeof(GoalType)))
            {
                GUI.backgroundColor = (selectedGoalBrush == type) ? Color.green : Color.white;
                if (GUILayout.Button(type.ToString(), GUILayout.Height(30))) selectedGoalBrush = type;
            }
        }
        
        GUI.backgroundColor = Color.white;
        GUILayout.EndHorizontal();
        EditorGUILayout.Space();
    }

    private void DrawGridPainter()
    {
        EditorGUILayout.BeginVertical(GUI.skin.box);
        
        for (int y = levelData.height - 1; y >= 0; y--)
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            for (int x = 0; x < levelData.width; x++)
            {
                int index = x + (y * levelData.width);
                CellType currentCellType = levelData.gridLayout[index];
                GoalType currentGoalType = levelData.goalLayout[index];

                // Görüntüyü ayarlama
                string buttonText = "";
                if (currentGoalType != GoalType.None)
                {
                    GUI.backgroundColor = colorGoal; // Üstünde hedef (Goal) varsa altın sarısı yap
                    buttonText = currentGoalType.ToString().Substring(0, 1); // İlk harfini yaz (Örn: Coin için "C")
                }
                else
                {
                    switch (currentCellType)
                    {
                        case CellType.Normal: GUI.backgroundColor = colorNormal; break;
                        case CellType.Obstacle: GUI.backgroundColor = colorObstacle; break;
                        case CellType.Void: GUI.backgroundColor = colorVoid; break;
                    }
                }

                if (GUILayout.Button(buttonText, GUILayout.Width(40), GUILayout.Height(40)))
                {
                    Undo.RecordObject(levelData, "Paint Grid Cell");
                    
                    if (currentMode == PaintMode.BaseLayer)
                    {
                        levelData.gridLayout[index] = selectedCellBrush;
                        if (selectedCellBrush == CellType.Void) levelData.goalLayout[index] = GoalType.None; // Void olan yerde eşya duramaz
                    }
                    else if (currentMode == PaintMode.GoalLayer)
                    {
                        if (currentCellType != CellType.Void) // Boşluğa eşya koyulamaz
                        {
                            levelData.goalLayout[index] = selectedGoalBrush;
                        }
                    }
                }
            }
            
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }
        
        GUI.backgroundColor = Color.white;
        EditorGUILayout.EndVertical();
    }

    private void ResizeGrid()
    {
        int totalCells = levelData.width * levelData.height;
        CellType[] newGrid = new CellType[totalCells];
        GoalType[] newGoalGrid = new GoalType[totalCells];
        
        if (levelData.gridLayout != null)
        {
            for (int y = 0; y < levelData.height; y++)
            {
                for (int x = 0; x < levelData.width; x++)
                {
                    int oldIndex = x + (y * levelData.width);
                    if (oldIndex < levelData.gridLayout.Length)
                    {
                        newGrid[x + (y * levelData.width)] = levelData.gridLayout[oldIndex];
                    }
                    if (levelData.goalLayout != null && oldIndex < levelData.goalLayout.Length)
                    {
                        newGoalGrid[x + (y * levelData.width)] = levelData.goalLayout[oldIndex];
                    }
                }
            }
        }
        
        levelData.gridLayout = newGrid;
        levelData.goalLayout = newGoalGrid;
    }
}