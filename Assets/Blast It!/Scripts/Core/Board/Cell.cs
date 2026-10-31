using UnityEngine;

public enum CellType
{
    Normal,
    Obstacle,
    Void
}

public class Cell
{
    public int X { get; private set; }
    public int Y { get; private set; }
    public CellType Type { get; set; }
    
    // Hücredeki Normal Taş
    public Item CurrentItem { get; private set; }
    
    // YENİ: Hücredeki Hedef Eşya (Kupa, Saat vb.)
    public GoalItem CurrentGoalItem { get; private set; } 

    public bool IsPlayable { get; set; }

    public Cell(int x, int y, CellType type = CellType.Normal)
    {
        X = x;
        Y = y;
        Type = type;
        CurrentItem = null;
        CurrentGoalItem = null;
        
        IsPlayable = (type == CellType.Normal);
    }

    // --- NORMAL TAŞ METOTLARI ---
    public void SetItem(Item newItem) => CurrentItem = newItem;
    
    public Item ExtractItem()
    {
        Item extracted = CurrentItem;
        CurrentItem = null;
        return extracted;
    }

    // --- HEDEF (GOAL) METOTLARI ---
    public void SetGoalItem(GoalItem newGoal) => CurrentGoalItem = newGoal;

    public GoalItem ExtractGoalItem()
    {
        GoalItem extracted = CurrentGoalItem;
        CurrentGoalItem = null;
        return extracted;
    }

    // --- DURUM KONTROLLERİ ---
    public bool HasItem => CurrentItem != null;
    public bool HasGoalItem => CurrentGoalItem != null;
    
    // Hücre tamamen boş mu? (Ne normal taş ne de hedef var)
    public bool IsEmpty => !HasItem && !HasGoalItem; 
}