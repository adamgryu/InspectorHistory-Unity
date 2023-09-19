using UnityEditor;
using UnityEngine;

namespace InspectorHistory {
    public static class HistoryPreferences {

        public enum PinnedOrder {
            PinnedAtBottom = 0,
            PinnedAtTop = 1,
        }

        private static string HISTORY_SIZE_PREF = "HistorySizePref";
        private static string PINNED_POSITION_PREF = "PinnedPositionPref";

        private static bool prefsLoaded = false;
        private static int historySize;
        private static PinnedOrder pinnedOrder;

        [SettingsProvider]
        public static SettingsProvider CreateSelectionHistorySettingsProvider() {
            var provider = new SettingsProvider("Preferences/Inspector History ", SettingsScope.User) {
                label = "Inspector History",

                guiHandler = (searchContext) => {
                    if (!prefsLoaded) {
                        historySize = EditorPrefs.GetInt(HISTORY_SIZE_PREF, 10);
                        pinnedOrder = EditorPrefs.GetBool(PINNED_POSITION_PREF, false) ? PinnedOrder.PinnedAtTop : PinnedOrder.PinnedAtBottom;
                        prefsLoaded = true;
                    }

                    EditorGUILayout.Space();
                    EditorGUI.indentLevel++;
                    EditorGUIUtility.labelWidth = 200;

                    historySize = EditorGUILayout.IntField("History Size", historySize);
                    // TODO: pinnedOrder = (PinnedOrder)EditorGUILayout.EnumPopup("Pinned Order", pinnedOrder);

                    EditorGUI.indentLevel--;

                    if (GUI.changed) {
                        EditorPrefs.SetInt(HISTORY_SIZE_PREF, historySize);
                        EditorPrefs.SetBool(PINNED_POSITION_PREF, pinnedOrder == PinnedOrder.PinnedAtTop ? true : false);

                        // Push changes to systems.
                        HistoryData.historySize = HistoryPreferences.GetHistorySize();
                    }
                },
            };

            return provider;
        }

        public static int GetHistorySize() {
            return EditorPrefs.GetInt(HISTORY_SIZE_PREF, 20);
        }
    }
}