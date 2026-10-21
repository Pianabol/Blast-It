using System.Collections.Generic;
using UnityEngine;

public class MatchDetector : MonoBehaviour
{
    public static MatchDetector Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public List<Cell> FindMatches(Cell cellA, Cell cellB)
    {
        HashSet<Cell> matchedCells = new HashSet<Cell>();

        matchedCells.UnionWith(GetMatchesForCell(cellA));
        matchedCells.UnionWith(GetMatchesForCell(cellB));

        return new List<Cell>(matchedCells);
    }

    public List<Cell> GetMatchesForCell(Cell startCell)
    {
        List<Cell> finalMatches = new List<Cell>();
        if (startCell == null || !startCell.HasItem) return finalMatches;

        ItemType typeToMatch = startCell.CurrentItem.Type;

        // --- YATAY TARAMA ---
        List<Cell> horizontalMatches = new List<Cell> { startCell };
        
        for (int x = startCell.X - 1; x >= 0; x--)
        {
            Cell c = BoardManager.Instance.GetCell(x, startCell.Y);
            if (c != null && c.HasItem && c.CurrentItem.Type == typeToMatch) horizontalMatches.Add(c);
            else break;
        }
        for (int x = startCell.X + 1; x < BoardManager.Instance.Width; x++)
        {
            Cell c = BoardManager.Instance.GetCell(x, startCell.Y);
            if (c != null && c.HasItem && c.CurrentItem.Type == typeToMatch) horizontalMatches.Add(c);
            else break;
        }

        if (horizontalMatches.Count >= 3)
        {
            finalMatches.AddRange(horizontalMatches);
        }

        // --- DİKEY TARAMA ---
        List<Cell> verticalMatches = new List<Cell> { startCell };
        
        for (int y = startCell.Y - 1; y >= 0; y--)
        {
            Cell c = BoardManager.Instance.GetCell(startCell.X, y);
            if (c != null && c.HasItem && c.CurrentItem.Type == typeToMatch) verticalMatches.Add(c);
            else break;
        }
        for (int y = startCell.Y + 1; y < BoardManager.Instance.Height; y++)
        {
            Cell c = BoardManager.Instance.GetCell(startCell.X, y);
            if (c != null && c.HasItem && c.CurrentItem.Type == typeToMatch) verticalMatches.Add(c);
            else break;
        }

        if (verticalMatches.Count >= 3)
        {
            finalMatches.AddRange(verticalMatches);
        }

        // Kesişimlerdeki çift kayıtları (startCell) temizlemek için HashSet üzerinden Listeye çeviriyoruz
        return new List<Cell>(new HashSet<Cell>(finalMatches));
    }

    public List<Cell> FindAllMatches()
    {
        HashSet<Cell> allMatches = new HashSet<Cell>();

        for (int x = 0; x < BoardManager.Instance.Width; x++)
        {
            for (int y = 0; y < BoardManager.Instance.Height; y++)
            {
                Cell cell = BoardManager.Instance.GetCell(x, y);
                if (cell != null && cell.IsPlayable && cell.HasItem)
                {
                    allMatches.UnionWith(GetMatchesForCell(cell));
                }
            }
        }

        return new List<Cell>(allMatches);
    }

    // ====================================================================
    // YENİ: DEADLOCK (TIKANIKLIK) KONTROL SİSTEMİ
    // ====================================================================

    /// <summary>
    /// Tahtada oyuncunun yapabileceği geçerli bir hamle olup olmadığını kontrol eder.
    /// Sadece sağındaki ve üstündeki taşla "Sahte Takas (Simulated Swap)" uygular.
    /// </summary>
    public bool HasAvailableMove()
    {
        int width = BoardManager.Instance.Width;
        int height = BoardManager.Instance.Height;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Cell currentCell = BoardManager.Instance.GetCell(x, y);

                // Eğer hücre boşsa veya oynatılamazsa (Void/Obstacle) geç
                if (currentCell == null || !currentCell.IsPlayable || !currentCell.HasItem) 
                    continue;

                // 1. SAĞDAKİ komşu ile hayali takas denemesi
                if (x + 1 < width)
                {
                    Cell rightCell = BoardManager.Instance.GetCell(x + 1, y);
                    if (rightCell != null && rightCell.IsPlayable && rightCell.HasItem)
                    {
                        if (SimulateSwapAndCheck(currentCell, rightCell))
                            return true; // Geçerli hamle bulundu, aramayı kes!
                    }
                }

                // 2. YUKARIDAKİ komşu ile hayali takas denemesi
                if (y + 1 < height)
                {
                    Cell upCell = BoardManager.Instance.GetCell(x, y + 1);
                    if (upCell != null && upCell.IsPlayable && upCell.HasItem)
                    {
                        if (SimulateSwapAndCheck(currentCell, upCell))
                            return true; // Geçerli hamle bulundu, aramayı kes!
                    }
                }
            }
        }

        // Bütün tahta tarandı ama hiçbir takas eşleşme yaratmadı. OYUN KİLİTLENDİ!
        Debug.Log("<color=red>[Deadlock]</color> Tahtada yapılabilecek hiçbir hamle kalmadı!");
        return false; 
    }

    /// <summary>
    /// İki hücrenin datasını anlık olarak değiştirip eşleşme var mı diye bakar, sonra veriyi geri çevirir.
    /// </summary>
    private bool SimulateSwapAndCheck(Cell cellA, Cell cellB)
    {
        // 1. Datayı Sahte Olarak Takas Et (Animasyon yok, sadece arka planda)
        BoardManager.Instance.SwapItemsData(cellA.X, cellA.Y, cellB.X, cellB.Y);

        // 2. Takas sonrası bu iki hücreden herhangi birinde 3'lü eşleşme oldu mu?
        bool isMatch = GetMatchesForCell(cellA).Count >= 3 || GetMatchesForCell(cellB).Count >= 3;

        // 3. Datayı Eski Haline Geri Getir (ÖNEMLİ: Tahtayı bozmamak için)
        BoardManager.Instance.SwapItemsData(cellA.X, cellA.Y, cellB.X, cellB.Y);

        return isMatch;
    }
}