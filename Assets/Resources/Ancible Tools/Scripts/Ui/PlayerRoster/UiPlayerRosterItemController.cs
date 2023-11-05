using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Animation.ColorOptions;
using Assets.Resources.Ancible_Tools.Scripts.System.Data;
using Assets.Resources.Ancible_Tools.Scripts.System.Zones;
using CauldronOnlineCommon.Data;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Resources.Ancible_Tools.Scripts.Ui.PlayerRoster
{
    public class UiPlayerRosterItemController : MonoBehaviour
    {
        [HideInInspector] public RegisteredPlayerData Player;
        [SerializeField] private UiAnimationController _sprite;
        [SerializeField] private Text _playerNameText;
        [SerializeField] private Text _zoneText;
        [SerializeField] private RectTransform _cursorPosition;

        public void Setup(RegisteredPlayerData data)
        {
            Player = data;
            _playerNameText.text = Player.DisplayName;
            var zone = WorldZoneManager.GetZoneByName(Player.Zone);
            if (zone)
            {
                _zoneText.text = zone.DisplayName;
            }
            ApplyColors(Player.SpriteColors);
            _sprite.WakeUp();
        }

        public void SetCursor(GameObject cursor)
        {
            cursor.SetActive(true);
            cursor.transform.SetParent(_cursorPosition);
            cursor.transform.SetLocalPosition(Vector2.zero);
        }

        public void UpdateData()
        {
            _playerNameText.text = Player.DisplayName;
            var zone = WorldZoneManager.GetZoneByName(Player.Zone);
            if (zone)
            {
                _zoneText.text = zone.DisplayName;
            }
            ApplyColors(Player.SpriteColors);
        }

        private void ApplyColors(SpriteColorData data)
        {
            var hair = ColorOptionFactory.GetOptionByName(data.Hair);
            if (hair)
            {
                hair.Apply(_sprite.Material, false);
            }

            var eyes = ColorOptionFactory.GetOptionByName(data.Eyes);
            if (eyes)
            {
                eyes.Apply(_sprite.Material, false);
            }

            var primaryShirt = ColorOptionFactory.GetOptionByName(data.PrimaryShirt);
            if (primaryShirt)
            {
                primaryShirt.Apply(_sprite.Material, false);
            }

            var secondaryShirt = ColorOptionFactory.GetOptionByName(data.SecondaryShirt);
            if (secondaryShirt)
            {
                secondaryShirt.Apply(_sprite.Material, true);
            }
        }

    }
}