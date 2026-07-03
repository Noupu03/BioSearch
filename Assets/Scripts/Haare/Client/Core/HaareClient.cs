using System.Threading.Tasks;
using Cysharp.Threading.Tasks;

using UnityEngine;

using Haare.Client.Core;
using Haare.Util.Logger;

/// <summary>
/// Haare 프레임워크의 진입점. 최초 씬 로드 전에 Processor를 생성하고 부트스트랩한다.
/// BioSearch는 StartScene/게임 씬 둘 다 이미 자체 EventSystem/AudioListener를 배치해 두었으므로
/// (원본 데모와 달리) 여기서 따로 만들지 않는다 — "없으면 생성" 가드는 BeforeSceneLoad 시점의
/// 비동기 딜레이가 씬 오브젝트 인스턴스화보다 먼저 재개될 수 있어 타이밍을 신뢰할 수 없고,
/// DontDestroyOnLoad로 만든 게 다음 씬의 것과 겹쳐 "There are 2 audio listeners" 경고를 냈다.
/// </summary>
public class HaareClient
{
    [RuntimeInitializeOnLoadMethod( RuntimeInitializeLoadType.BeforeSceneLoad )]
    static async void Main() {

        LogHelper.Log(LogHelper.FRAMEWORK,"Start Haare Framework");
        await Task.Delay( 1 );
        await Processor.WaitForCreation();

        await Processor.Instance.Constructor(InitializePlugin, RegisterProcesses);

    }

    static async UniTask InitializePlugin() {
        await UniTask.Delay( 0 );
    }


    static async UniTask RegisterProcesses()
    {
        LogHelper.LogTask(LogHelper.FRAMEWORK,"RegisterProcesses");
        await UniTask.CompletedTask;
    }

}
