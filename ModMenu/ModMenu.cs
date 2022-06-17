using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace ModMenu
{
    public static class ModMenu
    {
        private static GameObject Panel;
        
        public static void CreatePanel()
        {            
            if (Panel != null)
                return;
            Panel = DefaultControls.CreatePanel(new DefaultControls.Resources());
            Panel.name = "mod_ControlPanel";
            RectTransform rect = Panel.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(0, 1);
            rect.sizeDelta = new Vector2(600, 100);
            rect.pivot = new Vector2(0, 1);
            Panel.GetComponent<Image>().color = new Color(1, 1, 1, 0);
            Panel.transform.SetParent(SC_Interface.instance.canvasPrincipal.transform, false);
            GridLayoutGroup grid = Panel.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(40, 40);
        }
        public static Button CreateButton(string name, string iconPath)
        {
            if (Panel == null)
                CreatePanel();
            Transform oldButton = Panel.transform.Find(name);
            if (oldButton != null)
                Object.DestroyImmediate(oldButton.gameObject); 
            Texture2D tex = new Texture2D(2, 2, TextureFormat.ARGB32, false); 
            tex.LoadImage(File.ReadAllBytes(iconPath));
            DefaultControls.Resources uiResources = new DefaultControls.Resources(); 
            uiResources.standard = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            GameObject button = DefaultControls.CreateButton(uiResources); 
            button.transform.SetParent(Panel.transform, false); 
            button.name = name; 
            Object.DestroyImmediate(button.GetComponentInChildren<Text>().gameObject); 
            return button.GetComponent<Button>();            
        }  
        
        public static void Dispose()
        {
            Object.DestroyImmediate(Panel);
            Panel = null;
        }
    }
}
