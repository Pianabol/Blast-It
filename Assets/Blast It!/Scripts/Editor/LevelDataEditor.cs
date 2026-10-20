using UnityEditor;
using UnityEngine;

// Bu Editor scripti, typeof(LevelData) olan tüm ScriptableObject'lerin görünümünü değiştirir.
[CustomEditor(typeof(LevelData))]
public class LevelDataEditor : Editor
{
    private LevelData levelData;
    private CellType selectedBrush = CellType.Normal; // O an fırçaya seçtiğimiz tip

    // Renk paleti
    private readonly Color colorNormal = new Color(0.8f, 0.8f, 0.8f);
    private readonly Color colorObstacle = new Color(0.4f, 0.2f, 0.0f); // Kahverengi (Kutu/Duvar)
    private readonly Color colorVoid = new Color(0.1f, 0.1f, 0.1f);    // Koyu Gri/Siyah (Boşluk)

    private void OnEnable()
    {
        levelData = (LevelData)target; // İncelenen LevelData dosyasını al
    }

    public override void OnInspectorGUI()
    {
        // 1. Standart Inspector bileşenlerini (Hamleler, Hedefler, En-Boy) manuel çizelim
        serializedObject.Update();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Level Dimensions", EditorStyles.boldLabel);
        
        // Width ve Height değiştiğinde gridLayout dizisinin boyutunu güncellememiz lazım
        EditorGUI.BeginChangeCheck();
        int newWidth = EditorGUILayout.IntSlider("Width", levelData.width, 3, 12);
        int newHeight = EditorGUILayout.IntSlider("Height", levelData.height, 3, 12);
        
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(levelData, "Resize Grid");
            levelData.width = newWidth;
            levelData.height = newHeight;
            ResizeGrid(); // Boyut değişirse diziyi yeniden formatla
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Level Conditions", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("totalMoves"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("levelGoals"), true);

        // Grid dizisinin boyutunu güvenceye al
        if (levelData.gridLayout == null || levelData.gridLayout.Length != levelData.width * levelData.height)
        {
            ResizeGrid();
        }

        // 2. ÖZEL KISIM: Grid Boyama Aracı (Grid Painter)
        EditorGUILayout.Space(15);
        EditorGUILayout.LabelField("--- LEVEL DESIGNER (GRID PAINTER) ---", EditorStyles.boldLabel);
        
        DrawBrushToolbar();
        DrawGridPainter();

        serializedObject.ApplyModifiedProperties();

        // Eğer bir değişiklik yaptıysak dosyayı kaydet
        if (GUI.changed)
        {
            EditorUtility.SetDirty(levelData);
        }
    }

    /// <summary>
    /// Fırça seçimi için butonlar (Normal, Obstacle, Void)
    /// </summary>
    private void DrawBrushToolbar()
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label("Selected Brush:", GUILayout.Width(100));

        // Enum'daki tüm değerleri buton olarak çiz
        foreach (CellType type in System.Enum.GetValues(typeof(CellType)))
        {
            // Eğer o anki fırça buysa arka planı yeşilimsi yap
            GUI.backgroundColor = (selectedBrush == type) ? Color.green : Color.white;
            
            if (GUILayout.Button(type.ToString(), GUILayout.Height(30)))
            {
                selectedBrush = type;
            }
        }
        GUI.backgroundColor = Color.white; // Rengi normale döndür
        GUILayout.EndHorizontal();
        EditorGUILayout.Space();
    }

    /// <summary>
    /// Tıklanabilir butonlardan oluşan Grid'i çizer
    /// </summary>
    private void DrawGridPainter()
    {
        // En dışta bir arka plan kutusu çiz (Daha derli toplu görünmesi için)
        EditorGUILayout.BeginVertical(GUI.skin.box);
        
        // Unity'nin UI sistemi soldan sağa, YUKARIDAN AŞAĞIYA çizer.
        // Ama bizim oyun matematiğinde Y=0 tahtanın EN ALTI.
        // Bu yüzden Y eksenini tersten (height-1'den 0'a doğru) çizdiriyoruz ki ekrandaki görüntü ile oyundaki eşleşsin.
        for (int y = levelData.height - 1; y >= 0; y--)
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace(); // Izgarayı Inspector'ın ortasına hizala

            for (int x = 0; x < levelData.width; x++)
            {
                int index = x + (y * levelData.width);
                CellType currentType = levelData.gridLayout[index];

                // Hücrenin tipine göre düğmenin rengini ayarla
                switch (currentType)
                {
                    case CellType.Normal: GUI.backgroundColor = colorNormal; break;
                    case CellType.Obstacle: GUI.backgroundColor = colorObstacle; break;
                    case CellType.Void: GUI.backgroundColor = colorVoid; break;
                }

                // 40x40 boyutlarında butonlar çiz (İçinde yazı olmasın, sadece renkli kutu)
                if (GUILayout.Button("", GUILayout.Width(40), GUILayout.Height(40)))
                {
                    // Butona tıklanırsa, seçili olan fırçayı (selectedBrush) o hücreye daya!
                    Undo.RecordObject(levelData, "Paint Grid Cell");
                    levelData.gridLayout[index] = selectedBrush;
                }
            }
            
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }
        
        GUI.backgroundColor = Color.white; // UI renk paletini sıfırla
        EditorGUILayout.EndVertical();

        EditorGUILayout.HelpBox("Tasarım yapmak için yukarıdan fırça seçip kutulara tıkla. Y=0 (En alt satır) tasarımın alt kısmıdır.", MessageType.Info);
    }

    /// <summary>
    /// Width veya Height değişirse, eski dizideki verileri kaybetmeden diziyi yeni boyuta geçirir.
    /// </summary>
    private void ResizeGrid()
    {
        CellType[] newGrid = new CellType[levelData.width * levelData.height];
        
        if (levelData.gridLayout != null)
        {
            // Eski verileri kopyalamaya çalış
            for (int y = 0; y < levelData.height; y++)
            {
                for (int x = 0; x < levelData.width; x++)
                {
                    // Eski dizide bu koordinat var mıydı?
                    int oldIndex = x + (y * levelData.width); // Not: Genişlik değiştiğinde eski yapı biraz bozulabilir, ama Void'ler uçmasın diye basit bir kopyalama yapıyoruz.
                    
                    if (oldIndex < levelData.gridLayout.Length)
                    {
                        newGrid[x + (y * levelData.width)] = levelData.gridLayout[oldIndex];
                    }
                }
            }
        }
        
        levelData.gridLayout = newGrid;
    }
}