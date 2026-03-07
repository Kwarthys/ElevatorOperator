using Godot;
using System;

public partial class InputManager : Node
{
    [Export] private GameManager gameManager;

    public override void _Process(double delta)
    {
        bool up = Input.IsActionJustPressed("Up");
        bool down = Input.IsActionJustPressed("Down");
        bool left = Input.IsActionJustPressed("Left");
        bool right = Input.IsActionJustPressed("Right");

        if(up != down)
        {
            if(up)
                gameManager.OnInputUp();
            else
                gameManager.OnInputDown();
        }

        if(left != right)
        {
            if(left)
                gameManager.OnInputLeft();
            else
                gameManager.OnInputRight();
        }


    }

}
