using System.Threading.Tasks;
using Cysharp.Threading.Tasks;

using UnityEngine;
using UnityEngine.EventSystems;

using Haare.Client.Core;
using Haare.Util.Logger;

/// <summary>
/// Haare 프레임워크의 진입점. 최초 씬 로드 전에 Processor를 생성하고 부트스트랩한다.
/// BioSearch는 StartScene/게임 씬에 이미 EventSystem, AudioListener를 배치해 두었으므로
/// 여기서는 중복 생성을 피하기 위해 이미 존재하는지 먼저 확인한다.
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

        if (Object.FindObjectOfType<EventSystem>() == null)
        {
            var eventSystemObj = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            Object.DontDestroyOnLoad(eventSystemObj);
        }

        if (Object.FindObjectOfType<AudioListener>() == null)
        {
            var audioObj = new GameObject("AudioListener", typeof(AudioListener));
            Object.DontDestroyOnLoad(audioObj);
        }

        LogHelper.LogTask(LogHelper.FRAMEWORK,"RegisterProcesses -> end");
        await UniTask.CompletedTask;
    }

}
