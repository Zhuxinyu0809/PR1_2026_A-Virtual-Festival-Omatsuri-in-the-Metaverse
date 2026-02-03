using UnityEngine;

// ReSharper disable once CheckNamespace
namespace AkilliMum
{
    public class SimpleMove : MonoBehaviour
    {
        public Vector3 Direction;
        private float _speed = 1;
        public bool Dynamic;

        // ReSharper disable once UnusedMember.Local
        void Update()
        {
            gameObject.transform.position = Vector3.Lerp(gameObject.transform.position,
                gameObject.transform.position+Direction * _speed, Time.deltaTime);
        }

        // ReSharper disable once UnusedMember.Local
        void FixedUpdate()
        {
            if (Dynamic)
            {
                _speed += new System.Random().Next(1, 5) / 10f;
                //Debug.Log("Simple move test, speed: " + _speed);
            }
        }

        public float GetSpeed()
        {
            return _speed;
        }
    }
}