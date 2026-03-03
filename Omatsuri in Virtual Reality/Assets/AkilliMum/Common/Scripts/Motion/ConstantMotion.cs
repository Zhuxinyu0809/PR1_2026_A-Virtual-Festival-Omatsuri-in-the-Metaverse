using UnityEngine;

// ReSharper disable InconsistentNaming
// ReSharper disable once CheckNamespace
namespace AkilliMum.Motion
{
    [AddComponentMenu("Akilli Mum/Motion/Constant Motion")]
    public class ConstantMotion : MonoBehaviour
    {
        #region Nested Classes

        public enum TranslationMode {
            Off, XAxis, YAxis, ZAxis, Vector, Random
        };

        public enum RotationMode {
            Off, XAxis, YAxis, ZAxis, Vector, Random
        }

        #endregion

        #region Editable Properties

        [SerializeField]
        TranslationMode _translationMode = TranslationMode.Off;

        [SerializeField]
        Vector3 _translationVector = Vector3.forward;

        [SerializeField]
        float _translationSpeed = 1.0f;

        [SerializeField]
        RotationMode _rotationMode = RotationMode.Off;

        [SerializeField]
        Vector3 _rotationAxis = Vector3.up;

        [SerializeField]
        float _rotationSpeed = 30.0f;

        [SerializeField]
        bool _useLocalCoordinate = true;

        #endregion

        #region Public Properties

        public TranslationMode translationMode {
            get { return _translationMode; }
            set { _translationMode = value; }
        }

        public Vector3 translationVector {
            get { return _translationVector; }
            set { _translationVector = value; }
        }

        public float translationSpeed {
            get { return _translationSpeed; }
            set { _translationSpeed = value; }
        }

        public RotationMode rotationMode {
            get { return _rotationMode; }
            set { _rotationMode = value; }
        }

        public Vector3 rotationAxis {
            get { return _rotationAxis; }
            set { _rotationAxis = value; }
        }

        public float rotationSpeed {
            get { return _rotationSpeed; }
            set { _rotationSpeed = value; }
        }

        public bool useLocalCoordinate {
            get { return _useLocalCoordinate; }
            set { _useLocalCoordinate = value; }
        }

        #endregion

        #region Private Variables

        Vector3 _randomVectorT;
        Vector3 _randomVectorR;

        Vector3 TranslationVector {
            get {
                switch (_translationMode)
                {
                    case TranslationMode.XAxis:  return Vector3.right;
                    case TranslationMode.YAxis:  return Vector3.up;
                    case TranslationMode.ZAxis:  return Vector3.forward;
                    case TranslationMode.Vector: return _translationVector;
                }
                // TranslationMode.Random
                return _randomVectorT;
            }
        }

        Vector3 RotationVector {
            get {
                switch (_rotationMode)
                {
                    case RotationMode.XAxis:  return Vector3.right;
                    case RotationMode.YAxis:  return Vector3.up;
                    case RotationMode.ZAxis:  return Vector3.forward;
                    case RotationMode.Vector: return _rotationAxis;
                }
                // RotationMode.Random
                return _randomVectorR;
            }
        }

        #endregion

        #region MonoBehaviour Functions

        // ReSharper disable once UnusedMember.Local
        void Start()
        {
            _randomVectorT = Random.onUnitSphere;
            _randomVectorR = Random.onUnitSphere;
        }

        // ReSharper disable once UnusedMember.Local
        void Update()
        {
            var dt = Time.deltaTime;

            if (_translationMode != TranslationMode.Off)
            {
                var dp = TranslationVector * _translationSpeed * dt;

                if (_useLocalCoordinate)
                    transform.localPosition += dp;
                else
                    transform.position += dp;
            }

            if (_rotationMode != RotationMode.Off)
            {
                var dr = Quaternion.AngleAxis(
                    _rotationSpeed * dt, RotationVector);

                if (_useLocalCoordinate)
                    transform.localRotation = dr * transform.localRotation;
                else
                    transform.rotation = dr * transform.rotation;
            }
        }

        #endregion
    }
}
