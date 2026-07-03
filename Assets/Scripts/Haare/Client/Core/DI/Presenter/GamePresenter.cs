using Cysharp.Threading.Tasks;
using R3;
using VContainer;
using VContainer.Unity;

using Haare.Client.Routine.Service.SceneService;
using Haare.Scripts.Client.Data;
using Haare.Util.Logger;

namespace Haare.Client.Core.DI
{
    /// <summary>
    /// 현재 미사용 — GameLifetimeScope에 builder.RegisterEntryPoint&lt;GamePresenter&gt;()로
    /// 등록해야 실제로 동작한다 (SceneService/DataManager도 DI 등록 필요, 지금은 안 되어 있음).
    /// 씬 전환 완료(EndLoad) 시 씬 전용 Routine들을 정리해주는 훅이 필요해지면 사용.
    /// </summary>
    public class GamePresenter : IPostInitializable, System.IDisposable
    {
        [Inject]
        private SceneService _sceneService;

        [Inject] private DataManager _dataManager;

        public void Dispose()
        {
        }

        public void PostInitialize()
        {
            _sceneService.CurrentPhase.AsObservable()
                .Skip(1)
                .Subscribe(_ =>
                {
                    LogHelper.Log(LogHelper.FRAMEWORK, $"Phase : {_}");
                    if (_ != SceneLoadPhase.EndLoad)
                        return;
                    Processor.Instance.CheckDeleteProcessesForScene().Forget();
                    LogHelper.Log(LogHelper.FRAMEWORK, "Ended Clean Scene");
                });
            LogHelper.Log(LogHelper.FRAMEWORK, "GamePresenter PostInitialize");
        }
    }
}
