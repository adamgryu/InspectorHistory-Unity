using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace InspectorHistory {

    [InitializeOnLoad]
    [FilePath("Library/history.asset", FilePathAttribute.Location.ProjectFolder)]
    public class HistoryData : ScriptableSingleton<HistoryData> {

        // Static Fields
        public static int historySize = 5;
        public static bool ignoreSelectionChangedFlag;

        static HistoryData() {
            // Register these callbacks on editor load.
            Selection.selectionChanged += OnSelectionChanged;
            EditorSceneManager.sceneClosing += OnSceneClosing;
            EditorApplication.quitting += OnEditorQuitting;
        }

        // Public Properties
        public event Action onChanged;
        public Object lastNonPinnedElement => history.Count == 0 ? null : history[history.Count - 1];

        // Private State
        [SerializeField]
        private List<Object> history = new List<Object>();
        [SerializeField]
        private List<Object> pinned = new List<Object>();

        /// <summary>
        /// An indexer that combines the history and pinned into one accessor.
        /// </summary>
        public Object this[int i] {
            get { return i < history.Count ? history[i] : pinned[i - history.Count]; }
        }

        /// <summary>
        /// The count of history and pinned combined.
        /// </summary>
        public int Count => history.Count + pinned.Count;

        private void OnEnable() {
            historySize = HistoryPreferences.GetHistorySize();
            Debug.Log(historySize);
        }

        public void Add(Object activeObject) {
            history.Add(activeObject);
            while (history.Count > historySize) {
                history.RemoveAt(0);
            }
            onChanged?.Invoke();
        }

        public void Pin(Object objectToPin) {
            pinned.Add(objectToPin);
            onChanged?.Invoke();
        }

        public void Unpin(Object obj) {
            pinned.Remove(obj);
            onChanged?.Invoke();
        }

        public bool IsPinned(Object objectToPin) {
            return pinned.Contains(objectToPin);
        }

        public void Clean() {
            RemoveNullEntries(history);
            RemoveNullEntries(pinned);
            RemoveDuplicates();
        }

        private void RemoveNullEntries(List<Object> list) {
            for (int i = list.Count - 1; i >= 0; i--) {
                if (list[i] == null) {
                    list.RemoveAt(i);
                }
            }
        }

        private void RemoveDuplicates() {
            var hashSet = new HashSet<Object>();
            for (int i = Count - 1; i >= 0; i--) {
                var element = this[i];
                if (hashSet.Contains(element)) {
                    bool isPinned = i >= history.Count;
                    if (isPinned) {
                        pinned.RemoveAt(i - history.Count);
                    } else {
                        history.RemoveAt(i);
                    }
                } else {
                    hashSet.Add(element);
                }
            }
        }

        #region Static Callbacks

        private static void OnEditorQuitting() {
            HistoryData.instance.Save(true);
        }

        private static void OnSelectionChanged() {
            if (ignoreSelectionChangedFlag) {
                // In some occasions we don't want to fire a event when the selection is changed.
                ignoreSelectionChangedFlag = false;
                return;
            };
            if (Selection.activeObject != null) {
                HistoryData.instance.Add(Selection.activeObject);
            }
        }

        private static void OnSceneClosing(UnityEngine.SceneManagement.Scene scene, bool removingScene) {
            // Throws the last scene into the history, since I often want to go back anyway.
            var path = scene.path;
            var asset = AssetDatabase.LoadAssetAtPath<Object>(path);
            HistoryData.instance.Add(asset);
        }

        #endregion
    }
}