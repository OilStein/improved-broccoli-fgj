using Godot;
using System;

namespace Beatroot
{
    public abstract class BaseCharacter : Node
    {
        [Export]
        public int MaxHealth = 10;
        [Export]
        public int CurrentHealth = 10;
        [Export]
        public int Damage = 1;
    }
}