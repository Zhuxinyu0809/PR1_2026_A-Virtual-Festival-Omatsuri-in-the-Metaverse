using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class HeartFireworkBurst : MonoBehaviour
{
    private ParticleSystem _ps;
    private ParticleSystem.Particle[] _particles;

    [Header("Heart Burst Settings")]
    [Tooltip("Z軸厚度，建議調低啲 (例如 0.5) 等個心形更加清晰")]
    public float zThickness = 0.5f;

    [Header("VR Target Facing")]
    [Tooltip("爆炸時心形會自動面向玩家 (全 3D 視角)")]
    public bool faceCameraInVR = true;
    
    [Tooltip("Meta XR SDK 專用：請將 OVRCameraRig -> CenterEyeAnchor 拖入嚟！")]
    public Transform vrCameraTransform;

    private HashSet<uint> _processedParticles = new HashSet<uint>();
    
    // 用嚟保證粒子平均分佈，唔會因為隨機而炒埋一碟
    private int _particleCounter = 0; 
    [Tooltip("組成心形嘅粒子解析度 (建議 150-200)")]
    public int heartResolution = 150;

    private void Start()
    {
        _ps = GetComponent<ParticleSystem>();
        _particles = new ParticleSystem.Particle[_ps.main.maxParticles];

        if (vrCameraTransform == null && Camera.main != null)
        {
            vrCameraTransform = Camera.main.transform;
        }
    }

    private void LateUpdate()
    {
        if (_ps == null) return;

        int numParticlesAlive = _ps.GetParticles(_particles);
        if (numParticlesAlive == 0) return;

        bool needsUpdate = false;

        // 預先計算面向玩家嘅旋轉矩陣 (支援全 3D，玩家望上天都睇到正心形)
        Quaternion lookRotation = Quaternion.identity;
        if (faceCameraInVR && vrCameraTransform != null)
        {
            // 取得爆炸中心點 (用第一粒生存中嘅粒子位置代表)
            Vector3 burstCenter = _particles[0].position;
            Vector3 lookDir = vrCameraTransform.position - burstCenter;
            
            if (lookDir != Vector3.zero)
            {
                lookRotation = Quaternion.LookRotation(lookDir);
            }
        }

        for (int i = 0; i < numParticlesAlive; i++)
        {
            if (!_processedParticles.Contains(_particles[i].randomSeed))
            {
                _processedParticles.Add(_particles[i].randomSeed);
                needsUpdate = true;

                // 【完美分佈演算法】用 Counter 取代 Random，確保粒子圍住心形軌跡排得靚
                float t = ((float)(_particleCounter % heartResolution) / heartResolution) * Mathf.PI * 2f;
                _particleCounter++;

                float sinT = Mathf.Sin(t);
                float x = 16f * sinT * sinT * sinT;
                float y = 13f * Mathf.Cos(t) - 5f * Mathf.Cos(2f * t) - 2f * Mathf.Cos(3f * t) - Mathf.Cos(4f * t);

                // 組合方向並加入極少量 Z 軸厚度
                Vector3 heartDirection = new Vector3(x, y, Random.Range(-zThickness, zThickness)).normalized;

                // 面向玩家
                if (faceCameraInVR)
                {
                    heartDirection = lookRotation * heartDirection;
                }

                // 強制覆寫速度方向
                float originalSpeed = _particles[i].velocity.magnitude;
                // 防呆：如果 Unity SubEmitter 派發嘅速度太細，強制俾返個速度佢
                if (originalSpeed < 0.1f) originalSpeed = Random.Range(10f, 15f);

                _particles[i].velocity = heartDirection * originalSpeed;
            }
        }

        if (needsUpdate)
        {
            _ps.SetParticles(_particles, numParticlesAlive);
        }

        // 定期清理 HashSet 避免 Memory 增長
        if (_processedParticles.Count > _ps.main.maxParticles * 2)
        {
            _processedParticles.Clear();
            for (int i = 0; i < numParticlesAlive; i++)
            {
                _processedParticles.Add(_particles[i].randomSeed);
            }
        }
    }
}