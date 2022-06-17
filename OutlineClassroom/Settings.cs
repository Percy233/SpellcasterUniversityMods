using UnityModManagerNet;

namespace OutlineClassroom
{
    public enum LabelType
    {
        Black,
        White,
        Element
    }
    public class Settings : UnityModManager.ModSettings, IDrawable
    {
        [Draw("Draw Outline")] public bool DrawOutline = true;
        [Draw("Draw Label")] public bool DrawLabel = true;
        [Draw("Label Color", DrawType.PopupList)] public LabelType type = LabelType.Element;
        [Draw("Font Size", DrawType.Slider, Min = 1f, Max = 12f)] public float FontSize = 6f;

        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }

        public void OnChange()
        {
            Main.ApplySettings();
        }
    }
}
