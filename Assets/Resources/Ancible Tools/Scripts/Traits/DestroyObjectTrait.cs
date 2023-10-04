using Assets.Resources.Ancible_Tools.Scripts.System;
using CauldronOnlineCommon;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.Traits
{
    [CreateAssetMenu(fileName = "Destroy Object Trait", menuName = "Ancible Tools/Traits/General/Destroy Object")]
    public class DestroyObjectTrait : Trait
    {
        public override bool Instant => true;

        [SerializeField] private bool _applyEvent = true;

        public override void SetupController(TraitController controller)
        {
            base.SetupController(controller);
            if (_applyEvent)
            {
                var id = ObjectManager.GetId(_controller.transform.parent.gameObject);
                if (!string.IsNullOrEmpty(id))
                {
                    ClientController.SendToServer(new ClientDestroyObjectMessage { TargetId = id, Tick = TickController.ServerTick });
                }
            }
            ObjectManager.DestroyNetworkObject(_controller.transform.parent.gameObject);
        }
    }
}