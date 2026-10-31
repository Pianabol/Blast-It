using System.Collections.Generic;
using UnityEngine;

// Hücreye atanabilecek özel eşyalar (Varsayılan olarak "None" ise normal taş demektir)
public enum GoalType
{
    None,   // Boş, buraya normal renkli taş (Item) gelecek
    Clock,
    Coin,
    Time,
    Trophy
}

// Bölümün hedeflerini tutan yapı
[System.Serializable]
public struct LevelGoal
{
    public GoalType TargetGoalType; // Hangi hedef? (Saat, Kupa vb.)
    public int TargetAmount;        // Bölümü geçmek için toplam kaç tane toplanacak?
}

[CreateAssetMenu(fileName = "Level_001", menuName = "Blast It/Level Data")]
public class LevelData : ScriptableObject
{
    [Header("Board Dimensions")]
    [Min(3)] public int width = 8;
    [Min(3)] public int height = 8;

    [Header("Level Conditions")]
    public int totalMoves = 20;
    public List<LevelGoal> levelGoals;

    // 1. Dizi: Zemin türü (Normal, Void, Obstacle)
    [HideInInspector] public CellType[] gridLayout;
    
    // 2. Dizi: Hücrenin üstündeki özel hedef (None ise normal renkli taş düşer)
    [HideInInspector] public GoalType[] goalLayout; 

    public CellType GetCellType(int x, int y)
    {
        int index = x + (y * width);
        if (gridLayout == null || index < 0 || index >= gridLayout.Length) return CellType.Normal;
        return gridLayout[index];
    }

    public GoalType GetGoalType(int x, int y)
    {
        int index = x + (y * width);
        if (goalLayout == null || index < 0 || index >= goalLayout.Length) return GoalType.None;
        return goalLayout[index];
    }
}