using IsmaelNascimento.ScriptableObjects;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using IsmaelNascimento.Prefab;
using System.Collections.Generic;

namespace IsmaelNascimento.Manager
{
    public class GameManager : MonoBehaviour
    {
        #region VARIABLES

        private List<int> questionsSelected = new List<int>();
        private const string POINTS_PLAYERPREFS_NAME = "POINTS_PLAYERPREFS_NAME";
        private int counterQuestion;

        [Header("Parameters")]
        [SerializeField] private int limitMaxQuestions = 5;
        [SerializeField] private int timeMaxQuestionInSeconds = 20;
        [SerializeField] private float timeDecreaseTimerSliderInSeconds = 1f;
        [SerializeField] private float countDecreaseTimerSlider= 1f;

        [Header("Questions")]
        [SerializeField] private QuestionScriptableObject[] questionScriptableObjects;
        [Header("UserInterfaceGeneral")]
        [SerializeField] private List<GameObject> screens;
        [SerializeField] private AnswerPrefab answerPrefab;
        [SerializeField] private Button startGameButton;
        [SerializeField] private Button nextQuestionButton;
        [Header("QuestionScreen")]
        [SerializeField] private TMP_Text questionIdText;
        [SerializeField] private TMP_Text questionDescription;
        [SerializeField] private Transform rootAnswers;
        [SerializeField] private Slider timerSlider;
        [Header("AfterScreen")]
        [SerializeField] private TMP_Text afterQuestionIdText;
        [SerializeField] private TMP_Text afterQuestionDescription;
        [SerializeField] private TMP_Text afterQuestionDescriptionRigth;
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
                int questionRandom = Random.Range(0, questionScriptableObjects.Length);

                if (!questionsSelected.Contains(questionRandom))
                {
                    questionsSelected.Add(questionRandom);
                }
            }

            if(questionsSelected.Count < limitMaxQuestions)
            {
                GetQuestions();
            }
        }

        private void OnAnswerClick_Handler(bool isRigth)
        {
            CancelInvoke(nameof(DecreaseTimerSlider));

            if (isRigth)
            {
                int pointsCurrent = PlayerPrefs.GetInt(POINTS_PLAYERPREFS_NAME);
                int newPoints = pointsCurrent+1;
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
            if(value < 1f)
            {
                CancelInvoke(nameof(DecreaseTimerSlider));
                NextQuestionButton_Handler(true);
            }
        }

        private void DecreaseTimerSlider()
        {
            timerSlider.value -= countDecreaseTimerSlider;
        }

        private void SetupAfterScreen()
        {
            afterQuestionIdText.text = $"{counterQuestion+1}";
            afterQuestionDescription.text = questionScriptableObjects[counterQuestion].question;
            afterQuestionDescriptionRigth.text = questionScriptableObjects[counterQuestion].questionRightDescription;
            EnablePanel("AfterQuestion_Panel");
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
                EnablePanel("Question_Panel");
                InvokeRepeating(nameof(DecreaseTimerSlider), timeDecreaseTimerSliderInSeconds, timeDecreaseTimerSliderInSeconds);
            }
        }

        private void StartGameButton_Handler()
        {
            counterQuestion = 0;
            GetQuestions();
            timerSlider.value = timeMaxQuestionInSeconds;
            PlayerPrefs.SetInt(POINTS_PLAYERPREFS_NAME, 0);
            EnablePanel("Question_Panel");
            SetQuestion();
            InvokeRepeating(nameof(DecreaseTimerSlider), timeDecreaseTimerSliderInSeconds, timeDecreaseTimerSliderInSeconds);
        }

        private void HomeButton_Handler()
        {
            EnablePanel("Menu_Panel");
        }

        private bool VerifyIsLastQuestion()
        {
            if((counterQuestion+1) == limitMaxQuestions)
            {
                countAnswersRigth.text = PlayerPrefs.GetInt(POINTS_PLAYERPREFS_NAME).ToString();
                CancelInvoke(nameof(DecreaseTimerSlider));
                EnablePanel("Result_Panel");
                return true;
            }
            else
            {
                return false;
            }
        }

        private void EnablePanel(string screenName)
        {
            screens.ForEach(screen => screen.SetActive(false));

            GameObject screenForEnable = screens.Find(screen => screen.name == screenName);
            screenForEnable.SetActive(true);
        }

        private void SetQuestion()
        {
            int questionCurrentIndex = questionsSelected[counterQuestion];
            QuestionScriptableObject questionCurrent = questionScriptableObjects[questionCurrentIndex];

            for (int index = 0; index < rootAnswers.transform.childCount; index++)
            {
                Transform children = rootAnswers.transform.GetChild(index);
                Destroy(children.gameObject);
            }

            questionIdText.text = $"{counterQuestion+1}";
            questionDescription.text = questionCurrent.question;

            for (int index = 0; index < questionCurrent.answers.Length; index++)
            {
                AnswerPrefab answerPrefabCreated = Instantiate(answerPrefab, rootAnswers);
                answerPrefabCreated.SetText(questionCurrent.answers[index]);

                if(index == questionCurrent.answerRightIndex)
                {
                    answerPrefabCreated.SetRigth(true);
                }
            }
        }

        #endregion
    }
}