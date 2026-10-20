using UnityEngine;

// Hücrenin temel durumu. Şimdilik Boş ve Dolu yeterli, ileride Buz, Jöle eklenebilir.
public enum CellType
{
    Normal,     // Taş konulabilir, oynanabilir hücre
    Obstacle,   // Taş konulamaz duvar/engel
    Void        // Tahta dışı (köşeleri boş haritalar için)
}

public class Cell
{
    public int X { get; private set; }
    public int Y { get; private set; }
    public CellType Type { get; set; }
    
    // O an bu hücrenin üzerinde duran blok/taş
    public Item CurrentItem { get; private set; }

    public Cell(int x, int y, CellType type = CellType.Normal)
    {
        X = x;
        Y = y;
        Type = type;
        CurrentItem = null;
    }

    // Hücrenin oynanabilir olup olmadığını kontrol eder
    public bool IsPlayable => Type == CellType.Normal;

    // Hücreye yeni bir taş atar
    public void SetItem(Item newItem)
    {
        CurrentItem = newItem;
    }

    // Hücredeki taşı boşaltır ve referansını döndürür (Düşme mekaniği için)
    public Item ExtractItem()
    {
        Item extracted = CurrentItem;
        CurrentItem = null;
        return extracted;
    }

    // Hücrenin dolu olup olmadığını kontrol eder
    public bool HasItem => CurrentItem != null;
}