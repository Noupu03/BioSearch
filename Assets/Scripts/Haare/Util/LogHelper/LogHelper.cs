using UnityEngine;
using System.Text;

namespace Haare.Util.Logger
{
    public static class LogHelper
    {

        // ==========================================
        // 1. 기본 로그 (Log, LogTask)
        // ==========================================
        public static void Log(params string[] contents)
        {
            if (contents == null || contents.Length == 0) return;

            (string headers, string message) = ParseContents(contents);

            Debug.Log($"{headers}{message}");
        }

        public static void LogTask(params string[] contents)
        {
            if (contents == null || contents.Length == 0) return;

            (string headers, string message) = ParseContents(contents);

            Debug.Log($"{TASK} {headers}{message}");
        }

        // ==========================================
        // 2. 경고 및 에러 (Warning, Error)
        // ==========================================
        public static void Warning(params string[] contents)
        {
            if (contents == null || contents.Length == 0) return;

            string message = contents[contents.Length - 1];
            StringBuilder headerBuilder = new StringBuilder();

            for (int i = 0; i < contents.Length - 1; i++)
            {
                headerBuilder.Append($"<b><color=yellow>[{contents[i]}]</color></b> ");
            }

            Debug.LogWarning($"{headerBuilder}{message}");
        }

        public static void Error(params string[] contents)
        {
            if (contents == null || contents.Length == 0) return;

            string message = contents[contents.Length - 1];
            StringBuilder headerBuilder = new StringBuilder();

            for (int i = 0; i < contents.Length - 1; i++)
            {
                headerBuilder.Append($"<b><color=red>[{contents[i]}]</color></b> ");
            }

            Debug.LogError($"{headerBuilder}{message}");
        }

        // ==========================================
        // 3. 내부 유틸리티
        // ==========================================
        private static (string headers, string message) ParseContents(string[] contents)
        {
            if (contents.Length == 1) return ("", contents[0]);

            string message = contents[contents.Length - 1];

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < contents.Length - 1; i++)
            {
                sb.Append(contents[i]).Append(" ");
            }

            return (sb.ToString(), message);
        }


        public static string DEMO    = $"<b><color=#00FF00>[DEMO]</color></b>";

        public static string CANCELLED = $"<b><color=yellow>[CTS]</color></b>";
        public static string TASK      = $"<b><color=teal>[TASK]</color></b>";

        public static string SERVER    = $"<b><color=#00FF00>[SERVER]</color></b>";
        public static string CLIENT    = $"<b><color=cyan>[TWINKLE]</color></b>";
        public static string FRAMEWORK = $"<b><color=lightgreen>[HAARE]</color></b>";
        public static string SERVICE   = $"<b><color=#6495ED>[SERVICE]</color></b>";

        public static string ASSETLOADER = $"<b><color=orange>[ASSETLOADER]</color></b>";
        public static string DATAMANAGER = $"<b><color=silver>[DATAMANAGER]</color></b>";
    }
}
