using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class ScoreUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textScore;

    private void Start()
    {
        //ExcavatorController.Instance.onScoreChanged += ExcavatorController_onScoreChanged;
        ExcavatorScoreController.Instance.onScoreChanged += ExcavatorScoreController_onScoreChanged;
        textScore.text = "Score: " + ExcavatorScoreController.Instance.getScore().ToString();
    }

    private void ExcavatorScoreController_onScoreChanged(object sender, System.EventArgs e)
    {
        textScore.text = "Score: " + ExcavatorScoreController.Instance.getScore().ToString();
    }

    private void ExcavatorController_onScoreChanged(object sender, System.EventArgs e)
    {
        textScore.text = "Score: " + ExcavatorController.Instance.getScore().ToString();
    }
}
