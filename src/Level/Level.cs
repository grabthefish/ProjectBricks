using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

public class Level : Node2D
{
    private Node2D _bricksWrapper;

    private int _columns = 25;
    private int _rows = 15;
    private int _brickSize = 30;
    private int _brickMargin = 2;

    private float _gridOffsetX;
    private float _gridOffsetY;

    private Color[] _colors = new[] { Colors.Blue, Colors.Red, Colors.Green, Colors.Yellow };

    private Dictionary<Button, Color> _bricks = new Dictionary<Button, Color>();
    private List<Button> _selectedBricks = new List<Button>();

    public override void _Ready()
    {
        _bricksWrapper = GetNode<Node2D>("Bricks");

        Setup();
    }

    private void Setup()
    {
        var rand = new Random();

        var gridWidth = (_columns * (_brickSize + _brickMargin)) - _brickMargin;
        _gridOffsetX = (GetViewportRect().Size.x - gridWidth) / 2;

        var gridHeight = (_rows * (_brickSize + _brickMargin)) - _brickMargin;
        _gridOffsetY = (GetViewportRect().Size.y - gridHeight) - (_brickMargin * 3);

        for (var c = 0; c < _columns; c++)
        {
            for (var r = 0; r < _rows; r++)
            {
                var button = new Button();
                button.FocusMode = Control.FocusModeEnum.None;
                button.ToggleMode = true;

                // position button
                var x = c * (_brickSize + _brickMargin) + _gridOffsetX;
                var y = r * (_brickSize + _brickMargin) + _gridOffsetY;

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
                hoverPressedStyle.ShadowSize = _brickMargin * 2;
                button.AddStyleboxOverride("pressed", hoverPressedStyle);
                button.AddStyleboxOverride("hover", hoverPressedStyle);
                button.AddStyleboxOverride("focus", hoverPressedStyle);

                // hookup click signal
                button.Connect("pressed", this, nameof(BrickPressed), new Godot.Collections.Array() { button });

                _bricksWrapper.AddChild(button);
                _bricks[button] = color;
            }
        }
    }

    private void BrickPressed(Button sender)
    {
        if (sender.Pressed)
        {
            // new brick pressed
            // 1. unpress any pressed bricks that isn't the sender
            // 2. find surrounding bricks of same color and press
            //  a. use a basic maze solving algorithm adjusted to only find same colors
            //     https://en.wikipedia.org/wiki/Maze_generation_algorithm#Randomized_depth-first_search

            // 1. unpress any pressed bricks that isn't the sender
            foreach (var brick in _selectedBricks)
            {
                brick.Pressed = false;
            }
            _selectedBricks.Clear();

            // 2. find surrounding bricks of same color and press
            PressSurroundingBricksOfSameColor(sender);
        }
        else
        {
            // remove selected bricks only if 2 or more are selected else just deselect
            if (_selectedBricks.Count > 1)
            {
                foreach (var brick in _selectedBricks)
                {
                    _bricks.Remove(brick);
                    brick.QueueFree();
                }
                LowerBricks();
                _selectedBricks.Clear();
            }
            else
            {
                sender.Pressed = false;
            }
        }
    }

    private void LowerBricks()
    {
        var selectedColumns = _selectedBricks.GroupBy(x => x.MarginLeft);

        foreach (var column in selectedColumns)
        {
            var selectedColumnBricks = column.OrderBy(x => x.MarginTop).ToArray();

            foreach (var brick in selectedColumnBricks)
            {
                var above = GetBrickRelativeTo(brick, Vector2.Up);
                while (above != null)
                {
                    var next = GetBrickRelativeTo(above, Vector2.Up);

                    above.MarginTop += _brickSize + _brickMargin;
                    above.MarginBottom += _brickSize + _brickMargin;

                    above = next;
                }
            }
        }
    }

    private void PressSurroundingBricksOfSameColor(Button sender)
    {
        _selectedBricks.Add(sender);

        sender.Pressed = true;
        var senderColor = _bricks[sender];

        var buttonAbove = GetBrickRelativeTo(sender, Vector2.Up);
        if (buttonAbove != null && !buttonAbove.Pressed && _bricks[buttonAbove] == senderColor)
            PressSurroundingBricksOfSameColor(buttonAbove);

        var buttonBelow = GetBrickRelativeTo(sender, Vector2.Down);
        if (buttonBelow != null && !buttonBelow.Pressed && _bricks[buttonBelow] == senderColor)
            PressSurroundingBricksOfSameColor(buttonBelow);

        var buttonLeft = GetBrickRelativeTo(sender, Vector2.Left);
        if (buttonLeft != null && !buttonLeft.Pressed && _bricks[buttonLeft] == senderColor)
            PressSurroundingBricksOfSameColor(buttonLeft);

        var buttonRight = GetBrickRelativeTo(sender, Vector2.Right);
        if (buttonRight != null && !buttonRight.Pressed && _bricks[buttonRight] == senderColor)
            PressSurroundingBricksOfSameColor(buttonRight);
    }

    private Button GetBrickRelativeTo(Button origin, Vector2 direction)
    {
        var column = (origin.MarginLeft - _gridOffsetX) / (_brickSize + _brickMargin);
        var row = (origin.MarginTop - _gridOffsetY) / (_brickSize + _brickMargin);

        var neededX = (column + direction.x) * (_brickSize + _brickMargin) + _gridOffsetX;
        var neededY = (row + direction.y) * (_brickSize + _brickMargin) + _gridOffsetY;

        foreach (var brick in _bricks)
        {
            if (brick.Key.MarginLeft == neededX && brick.Key.MarginTop == neededY)
            {
                return brick.Key;
            }
        }

        return null;
    }
}
