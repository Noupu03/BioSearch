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
        }
    }
}
