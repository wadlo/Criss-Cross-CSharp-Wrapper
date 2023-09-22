using Godot;
using System.Collections.Generic;

using System;
using System.Linq;

public partial class GridsManager : Node
{
    [Export]
    private Grid2D[] grids;

    [Export]
    private Grid2D[] interactableGrids;

    public Grid2D? getGridAtPosition(Vector2 position)
    {
        foreach (Grid2D grid in grids)
        {
            if (grid.HasCell(grid.WorldToMap(position)))
            {
                return grid;
            }
        }

        return null;
    }

    public bool CanMoveToGrid(Grid2D currentGrid, Grid2D targetGrid)
    {
        for (int i = 0; i < grids.Length; i++)
        {
            if (grids[i] == currentGrid)
            {
                Grid2D interactableGrid = interactableGrids[i];
                if (interactableGrid == targetGrid)
                {
                    return true;
                }
            }
        }
        return false;
    }
}
