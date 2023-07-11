using System;
using UnityEditor;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

// A manipulator that adds drag and drop functionality to a visual element.
public class DragAndDropSourceManipulator : MouseManipulator {

    private bool dragReady = false;
    private readonly Func<VisualElement, Object> elementToDragObject; // Finds the data object to use with DragAndDrop from the visual element.

    public DragAndDropSourceManipulator(Func<VisualElement, Object> elementToDragObject) {
        this.elementToDragObject = elementToDragObject;
    }

    protected override void RegisterCallbacksOnTarget() {
        target.RegisterCallback<MouseDownEvent, VisualElement>(OnMouseDown, target);
        target.RegisterCallback<MouseUpEvent, VisualElement>(OnMouseUp, target);
        target.RegisterCallback<MouseMoveEvent, VisualElement>(OnMouseMoved, target);
        target.RegisterCallback<DragUpdatedEvent, VisualElement>(OnDragUpdated, target);
    }

    protected override void UnregisterCallbacksFromTarget() {
        target.UnregisterCallback<MouseDownEvent, VisualElement>(OnMouseDown);
        target.UnregisterCallback<MouseUpEvent, VisualElement>(OnMouseUp);
        target.UnregisterCallback<MouseMoveEvent, VisualElement>(OnMouseMoved);
        target.UnregisterCallback<DragUpdatedEvent, VisualElement>(OnDragUpdated);
    }

    private void OnMouseDown(MouseDownEvent evt, VisualElement root) {
        dragReady = true;
    }

    private void OnMouseMoved(MouseMoveEvent evt, VisualElement root) {
        if (dragReady) {
            var obj = elementToDragObject(root);
            DragAndDrop.PrepareStartDrag();
            DragAndDrop.paths = new[] { AssetDatabase.GetAssetPath(obj) };
            DragAndDrop.objectReferences = new[] { obj };
            DragAndDrop.StartDrag(obj.name);
            dragReady = false;
        }
    }

    private void OnMouseUp(MouseUpEvent evt, VisualElement root) {
        dragReady = false;
    }

    private void OnDragUpdated(DragUpdatedEvent evt, VisualElement userArgs) {
        DragAndDrop.visualMode = DragAndDropVisualMode.Link;
    }
}