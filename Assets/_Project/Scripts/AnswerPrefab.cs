using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;

namespace IsmaelNascimento.Prefab
{
    public class AnswerPrefab : MonoBehaviour
    {
        #region VARIABLES

        private bool isRigth;
        public static Action<bool> OnAnswerClick;

        [Header("UserInterface")]
        [SerializeField] private TMP_Text answerText;

        #endregion

        #region MONOBEHAVIOUR_METHODS

        private void Start()
        {
            GetComponent<Button>().onClick.AddListener(OnAnswerClick_Handler);
        }

        #endregion

        #region PUBLIC_METHODS

        public void SetRigth(bool value)
        {
            isRigth = value;
        }

        public void SetText(string text)
        {
            answerText.text = text;
        }

        private void OnAnswerClick_Handler()
        {
            OnAnswerClick?.Invoke(isRigth);
        }

        #endregion
    }
}