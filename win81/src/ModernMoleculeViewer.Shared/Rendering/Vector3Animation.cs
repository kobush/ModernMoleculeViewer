using System;
using SharpDX;
using SharpDX.Toolkit;

namespace ModernMoleculeViewer.Rendering
{
    public class Vector3Animation
    {
        private readonly Action<Vector3> _applyAction;
        private TimeSpan? _startTime;

        public Vector3Animation(Vector3 @from, Vector3 to, TimeSpan duration, Action<Vector3> applyAction)
        {
            _applyAction = applyAction;
            From = @from;
            To = to;
            Duration = duration;

            if (From == To)
            {
                Value = To;
                IsCompleted = true;
            }
        }

        public TimeSpan Duration { get; private set; }

        public Vector3 From { get; private set; }

        public Vector3 To { get; private set; }

        public Vector3 Value { get; private set; }

        public bool IsCompleted { get; private set; }

        public void Update(GameTime gameTime)
        {
            if (_startTime == null)
            {
                _startTime = gameTime.TotalGameTime;
                Value = From;
            }
            else
            {
                TimeSpan elapsedTimeSpan = gameTime.TotalGameTime - _startTime.Value;
                if (elapsedTimeSpan >= Duration)
                {
                    IsCompleted = true;
                    Value = To;
                }
                else
                {
                    float t = elapsedTimeSpan.Ticks/(float)Duration.Ticks;

                    // apply easing function
                    t = 3 * (float)Math.Pow(t, 2) - 2 * (float)Math.Pow(t, 3);

                    InterpolateValue(t);
                }
            }

            ApplyValue();
        }

        private void ApplyValue()
        {
            if (_applyAction != null)
                _applyAction(Value);
        }

        private void InterpolateValue(float amount)
        {
            Value = Vector3.Lerp(From, To, amount);
        }
    }
}