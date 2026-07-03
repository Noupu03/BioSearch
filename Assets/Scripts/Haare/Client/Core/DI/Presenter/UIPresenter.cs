using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using R3;
using VContainer;

using Haare.Client.UI;
using Haare.Util.Logger;

namespace Haare.Client.Core.DI
{
    /// <summary>
    /// 현재 미사용 — VContainer로 주입되는 Presenter를 만들 때 상속할 기반 클래스.
    /// 원본 HaareFramework의 UIPresenter에서 LoadingFadePanel 기반 페이드 인/아웃
    /// (FadeIn/FadeOut/OpenPanelWithFade/ClosePanelWithFade)과 ICustomPanel.ConvertTo
    /// 기반 제네릭 BindPanelEvents 오버로드는 뺐다 — 둘 다 이 프로젝트에 없는 원본 데모
    /// 전용 타입/메서드에 의존해서 그대로는 컴파일이 안 된다. 필요해지면 그때 페이드
    /// 패널을 직접 만들고 다시 추가하면 된다.
    /// </summary>
    public abstract class UIPresenter : IPresenter
    {
        public CompositeDisposable disposables { get; } = new CompositeDisposable();
        public bool isInitialized { get; private set; } = false;

        [Inject] protected CoreUIManager _coreUIManager;
        [Inject] protected SceneUIManager _sceneUiManager;
        [Inject] protected readonly IObjectResolver _resolver;

        public virtual void Dispose()
        {
            disposables.Dispose();
            LogHelper.Log(LogHelper.FRAMEWORK, $"{this.GetType()} Disposed");
        }

        public virtual void PostInitialize()
        {
        }

        protected virtual async UniTask PostInitializeAsync()
        {
            await Task.CompletedTask;
            isInitialized = true;
        }

        protected virtual void BindPanelEvents()
        {
        }

        protected async UniTask OpenPanel<T>() where T : UnityEngine.Component, ICustomPanel
        {
            await _sceneUiManager.LoadPanel<T>();
        }

        protected void ClosePanel<T>() where T : UnityEngine.Component, ICustomPanel
        {
            _sceneUiManager.ClosePanel<T>();
        }

        protected virtual async UniTask StartSequence()
        {
            await UniTask.WaitUntil(() => isInitialized);
        }
    }
}
