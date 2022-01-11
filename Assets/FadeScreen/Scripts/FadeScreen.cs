using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeScreen : MonoBehaviour
{
    public static FadeScreen instance;
    [SerializeField] private float fadeStep;

    private float target;
    private CanvasGroup canvasGroup;
    private void Awake() {
        instance = this;
        canvasGroup = GetComponent<CanvasGroup>();
    }

    private void Update() {
        if(canvasGroup.alpha < target) {
            canvasGroup.alpha += fadeStep;
        }

        if (canvasGroup.alpha > target) {
            canvasGroup.alpha -= fadeStep;
        }

        if(canvasGroup.alpha <= 0) {
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }

        if(canvasGroup.alpha >= fadeStep) {
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
        }
    }

    public void FadeOut() {
        target = 1;
    }

    public void FadeIn() {
        target = 0;
    }


}
