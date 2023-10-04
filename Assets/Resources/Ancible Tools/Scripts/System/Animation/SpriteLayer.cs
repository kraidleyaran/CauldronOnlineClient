using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System.Animation
{
    [CreateAssetMenu(fileName = "Sprite Layer", menuName = "Ancible Tools/Sprite Layer")]
    public class SpriteLayer : ScriptableObject
    {
        [SerializeField] private string _layerName;

        public int Id => SortingLayer.NameToID(_layerName);
        public int Value => SortingLayer.GetLayerValueFromName(_layerName);
    }
}