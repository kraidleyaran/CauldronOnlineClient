using System.Collections.Generic;
using System.Linq;
using ConcurrentMessageBus;
using DG.Tweening;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.WorldCamera
{
    public class CameraController : MonoBehaviour
    {
        public static Camera Camera => _instance._camera;

        private static CameraController _instance = null;

        [SerializeField] private int _moveSpeed = 1;
        [SerializeField] private Rigidbody2D _rigidBody = null;
        [SerializeField] private float _cameraInterpolation = 66f;
        [SerializeField] private Camera _camera = null;
        [SerializeField] private int _zoneTransitionSpeed = 1;
        [SerializeField] private int _minimumZoneTransitionDistance = 1;

        private Vector2 _position = Vector2.zero;
        private List<CameraZoneController> _cameraZones = new List<CameraZoneController>();
        private CameraZoneController _currentZone = null;

        private Tween _cameraTween = null;

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
            if (_instance._cameraTween == null)
            {
                if (!_instance._currentZone)
                {
                    if (_instance._cameraZones.Count > 0)
                    {
                        var closestZone = _instance._cameraZones.Where(z => z.ContainsPoint(position)).OrderBy(z => (z.ClosestPos(position) - position).sqrMagnitude).FirstOrDefault();
                        if (!closestZone)
                        {
                            closestZone = _instance._cameraZones.OrderBy(z => (z.ClosestPos(position) - position).sqrMagnitude).First();
                        }
                        var closestPos = closestZone.GetCameraPosition(position);
                        _instance._position = closestPos;
                        _instance._currentZone = closestZone;
                        if (instant)
                        {
                            _instance._rigidBody.position = _instance._position;
                        }
                    }
                    else
                    {
                        _instance._position = position;
                        if (instant)
                        {
                            _instance._rigidBody.position = position;
                        }
                    }
                }
                else if (!_instance._currentZone.ContainsPoint(position) && _instance._cameraZones.Count > 1)
                {
                    var currentClosePos = _instance._currentZone.GetCameraPosition(position);/*.ToPixelPerfect();*/
                    var closestZone = _instance._cameraZones.Where(z => z != _instance._currentZone).OrderBy(z => (z.ClosestPos(position) - position).sqrMagnitude).First();
                    if (closestZone)
                    {
                        var closestPos = closestZone.GetCameraPosition(position);/*.ToPixelPerfect();*/

                        var closestDistance = (closestPos - position).magnitude;
                        var currentDistance = (currentClosePos - position).magnitude;

                        if (currentDistance > closestDistance)
                        {
                            _instance._currentZone = closestZone;
                            _instance._position = closestPos;
                            if (DataController.WorldState == WorldState.Active)
                            {
                                if (closestDistance >= _instance._minimumZoneTransitionDistance * DataController.Interpolation)
                                {
                                    if (_instance._rigidBody.position != _instance._position)
                                    {
                                        if (_instance._cameraTween != null)
                                        {
                                            if (_instance._cameraTween.IsActive())
                                            {
                                                _instance._cameraTween.Kill();
                                            }

                                            _instance._cameraTween = null;
                                        }
                                        var speed = _instance._zoneTransitionSpeed * TickController.TickRate;
                                        _instance._cameraTween = _instance._rigidBody.DOMove(_instance._position, speed).SetEase(Ease.Linear).OnComplete(() => { _instance._cameraTween = null; });
                                    }
                                }
                                else
                                {
                                    _instance._position = currentClosePos;
                                    if (instant)
                                    {
                                        _instance._rigidBody.position = _instance._position;
                                    }
                                }
                            }

                        }
                        else
                        {
                            _instance._position = currentClosePos;
                            if (instant)
                            {
                                _instance._rigidBody.position = _instance._position;
                            }
                            //if (DataController.WorldState == WorldState.Active)
                            //{
                            //    _rigidBody.position = _position;
                            //    _moved = true;
                            //}
                        }
                    }
                    else
                    {
                        _instance._position = _instance._currentZone.GetCameraPosition(position).ToPixelPerfect();
                        if (instant)
                        {
                            _instance._rigidBody.position = _instance._position;
                        }
                        //if (DataController.WorldState == WorldState.Active)
                        //{
                        //    _rigidBody.position = _position;
                        //    _moved = true;
                        //}
                    }
                }
                else
                {
                    var prev = _instance._position;
                    _instance._position = _instance._currentZone.GetCameraPosition(position);//.ToPixelPerfect();

                    if (DataController.WorldState == WorldState.Active)
                    {
                        var currentDistance = (_instance._position - prev).magnitude;
                        if (currentDistance >= _instance._minimumZoneTransitionDistance * DataController.Interpolation)
                        {
                            if (_instance._cameraTween != null)
                            {
                                if (_instance._cameraTween.IsActive())
                                {
                                    _instance._cameraTween.Kill();
                                }

                                _instance._cameraTween = null;
                            }

                            var speed = _instance._zoneTransitionSpeed * TickController.TickRate;
                            _instance._cameraTween = _instance._rigidBody.DOMove(_instance._position, speed).SetEase(Ease.Linear).OnComplete(() => { _instance._cameraTween = null; });
                        }
                        else if (instant)
                        {
                            _instance._rigidBody.position = _instance._position;
                        }

                    }
                }
            }
            

        }

        public static void AddZone(CameraZoneController zone)
        {
            if (!_instance._cameraZones.Contains(zone))
            {
                _instance._cameraZones.Add(zone);
                if (!_instance._currentZone)
                {
                    _instance._currentZone = zone;
                }
            }

        }

        public static void RemoveZone(CameraZoneController zone)
        {
            if (_instance._currentZone && _instance._currentZone == zone)
            {
                _instance._currentZone = null;
            }
            _instance._cameraZones.Remove(zone);
        }



        private void SubscribeToMessages()
        {
            //gameObject.Subscribe<UpdateTickMessage>(UpdateTick);
            gameObject.Subscribe<FixedUpdateTickMessage>(FixedUpdateTick);
        }

        private void UpdateTick(UpdateTickMessage msg)
        {
            if (_cameraTween == null)
            {
                _rigidBody.position = _position;
            }
            
        }

        private void FixedUpdateTick(FixedUpdateTickMessage msg)
        {
            if (_cameraTween == null)
            {
                _rigidBody.position = Vector2.Lerp(_rigidBody.position, _position, _cameraInterpolation * Time.fixedDeltaTime);
            }
            
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