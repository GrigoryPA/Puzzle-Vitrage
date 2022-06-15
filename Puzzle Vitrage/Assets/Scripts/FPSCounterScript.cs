using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FPSCounterScript : MonoBehaviour
{
    public Text FPSTextUI;
    int FPSCounter = 0;
    float mTimeCounter = 0.0f;
    float lastFrameRate = 0.0f;
    public float refreshTime = 0.5f;
    const string format = "{0} FPS";

    // Update is called once per frame
    void Update()
    {
        if (mTimeCounter < refreshTime)
        {
            mTimeCounter += Time.deltaTime;
            ++FPSCounter;
        }
        else 
        {
            lastFrameRate = (float)(FPSCounter / mTimeCounter);
            mTimeCounter = 0.0f; 
            FPSCounter = 0;
            FPSTextUI.text = string.Format(format, (int)lastFrameRate);
        }
    }
}
