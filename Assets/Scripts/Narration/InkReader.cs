using System.Collections;
using System.Collections.Generic;
using Ink.Runtime;
using UnityEngine;
using QFramework;

#if UNITY_EDITOR

using UnityEditor;

# endif

namespace Phosphorescence.Narration
{
    public class InkReader : MonoBehaviour
    {
        private PlotData _currentPlot;
        private Story _currentStory;
        [SerializeField] private TextAsset currentStory;
        private bool isStoryPlaying = false;

        // 公共属性
        public bool IsStoryPlaying => isStoryPlaying;
        public TextAsset CurrentStory => currentStory;
        public TextAsset ToInitializeStory { get; set; }

        private void Awake()
        {
            TypeEventSystem.Global.Register<InitializeStoryEvent>(e => {
                _currentPlot = e.plot;
                Initialize(e.plot.Script).Continue();
            }).UnRegisterWhenGameObjectDestroyed(gameObject);

            TypeEventSystem.Global.Register<RequestNewLineEvent>(e => Continue())
                .UnRegisterWhenGameObjectDestroyed(gameObject);

            TypeEventSystem.Global.Register<SelectOptionEvent>(e => {
                if (_currentStory == null) return;

                _currentStory.ChooseChoiceIndex(e.index);
                // Continue();
            }).UnRegisterWhenGameObjectDestroyed(gameObject);

            TypeEventSystem.Global.Register<RequestSetVariableEvent>(e => {
                if (_currentStory == null) return;

                _currentStory.variablesState[e.variableName] = e.value;
            }).UnRegisterWhenGameObjectDestroyed(gameObject);
        }

        // 设置并初始化故事
        public void SetAndInitializeStory(TextAsset storyAsset)
        {
            if (storyAsset == null)
            {
                Debug.LogError("Story asset is null!");
                return;
            }

            currentStory = storyAsset;
            InitializeStory();
        }

        // 初始化当前故事
        public void InitializeStory()
        {
            if (currentStory == null)
            {
                Debug.LogError("No story assigned!");
                return;
            }

            if (isStoryPlaying)
            {
                StopStory();
            }

            // 调用您的原有初始化方法
            Initialize(currentStory)?.Continue();
            isStoryPlaying = true;

            Debug.Log($"Started story: {currentStory.name}");
        }

        // 停止故事
        public void StopStory()
        {
            Stop(); // 调用您原有的Stop方法
            isStoryPlaying = false;
        }

        public InkReader Initialize(TextAsset rawStory)
        {
            if (rawStory == null) return this;

            _currentStory = new Story(rawStory.text);
            return this;
        }

        public void Stop()
        {
            NarrationManager.Instance.StopNarration();
            _currentStory = null;
        }

        public void Continue()
        {
            if (_currentStory == null) return;

            Debug.Log($"Continue: {_currentStory.canContinue}\n{_currentStory.currentText}\n{string.Join(", ", _currentStory.currentTags)}");

            if (_currentStory.canContinue)
            {
                _currentStory.Continue();

                if (_currentStory.currentChoices.Count > 0)
                {
                    var linesEvent = new OnLinesReadEvent
                    {
                        lines = new(),
                        tags = ParseTags(_currentStory.currentTags)
                    };

                    foreach (var choice in _currentStory.currentChoices)
                    {
                        linesEvent.lines.Add(new OnLineReadEvent
                        {
                            content = choice.text,
                            tags = ParseTags(choice.tags)
                        });
                    }

                    TypeEventSystem.Global.Send(linesEvent);
                }
                else
                {
                    var content = _currentStory.currentText;
                    var tags = ParseTags(_currentStory.currentTags);

                    TypeEventSystem.Global.Send(new OnLineReadEvent
                    {
                        content = content,
                        tags = tags
                    });
                }
            }
            else if (_currentStory.currentChoices.Count > 0) return;
            else  // Story End
            {
                TypeEventSystem.Global.Send(new OnStoryEndEvent());

                _currentPlot = null;
                _currentStory = null;
                isStoryPlaying = false;
            }
        }

        private Dictionary<string, string> ParseTags(List<string> rawTags)
        {
            var tags = new Dictionary<string, string>();
            if (rawTags == null) return tags;

            foreach (var rawTag in rawTags)
            {
                var splitTag = rawTag.Split(':');
                if (splitTag.Length != 2)
                {
                    Debug.LogError($"Tag could not be parsed: {rawTag}");
                    continue;
                }

                var key = splitTag[0].Trim().ToLower();
                var value = splitTag[1].Trim();  // Tag value is case sensitive

                tags.Add(key, value);
            }

            return tags;
        }
    }

    public struct InitializeStoryEvent
    {
        public PlotData plot;
    }

    public struct OnLineReadEvent
    {
        public string content;
        public Dictionary<string, string> tags;
    }

    public struct OnLinesReadEvent
    {
        public Dictionary<string, string> tags;
        public List<OnLineReadEvent> lines;
    }

    public struct OnStoryEndEvent
    {
        public PlotData plot;
    }

    public struct OnStoryEventTriggerEvent
    {
        public string eventName;
    }

    public struct RequestNewLineEvent { }

    public struct SelectOptionEvent
    {
        public int index;
    }

    public struct RequestSetVariableEvent
    {
        public string variableName;
        public object value;
    }


#if UNITY_EDITOR

    [CustomEditor(typeof(InkReader))]
    [CanEditMultipleObjects]
    public class SceneControlEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            InkReader manager = (InkReader)target;

            EditorGUILayout.Separator();

            EditorGUILayout.LabelField("Control", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            manager.ToInitializeStory = (TextAsset)EditorGUILayout.ObjectField("Story", manager.ToInitializeStory, typeof(TextAsset), false);
            if (GUILayout.Button("Initialize"))
            {
                manager.Initialize(manager.ToInitializeStory).Continue();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Separator();

            EditorGUILayout.LabelField("Debug", EditorStyles.boldLabel);
            if (GUILayout.Button("Stop"))
            {
                manager.Stop();
            }
        }
    }

#endif

}