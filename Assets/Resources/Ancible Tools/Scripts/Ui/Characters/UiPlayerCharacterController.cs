using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Animation.ColorOptions;
using Assets.Resources.Ancible_Tools.Scripts.System.Data;
using Assets.Resources.Ancible_Tools.Scripts.System.Zones;
using ConcurrentMessageBus;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Resources.Ancible_Tools.Scripts.Ui.Characters
{
    public class UiPlayerCharacterController : MonoBehaviour
    {
        [SerializeField] private Text _nameText;
        [SerializeField] private UiAnimationController _spriteController;
        [SerializeField] private Text _levelText;
        [SerializeField] private Text _zoneText;
        [SerializeField] private Text _playtimeText;
        [SerializeField] private Transform _cursorPosition;

        public WorldCharacterData Data;
        public Vector2Int Position;

        public void Setup(WorldCharacterData data, Vector2Int position)
        {
            Data = data;
            Position = position;
            _nameText.text = data.Name;
            _spriteController.WakeUp();
            _spriteController.TogglePause();
            var zone = WorldZoneManager.GetZoneByName(data.Zone);
            if (zone)
            {
                _zoneText.text = zone.DisplayName;
            }

            _levelText.text = $"Level {data.Level + 1}";
            _playtimeText.text = data.PlayTime.ToPlayTime();
            ApplyColors(data);
        }

        public void SetCursor(GameObject cursor)
        {
            cursor.transform.SetParent(_cursorPosition);
            cursor.transform.SetLocalPosition(Vector2.zero);
            cursor.gameObject.SetActive(true);
            _spriteController.TogglePause();
        }

        public void RemoveCursor()
        {
            _spriteController.TogglePause();
        }

        public void Click()
        {
            var setSelectedControllerMsg = MessageFactory.GenerateSetSelectedPlayerCharacterControllerMsg();
            setSelectedControllerMsg.Controller = this;
            gameObject.SendMessage(setSelectedControllerMsg);
            MessageFactory.CacheMessage(setSelectedControllerMsg);
        }

        private void ApplyColors(WorldCharacterData data)
        {
            var hair = ColorOptionFactory.GetOptionByName(data.Colors.Hair);
            if (hair)
            {
                hair.Apply(_spriteController.Material, false);
            }

            var eyes = ColorOptionFactory.GetOptionByName(data.Colors.Eyes);
            if (eyes)
            {
                eyes.Apply(_spriteController.Material, false);
            }

            var primaryShirt = ColorOptionFactory.GetOptionByName(data.Colors.PrimaryShirt);
            if (primaryShirt)
            {
                primaryShirt.Apply(_spriteController.Material, false);
            }

            var secondaryShirt = ColorOptionFactory.GetOptionByName(data.Colors.SecondaryShirt);
            if (secondaryShirt)
            {
                secondaryShirt.Apply(_spriteController.Material, true);
            }
        }
    }
}