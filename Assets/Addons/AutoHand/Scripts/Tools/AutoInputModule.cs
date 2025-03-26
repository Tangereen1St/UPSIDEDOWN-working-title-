using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;

namespace Autohand
{
    public class AutoInputModule : BaseInputModule
    {
        private List<HandCanvasPointer> pointers = new List<HandCanvasPointer>();
        private PointerEventData[] eventDatas;

        AutoInputModule _instance;
        private bool _isDestroyed = false;

        /// <summary>Returns the current pointer being checked when triggering input events, should only be used during built in UI input events</summary>
        public HandCanvasPointer currentPointer { get; private set; }

        public AutoInputModule Instance
        {
            get
            {
                if (_isDestroyed)
                    return null;

                if (_instance == null)
                {
                    _instance = AutoHandExtensions.CanFindObjectOfType<AutoInputModule>();
                    
                    if (_instance == null)
                    {
                        _instance = new GameObject("AutoInputModule").AddComponent<AutoInputModule>();
                        _instance.transform.parent = AutoHandExtensions.transformParent;
                    }

                    // Fix array initialization and null checks
                    BaseInputModule[] inputModule = AutoHandExtensions.CanFindObjectsOfType<BaseInputModule>();
                    if (inputModule != null && inputModule.Length > 1)
                    {
                        for (int i = inputModule.Length - 1; i >= 0; i--)
                        {
                            if (inputModule[i] != null && !inputModule[i].gameObject.GetComponent<AutoInputModule>())
                            {
                                Destroy(inputModule[i]);
                                Debug.LogWarning("AUTO HAND: Removing additional input module from the scene");
                            }
                        }
                    }

                    EventSystem[] systems = AutoHandExtensions.CanFindObjectsOfType<EventSystem>();
                    if (systems != null && systems.Length > 1)
                    {
                        for (int i = systems.Length - 1; i >= 0; i--)
                        {
                            if (systems[i] != null && !systems[i].gameObject.GetComponent<AutoInputModule>())
                            {
                                Destroy(systems[i]);
                                Debug.LogWarning("AUTO HAND: Removing additional event system from the scene");
                            }
                        }
                    }
                }

                return _instance;
            }
        }

        protected override void Awake()
        {
            base.Awake();
        }

        protected override void OnDestroy()
        {
            _isDestroyed = true;
        }

        public int AddPointer(HandCanvasPointer pointer)
        {
            if (!pointers.Contains(pointer))
            {
                pointers.Add(pointer);
                eventDatas = new PointerEventData[pointers.Count];

                for (int i = 0; i < eventDatas.Length; i++)
                {
                    eventDatas[i] = new PointerEventData(eventSystem);
                    eventDatas[i].delta = Vector2.zero;
                    eventDatas[i].position = new Vector2(Screen.width / 2, Screen.height / 2);
                }
            }

            return pointers.IndexOf(pointer);
        }

        public void RemovePointer(HandCanvasPointer pointer)
        {
            int pIndex = pointers.IndexOf(pointer);
            ProcessRelease(pIndex);
            ProcessExit(pIndex);

            if (pointers.Contains(pointer))
                pointers.Remove(pointer);
            foreach (var point in pointers)
            {
                point.SetIndex(pointers.IndexOf(point));
            }
            eventDatas = new PointerEventData[pointers.Count];
            for (int i = 0; i < eventDatas.Length; i++)
            {
                eventDatas[i] = new PointerEventData(eventSystem);
                eventDatas[i].delta = Vector2.zero;
                eventDatas[i].position = new Vector2(Screen.width / 2, Screen.height / 2);
            }
        }

        public override void Process()
        {
            if(pointers == null || eventDatas == null)
                return;
            
            for (int index = 0; index < pointers.Count && index < eventDatas.Length; index++)
            {
                try
                {
                    if (pointers[index] != null && pointers[index].enabled)
                    {
                        currentPointer = pointers[index];
                        pointers[index].Preprocess();
                        
                        if(eventDatas[index] != null)
                        {
                            eventSystem.RaycastAll(eventDatas[index], m_RaycastResultCache);
                            eventDatas[index].pointerCurrentRaycast = FindFirstRaycast(m_RaycastResultCache);

                            HandlePointerExitAndEnter(eventDatas[index], eventDatas[index].pointerCurrentRaycast.gameObject);

                            if(eventDatas[index].pointerDrag != null)
                                ExecuteEvents.Execute(eventDatas[index].pointerDrag, eventDatas[index], ExecuteEvents.dragHandler);
                        }
                    }
                }
                catch (System.Exception e) 
                {
                    Debug.LogError($"Error in AutoInputModule Process: {e.Message}");
                }
            }
        }

        public void ProcessPress(int index)
        {
            pointers[index].Preprocess();
            // Hooks in to Unity's event system to process a release
            eventDatas[index].pointerPressRaycast = eventDatas[index].pointerCurrentRaycast;

            eventDatas[index].pointerPress = ExecuteEvents.GetEventHandler<IPointerClickHandler>(eventDatas[index].pointerPressRaycast.gameObject);
            eventDatas[index].pointerDrag = ExecuteEvents.GetEventHandler<IDragHandler>(eventDatas[index].pointerPressRaycast.gameObject);

            ExecuteEvents.Execute(eventDatas[index].pointerPress, eventDatas[index], ExecuteEvents.pointerDownHandler);
            ExecuteEvents.Execute(eventDatas[index].pointerDrag, eventDatas[index], ExecuteEvents.beginDragHandler);
        }

        public void ProcessRelease(int index)
        {
            pointers[index].Preprocess();
            // Hooks in to Unity's event system to process a press
            GameObject pointerRelease = ExecuteEvents.GetEventHandler<IPointerClickHandler>(eventDatas[index].pointerCurrentRaycast.gameObject);

            if (eventDatas[index].pointerPress == pointerRelease)
                ExecuteEvents.Execute(eventDatas[index].pointerPress, eventDatas[index], ExecuteEvents.pointerClickHandler);

            ExecuteEvents.Execute(eventDatas[index].pointerPress, eventDatas[index], ExecuteEvents.pointerUpHandler);
            ExecuteEvents.Execute(eventDatas[index].pointerDrag, eventDatas[index], ExecuteEvents.endDragHandler);

            eventDatas[index].pointerPress = null;
            eventDatas[index].pointerDrag = null;

            eventDatas[index].pointerCurrentRaycast.Clear();
        }

        public void ProcessExit(int index) {
            if (index >= 0 && index < eventDatas.Length && eventDatas[index] != null && 
                eventDatas[index].pointerCurrentRaycast.gameObject != null)
            {
                GameObject pointerRelease = ExecuteEvents.GetEventHandler<IPointerExitHandler>(eventDatas[index].pointerCurrentRaycast.gameObject);
                if(pointerRelease != null)
                    ExecuteEvents.Execute(pointerRelease, eventDatas[index], ExecuteEvents.pointerExitHandler);
            }
        }

        public PointerEventData GetData(int index) { return eventDatas[index]; }
    }
}