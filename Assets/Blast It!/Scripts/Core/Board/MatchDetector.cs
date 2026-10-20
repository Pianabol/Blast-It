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
}