using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class ExcavatorScoreController : MonoBehaviour
{
    public static ExcavatorScoreController Instance { get; private set; }
    private void Awake()
    {
        Instance = this;
    }

    [SerializeField] private float score = 1000;
    [SerializeField] private float scoreReduceAmount = 10;
    public event EventHandler onScoreChanged;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == 6)
        {
            reduceScore();
        }
    }
    private void reduceScore()
    {
        score -= scoreReduceAmount;
        onScoreChanged?.Invoke(this, EventArgs.Empty);
    }

    public float getScore()
    {
        return score;
    }
}
