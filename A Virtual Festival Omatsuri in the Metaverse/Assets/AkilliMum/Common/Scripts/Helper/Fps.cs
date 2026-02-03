using UnityEngine;

// ReSharper disable InconsistentNaming
// ReSharper disable once CheckNamespace
namespace AkilliMum
{
    public class Fps : MonoBehaviour
    {
        /// <summary>
        /// FPS update time in seconds
        /// </summary>
        public float FpsUpdateTime = 0.5f;

        private float _accumulator; // FPS accumulated over the interval
        private int _frames; // Frames drawn over the interval
        private float _timeLeft; // Left time for current interval
        private float _fps;

        internal virtual void Start()
		{
            _timeLeft = FpsUpdateTime;  
		}

		internal virtual void Update()
        {
            _timeLeft -= Time.deltaTime;
            _accumulator += Time.timeScale / Time.deltaTime;
            ++_frames;

            // Interval ended - update and start new interval
            if (_timeLeft <= 0.0)
            {
                _fps = _accumulator / _frames;
                _timeLeft = FpsUpdateTime;
                _accumulator = 0.0F;
                _frames = 0;
            }
        }

        internal float GetFps()
        {
            return _fps;
        }
    }
}