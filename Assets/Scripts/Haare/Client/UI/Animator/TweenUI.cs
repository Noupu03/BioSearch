using Cysharp.Threading.Tasks;
using UnityEngine;
using DG.Tweening;

namespace Haare.Scripts.Client.UI.Animator
{
    public class UIAnimator
    {
        // Essential
        private Transform targetTransform; // 애니메이션을 적용할 대상의 Transform
        private Tween currentHoverTween;

        //touch?
        private Vector3 originalScale;     // 대상의 원래 크기


        // Slider
        public RectTransform panelRectTransform;


        /// <summary>
        /// Make UI Tween
        /// </summary>
        /// <param name="target"> object to use animation </param>
        public UIAnimator(
            GameObject target)
        {
            this.targetTransform = target.transform;
            this.originalScale = target.transform.localScale;
        }

#region Touch/Click

        /// <summary>
        /// HoverAnimation - Enter
        /// </summary>
        public void TriggerHoverEnter(float scaleMultiplier, float duration)
        {
            if (currentHoverTween != null && currentHoverTween.IsActive())
            {
                currentHoverTween.Kill();
            }
            currentHoverTween = targetTransform.DOScale(originalScale * scaleMultiplier, duration).SetEase(Ease.OutQuad);
        }

        /// <summary>
        /// HoverAnimation - Exit
        /// </summary>
        public void TriggerHoverExit(float duration)
        {
            if (currentHoverTween != null && currentHoverTween.IsActive())
            {
                currentHoverTween.Kill();
            }
            currentHoverTween = targetTransform.DOScale(originalScale, duration).SetEase(Ease.OutQuad);
        }
        public async UniTask TriggerClickAsync(float punchScale, float duration)
        {
            // DOTween이 UPM(com.demigiant.dotween)이 아닌 raw dll로 설치되어 있어
            // UniTask의 DOTween 연동 어셈블리(ToUniTask)가 활성화되지 않는다.
            // 대신 Tween이 끝날 때까지(Kill 포함) 폴링으로 대기한다.
            var tween = targetTransform.DOPunchScale(
                new Vector3(punchScale, punchScale, punchScale),
                duration,
                1,
                1
            );
            await UniTask.WaitUntil(() => !tween.IsActive(), cancellationToken: targetTransform.GetCancellationTokenOnDestroy());
        }
        /// <summary>
        /// Click Animation
        /// </summary>
        public void TriggerClick(float punchScale, float duration)
        {

            targetTransform.DOPunchScale(new Vector3(punchScale, punchScale, punchScale), duration, 1, 1);
        }
#endregion

#region Slide

        public void clearSlidePostion(RectTransform offScreenPosition)
        {
            panelRectTransform.anchoredPosition = offScreenPosition.anchoredPosition;
        }

        public void SlideOpenPanel(float slideDuration,RectTransform onScreenPosition,Ease easeType)
        {
            panelRectTransform.DOAnchorPos(onScreenPosition.anchoredPosition, slideDuration).SetEase(easeType);
        }

        public void SlideClosePanel(float slideDuration,RectTransform offScreenPosition,Ease easeType)
        {
            panelRectTransform.DOAnchorPos(offScreenPosition.anchoredPosition, slideDuration).SetEase(easeType);
        }

    #endregion

    #region Popup


    public void OpenPopup(float duration,Ease easeType)
    {
        panelRectTransform.localScale = Vector3.one * 0.8f;
        Sequence sequence = DOTween.Sequence();
        sequence.Append(panelRectTransform.DOScale(Vector3.one, duration)
            .SetEase(easeType));
    }

    public void ClosePopup(float duration,Ease easeType)
    {
        Sequence sequence = DOTween.Sequence();

        sequence.Append(panelRectTransform.DOScale(Vector3.one * 0.8f, duration)
            .SetEase(easeType));
    }

    #endregion
        public void KillAllTweens()
        {
            targetTransform.DOKill();
        }
    }
}
