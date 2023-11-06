using UnityEditor;
using UnityEngine;

namespace InspectorHistory {
    public static class HistoryPreferences {

        private const string HISTORY_SIZE_PREF = "HistorySizePref";
        private const int DEFAULT_HISTORY_SIZE = 20;

        private static bool prefsLoaded;
        private static int historySize;

        [SettingsProvider]
        public static SettingsProvider CreateSelectionHistorySettingsProvider() {
            var provider = new SettingsProvider("Preferences/Inspector History", SettingsScope.User) {
                label = "Inspector History",

                guiHandler = (searchContext) => {
                    if (!prefsLoaded) {
                        historySize = EditorPrefs.GetInt(HISTORY_SIZE_PREF, DEFAULT_HISTORY_SIZE);
                        prefsLoaded = true;
                    }

                    EditorGUILayout.Space();
                    EditorGUI.indentLevel++;
                    EditorGUIUtility.labelWidth = 200;

                    historySize = EditorGUILayout.IntField("History Size", historySize);

                    EditorGUI.indentLevel--;

                    if (GUI.changed) {
                        EditorPrefs.SetInt(HISTORY_SIZE_PREF, historySize);

                        // Push changes to systems.
                        HistoryData.historySize = HistoryPreferences.GetHistorySize();
                    }
                },
            };

            return provider;
        }

        public static int GetHistorySize() {
            return EditorPrefs.GetInt(HISTORY_SIZE_PREF, DEFAULT_HISTORY_SIZE);
        }
    }
}