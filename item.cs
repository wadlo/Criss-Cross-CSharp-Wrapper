using Godot;
using System.Collections.Generic;

public partial class item : Area2D
{
    [Export]
    private Node2D gridRef;
    private Grid2D grid;

    [Export]
    private Vector2I size = Vector2I.One;

    bool dragging = false;
    Vector2I dragStartPos = Vector2I.Zero;
    List<Vector2I> occupiedCells = new List<Vector2I>();
    Vector2I _topLeftCell;
    Tween tween;
    Vector2I TopLeftCell
    {
        get { return _topLeftCell; }
        set
        {
            UpdateGrid();

            if (grid.HasCells(GetCells(value)))
            {
                _topLeftCell = value;
            }

            var worldPos = grid.MapToWorld(TopLeftCell);
            tween = CreateTween().SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Quint);
            tween.TweenProperty(this, "global_position", worldPos, 0.3f);
        }
    }

    void UpdateGrid()
    {
        if (grid == null)
        {
            grid = new Grid2D(gridRef);
        }
    }

    public override void _Ready()
    {
        UpdateGrid();
        this.InputEvent += OnClick;

        for (int x = 0; x < size.X; x++)
        {
            for (int y = 0; y < size.Y; y++)
            {
                occupiedCells.Add(new Vector2I(x, y));
            }
        }

        MoveTo(grid.WorldToMap(GlobalPosition));
    }

    public override void _Process(double delta)
    {
        if (dragging)
        {
            GlobalPosition = GetGlobalMousePosition() - GetCenteredOffset();
            QueueRedraw();
        }
    }

    public void OnClick(Node viewport, InputEvent @event, long shapeIdx)
    {
        if (@event is InputEventMouseButton eventMouseButton)
        {
            if (eventMouseButton.ButtonIndex == MouseButton.Left && eventMouseButton.Pressed)
            {
                StartDragging();
            }
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseButtonEvent)
        {
            if (mouseButtonEvent.ButtonIndex == MouseButton.Left && mouseButtonEvent.IsReleased())
            {
                if (dragging)
                {
                    StopDragging();
                }
            }
        }
    }

    private void StartDragging()
    {
        tween.Stop();
        ZIndex = 1;
        dragging = true;
        dragStartPos = TopLeftCell;

        grid.SetValues(GetCells(TopLeftCell), null);
        gridRef.QueueRedraw();
    }

    private void StopDragging()
    {
        ZIndex = 0;
        dragging = false;

        var targetCell = GetMouseCell();
        var targetCells = GetCells(targetCell);
        if (!grid.HasCells(targetCells) || !grid.AreValuesNull(targetCells))
        {
            targetCell = dragStartPos;
        }

        MoveTo(targetCell);
        QueueRedraw();
    }

    private void MoveTo(Vector2I cell)
    {
        var targetCells = GetCells(cell);
        if (!grid.HasCells(targetCells) || !grid.AreValuesNull(targetCells))
        {
            cell = GetNextEmptyCellInGrid() ?? cell;
        }

        TopLeftCell = cell;
        grid.SetValues(GetCells(TopLeftCell), this);
        gridRef.QueueRedraw();
    }

    private Vector2I? GetNextEmptyCellInGrid()
    {
        foreach (Vector2I cell in grid.GetCells())
        {
            if (grid.IsValueNull(cell))
            {
                return cell;
            }
        }
        return null;
    }

    private Vector2 GetCenteredOffset()
    {
        return new Vector2(size.X, size.Y) * 0.5f * grid.CellSize;
    }

    private List<Vector2I> GetCells(Vector2I origin)
    {
        List<Vector2I> cells = new List<Vector2I>();
        foreach (Vector2I occupiedCell in occupiedCells)
        {
            cells.Add(occupiedCell + origin);
        }
        return cells;
    }

    private Vector2I GetMouseCell()
    {
        return grid.WorldToMap(
            GetGlobalMousePosition() - (GetCenteredOffset() - grid.CellSize * Vector2.One / 2.0f)
        );
    }

    public override void _Draw()
    {
        if (dragging)
        {
            var currentCell = GetMouseCell();
            if (!grid.HasCells(GetCells(currentCell)))
            {
                return;
            }

            var rectPosition = ToLocal(grid.MapToWorld(currentCell));
            var rectSize = size * grid.CellSize;
            DrawRect(new Rect2(rectPosition, rectSize), new Color(1.0f, 1.0f, 1.0f, 0.3f));
        }
    }
}
