using System.Collections.Generic;
using UnityEngine;

// Hedefleri (Goal) Inspector'da gösterebilmek için seri hale getirilebilir bir yapı kuruyoruz
[System.Serializable]
public struct LevelGoal
{
    public ItemType TargetItemType; // Hangi renk/tip taş?
    public int TargetAmount;        // Kaç tane patlatılacak?
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

    [Header("Grid Layout (1D Array mapped to 2D)")]
    [Tooltip("Tahta boyutu (Width x Height) kadar eleman olmalıdır. Sol alttan (0,0) başlayarak sağa ve yukarı doğru okunur.")]
    public CellType[] gridLayout;

    /// <summary>
    /// X ve Y koordinatını 1 boyutlu dizi indeksine çevirir.
    /// </summary>
    public CellType GetCellType(int x, int y)
    {
        int index = x + (y * width);
        
        // Eğer dizi henüz doldurulmadıysa veya boyutu uyuşmuyorsa varsayılan olarak Normal dön
        if (gridLayout == null || index < 0 || index >= gridLayout.Length)
            return CellType.Normal;

        return gridLayout[index];
    }
}