using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Haare.Client.Core.Singleton;
using Haare.Util.Logger;
using R3;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;

namespace Haare.Client.Routine.Service.SceneService
{
    /// <summary>
    /// BioSearch의 실제 씬. Addressables 셋업 툴(Tools → Haare Addressables 셋업)이
    /// 이 이름 그대로 주소를 등록한다 — 여기서 이름을 바꾸면 그쪽도 같이 바꿔야 한다.
    /// </summary>
    public enum SceneName
    {
        StartScene,
        SceneSSH,
    }

    public enum SceneLoadPhase
    {
        UnloadCurrent,
        StartLoad,
        Loading,
        EndLoad
    }

    /// <summary>
    /// Scene 전환을 전담하는 Routine. VContainer DI 없이도 쓸 수 있도록 Singleton&lt;T&gt;로 구현했다
    /// (GameLoopManager처럼 DI 컨테이너를 거치지 않는 씬 MonoBehaviour에서 바로 호출하기 위함).
    /// 원본과 달리 로딩 화면(LoadSceneWithLoad)은 아직 이식하지 않았다 — CoreUIManager/Panel
    /// 시스템이 들어오면 그때 같이 추가한다.
    /// </summary>
    public class SceneService : Singleton<SceneService>
    {
        private static readonly Dictionary<SceneName, string> AddressableAddress = new()
        {
            { SceneName.StartScene, "StartScene" },
            { SceneName.SceneSSH,   "SceneSSH" },
        };

        Scene sceneToUnload;

        public override bool isInSceneOnly => false;

        private readonly ReactiveProperty<SceneLoadPhase> currentPhaseReactive = new ReactiveProperty<SceneLoadPhase>(SceneLoadPhase.UnloadCurrent);
        public ReactiveProperty<SceneLoadPhase> CurrentPhase => currentPhaseReactive;

        private readonly ReactiveProperty<float> _loadProgress = new ReactiveProperty<float>(0f);
        public ReadOnlyReactiveProperty<float> LoadProgress => _loadProgress;

        public SceneLoadRequest LoadSceneRequest;

        public override async UniTask Initialize(CancellationToken cts)
        {
            await base.Initialize(cts);
            LogHelper.LogTask(LogHelper.SERVICE, "SceneService Initialize");
            await UniTask.CompletedTask;
        }

        public async UniTask LoadScene(SceneName scene, LoadSceneMode mode = LoadSceneMode.Single)
        {
            LoadSceneRequest = new SceneLoadRequest(scene, mode);
            await LoadSceneInternal(LoadSceneRequest);
        }

        private async UniTask LoadSceneInternal(SceneLoadRequest request)
        {
            if (request == null)
            {
                LogHelper.Error(LogHelper.SERVICE, "[LoadSceneInternal] 목표 씬 정의되어 있지 않습니다.");
                return;
            }

            if (!AddressableAddress.TryGetValue(request.Scene, out var address))
            {
                LogHelper.Error(LogHelper.SERVICE, $"[LoadSceneInternal] 등록된 주소가 없습니다: {request.Scene}");
                return;
            }

            LogHelper.LogTask(LogHelper.SERVICE, ">>> Phase: StartLoad");
            LogHelper.LogTask(LogHelper.SERVICE, $"Load To : {request.Scene}");
            currentPhaseReactive.Value = SceneLoadPhase.StartLoad;

            if (request.Mode == LoadSceneMode.Additive)
            {
                sceneToUnload = SceneManager.GetActiveScene();
                LogHelper.LogTask(LogHelper.SERVICE, $"sceneToUnload : {sceneToUnload.name}");
            }

            LogHelper.LogTask(LogHelper.SERVICE, ">>> Phase: Loading");
            currentPhaseReactive.Value = SceneLoadPhase.Loading;

            var loadOperation = Addressables.LoadSceneAsync(address, request.Mode, activateOnLoad: false);

            await LoadSceneProgressTask(loadOperation);

            if (loadOperation.Status != AsyncOperationStatus.Succeeded)
            {
                LogHelper.Error(LogHelper.SERVICE,
                    $"[LoadSceneInternal] 씬 로드 실패! 주소: '{address}'\n" +
                    $"Status: {loadOperation.Status}, 원인: {loadOperation.OperationException}\n" +
                    "Addressable 등록(Tools → Haare Addressables 셋업)을 확인하세요.");
                currentPhaseReactive.Value = SceneLoadPhase.UnloadCurrent;
                return;
            }

            var sceneInstance = loadOperation.Result;

            LogHelper.LogTask(LogHelper.SERVICE, ">>> Phase: EndLoad");
            currentPhaseReactive.Value = SceneLoadPhase.EndLoad;

            await sceneInstance.ActivateAsync();
            OnSceneLoadedHandler(sceneInstance.Scene, request.Argument);

            if (request.Mode == LoadSceneMode.Additive &&
                sceneToUnload.IsValid() && sceneToUnload.isLoaded)
            {
                await SceneManager.UnloadSceneAsync(sceneToUnload);
            }
        }

        private async UniTask LoadSceneProgressTask(AsyncOperationHandle loadOperation)
        {
            while (loadOperation.PercentComplete < 0.9f)
            {
                _loadProgress.Value = loadOperation.PercentComplete / 0.9f;
                await UniTask.Yield();
            }
            _loadProgress.Value = 1f;
        }

        private void OnSceneLoadedHandler(Scene loadedscene, object argument)
        {
            foreach (var root in loadedscene.GetRootGameObjects())
            {
                ExecuteEvents.Execute<ISceneWasLoaded>(root, null,
                    (receiver, e) => receiver.OnSceneWasLoaded(argument));
            }
        }
    }
}
