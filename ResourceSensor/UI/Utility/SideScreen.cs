﻿// CREDITS TO https://github.com/aki-art/ONI-MODS
using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;
using static DetailsScreen;

namespace FUtility.FUI
{
    public class SideScreen
    {
        /// <summary>
        /// Create sidescreen by cloning an already existing one.
        /// </summary>
        public static T AddClonedSideScreen<T>(string name, string originalName, Type originalType, bool addToScreens = true) where T : class
        {
            bool elementsReady = GetElements(out List<SideScreenRef> screens, out GameObject contentBody);
            if (elementsReady)
            {
                var oldPrefab = FindOriginal(originalName, screens);
                var newPrefab = Copy<T>(oldPrefab, contentBody, name, originalType);

                if (addToScreens)
                    screens.Add(NewSideScreen(name, newPrefab));

                return newPrefab as T;
            }

            return default;
        }

        /// <summary>
        /// Create sidescreen from a custom prefab.
        /// </summary>
        public static void AddCustomSideScreen<T>(string name, GameObject prefab)
        {
            bool elementsReady = GetElements(out List<SideScreenRef> screens, out GameObject contentBody);
            if (elementsReady)
            {
                var newScreen = prefab.AddComponent(typeof(T)) as SideScreenContent;
                screens.Add(NewSideScreen(name, newScreen));
            }
        }

        private static bool GetElements(out List<SideScreenRef> screens, out GameObject contentBody)
        {
            var detailsScreen = Traverse.Create(DetailsScreen.Instance);
            screens = detailsScreen.Field("sideScreens").GetValue<List<SideScreenRef>>();
            contentBody = detailsScreen.Field("sideScreenConfigContentBody").GetValue<GameObject>();

            return screens != null && contentBody != null;
        }

        private static SideScreenContent FindOriginal(string name, List<SideScreenRef> screens)
        {
            var result = screens.Find(s => s.name == name).screenPrefab;

            if (result == null)
                Debug.LogWarning("Could not find a sidescreen with the name " + name);

            return result;
        }

        private static SideScreenContent Copy<T>(SideScreenContent original, GameObject contentBody, string name, Type originalType)
        {
            var screen = Util.KInstantiateUI<SideScreenContent>(original.gameObject, contentBody).gameObject;
            UnityEngine.Object.Destroy(screen.GetComponent(originalType));

            var prefab = screen.AddComponent(typeof(T)) as SideScreenContent;
            prefab.name = name.Trim();

            screen.SetActive(false);
            return prefab;
        }

        private static SideScreenRef NewSideScreen(string name, SideScreenContent prefab)
        {
            return new SideScreenRef
            {
                name = name,
                offset = Vector2.zero,
                screenPrefab = prefab
            };
        }
    }
}
