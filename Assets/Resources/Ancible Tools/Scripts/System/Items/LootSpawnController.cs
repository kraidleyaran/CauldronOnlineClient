using System.Linq;
using CauldronOnlineCommon.Data.Items;
using CauldronOnlineCommon.Data.Math;
using ConcurrentMessageBus;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Items
{
    public class LootSpawnController : MonoBehaviour
    {
        private const string FILTER = "LootSpawn";

        private GameObject _owner = null;

        private LootTableData _lootTable = null;
        private WorldIntRange _drops = new WorldIntRange(0,0);

        public void Setup(LootTableData lootTable, WorldIntRange drops, GameObject owner)
        {
            _lootTable = lootTable;
            _drops = drops;
            _owner = owner;
            SubscribeToMessages();
        }

        private void SubscribeToMessages()
        {
            _owner.SubscribeWithFilter<DestroyingObjectMessage>(SpawnLoot, FILTER);
        }

        private void SpawnLoot(DestroyingObjectMessage msg)
        {
            var spawnCount = _drops.Roll(true);
            if (spawnCount > 0)
            {
                for (var i = 0; i < spawnCount; i++)
                {
                    var itemRoll = Random.Range(0f, 1f);
                    var availbleItems = _lootTable.LootRolls.Where(l => l.ChanceToDrop >= itemRoll).ToArray();
                    if (availbleItems.Length > 0)
                    {
                        var loot = availbleItems.Length > 1 ? availbleItems[Random.Range(0, availbleItems.Length)] : availbleItems[0];
                        var stack = loot.Stack.Roll(true);
                        var item = ItemFactory.GetItemByName(loot.Item);
                        if (stack > 0 && item)
                        {
                            if (loot.SpawnEachStack || stack <= 1)
                            {
                                for (var s = 0; s < stack; s++)
                                {
                                    var controller = Instantiate(ItemFactory.ItemLoot, transform.position.ToVector2(), Quaternion.identity);
                                    controller.Setup(item, 1, StaticMethods.RandomDirection(), false);
                                    ObjectManager.RegisterObject(controller.gameObject);
                                }
                            }
                            else
                            {
                                var controller = Instantiate(ItemFactory.ItemLoot, transform.position.ToVector2(), Quaternion.identity);
                                controller.Setup(item, stack, StaticMethods.RandomDirection(), true);
                                ObjectManager.RegisterObject(controller.gameObject);
                            }

                        }
                    }
                }
            }

            ObjectManager.DestroyNetworkObject(gameObject);
        }

        void OnDestroy()
        {
            gameObject.UnsubscribeFromAllMessages();
        }
    }
}