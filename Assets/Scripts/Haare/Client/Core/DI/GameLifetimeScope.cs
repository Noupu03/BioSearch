using Haare.Client.UI;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class GameLifetimeScope : LifetimeScope
{
    [SerializeField] private CoreUIManager coreUIManagerPrefab;

    protected override void Configure(IContainerBuilder builder)
    {
        if (coreUIManagerPrefab != null)
        {
            builder.RegisterComponentInNewPrefab(coreUIManagerPrefab, Lifetime.Singleton)
                .DontDestroyOnLoad()
                .AsSelf();

            // RegisterComponentInNewPrefab은 지연 생성이라, 아무도 주입받지 않으면
            // CoreUIManager가 실제로 인스턴스화되지 않는다. SelectPopupManager 등은
            // DI 없이 FindObjectOfType으로 찾으므로, 빌드 직후 강제로 resolve해서
            // 씬에 실제 GameObject가 존재하도록 만든다.
            builder.RegisterBuildCallback(container => container.Resolve<CoreUIManager>());
        }
    }
}
