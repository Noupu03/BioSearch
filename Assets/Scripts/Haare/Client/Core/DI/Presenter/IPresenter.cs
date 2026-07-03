using System;
using R3;
using VContainer.Unity;

namespace Haare.Client.Core.DI
{
    public interface IPresenter : IPostInitializable, IDisposable
    {
        public CompositeDisposable disposables { get; }
    }
}
