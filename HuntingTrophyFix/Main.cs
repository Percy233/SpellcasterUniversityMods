using UnityModManagerNet;
using AssemblyCSharp;
using HarmonyLib;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;

namespace HuntingTrophyFix
{
#if DEBUG
    [EnableReloading]
#endif
    public class Main
    {
        private static UnityModManager.ModEntry ModEntry;
        internal static bool ModActive = false;
        internal static Harmony HarmonyInstance { get; private set; }
        internal static Dictionary<Personnage, float> BuffsApplied = new Dictionary<Personnage, float>();
        internal static float SpeedBuff = 0.4f;

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
            if (!active)
                Stop();
            return true;
        }

        internal static void Stop()
        {
            foreach (Personnage person in BuffsApplied.Keys.ToList())
            {
                if (person != null)
                {
                    person.modificateurVitesse -= Main.BuffsApplied[person];
                    Main.BuffsApplied.Remove(person);
                }
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

    [HarmonyPatch(typeof(Personnage))]
    [HarmonyPatch("notifierEntree")]
    class HuntingTrophyFix_EnterRoom_Patch
    {
        static void Prefix(Personnage __instance, Cellule c)
        {
            Cellule cell = c;
            if (!Main.ModActive)
                return;
            if (cell.salle == null)
                return;
            if (Main.BuffsApplied.ContainsKey(__instance))
            {
                __instance.modificateurVitesse -= Main.BuffsApplied[__instance];
                Main.BuffsApplied.Remove(__instance);
            }
            SC_Salle salle = cell.salle.GetComponent<SC_Salle>();
            float buffAmount = 0f;
            foreach (SC_Artefact artefact in salle.artefacts)
            {
                if (artefact.artefactLogique.modeleArtefact.id == "TropheeDeChasse")
                    buffAmount += Main.SpeedBuff * artefact.efficaciteArtefact;
            }
            foreach (SC_Salle tropheesAdjacente in salle.salleDesTropheesAdjacentes.Distinct())
            {
                foreach (SC_Artefact artefact in tropheesAdjacente.artefacts)
                {
                    if (artefact.artefactLogique.modeleArtefact.id == "TropheeDeChasse")
                        buffAmount += Main.SpeedBuff * artefact.efficaciteArtefact;
                }
            }
            if (buffAmount == 0f)
                return;
            __instance.modificateurVitesse += buffAmount;
            Main.BuffsApplied[__instance] = buffAmount;
        }
    }

    [HarmonyPatch(typeof(Personnage))]
    [HarmonyPatch("notifierSortie")]
    class HuntingTrophyFix_ExitRoom_Patch
    {
        static void Postfix(Personnage __instance)
        {
            if (!Main.ModActive)
                return;
            if (!Main.BuffsApplied.ContainsKey(__instance))
                return;
            if (__instance.salleOccupee != null)
                return;
            __instance.modificateurVitesse -= Main.BuffsApplied[__instance];
            Main.BuffsApplied.Remove(__instance);
        }
    }
}
