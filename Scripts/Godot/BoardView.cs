using Godot;
using RuneGrid.Tactics.Core;

namespace RuneGrid.Tactics.Godot;

public partial class BoardView : Control
{
    private const float TileSize = 64f;
    private const float BoardPadding = 20f;
    public GameSession? Session { get; private set; }
    public event Action<GridPoint>? TileChosen;

    public BoardView()
    {
        MouseFilter = MouseFilterEnum.Stop;
        CustomMinimumSize = new Vector2(650, 500);
    }

    public void Bind(GameSession session)
    {
        Session = session;
        session.StateChanged += QueueRedraw;
        QueueRedraw();
    }

    public override void _Draw()
    {
        if (Session is null)
        {
            DrawString(ThemeDB.FallbackFont, new Vector2(24, 36), "No field selected.", HorizontalAlignment.Left, -1, 18, new Color("B9C0B6"));
            return;
        }

        var grid = Session.Encounter.Grid;
        var highlights = Session.GetHighlights();
        DrawStyleBox(MakePanel(new Color("111E1F"), new Color("D4BF7E")), new Rect2(Vector2.Zero, Size));
        foreach (var tile in grid.Tiles)
        {
            var rectangle = TileRect(tile.Position);
            var color = TileColor(tile.Kind);
            if (highlights.Danger.Contains(tile.Position)) color = color.Lerp(new Color("743D36"), 0.35f);
            if (highlights.Reachable.Contains(tile.Position)) color = new Color("278794");
            if (highlights.Targets.Contains(tile.Position)) color = new Color("B65E52");
            if (highlights.Selected == tile.Position) color = new Color("56D6E6");
            DrawRect(rectangle, color, true);
            DrawRect(rectangle, new Color("7C8C85", 0.42f), false, 1.25f);
            var marker = tile.Kind switch { TileKind.Healing => "+", TileKind.Hazard => "!", TileKind.Teleport => "◇", TileKind.Difficult => "≈", TileKind.Destructible => "×", TileKind.Wall => "█", _ => string.Empty };
            if (!string.IsNullOrEmpty(marker)) DrawString(ThemeDB.FallbackFont, rectangle.Position + new Vector2(23, 37), marker, HorizontalAlignment.Left, -1, 20, new Color("E9E0C9"));
        }
        foreach (var unit in Session.Encounter.Units.Where(unit => unit.IsAlive))
        {
            var rectangle = TileRect(unit.Position).Grow(-11f);
            var tokenColor = new Color(unit.Template.Color);
            DrawCircle(rectangle.GetCenter(), 19f, tokenColor);
            DrawArc(rectangle.GetCenter(), 20f, 0, Mathf.Tau, 24, unit.Faction == Faction.Hero ? new Color("EFE4C8") : new Color("7D3F3C"), 2.4f);
            DrawString(ThemeDB.FallbackFont, rectangle.Position + new Vector2(8, 26), unit.Template.Name[..1], HorizontalAlignment.Left, -1, 16, new Color("102123"));
            var vitality = Math.Clamp((float)unit.Health / unit.Template.MaxHealth, 0f, 1f);
            var bar = new Rect2(rectangle.Position + new Vector2(0, 42), new Vector2(rectangle.Size.X, 4));
            DrawRect(bar, new Color("263738"), true);
            DrawRect(new Rect2(bar.Position, new Vector2(bar.Size.X * vitality, bar.Size.Y)), new Color("75DFDF"), true);
        }
    }

    public override void _GuiInput(InputEvent input)
    {
        if (input is not InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left } || Session is null) return;
        var local = GetLocalMousePosition() - new Vector2(BoardPadding, BoardPadding);
        var point = new GridPoint((int)(local.X / TileSize), (int)(local.Y / TileSize));
        if (Session.Encounter.Grid.InBounds(point)) TileChosen?.Invoke(point);
    }

    private static StyleBoxFlat MakePanel(Color background, Color border)
    {
        var style = new StyleBoxFlat { BgColor = background, BorderColor = border };
        style.SetBorderWidthAll(1);
        return style;
    }

    private static Color TileColor(TileKind kind) => kind switch
    {
        TileKind.Wall => new Color("48595A"), TileKind.Difficult => new Color("515845"), TileKind.Healing => new Color("426D5B"),
        TileKind.Hazard => new Color("81533B"), TileKind.Teleport => new Color("225E71"), TileKind.Gate => new Color("664B36"),
        TileKind.Destructible => new Color("6B5145"), _ => new Color("283738")
    };

    private static Rect2 TileRect(GridPoint point) => new(new Vector2(BoardPadding + point.X * TileSize, BoardPadding + point.Y * TileSize), new Vector2(TileSize - 2, TileSize - 2));
}
