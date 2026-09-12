namespace Squidbasket.Player
{
    /// <summary>
    /// Pure ping-pong oscillation model for the Power Bar (ADR-0002). Loops continuously
    /// between Min and Max rather than filling once; a Shot Timeout is reached at 2.5 loops.
    /// Callers must feed <see cref="Advance"/> unscaled delta time — the Power Bar is the sole
    /// exemption from Bullet Time, so this class applies no time-scaling of its own.
    /// </summary>
    public sealed class PowerBarOscillator
    {
        private const float TimeoutLoops = 2.5f;

        private readonly float _min;
        private readonly float _max;
        private readonly float _speed;
        private float _elapsed;

        public PowerBarOscillator(float min, float max, float speed)
        {
            _min = min;
            _max = max;
            _speed = speed;
        }

        public float Min => _min;
        public float Max => _max;

        public float CurrentValue
        {
            get
            {
                float range = _max - _min;
                if (range <= 0f)
                {
                    return _min;
                }

                float cycle = range * 2f;
                float t = _elapsed % cycle;
                return t <= range ? _min + t : _max - (t - range);
            }
        }

        public float LoopsCompleted
        {
            get
            {
                float range = _max - _min;
                return range <= 0f ? 0f : _elapsed / (range * 2f);
            }
        }

        public bool HasTimedOut => LoopsCompleted >= TimeoutLoops;

        public void Advance(float unscaledDeltaTime)
        {
            _elapsed += unscaledDeltaTime * _speed;
        }

        public void Reset()
        {
            _elapsed = 0f;
        }
    }
}
