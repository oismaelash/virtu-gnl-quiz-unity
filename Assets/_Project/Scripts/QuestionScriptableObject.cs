using UnityEngine;

namespace IsmaelNascimento.ScriptableObjects
{
    [CreateAssetMenu(fileName = "QuestionScriptableObject", menuName = "ScriptableObjects/Question")]
    public class QuestionScriptableObject : ScriptableObject
    {
        #region VARIABLES

        [TextArea]
        public string question;
        [TextArea]
        public string[] answers;
        public int answerRightIndex;
        [TextArea(1, 100)]
        public string questionRightDescription;

        #endregion
    }
}