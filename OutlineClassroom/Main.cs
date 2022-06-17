using UnityEngine;
using UnityModManagerNet;
using AssemblyCSharp;
using HarmonyLib;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine.UI;
using System.IO;

namespace OutlineClassroom
{
#if DEBUG
    [EnableReloading]
#endif
    public class Main
    {
        internal static Settings Settings;
        internal static UnityModManager.ModEntry ModEntry;
        internal static bool GameActive = false;
        internal static bool ModActive = false;
        internal static bool OutlineActive = true;
        internal static bool LabelActive = true;
        private static bool ModVisible = false;
        internal static LabelType LabelType = LabelType.Element;
        internal static float FontSize = 6f;
        private static Button EnableButton;
        internal static Harmony HarmonyInstance { get; private set; }
        private static readonly Dictionary<string, ECOLES> ClassroomTypes = new Dictionary<string, ECOLES>() {  {"DojoDesElementalistes",ECOLES.ARCANE},{"SalleArcanes",ECOLES.ARCANE},{"SalleInterdimensionelle",ECOLES.ARCANE},{"Temporelle",ECOLES.ARCANE},
                                                                                                {"ChapelleDesHeros",ECOLES.LUMIERE},{"CloitreDesJustes",ECOLES.LUMIERE},{"ChambreDeVerite",ECOLES.LUMIERE},{"SalleDeLaLumiere",ECOLES.LUMIERE},
                                                                                                {"AreneDesAssassins",ECOLES.OMBRE},{"Crypte",ECOLES.OMBRE},{"SalleDemonologie",ECOLES.OMBRE},{"SalleOmbre",ECOLES.OMBRE},
                                                                                                {"Dolmen",ECOLES.NATURE},{"SalleDeLaNature",ECOLES.NATURE},{"Serre",ECOLES.NATURE},{"Bestiaire",ECOLES.NATURE},
                                                                                                {"Potions",ECOLES.ALCHIMIE},{"SalleAlchimie",ECOLES.ALCHIMIE},{"Atelier",ECOLES.ALCHIMIE},{"ScriptoriumRunique",ECOLES.ALCHIMIE} };

        static void Load(UnityModManager.ModEntry modEntry)
        {
            Settings = Settings.Load<Settings>(modEntry);
            Main.ModEntry = modEntry;
            HarmonyInstance = new Harmony(modEntry.Info.Id);
            HarmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
#if DEBUG
            modEntry.OnUnload = Unload;
#endif
            modEntry.OnToggle = OnToggle;
            modEntry.OnSaveGUI = OnSaveGUI;
            modEntry.OnGUI = OnGUI;
        }

        static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            Settings.Save(modEntry);
        }
        static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            Settings.Draw(modEntry);
        }
        static bool OnToggle(UnityModManager.ModEntry modEntry, bool active)
        {
            ModActive = active;
            if (active)
            {
                ApplySettings();
                Start();
            }
            else
                Stop();
            return true;
        }
        public static void ApplySettings()
        {
            OutlineActive = Settings.DrawOutline;
            LabelActive = Settings.DrawLabel;
            FontSize = Settings.FontSize;
            LabelType = Settings.type;
            CreateOutlines();

        }
        internal static void Start()
        {
            if (SC_ScriptPrincipal.instance != null)
                GameActive = true;
            if (GameActive && ModActive)
            {
                EnableButton = ModMenu.ModMenu.CreateButton("mod_OutlineClassroom_EnableButton", Path.Combine(ModEntry.Path, "ancient-pavilion-line.png"));
                EnableButton.onClick.AddListener(ButtonOnClick);
            }
        }

        internal static void Stop()
        {
            OutlineBox.RemoveAll();
            if (EnableButton != null)
                Object.DestroyImmediate(EnableButton.gameObject);            
        }

        static void ButtonOnClick()
        {
            ModVisible = !ModVisible;
            if (ModVisible)
                CreateOutlines();
            else
                RemoveOutlines();
        }

        internal static void CreateOutlines()
        {
            if (!GameActive || !ModVisible)
                return;
            OutlineBox.RemoveAll();
            foreach (SC_Salle classroom in Monde.instance.listeDesSalles)
            {
                Color outlineColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
                if (ClassroomTypes.ContainsKey(classroom.id))
                    outlineColor = Z.getColor(ClassroomTypes[classroom.id]);
                Vector3 min = classroom.cellule.zoneDeConstruction.gameObject.GetComponent<Renderer>().bounds.min;
                Vector3 max = classroom.cellule.zoneDeConstruction.gameObject.GetComponent<Renderer>().bounds.max;
                if (classroom.celluleSecondaire != null)
                    max = classroom.celluleSecondaire.zoneDeConstruction.gameObject.GetComponent<Renderer>().bounds.max;
                new OutlineBox(min, max, classroom.transform, outlineColor, Z.extraireLeTitre(Z.get("Salle_" + classroom.id)));
            }
        }

        static void RemoveOutlines()
        {
            OutlineBox.RemoveAll();
        }

#if DEBUG
        static bool Unload(UnityModManager.ModEntry modEntry)
        {
            HarmonyInstance.UnpatchAll(modEntry.Info.Id);
            return true;
        }
#endif
    }

    [HarmonyPatch(typeof(SC_ScriptPrincipal))]
    [HarmonyPatch("retourAuMenuPrincipal")]
    class OutlineClassroom_ReturnToMenu_Patch
    {
        static void Prefix()
        {
            Main.GameActive = false;
            Main.Stop();
        }
    }

    [HarmonyPatch(typeof(SC_ScriptPrincipal))]
    [HarmonyPatch("Start")]
    class OutlineClassroom_Start_Patch
    {
        static void Postfix()
        {
            Main.GameActive = true;
            Main.Start();
        }
    }

    [HarmonyPatch(typeof(Cellule))]
    [HarmonyPatch("construireSalle")]
    class OutlineClassroom_BuildClassroom_Patch
    {
        static void Postfix()
        {
            Main.CreateOutlines();
        }
    }
}
