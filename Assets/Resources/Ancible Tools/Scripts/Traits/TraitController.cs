using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.Traits
{
    public class TraitController : MonoBehaviour
    {
        public Trait Trait { get; private set; }
        public object Sender { get; private set; }

        private string _worldId = string.Empty;

        public void Setup(Trait trait, object sender = null)
        {
            Sender = sender;
            trait.SetWorldId(_worldId);
            if (trait.Instant)
            {
                trait.SetupController(this);
            }
            else
            {
                Trait = Instantiate(trait, transform);
                Trait.name = trait.name;
                name = $"{Trait.name} Controller";
                Trait.SetupController(this);
            }
        }

        public void SetWorldId(string worldId)
        {
            _worldId = worldId;
        }

        public void Destroy()
        {
            if (Trait)
            {
                Trait.Destroy();
                Destroy(Trait);
            }
        }

        public void ResetController()
        {
            if (Trait)
            {
                Trait.ResetTrait();
            }
        }

        public void Clear()
        {
            if (Trait)
            {
                Trait.Clear();
            }
            _worldId = string.Empty;
        }
    }
}
