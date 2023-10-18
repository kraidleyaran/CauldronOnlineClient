using System.Collections.Generic;
using Assets.Resources.Ancible_Tools.Scripts.System;
using CauldronOnlineCommon;
using ConcurrentMessageBus;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Projectile Manager Trait", menuName = "Ancible Tools/Traits/Combat/Projectile/Projectile Manager")]
    public class ProjectileManagerTrait : Trait
    {
        private Dictionary<GameObject, string> _objectLookup = new Dictionary<GameObject, string>();
        private Dictionary<string, int> _projectileMax = new Dictionary<string, int>();
        private Dictionary<string, int> _currentProjectiles = new Dictionary<string, int>();

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            SubscribeToMessages();
        }

        private void SubscribeToMessages()
        {
            _controller.transform.parent.gameObject.SubscribeWithFilter<RegisterProjectileMessage>(RegisterProjectile, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<ProjectileReturnedMessage>(ProjectileReturned, _instanceId);
            _controller.transform.parent.gameObject.SubscribeWithFilter<ProjectileAvailableCheckMessage>(ProjectileAvailbleCheck, _instanceId);
        }

        private void RegisterProjectile(RegisterProjectileMessage msg)
        {
            if (!_projectileMax.TryGetValue(msg.ProjectileName, out var max))
            {
                max = msg.MaxStack;
                _projectileMax.Add(msg.ProjectileName, max);
            }

            if (!_currentProjectiles.TryGetValue(msg.ProjectileName, out var current))
            {
                current = 0;
                _currentProjectiles.Add(msg.ProjectileName, current);
            }

            if (current < max && !_objectLookup.TryGetValue(msg.Projectile, out var projectile))
            {
                projectile = msg.ProjectileName;
                _objectLookup.Add(msg.Projectile, projectile);
                _currentProjectiles[msg.ProjectileName]++;
            }
        }

        private void ProjectileReturned(ProjectileReturnedMessage msg)
        {
            if (_objectLookup.TryGetValue(msg.Projectile, out var projectile))
            {
                if (_currentProjectiles.TryGetValue(projectile, out var current) && current > 0)
                {
                    _currentProjectiles[projectile]--;
                }

                _objectLookup.Remove(msg.Projectile);
            }
            var objId = ObjectManager.GetId(msg.Projectile);
            if (!string.IsNullOrEmpty(objId))
            {
                ClientController.SendToServer(new ClientDestroyObjectMessage{TargetId = objId, Tick = TickController.ServerTick});
            }
        }

        private void ProjectileAvailbleCheck(ProjectileAvailableCheckMessage msg)
        {
            var projectileAvailable = true;
            if (_projectileMax.TryGetValue(msg.Projectile, out var max))
            {
                if (_currentProjectiles.TryGetValue(msg.Projectile, out var current))
                {
                    if (current >= max)
                    {
                        projectileAvailable = false;
                    }
                }
            }

            if (projectileAvailable)
            {
                msg.DoAfter.Invoke();
            }
        }
        
    }
}