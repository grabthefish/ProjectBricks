using Godot;
using System;
using System.Threading.Tasks;

public class Level : Node2D
{
    private Node2D _bricksWrapper;

    private int _columns = 25;
    private int _rows = 15;
    private int _brickSize = 30;
    private int _brickMargin = 2;

    private Color[] _colors = new[] { Colors.Blue, Colors.Red, Colors.Green, Colors.Yellow };

    public override void _Ready()
    {
        _bricksWrapper = GetNode<Node2D>("Bricks");

        Setup();
    }

    private void Setup()
    {
        var rand = new Random();

        var gridWidth = (_columns * (_brickSize + _brickMargin)) - _brickMargin;
        var xOffset = (GetViewportRect().Size.x - gridWidth) / 2;

        var gridHeight = (_rows * (_brickSize + _brickMargin)) - _brickMargin;
        var yOffset = (GetViewportRect().Size.y - gridHeight) - (_brickMargin * 3);

        for (var c = 0; c < _columns; c++)
        {
            for (var r = 0; r < _rows; r++)
            {
                var button = new Button();
                button.FocusMode = Control.FocusModeEnum.None;
                button.ToggleMode = true;

                // position button
                var x = (c * (_brickSize + _brickMargin)) + xOffset;
                var y = r * (_brickSize + _brickMargin) + yOffset;

                button.MarginLeft = x;
                button.MarginTop = y;
                button.MarginRight = x + _brickSize;
                button.MarginBottom = y + _brickSize;

                // style button
                var color = _colors[rand.Next(_colors.Length)];

                var normalStyle = new StyleBoxFlat();
                normalStyle.BgColor = color;
                button.AddStyleboxOverride("normal", normalStyle);

                var hoverPressedStyle = new StyleBoxFlat();
                hoverPressedStyle.BgColor = color;
                hoverPressedStyle.ShadowColor = Colors.White;
                hoverPressedStyle.ShadowSize = _brickMargin*2;
                button.AddStyleboxOverride("pressed", hoverPressedStyle);
                button.AddStyleboxOverride("hover", hoverPressedStyle);

                // hookup click signal
                button.Connect("toggled", this, nameof(BrickPressed));

                _bricksWrapper.AddChild(button);
            }
        }
    }

    private void BrickPressed(bool newState)
    {
        GD.Print(newState);
    }
}
