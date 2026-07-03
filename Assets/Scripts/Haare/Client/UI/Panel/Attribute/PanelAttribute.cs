using System;

namespace Haare.Client.UI
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class PanelAttribute : Attribute
    {
        public string AddressablePath { get; }

        public PanelAttribute(string addressablePath)
        {
            AddressablePath = addressablePath;
        }
    }
}
