using Assets.Resources.Ancible_Tools.Scripts.System;
using Assets.Resources.Ancible_Tools.Scripts.System.Aspects;
using MessageBusLib;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Resources.Ancible_Tools.Scripts.Ui
{
    public class UiExperienceBarController : MonoBehaviour
    {
        [SerializeField] private UiFillBarController _fillBar;
        [SerializeField] private Text _amountText;
        [SerializeField] private Text _levelText;
        [SerializeField] private Image _aspectPointsAvailableImage;
        [SerializeField] private Text _aspectPointsText;

        void Awake()
        {
            RefreshExperience();
            RefreshAspects();
            SubscribeToMessages();
        }

        private void RefreshExperience()
        {
            var queryExperienceMsg = MessageFactory.GenerateQueryExperienceMsg();
            queryExperienceMsg.DoAfter = UpdateExperience;
            gameObject.SendMessageTo(queryExperienceMsg, ObjectManager.Player);
            MessageFactory.CacheMessage(queryExperienceMsg);
        }

        private void RefreshAspects()
        {
            var queryAspectsMsg = MessageFactory.GenerateQueryAspectsMsg();
            queryAspectsMsg.DoAfter = UpdateAspects;
            gameObject.SendMessageTo(queryAspectsMsg, ObjectManager.Player);
            MessageFactory.CacheMessage(queryAspectsMsg);
        }

        private void UpdateAspects(WorldAspectInstance[] aspects, int available)
        {
            if (available > 0)
            {
                _aspectPointsAvailableImage.gameObject.SetActive(true);
                _aspectPointsText.text = $"{available}";
            }
            else if (_aspectPointsAvailableImage.gameObject.activeSelf)
            {
                _aspectPointsAvailableImage.gameObject.SetActive(false);
                _aspectPointsText.text = $"{available}";
            }
        }

        private void UpdateExperience(int level, int experience, int nextLevel)
        {
            _levelText.text = $"Level {level + 1}";
            _amountText.text = $"{experience:n0}/{nextLevel:n0} XP";
            _fillBar.Setup((float)experience / nextLevel);
        }

        private void SubscribeToMessages()
        {
            gameObject.Subscribe<PlayerExperienceUpdatedMessage>(PlayerExperienceUpdated);
            gameObject.Subscribe<PlayerAspectsUpdatedMessage>(PlayerAspectsUpdated);
        }

        private void PlayerExperienceUpdated(PlayerExperienceUpdatedMessage msg)
        {
            RefreshExperience();
        }

        private void PlayerAspectsUpdated(PlayerAspectsUpdatedMessage msg)
        {
            RefreshAspects();
        }

        void OnDestroy()
        {
            gameObject.UnsubscribeFromAllMessages();
        }
    }
}