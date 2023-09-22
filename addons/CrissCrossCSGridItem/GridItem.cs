using Godot;
using System;
using System.Collections.Generic;

public partial class GridItem : Area2D
{
    // The grid this item is actually attached to.
    [Export]
    private Grid2D grid;

    // The grid that this item appears to be on. For example, while something is being dragged.
    private Grid2D previewGrid;

    [Export]
    public bool canMove = true;

    [Export]
    public bool canDrag = true;

    [Export]
    private Vector2I size = Vector2I.One;

    [Export]
    private GridsManager gridsManager;

    bool dragging = false;
    Vector2I dragStartPos = Vector2I.Zero;
    List<Vector2I> occupiedCells = new List<Vector2I>();
    Tween tween;

    public Func<Grid2D, bool> CanCompleteMoveAction = (grid) =>
    {
        return true;
    };

    public Action CompleteMoveAction;

    // The actual cell that this item is currently on
    Vector2I _topLeftCell;
    Vector2I TopLeftCell
    {
        get { return _topLeftCell; }
        set
        {
            if (grid.HasCells(GetCells(value)))
            {
                _topLeftCell = value;
            }

            var worldPos = grid.MapToWorld(TopLeftCell) + GetCenteredOffset();
            tween = CreateTween().SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Quint);
            tween.TweenProperty(this, "global_position", worldPos, 0.3f);
        }
    }

    // The cell this item appears to be on. For example, while pushed by a dragged item.
    Vector2 previewedTopLeftCell;

    public override void _Ready()
    {
        previewGrid = grid;

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
            Grid2D newGrid = gridsManager.getGridAtPosition(GetGlobalMousePosition());
            if (newGrid != null)
            {
                previewGrid = newGrid;
            }
            GlobalPosition = GetGlobalMousePosition();
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
        if (canDrag)
        {
            tween.Stop();
            ZIndex = 1;
            dragging = true;
            dragStartPos = TopLeftCell;

            grid.getGDGridRef().QueueRedraw();
        }
    }

    private void StopDragging()
    {
        ZIndex = 0;
        dragging = false;

        if (canMove)
        {
            grid.SetValues(GetCells(dragStartPos), null);

            var targetCell = GetMouseCell(previewGrid);
            var targetCells = GetCells(targetCell);
            if (
                IsValidTargetPositionAndEmpty(previewGrid, targetCells)
                && CanCompleteMoveAction(previewGrid)
            )
            {
                grid = previewGrid;
                CompleteMoveAction();
            }
            else
            {
                targetCell = dragStartPos;
            }

            MoveTo(targetCell);
        }
        else
        {
            MoveTo(dragStartPos);
        }

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
        grid.getGDGridRef().QueueRedraw();
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

    private Vector2I GetMouseCell(Grid2D _grid)
    {
        return _grid.WorldToMap(
            GetGlobalMousePosition() - (GetCenteredOffset() - grid.CellSize * Vector2.One / 2.0f)
        );
    }

    private bool IsValidTargetPosition(Grid2D _grid, List<Vector2I> targetCells)
    {
        return _grid.HasCells(targetCells) && gridsManager.CanMoveToGrid(grid, _grid);
    }

    private bool IsValidTargetPositionAndEmpty(Grid2D _grid, List<Vector2I> targetCells)
    {
        return IsValidTargetPosition(_grid, targetCells) && _grid.AreValuesNull(targetCells);
    }

    public override void _Draw()
    {
        if (dragging)
        {
            var currentCell = GetMouseCell(previewGrid);
            if (!IsValidTargetPosition(previewGrid, GetCells(currentCell)))
            {
                return;
            }

            var rectPosition = ToLocal(previewGrid.MapToWorld(currentCell));
            var rectSize = size * previewGrid.CellSize;
            DrawRect(new Rect2(rectPosition, rectSize), new Color(1.0f, 1.0f, 1.0f, 0.3f));
        }
    }
}
