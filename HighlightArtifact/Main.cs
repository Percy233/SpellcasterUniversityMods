using UnityEngine;
using UnityModManagerNet;
using AssemblyCSharp;
using HarmonyLib;
using System.Reflection;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.UI;
using System.IO;
using ModMenu;

namespace HighlightArtifact
{
#if DEBUG
    [EnableReloading]
#endif
    static class Main
    {
        private static UnityModManager.ModEntry ModEntry;
        private static Button EnableButton;
        internal static bool FlashingActive = false;
        internal static bool GameActive = false;
        internal static bool ModActive = false;
        internal static Harmony HarmonyInstance { get; private set; }

        static void Load(UnityModManager.ModEntry modEntry)
        {
            Main.ModEntry = modEntry;
            HarmonyInstance = new Harmony(modEntry.Info.Id);
            HarmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
#if DEBUG
            modEntry.OnUnload = Unload;
#endif            
            modEntry.OnToggle = OnToggle;
        }

        static bool OnToggle(UnityModManager.ModEntry modEntry, bool active)
        {
            ModActive = active;
            if (active)
                Start();
            else
                Stop();
            return true;
        }

        internal static void Start()
        {
            if (SC_ScriptPrincipal.instance != null)
                GameActive = true;
            if (GameActive && ModActive)
            {
                ModMenu.ModMenu.CreatePanel();
                EnableButton = ModMenu.ModMenu.CreateButton("mod_HighlightArtifact_EnableButton", Path.Combine(ModEntry.Path, "plant-fill.png"));
                EnableButton.onClick.AddListener(ButtonOnClick);
            }
        }

        internal static void Stop()
        {
            FlashingActive = false;
            if (EnableButton != null)
                Object.DestroyImmediate(EnableButton.gameObject);
            StopFlashing();
        }

        static void ButtonOnClick()
        {
            FlashingActive = !FlashingActive;
            if (FlashingActive)
                StartFlashing();
            else
                StopFlashing();
        }

        internal static void StartFlashing()
        {
            if (!GameActive)
                return;
            foreach (SC_Salle classroom in Monde.instance.listeDesSalles)
            {
                foreach (SC_Artefact artefact in classroom.artefacts)
                {
                    IEnumerator coroutine = FlashingCoroutine(artefact);
                    artefact.StartCoroutine(coroutine);
                }
            }
        }

        private static IEnumerator FlashingCoroutine(SC_Artefact artefact)
        {
            IEnumerable<Renderer> rendererComponents = artefact.gameObject.GetComponentsInChildren<Renderer>().Where(a => a is MeshRenderer || a is SkinnedMeshRenderer);
            List<cakeslice.Outline> outlines = new List<cakeslice.Outline>();
            foreach (Renderer renderer in rendererComponents)
                outlines.Add(renderer.gameObject.AddComponent<cakeslice.Outline>());
            while (true)
            {
                foreach (cakeslice.Outline outline in outlines)
                    outline.color = 1;
                yield return new WaitForSecondsRealtime(0.2f);
                foreach (cakeslice.Outline outline in outlines)
                    outline.color = 0;
                yield return new WaitForSecondsRealtime(0.2f);
            }
        }

        internal static void StopFlashing()
        {
            if (!GameActive)
                return;
            foreach (SC_Artefact artefact in Monde.instance.listeDesSalles.SelectMany(s => s.artefacts))
            {
                artefact.StopAllCoroutines();
                foreach (cakeslice.Outline outline in artefact.gameObject.GetComponentsInChildren<cakeslice.Outline>())
                    Object.DestroyImmediate(outline);
            }
        }
#if DEBUG
        static bool Unload(UnityModManager.ModEntry modEntry)
        {
            HarmonyInstance.UnpatchAll(modEntry.Info.Id);
            return true;
        }
#endif
    }

    [HarmonyPatch(typeof(SC_Artefact))]
    [HarmonyPatch("deselectionner")]
    class HighlightArtifact_DeselectArtefact_Patch
    {
        static void Postfix()
        {
            if (Main.FlashingActive && Main.GameActive)
            {
                Main.StopFlashing();
                Main.StartFlashing();
            }
        }
    }
    [HarmonyPatch(typeof(SC_ScriptPrincipal))]
    [HarmonyPatch("retourAuMenuPrincipal")]
    class HighlightArtifact_ReturnToMenu_Patch
    {
        static void Prefix()
        {
            Main.GameActive = false;
            Main.Stop();
        }
    }
    [HarmonyPatch(typeof(SC_ScriptPrincipal))]
    [HarmonyPatch("Start")]
    class HighlightArtifact_Start_Patch
    {
        static void Postfix()
        {
            Main.GameActive = true;
            Main.Start();
        }
    }
}
