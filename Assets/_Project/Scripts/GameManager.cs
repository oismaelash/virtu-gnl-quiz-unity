using IsmaelNascimento.ScriptableObjects;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using IsmaelNascimento.Prefab;
using System.Collections.Generic;
using System.Collections;
using Proyecto26;
using UnityEngine.SceneManagement;

namespace IsmaelNascimento.Manager
{
    public class GameManager : MonoBehaviour
    {
        #region VARIABLES

        private readonly List<int> questionsSelected = new();
        private const string POINTS_PLAYERPREFS_NAME = "POINTS_PLAYERPREFS_NAME";
        private int counterQuestion;

        private const string URL_CAN_GAME = "https://virtu-gnl-quiz-default-rtdb.firebaseio.com/demo.json";
        private const string URL_PLAYED_INCREMENT = "https://virtu-gnl-quiz-default-rtdb.firebaseio.com/played.json";
        private const string PAYLOAD_PLAYED_INCREMENT = "{ \".sv\": { \"increment\": 1 } }";

        [Space()]
        [Header("Parameters")]
        [SerializeField] private int limitMaxQuestions = 5;
        [SerializeField] private int timeMaxQuestionInSeconds = 60;
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
        
        [Space()]
        [Header("QuestionScreen")]
        [SerializeField] private TMP_Text questionIdText;
        [SerializeField] private TMP_Text questionDescription;
        [SerializeField] private TMP_Text questionCounterText;
        [SerializeField] private Transform rootAnswers;
        [SerializeField] private Slider timerSlider;
        [SerializeField] private Button restartGameQuestionButton;
        [SerializeField] private Button nextQuestionQuestionButton;

        [Space()]
        [Header("FeedbackScreen")]
        [SerializeField] private Color successColor;
        [SerializeField] private Color wrongColor;
        [SerializeField] private string successDescriptionText;
        [SerializeField] private string wrongDescriptionText;
        [SerializeField] private Image backgroundFeedbackImage;
        [SerializeField] private TMP_Text afterQuestionIdText;
        [SerializeField] private TMP_Text afterQuestionDescription;
        [SerializeField] private TMP_Text afterQuestionDescriptionRigth;
        [SerializeField] private Button restartGameFeedbackButton;
        [SerializeField] private Button nextQuestionFeedbackButton;

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
            startGameButton.onClick.AddListener(StartGameButton_Handler);
            homeButton.onClick.AddListener(HomeButton_Handler);
            nextQuestionFeedbackButton.onClick.AddListener(() => NextQuestionButton_Handler(false));
            nextQuestionQuestionButton.onClick.AddListener(() => NextQuestionButton_Handler(false));
            restartGameFeedbackButton.onClick.AddListener(OnRestartClick_Handler);
            restartGameQuestionButton.onClick.AddListener(OnRestartClick_Handler);
        }

        private void Start()
        {
            RestClient.Get(URL_CAN_GAME).Then(response => {
                if (!bool.Parse(response.Text))
                {
                    startGameButton.onClick.RemoveAllListeners();
                }
            });
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

        private void SetupFeedbackScreen(bool isRigth)
        {
            if (isRigth)
            {
                int pointsCurrent = PlayerPrefs.GetInt(POINTS_PLAYERPREFS_NAME);
                int newPoints = pointsCurrent + 1;
                PlayerPrefs.SetInt(POINTS_PLAYERPREFS_NAME, newPoints);
            }

            afterQuestionIdText.text = $"{GetCounterQuestionCurrentForShow()}";
            afterQuestionDescription.text = isRigth ? successDescriptionText : wrongDescriptionText;
            afterQuestionDescriptionRigth.text = questionScriptableObjects[counterQuestion].questionRightDescription;
            restartGameFeedbackButton.GetComponentInChildren<TMP_Text>().color = isRigth ? successColor : wrongColor;
            nextQuestionFeedbackButton.GetComponentInChildren<TMP_Text>().color = isRigth ? successColor : wrongColor;
            restartGameFeedbackButton.gameObject.SetActive(!isRigth);
            backgroundFeedbackImage.color = isRigth ? successColor : wrongColor;

            EnableScreen("FeedbackQuestion_Panel");

            Debug.Log($"User rigth question = {isRigth} | points = {PlayerPrefs.GetInt(POINTS_PLAYERPREFS_NAME)}");
        }

        private bool VerifyIsLastQuestion()
        {
            if(GetCounterQuestionCurrentForShow() == limitMaxQuestions)
            {
                countAnswersRigth.text = PlayerPrefs.GetInt(POINTS_PLAYERPREFS_NAME).ToString();
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

            questionCounterText.text = $"Pergunta {GetCounterQuestionCurrentForShow()} / {limitMaxQuestions}";
        }

        private int GetCounterQuestionCurrentForShow()
        {
            return counterQuestion + 1;
        }

        #endregion

        #region HANDLER_METHODS

        private void OnRestartClick_Handler()
        {
            SceneManager.LoadScene(0);
        }

        private void OnAnswerClick_Handler(bool isRigth)
        {
            CancelInvoke(nameof(DecreaseTimerSlider));
            SetupFeedbackScreen(isRigth);
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
            CancelInvoke(nameof(DecreaseTimerSlider));

            if (VerifyIsLastQuestion())
            {
                return;
            }

            timerSlider.value = timeMaxQuestionInSeconds;
            counterQuestion++;
            SetQuestion();
            InvokeRepeating(nameof(DecreaseTimerSlider), timeDecreaseTimerSliderInSeconds, timeDecreaseTimerSliderInSeconds);

            if (!isAuto)
            {
                EnableScreen("Question_Panel");
            }
        }

        private void StartGameButton_Handler()
        {
            RestClient.Put(URL_PLAYED_INCREMENT, PAYLOAD_PLAYED_INCREMENT).Then(response => {
                counterQuestion = 0;
                GetQuestions();
                timerSlider.value = timeMaxQuestionInSeconds;
                PlayerPrefs.SetInt(POINTS_PLAYERPREFS_NAME, 0);
                EnableScreen("Question_Panel");
                SetQuestion();
                InvokeRepeating(nameof(DecreaseTimerSlider), timeDecreaseTimerSliderInSeconds, timeDecreaseTimerSliderInSeconds);
            }).Catch(err =>
            {
                Debug.LogError(err.Message);
            });
        }

        private void HomeButton_Handler()
        {
            EnableScreen("Wait_Panel");
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