using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace InspectorHistory {

    public class HistoryWindow : EditorWindow {

        // The template each element in the history window.
        public VisualTreeAsset windowTemplate;
        public VisualTreeAsset elementTemplate;

        private ScrollView scrollView;
        private List<VisualElement> elements = new List<VisualElement>();
        private Object lastClickedObject; // Used to detect double clicks.

        [MenuItem("Window/General/History")]
        public static void ShowWindow() {
            HistoryWindow wnd = GetWindow<HistoryWindow>();
            wnd.titleContent = new GUIContent("History");
        }

        private void OnFocus() {
            SetClass(rootVisualElement, "LightSkin", !EditorGUIUtility.isProSkin);
            rootVisualElement.AddToClassList("Focused");
        }

        private void OnLostFocus() {
            rootVisualElement.RemoveFromClassList("Focused");
            lastClickedObject = null;
        }

        public void CreateGUI() {
            var window = windowTemplate.Instantiate();
            window.style.height = new StyleLength(Length.Percent(100)); // HACK: The template container doesn't fill the window for some reason.
            rootVisualElement.Add(window);
            scrollView = rootVisualElement.Q<ScrollView>();

            RefreshElements();
            HistoryData.instance.onChanged += RefreshElements;
        }

        private void RefreshElements() {
            HistoryData history = HistoryData.instance;
            history.Clean();

            // Create additional visual elements if there aren't enough to show all of the histroy.
            while (history.Count > elements.Count) {
                var tree = elementTemplate.Instantiate();
                scrollView.Add(tree);
                elements.Add(tree);
                tree.RegisterCallback<ClickEvent, VisualElement>(OnElementClicked, tree);
                tree.RegisterCallback<DragPerformEvent, VisualElement>(OnDraggedIntoElement, tree);
                tree.AddManipulator(new ContextualMenuManipulator((ContextualMenuPopulateEvent evt) => OnElementRightClicked(evt, tree)));
                tree.AddManipulator(new DragAndDropSourceManipulator(ElementToObject));
                tree.Q("PinIcon").RegisterCallback<ClickEvent, VisualElement>(OnPinClicked, tree);
            }

            // Iterate across each element and update it to match the history data.
            // TODO: Could improve performance by caching queries (easy), or by rearranging elements manually (complicated!).
            for (int i = 0; i < elements.Count; i++) {
                var element = elements[i];
                if (i < history.Count) {
                    var elementObj = history[i];
                    element.style.display = DisplayStyle.Flex;

                    // Update the display info.
                    element.Q<Label>("ObjectLabel").text = elementObj.name;
                    element.Q<Image>("ObjectIcon").image = AssetPreview.GetMiniThumbnail(elementObj);
                    SetClass(element, "Selected", elementObj == Selection.activeObject);
                    SetClass(element, "SceneObj", !EditorUtility.IsPersistent(elementObj));
                    SetClass(element, "Pinned", history.IsPinned(elementObj));
                } else {
                    element.style.display = DisplayStyle.None;
                }
            }

            // Scroll to the bottom of the window if a new item was selected.
            if (Selection.activeObject == history.lastNonPinnedElement) {
                var scroller = scrollView.verticalScroller;
                scroller.value = scroller.highValue > 0 ? scroller.highValue : 0;
            }
        }

        private void SetClass(VisualElement element, string ussClass, bool value) {
            if (value) {
                element.AddToClassList(ussClass);
            } else {
                element.RemoveFromClassList(ussClass);
            }
        }

        private void OnElementClicked(ClickEvent evt, VisualElement root) {
            Object obj = ElementToObject(root);
            if (Selection.activeObject == obj) {
                if (lastClickedObject == obj) {
                    AssetDatabase.OpenAsset(obj);
                }
            } else {
                HistoryData.ignoreSelectionChangedFlag = true; // Don't update the list order for better UX.
                Selection.activeObject = obj;
                RefreshElements(); // Do refresh the list to highlight the new selection.
            }
            lastClickedObject = obj;
        }

        private void OnElementRightClicked(ContextualMenuPopulateEvent evt, TemplateContainer root) {
            Object obj = ElementToObject(root);
            EditorGUIUtility.PingObject(obj);
        }

        private void OnDraggedIntoElement(DragPerformEvent evt, VisualElement root) {
            var targetObject = ElementToObject(root);
            if (EditorUtility.IsPersistent(targetObject)) {
                return; // Don't drop scene objects into assets.
            }

            foreach (var droppedObject in DragAndDrop.objectReferences) {
                if (!EditorUtility.IsPersistent(droppedObject)) {
                    var a = droppedObject as GameObject;
                    var b = targetObject as GameObject;
                    if (a && b) {
                        a.transform.parent = b.transform;
                    }
                }
            }
        }

        private void OnPinClicked(ClickEvent evt, VisualElement element) {
            var history = HistoryData.instance;
            var elementObj = ElementToObject(element);
            if (!history.IsPinned(elementObj)) {
                history.Pin(elementObj);
            } else {
                history.Unpin(elementObj);
            }
            evt.StopPropagation();
        }

        public static Object ElementToObject(VisualElement root) {
            int index = root.parent.IndexOf(root);
            var obj = HistoryData.instance[index];
            return obj;
        }
    }
}