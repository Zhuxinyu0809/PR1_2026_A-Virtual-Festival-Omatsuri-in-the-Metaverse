using UnityEngine;
using System.Collections;

namespace AkilliMum
{
    public class SimpleRotaterAround : MonoBehaviour
    {

        public Vector3 Around;
        public float Angle;
        public bool UseLocalRotation;
        public GameObject ToRotateAround;

        private Renderer _renderer;

        public void Start()
        {
            _renderer = ToRotateAround?.GetComponent<Renderer>();
        }

        void Update()
        {
            var rotate = Angle * Time.deltaTime;
            if (UseLocalRotation)
            {
                if (_renderer == null) { 
                    Debug.LogWarning("Local rotation needs renderer on target! No renderers found, returning...");
                    return;
                }
                transform.RotateAround(_renderer.bounds.center, Around, rotate);
            }
            else
                transform.RotateAround(ToRotateAround.transform.position, Around, rotate);
        }
    }
}
