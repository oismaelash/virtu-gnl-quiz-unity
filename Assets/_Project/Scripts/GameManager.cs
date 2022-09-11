using IsmaelNascimento.ScriptableObjects;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using IsmaelNascimento.Prefab;
using System.Collections.Generic;
using System.Collections;

namespace IsmaelNascimento.Manager
{
    public class GameManager : MonoBehaviour
    {
        #region VARIABLES

        private readonly List<int> questionsSelected = new();
        private const string POINTS_PLAYERPREFS_NAME = "POINTS_PLAYERPREFS_NAME";
        private int counterQuestion;

        [Space()]
        [Header("Parameters")]
        [SerializeField] private int limitMaxQuestions = 5;
        [SerializeField] private int timeMaxQuestionInSeconds = 20;
        [SerializeField] private float timeDecreaseTimerSliderInSeconds = 1f;
        [SerializeField] private float countDecreaseTimerSlider= 1f;

        [Space()]
        [Header("Questions")]
        [SerializeField] private QuestionScriptableObject[] questionScriptableObjects;
        
        [Space()]
        [Header("UserInterfaceGeneral")]
        [SerializeField] private List<GameObject> screens;
        [SerializeField] private AnswerPrefab answerPrefab;
        [SerializeField] private Button startGameButton;
        [SerializeField] private Button nextQuestionButton;
        
        [Space()]
        [Header("QuestionScreen")]
        [SerializeField] private TMP_Text questionIdText;
        [SerializeField] private TMP_Text questionDescription;
        [SerializeField] private Transform rootAnswers;
        [SerializeField] private Slider timerSlider;
        
        [Space()]
        [Header("AfterScreen")]
        [SerializeField] private TMP_Text afterQuestionIdText;
        [SerializeField] private TMP_Text afterQuestionDescription;
        [SerializeField] private TMP_Text afterQuestionDescriptionRigth;
        
        [Space()]
        [Header("ResultScreen")]
        [SerializeField] private TMP_Text countAnswersRigth;
        [SerializeField] private Button homeButton;

        #endregion

        #region MONOBEHAVIOUR_METHODS

        private void OnEnable()
        {
            AnswerPrefab.OnAnswerClick += OnAnswerClick_Handler;
            timerSlider.onValueChanged.AddListener(TimerSliderOnValueChanged_Handler);
            nextQuestionButton.onClick.AddListener(() => NextQuestionButton_Handler(false));
            startGameButton.onClick.AddListener(StartGameButton_Handler);
            homeButton.onClick.AddListener(HomeButton_Handler);
        }

        private void OnDisable()
        {
            AnswerPrefab.OnAnswerClick -= OnAnswerClick_Handler;
        }

        #endregion

        #region PRIVATE_METHODS

        private void GetQuestions()
        {
            questionsSelected.Clear();

            for (int index = 0; index <= limitMaxQuestions; index++)
            {
                int newQuestion = Random.Range(0, questionScriptableObjects.Length);

                if (!questionsSelected.Contains(newQuestion))
                {
                    questionsSelected.Add(newQuestion);
                }
            }

            if(questionsSelected.Count < limitMaxQuestions)
            {
                GetQuestions();
            }
        }

        private void DecreaseTimerSlider()
        {
            timerSlider.value -= countDecreaseTimerSlider;
        }

        private void SetupAfterScreen()
        {
            afterQuestionIdText.text = $"{GetCounterQuestionCurrentForShow()}";
            afterQuestionDescription.text = questionScriptableObjects[counterQuestion].question;
            afterQuestionDescriptionRigth.text = questionScriptableObjects[counterQuestion].questionRightDescription;
            EnableScreen("AfterQuestion_Panel");
        }

        private bool VerifyIsLastQuestion()
        {
            if(GetCounterQuestionCurrentForShow() == limitMaxQuestions)
            {
                countAnswersRigth.text = PlayerPrefs.GetInt(POINTS_PLAYERPREFS_NAME).ToString();
                CancelInvoke(nameof(DecreaseTimerSlider));
                EnableScreen("Result_Panel");
                StartCoroutine(nameof(EnableHomeScreenAutomatic_Coroutine));
                return true;
            }

            return false;
        }

        private void EnableScreen(string screenName)
        {
            screens.ForEach(screen => screen.SetActive(false));

            GameObject screenForEnable = screens.Find(screen => screen.name == screenName);
            screenForEnable.SetActive(true);
        }

        private void SetQuestion()
        {
            int questionCurrentIndex = questionsSelected[counterQuestion];
            QuestionScriptableObject questionCurrent = questionScriptableObjects[questionCurrentIndex];

            // Clear answers previous
            for (int index = 0; index < rootAnswers.transform.childCount; index++)
            {
                Transform children = rootAnswers.transform.GetChild(index);
                Destroy(children.gameObject);
            }

            // Set information on UI
            questionIdText.text = $"{GetCounterQuestionCurrentForShow()}";
            questionDescription.text = questionCurrent.question;


            // Add answers for question current
            for (int index = 0; index < questionCurrent.answers.Length; index++)
            {
                AnswerPrefab answerPrefabCreated = Instantiate(answerPrefab, rootAnswers);
                answerPrefabCreated.SetText(questionCurrent.answers[index]);

                if(index == questionCurrent.answerRightIndex)
                {
                    answerPrefabCreated.SetRigth(true);
                }
            }

            timerSlider.maxValue = timeMaxQuestionInSeconds;
            timerSlider.value = timeMaxQuestionInSeconds;
        }

        private int GetCounterQuestionCurrentForShow()
        {
            return counterQuestion + 1;
        }

        #endregion

        #region HANDLER_METHODS

        private void OnAnswerClick_Handler(bool isRigth)
        {
            CancelInvoke(nameof(DecreaseTimerSlider));

            if (isRigth)
            {
                int pointsCurrent = PlayerPrefs.GetInt(POINTS_PLAYERPREFS_NAME);
                int newPoints = pointsCurrent + 1;
                PlayerPrefs.SetInt(POINTS_PLAYERPREFS_NAME, newPoints);
                NextQuestionButton_Handler(true);
            }
            else
            {
                SetupAfterScreen();
            }

            Debug.Log($"User rigth question = {isRigth} | points = {PlayerPrefs.GetInt(POINTS_PLAYERPREFS_NAME)}");
        }

        private void TimerSliderOnValueChanged_Handler(float value)
        {
            if (value < 1f)
            {
                CancelInvoke(nameof(DecreaseTimerSlider));
                NextQuestionButton_Handler(true);
            }
        }

        private void NextQuestionButton_Handler(bool isAuto)
        {
            if (VerifyIsLastQuestion())
            {
                return;
            }

            timerSlider.value = timeMaxQuestionInSeconds;
            counterQuestion++;
            SetQuestion();

            if (isAuto)
            {
                InvokeRepeating(nameof(DecreaseTimerSlider), timeDecreaseTimerSliderInSeconds, timeDecreaseTimerSliderInSeconds);
            }
            else
            {
                EnableScreen("Question_Panel");
                InvokeRepeating(nameof(DecreaseTimerSlider), timeDecreaseTimerSliderInSeconds, timeDecreaseTimerSliderInSeconds);
            }
        }

        private void StartGameButton_Handler()
        {
            counterQuestion = 0;
            GetQuestions();
            timerSlider.value = timeMaxQuestionInSeconds;
            PlayerPrefs.SetInt(POINTS_PLAYERPREFS_NAME, 0);
            EnableScreen("Question_Panel");
            SetQuestion();
            InvokeRepeating(nameof(DecreaseTimerSlider), timeDecreaseTimerSliderInSeconds, timeDecreaseTimerSliderInSeconds);
        }

        private void HomeButton_Handler()
        {
            EnableScreen("Menu_Panel");
        }

        #endregion

        #region COROUTINE_METHODS

        private IEnumerator EnableHomeScreenAutomatic_Coroutine()
        {
            yield return new WaitForSeconds(5f);
            EnableScreen("Menu_Panel");
        }

        #endregion
    }
}