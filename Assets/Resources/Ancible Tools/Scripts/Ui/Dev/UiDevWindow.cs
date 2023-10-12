using Assets.Resources.Ancible_Tools.Scripts.System;
using ConcurrentMessageBus;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Resources.Ancible_Tools.Scripts.Ui.Dev
{
    public class UiDevWindow : UiWindowBase
    {
        public override bool Static => true;
        public override bool Movable => false;
        

        public void LevelUp()
        {
            gameObject.SendMessageTo(LevelUpMessage.INSTANCE, ObjectManager.Player);
        }

    }
}