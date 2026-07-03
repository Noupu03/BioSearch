using System.Threading;
using Cysharp.Threading.Tasks;

namespace Haare.Client.UI
{
    public class CoreUIManager : SceneUIManager
    {
        public override async UniTask Initialize(CancellationToken cts)
        {
            await base.Initialize(cts);
        }
    }
}
