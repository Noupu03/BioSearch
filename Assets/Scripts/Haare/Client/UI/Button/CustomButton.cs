using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Haare.Client.Routine;
using Haare.Scripts.Client.UI.Animator;
using Haare.Util.Logger;
using R3;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Haare.Client.UI
{
    public class CustomButton : MonoRoutine,
        IPointerClickHandler,
        IPointerDownHandler,
        IPointerExitHandler,
        IPointerEnterHandler
    {
        [Header("Text Field")] public CustomText ButtonText;
        [Header("Image Field")] public CustomImage ButtonImage;

        public Subject<Unit> Onclicked { get; } = new Subject<Unit>();
        public Subject<Unit> Onhovered { get; }= new Subject<Unit>();
        public Subject<Unit> Onexited { get; }= new Subject<Unit>();

        [Header("Option Field")]
        [SerializeField]
        public bool INTERACTIABLE = true ;

        [SerializeField]
        public bool OPTION_HOVERIMAGE = false ;
        [SerializeField]
        public bool OPTION_HOVERALPHA = true ;
        [SerializeField]
        public bool OPTION_ANIMATION = false;

        [SerializeField] public bool HOVERANIMATION = false;
        [SerializeField] public float hoverScale = 1.1f;
        [SerializeField] public float hoverDuration = 0.2f;

        [SerializeField] public bool CLICKANIMATION = false;
        [SerializeField] public float clickPunchScale = 0.3f;
        [SerializeField] public float clickDuration = 0.1f;

        [Header("Settings")]
        [SerializeField] private int cooldownMilliseconds = 500; // 0.5초 쿨타임

        private bool _isLocked = false;

        private UIAnimator _animator;

        [Header("Disabled Settings")]
        [SerializeField] private Color disabledColor = new Color(0.4f, 0.4f, 0.4f, 1f);
        public override UniTask Initialize(CancellationToken cts)
        {
            ButtonImage = GetComponentInChildren<CustomImage>();
            ButtonText = GetComponentInChildren<CustomText>();

            if (!INTERACTIABLE)
            {
                SetInteractable(false);
            }
            if (OPTION_ANIMATION)
            {
                _animator = new UIAnimator(
                    this.gameObject
                    );
            }
            return base.Initialize(cts);
        }
        public void SetInteractable(bool isInteractable)
        {
            INTERACTIABLE = isInteractable;

            if (ButtonImage == null) return;

            if (!isInteractable)
            {
                ButtonImage.ChangeColor(disabledColor);
            }
            else
            {
                if (OPTION_HOVERALPHA)
                {
                    ButtonImage.ChangeCommonColor();
                }
                else
                {
                    ButtonImage.ChangeColor(Color.white);
                }

                if (OPTION_HOVERIMAGE)
                {
                    ButtonImage.ChangeCommonImage();
                }
            }
        }
        public override UniTask Finalize()
        {
            if (OPTION_ANIMATION)
                _animator.KillAllTweens();

            return base.Finalize();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (CheckAndStartCooldown(applyLockNow: true))
                return;
            Onclicked.OnNext(Unit.Default);

        }
        public void OnPointerDown(PointerEventData eventData)
        {
            if (CheckAndStartCooldown(applyLockNow: false))
                return;
            if (OPTION_ANIMATION)
            {
                if(CLICKANIMATION)
                    _animator.TriggerClick(clickPunchScale, clickDuration);
            }
            if (OPTION_HOVERALPHA)
            {
                ButtonImage.ChangeClickedColor();
            }
        }
        public void OnPointerExit(PointerEventData eventData)
        {

            if(!INTERACTIABLE)
                return;
            Onexited.OnNext(Unit.Default);
            if (OPTION_HOVERIMAGE)
            {
                ButtonImage.ChangeCommonImage();
            }
            if (OPTION_HOVERALPHA)
            {
                ButtonImage.ChangeCommonColor();
            }
            if (OPTION_ANIMATION)
            {
                if(HOVERANIMATION)
                    _animator.TriggerHoverExit(hoverDuration);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {

            if(!INTERACTIABLE)
                return;
            Onhovered.OnNext(Unit.Default);
            if(OPTION_HOVERIMAGE){
                ButtonImage.ChangeHoverImage();
            }
            if (OPTION_HOVERALPHA)
            {
                ButtonImage.ChangeHoverColor();
            }
            if (OPTION_ANIMATION){
                if(HOVERANIMATION)
                    _animator.TriggerHoverEnter(hoverScale,hoverDuration);
            }
        }


        private bool CheckAndStartCooldown(bool applyLockNow = false)
        {
            if (!INTERACTIABLE || _isLocked)
                return true;

            if (applyLockNow)
            {
                CooldownRoutine().Forget();
            }
            return false;
        }
        private async UniTaskVoid CooldownRoutine()
        {
            _isLocked = true;
            await UniTask.Delay(cooldownMilliseconds, cancellationToken: this.GetCancellationTokenOnDestroy());
            _isLocked = false;
        }
    }
}
