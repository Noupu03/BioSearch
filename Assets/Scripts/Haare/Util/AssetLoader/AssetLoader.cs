using System;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Haare.Util.Logger;
using R3;
using Object = UnityEngine.Object;

namespace Haare.Util.Loader
{
    /// <summary>
    /// Addressables를 UniTask로 감싼 로더. BioSearch에서는 AssetPath(데모 전용 경로 상수)를
    /// 가져오지 않았으므로, 호출부가 Addressable 주소 문자열을 직접 넘긴다.
    /// </summary>
    public static class AssetLoader
    {
        private static readonly ReactiveProperty<float> _dlProgress = new ReactiveProperty<float>(0f);
        public static ReadOnlyReactiveProperty<float> DownloadProgress => _dlProgress;
        public static readonly Subject<bool> AssetDownloadTaskFinished = new Subject<bool>();

        public static async UniTask<T> InstantiatePrefab<T>(Transform parent, string address,
            CancellationToken cts = default) where T : Component
        {
            AsyncOperationHandle<GameObject> handle;
            handle = Addressables.InstantiateAsync(address, parent, false);
            try
            {
                GameObject instance = await handle.WithCancellation(cts);

                if (instance != null && instance.TryGetComponent<T>(out var component))
                {
                    return component;
                }

                if (instance != null)
                {
                    LogHelper.Warning(LogHelper.ASSETLOADER, $"생성된 프리팹 '{instance.name}'에서 '{typeof(T).Name}' 컴포넌트를 찾을 수 없습니다.");
                }

                return null;
            }
            catch (Exception e)
            {
                LogHelper.Error(LogHelper.ASSETLOADER, $"에셋 인스턴스화 실패! 주소: {address}\n오류: {e.Message}");
                return null;
            }
        }

        public static async UniTask<T> LoadAsset<T>(string assetAddress) where T : Object
        {
            AsyncOperationHandle<T> handle;
            handle = Addressables.LoadAssetAsync<T>(assetAddress);
            try
            {
                T asset = await handle.ToUniTask();

                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    // 이 에셋을 더 이상 사용하지 않을 때 Addressables.Release(handle)를 호출해 메모리를 해제해야 함
                    return asset;
                }

                LogHelper.Warning(LogHelper.ASSETLOADER, $"에셋 '{assetAddress}'을(를) 로드할 수 없습니다.");
                if (handle.IsValid()) Addressables.Release(handle);
                return null;
            }
            catch (Exception e)
            {
                LogHelper.Error(LogHelper.ASSETLOADER, $"에셋 로드 실패! 주소: {assetAddress}\n오류: {e.Message}");
                if (handle.IsValid()) Addressables.Release(handle);
                return null;
            }
        }

        private static string BasePath => Application.persistentDataPath;

        /// <summary>
        /// 데이터를 로컬 파일(JSON)로 저장합니다.
        /// </summary>
        public static async UniTask SaveJson(string filePath, object data)
        {
            try
            {
                string path = Path.Combine(BasePath, filePath);
                string json = JsonUtility.ToJson(data, true);
                await System.IO.File.WriteAllTextAsync(path, json);

                LogHelper.Log(LogHelper.DATAMANAGER, $"Saved to local: {path}");
            }
            catch (Exception e)
            {
                LogHelper.Error(LogHelper.DATAMANAGER, $"File Save Error: {e.Message}");
            }
        }

        /// <summary>
        /// 로컬 파일이 존재하는지 확인합니다.
        /// </summary>
        public static bool Exists(string fileName)
        {
            string path = Path.Combine(BasePath, fileName);
            return System.IO.File.Exists(path);
        }

        public static async UniTask<TextAsset> LoadJsonAsync(string fileName)
        {
            string path = Path.Combine(BasePath, fileName);

            if (!System.IO.File.Exists(path))
            {
                return null;
            }

            try
            {
                string json = await System.IO.File.ReadAllTextAsync(path);

                var textAsset = new TextAsset(json);
                textAsset.name = fileName;
                return textAsset;
            }
            catch (Exception e)
            {
                LogHelper.Error(LogHelper.DATAMANAGER, $"Local File Load Error: {e.Message}");
                return null;
            }
        }
    }
}
