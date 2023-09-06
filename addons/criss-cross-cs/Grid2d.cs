using System.Collections.Generic;
using System.Linq;
using Godot;

/// <summary>
/// Class <c>Grid2D</c> models a grid in two-dimensional space. This wraps the gdscript grid2d class and gives C# types as function and variable values.
/// </summary>
public class Grid2D
{
    private Node2D gdGridRef;

    public Grid2D(Node2D node)
    {
        gdGridRef = node;
    }

    public bool HasCells(List<Vector2I> cells)
    {
        // https://github.com/godotengine/godot-proposals/issues/5486
        // List<Vector2I> and Vector2I[] are not compatible with Variants, so we just manually loop here.
        foreach (Vector2I cell in cells)
        {
            if (!this.HasCell(cell))
            {
                return false;
            }
        }
        return true;
    }

    public bool HasCell(Vector2I cell)
    {
        return gdGridRef.Call("has_cell", cell).As<bool>();
    }

    public Vector2 MapToWorld(Vector2I point)
    {
        Variant _point = Variant.From(point);
        return gdGridRef.Call("map_to_world", _point).As<Vector2>();
    }

    public Vector2I WorldToMap(Vector2 point)
    {
        Variant _point = Variant.From(point);
        return gdGridRef.Call("world_to_map", _point).As<Vector2I>();
    }

    public void SetValues(List<Vector2I> cells, Node node)
    {
        foreach (Vector2I cell in cells)
        {
            SetValue(cell, node);
        }
    }

    public void SetValue(Vector2I cell, Node node)
    {
        Variant _cell = Variant.From(cell);
        gdGridRef.Call("set_value", _cell, Variant.From(node));
    }

    public bool AreValuesNull(List<Vector2I> cells)
    {
        foreach (Vector2I cell in cells)
        {
            if (!IsValueNull(cell))
            {
                return false;
            }
        }
        return true;
    }

    public bool IsValueNull(Vector2I cell)
    {
        return gdGridRef.Call("is_value_null", cell).As<bool>();
    }

    public Vector2I CellSize
    {
        get { return gdGridRef.Get("cell_size").As<Vector2I>(); }
        set { gdGridRef.Set("cell_size", value); }
    }

    public List<Vector2I> GetCells()
    {
        var vector2Result = gdGridRef.Call("get_cells").As<Vector2[]>();
        return vector2Result.OfType<Vector2I>().ToList();
    }
}
