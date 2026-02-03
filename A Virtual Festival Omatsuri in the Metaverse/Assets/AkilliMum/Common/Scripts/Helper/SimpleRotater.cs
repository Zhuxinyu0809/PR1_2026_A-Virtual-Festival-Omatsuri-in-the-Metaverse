using UnityEngine;
using System.Collections;

namespace AkilliMum
{
    public class SimpleRotater : MonoBehaviour
    {
        public Vector3 direction = Vector3.right;
        public float Speed = 5;

        public bool FollowSimpleMove;
        private SimpleMove _simpleMove;

        public Material SpinMaterial;
        public ParticleSystem SpinParticleSystem;
        public float DecreaseValueSpin = 20f;

        public Material SmokeMaterial;
        public ParticleSystem SmokeParticleSystem;
        public float DecreaseValueSmoke = 2.5f;

        public ParticleSystem[] FlameParticles;

        public Transform OuterPoint;

        //private float _spinStart;
        //private float _smokeStart;

        void Start()
        {
            if (FollowSimpleMove)
                _simpleMove = FindObjectOfType<SimpleMove>(true);


        }

        void OnDisable()
        {
            //SmokeParticleSystem.emission.rateOverTimeMultiplier = _spinStart;
            //_smokeStart = SpinParticleSystem.emission.rateOverTimeMultiplier;

            //var alpha = SmokeMaterial.GetColor("_TintColor");
            //alpha.a = 1;
            //SmokeMaterial.SetColor("_TintColor", alpha);
        }

        void Update()
        {
            var speed = Speed;
            if (FollowSimpleMove)
                speed *= (50000 / _simpleMove.GetSpeed());
            gameObject.transform.RotateAround(gameObject.transform.position,
                direction,
                speed * Time.deltaTime);

            if (OuterPoint != null)
            {
                var pos = Camera.main.WorldToScreenPoint(OuterPoint.position);
                //Debug.Log("object center WS: " + pos);
                if (SpinMaterial != null)
                {
                    SpinMaterial.SetVector("_ScreenPos", pos);

                    var e = SpinParticleSystem.main;
                    var color = e.startColor.color;
                    color.a -= DecreaseValueSpin * Time.deltaTime * (Time.frameCount / 100f);
                    if (color.a < 0) color.a = 0;
                    e.startColor = color;
                    //var e = SpinParticleSystem.emission;
                    //e.rateOverTimeMultiplier -= DecreaseValueSpin * Time.deltaTime * (Time.frameCount / 100f);
                    //var e = SpinParticleSystem.main;
                    //e.startSizeMultiplier -= DecreaseValueSpin * Time.deltaTime * (Time.frameCount / 100f);
                }
                if (SmokeMaterial != null)
                {
                    //var e = SmokeParticleSystem.main;
                    //e.startSizeMultiplier -= DecreaseValueSmoke * Time.deltaTime * (Time.frameCount / 100f);
                    //var e = SmokeParticleSystem.emission;
                    //e.rateOverTimeMultiplier -= DecreaseValueSmoke * Time.deltaTime * (Time.frameCount / 100f);
                    //var e = SmokeParticleSystem.main;
                    //e.startSizeMultiplier -= DecreaseValueSmoke * Time.deltaTime * (Time.frameCount / 100f);
                    //var alpha = SmokeMaterial.GetColor("_TintColor");
                    //alpha.a -= DecreaseValueSmoke * Time.deltaTime * (Time.frameCount / 100f);
                    //if (alpha.a < 0) alpha.a = 0;
                    //SmokeMaterial.SetColor("_TintColor", alpha);
                    var e = SmokeParticleSystem.main;
                    var color = e.startColor.color;
                    color.a -= DecreaseValueSmoke * Time.deltaTime * (Time.frameCount / 100f);
                    if(color.a<0)color.a = 0;
                    e.startColor = color;
                    //e.startColor = ;
                }
            }
        }

        public float GetSmokeAlpha()
        {
            return SmokeParticleSystem.main.startColor.color.a;
        }

        public void FireFlame()
        {
            foreach (var flameParticle in FlameParticles)
            {
                flameParticle.Stop(true);
                flameParticle.Play(true);
            }
        }
    }
}
