using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheAdventure.Models
{
    public class Player2Object : RenderableGameObject
    {
        private const int _speed = 128; // pixels per second

        private DateTimeOffset _lastDashTime = DateTimeOffset.MinValue;
        private TimeSpan _dashCooldown = TimeSpan.FromSeconds(2);
        private int _dashDistance = 50;

        private bool _isInvulnerable = false;
        private DateTimeOffset _invulnerabilityEndTime = DateTimeOffset.MinValue;

        public bool IsInvulnerable => _isInvulnerable;

        public void TryDash()
        {
            var now = DateTimeOffset.Now;
            if (now - _lastDashTime < _dashCooldown || State.State == PlayerState.GameOver)
            {
                return;
            }

            var (x, y) = Position;

            switch (State.Direction)
            {
                case PlayerStateDirection.Right:
                    x += _dashDistance;
                    break;
                case PlayerStateDirection.Left:
                    x -= _dashDistance;
                    break;
                case PlayerStateDirection.Up:
                    y -= _dashDistance;
                    break;
                case PlayerStateDirection.Down:
                    y += _dashDistance;
                    break;
                default:
                    x += _dashDistance;
                    break;
            }

            Position = (x, y);
            _lastDashTime = now;
        }

        public void ActivateInvulnerability(TimeSpan duration)
        {
            if (State.State == PlayerState.GameOver) return;

            _isInvulnerable = true;
            _invulnerabilityEndTime = DateTimeOffset.Now.Add(duration);
        }

        public void UpdateInvulnerability()
        {
            if (_isInvulnerable && DateTimeOffset.Now >= _invulnerabilityEndTime)
            {
                _isInvulnerable = false;
            }
        }

        public enum PlayerStateDirection
        {
            None = 0,
            Down,
            Up,
            Left,
            Right
        }

        public enum PlayerState
        {
            None = 0,
            Idle,
            Move,
            Attack,
            GameOver
        }

        public (PlayerState State, PlayerStateDirection Direction) State { get; private set; }

        public Player2Object(SpriteSheet spriteSheet, int x, int y) : base(spriteSheet, (x, y))
        {
            SetState(PlayerState.Idle, PlayerStateDirection.Down);
        }

        public void SetState(PlayerState state)
        {
            SetState(state, State.Direction);
        }

        public void SetState(PlayerState state, PlayerStateDirection direction)
        {
            if (State.State == PlayerState.GameOver)
            {
                return;
            }

            if (State.State == state && State.Direction == direction)
            {
                return;
            }

            if (state == PlayerState.None && direction == PlayerStateDirection.None)
            {
                SpriteSheet.ActivateAnimation(null);
            }
            else if (state == PlayerState.GameOver)
            {
                SpriteSheet.ActivateAnimation(Enum.GetName(state));
            }
            else
            {
                var animationName = Enum.GetName(state) + Enum.GetName(direction);
                SpriteSheet.ActivateAnimation(animationName);
            }

            State = (state, direction);
        }

        public void GameOver()
        {
            SetState(PlayerState.GameOver, PlayerStateDirection.None);
        }

        public void Attack()
        {
            if (State.State == PlayerState.GameOver)
            {
                return;
            }

            var direction = State.Direction;
            SetState(PlayerState.Attack, direction);
        }

        public void UpdatePosition(double up, double down, double left, double right, int width, int height, double time)
        {
            if (State.State == PlayerState.GameOver)
            {
                return;
            }

            var pixelsToMove = _speed * (time / 1000.0);

            var x = Position.X + (int)(right * pixelsToMove);
            x -= (int)(left * pixelsToMove);

            var y = Position.Y + (int)(down * pixelsToMove);
            y -= (int)(up * pixelsToMove);

            var newState = State.State;
            var newDirection = State.Direction;

            if (x == Position.X && y == Position.Y)
            {
                if (State.State == PlayerState.Attack)
                {
                    if (SpriteSheet.AnimationFinished)
                    {
                        newState = PlayerState.Idle;
                    }
                }
                else
                {
                    newState = PlayerState.Idle;
                }
            }
            else
            {
                newState = PlayerState.Move;

                if (y < Position.Y && newDirection != PlayerStateDirection.Up)
                {
                    newDirection = PlayerStateDirection.Up;
                }

                if (y > Position.Y && newDirection != PlayerStateDirection.Down)
                {
                    newDirection = PlayerStateDirection.Down;
                }

                if (x < Position.X && newDirection != PlayerStateDirection.Left)
                {
                    newDirection = PlayerStateDirection.Left;
                }

                if (x > Position.X && newDirection != PlayerStateDirection.Right)
                {
                    newDirection = PlayerStateDirection.Right;
                }
            }

            if (newState != State.State || newDirection != State.Direction)
            {
                SetState(newState, newDirection);
            }

            Position = (x, y);
        }

    }

}
