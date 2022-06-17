using UnityEngine;
using System;
using TMPro;

namespace OutlineClassroom
{
    class OutlineBox
    {
        static float Width = 0.05f;
        static float Depth = 0.01f;
        static float Offset = 0.2f;
        Vector3 Min;
        Vector3 Max;
        Color OutlineColor;
        Material OutlineMaterial;
        GameObject[] sides = new GameObject[4];

        public OutlineBox(Vector3 min, Vector3 max, Transform parent, Color color, string label)
        {
            this.Min = min;
            this.Max = max;
            this.OutlineColor = color;
            this.OutlineColor.a = 0.1f;
            OutlineMaterial = GenerateMaterial();

            if (Main.LabelActive)
                CreateLabel(parent, label);
            if (Main.OutlineActive)
                CreateOutline(parent);
        }

        private void CreateLabel(Transform parent, String text)
        {
            GameObject TMP = new GameObject("ClassroomLabel");
            TMP.layer = LayerMask.NameToLayer("Ignore Raycast");
            TMP.transform.SetParent(parent, true);
            TextMeshPro component = TMP.AddComponent<TextMeshPro>();
            switch (Main.LabelType)
            {
                case LabelType.Black:
                    component.color = Color.black;
                    break;
                case LabelType.White:
                    component.color = Color.white;
                    break;
                case LabelType.Element:
                    component.color = new Color(OutlineColor.r, OutlineColor.g, OutlineColor.b, 1);
                    break;
            }
            component.text = text;
            component.fontSize = Main.FontSize;
            component.enableAutoSizing = false;
            component.enableWordWrapping = true;
            component.autoSizeTextContainer = false;
            component.alignment = TextAlignmentOptions.Center;
            component.rectTransform.sizeDelta = new Vector2((Max - Min).x, (Max - Min).y);
            component.rectTransform.localPosition = new Vector3(0, (Max - Min).y / 2, Max.z);
        }
        private void CreateOutline(Transform parent)
        {
            sides[0] = CreateSide(parent, new Vector3(Min.x + (Max - Min).x / 2, Min.y, Min.z + Offset), new Vector3((Max - Min).x, Width, Depth));//Bottom
            sides[1] = CreateSide(parent, new Vector3(Min.x + (Max - Min).x / 2, Max.y - Width, Min.z + Offset), new Vector3((Max - Min).x, Width, Depth));//Top
            sides[2] = CreateSide(parent, new Vector3(Min.x + Width / 2, Min.y + (Max - Min).y / 2 - Width / 2, Min.z + Offset), new Vector3(Width, (Max - Min).y - 2 * Width, Depth));//Left
            sides[3] = CreateSide(parent, new Vector3(Max.x - Width / 2, Min.y + (Max - Min).y / 2 - Width / 2, Min.z + Offset), new Vector3(Width, (Max - Min).y - 2 * Width, Depth));//Right

            foreach (GameObject side in sides)
            {
                Light light = side.AddComponent<Light>();
                light.type = LightType.Directional;
                light.color = Color.white;
                light.intensity = 0.6f;
                light.cullingMask = 1 << 15;
            }
        }
        GameObject CreateSide(Transform parent, Vector3 position, Vector3 scale)
        {
            GameObject side = GameObject.CreatePrimitive(PrimitiveType.Cube);
            side.name = "OutlineCube";
            side.layer = LayerMask.NameToLayer("Ignore Raycast");
            side.transform.position = position;
            side.transform.localScale = scale;
            side.GetComponent<Renderer>().material = OutlineMaterial;
            side.transform.SetParent(parent, true);
            side.layer = 15;
            return side;
        }

        private Material GenerateMaterial()
        {
            Material outlineMaterial = new Material(Shader.Find("Standard"));
            outlineMaterial.SetFloat("_Mode", 3);
            outlineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
            outlineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            outlineMaterial.SetInt("_ZWrite", 0);
            outlineMaterial.DisableKeyword("_ALPHATEST_ON");
            outlineMaterial.DisableKeyword("_ALPHABLEND_ON");
            outlineMaterial.EnableKeyword("_ALPHAPREMULTIPLY_ON");
            outlineMaterial.renderQueue = 3000;
            outlineMaterial.color = OutlineColor;
            return outlineMaterial;
        }
        public static void RemoveAllLabels()
        {
            foreach (TextMeshPro textMeshPro in GameObject.FindObjectsOfType<TextMeshPro>())
            {
                if (textMeshPro.gameObject.name.Equals("ClassroomLabel"))
                    UnityEngine.Object.DestroyImmediate(textMeshPro.gameObject);
            }
        }
        public static void RemoveAllBorders()
        {
            foreach (BoxCollider collider in GameObject.FindObjectsOfType<BoxCollider>())
            {
                if (collider.gameObject.name.Equals("OutlineCube"))
                    UnityEngine.Object.DestroyImmediate(collider.gameObject);
            }
        }
        public static void RemoveAll() { RemoveAllLabels(); RemoveAllBorders(); }
    }
}
