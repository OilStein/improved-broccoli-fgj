using Godot;
using System;
using System.Drawing;
using System.Security.Cryptography.X509Certificates;

namespace Beatroot
{
    public class Beetroot : BaseCharacter
    {
        enum AnimationState
        {
            Idle,
            Moving,
            Moving2
        }

        private Node2D _leafLeft;
        private Node2D _leafRight;
        private Sprite _dayFace;
        private Sprite _nightFace;

        private AnimationState _animationState;

        private float _timer;
        private float _rotationMultiplier = 2.5f;
        private float _animationDelay = 3;

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            _leafLeft = GetNode<Node2D>("./RotateL");
            _leafRight = GetNode<Node2D>("./RotateR");
            _dayFace = GetNode<Sprite>("./Face/Day");
            _nightFace = GetNode<Sprite>("./Face/Night");

            _animationState = AnimationState.Idle;
            _timer = 0;

            var mainState = GetNode<MainState>("/root/MainState");
            mainState.Connect("TransitionToDay", this, "TransitionToDay");
            mainState.Connect("TransitionToNight", this, "TransitionToNight");
        }

        public override void _Process(float delta)
        {
            if (_animationState == AnimationState.Idle)
            {
                if (_timer > _animationDelay)
                {
                    _animationState = AnimationState.Moving;
                }
                else
                {
                    _timer += delta;
                }
            }
            else if (_animationState == AnimationState.Moving || _animationState == AnimationState.Moving2)
            {
                _leafLeft.Rotate(-_rotationMultiplier * delta);
                _leafRight.Rotate(_rotationMultiplier * delta);

                if (_leafLeft.Rotation <= -0.3)
                {
                    _rotationMultiplier *= -1;
                }
                else if (_leafLeft.Rotation >= 0)
                {
                    _rotationMultiplier *= -1;
                    _animationState = _animationState == AnimationState.Moving ? AnimationState.Moving2 : AnimationState.Idle;
                    _animationDelay = new Random().Next(3, 12);
                    _timer = 0;
                }
            }
        }

        public void TransitionToDay()
        {
            _dayFace.Visible = true;
            _nightFace.Visible = false;
        }

        public void TransitionToNight()
        {
            _dayFace.Visible = false;
            _nightFace.Visible = true;
        }
    }
}
