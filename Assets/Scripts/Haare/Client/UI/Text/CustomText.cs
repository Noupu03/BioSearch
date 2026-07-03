using TMPro;

using Haare.Client.Routine;
using UnityEngine;

namespace Haare.Client.UI
{
    public class CustomText : MonoRoutine
    {
        [SerializeField]
        private TMP_Text _text;
        public TMP_Text Text
        {
            get => _text ??= GetComponentInChildren<TMP_Text>(true);

            set => _text = value;
        }
        protected override void Constructor()
        {
            base.Constructor();
            Text = GetComponent<TMP_Text>();
        }
        public void SetupText(string value)
        {
            Text.text = value;
        }
        public void SetupTextColor(Color32 color)
        {
            Text.color = color;
        }
    }
}
