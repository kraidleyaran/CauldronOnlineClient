using MessageBusLib;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System
{
    public class CameraController : MonoBehaviour
    {
        private static CameraController _instance = null;

        [SerializeField] private int _moveSpeed = 1;
        [SerializeField] private Rigidbody2D _rigidBody = null;
        [SerializeField] private float _cameraInterpolation = 66f;

        private Vector2 _position = Vector2.zero;

        void Awake()
        {
            if (_instance)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            SubscribeToMessages();
        }

        public static void SetPosition(Vector2 position, bool instant = false)
        {
            _instance._position = position;
            if (instant)
            {
                _instance._rigidBody.position = position;
            }
        }

        private void SubscribeToMessages()
        {
            gameObject.Subscribe<UpdateTickMessage>(UpdateTick);
            gameObject.Subscribe<FixedUpdateTickMessage>(FixedUpdateTick);
        }

        private void UpdateTick(UpdateTickMessage msg)
        {
            _rigidBody.position = _position;
        }

        private void FixedUpdateTick(FixedUpdateTickMessage msg)
        {
            _rigidBody.position = Vector2.Lerp(_rigidBody.position, _position, _cameraInterpolation * Time.fixedDeltaTime);
            //_rigidBody.position = _position;
            //if (_rigidBody.position != _position)
            //{
            //    var moveSpeed = _moveSpeed * DataController.Interpolation * Time.fixedDeltaTime;
            //    var diff = (_position - _rigidBody.position);
            //    if (diff.magnitude > moveSpeed)
            //    {
            //        _rigidBody.position += Vector2.ClampMagnitude(diff.normalized * moveSpeed, moveSpeed);
            //    }
            //    else
            //    {
            //        _rigidBody.position = _position;
            //    }
            //}
        }


    }
}