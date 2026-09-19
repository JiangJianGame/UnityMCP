#if !UNITY_2020_1_OR_NEWER
using System;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace UnityEngine.UIElements
{
    public class DropdownField : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<DropdownField, UxmlTraits> { }
        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            private UxmlStringAttributeDescription m_Label = new UxmlStringAttributeDescription { name = "label" };
            private UxmlStringAttributeDescription m_Choices = new UxmlStringAttributeDescription { name = "choices" };
            private UxmlIntAttributeDescription m_Index = new UxmlIntAttributeDescription { name = "index", defaultValue = 0 };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                var f = (DropdownField)ve;
                f.label = m_Label.GetValueFromBag(bag, cc);
                string choicesStr = m_Choices.GetValueFromBag(bag, cc);
                if (!string.IsNullOrEmpty(choicesStr))
                {
                    f.choices = new List<string>(choicesStr.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries));
                }
                f.index = m_Index.GetValueFromBag(bag, cc);
            }
        }

        private List<string> m_Choices = new List<string>();
        private string m_Value = "";
        private int m_Index = -1;
        private string m_Label = "";
        private Label m_LabelElement;
        private Button m_Button;
        private List<EventCallback<ChangeEvent<string>>> m_Callbacks = new List<EventCallback<ChangeEvent<string>>>();

        public string label
        {
            get => m_Label;
            set
            {
                m_Label = value;
                if (!string.IsNullOrEmpty(m_Label))
                {
                    if (m_LabelElement == null)
                    {
                        m_LabelElement = new Label(m_Label);
                        m_LabelElement.AddToClassList("unity-base-field__label");
                        Insert(0, m_LabelElement);
                    }
                    else
                    {
                        m_LabelElement.text = m_Label;
                    }
                }
                else if (m_LabelElement != null)
                {
                    Remove(m_LabelElement);
                    m_LabelElement = null;
                }
            }
        }

        public List<string> choices
        {
            get => m_Choices;
            set
            {
                m_Choices = value ?? new List<string>();
                if (m_Index >= 0 && m_Index < m_Choices.Count)
                {
                    m_Value = m_Choices[m_Index];
                }
                else if (m_Choices.Count > 0)
                {
                    m_Index = 0;
                    m_Value = m_Choices[0];
                }
                else
                {
                    m_Index = -1;
                    m_Value = "";
                }
                UpdateDisplay();
            }
        }

        public string value
        {
            get => m_Value;
            set
            {
                if (m_Value != value)
                {
                    string oldVal = m_Value;
                    m_Value = value;
                    m_Index = m_Choices != null ? m_Choices.IndexOf(value) : -1;
                    UpdateDisplay();
                    NotifyChanged(oldVal, m_Value);
                }
            }
        }

        public int index
        {
            get => m_Index;
            set
            {
                if (m_Index != value)
                {
                    string oldVal = m_Value;
                    m_Index = value;
                    if (m_Choices != null && value >= 0 && value < m_Choices.Count)
                    {
                        m_Value = m_Choices[value];
                    }
                    else
                    {
                        m_Value = "";
                    }
                    UpdateDisplay();
                    NotifyChanged(oldVal, m_Value);
                }
            }
        }

        public DropdownField()
        {
            style.flexDirection = FlexDirection.Row;
            m_Button = new Button(ShowMenu);
            m_Button.style.flexGrow = 1f;
            m_Button.style.unityTextAlign = TextAnchor.MiddleLeft;
            Add(m_Button);
            UpdateDisplay();
        }

        public DropdownField(string label, List<string> choices, int defaultIndex) : this()
        {
            this.label = label;
            this.choices = choices;
            this.index = defaultIndex;
        }

        private void UpdateDisplay()
        {
            if (m_Button != null)
            {
                m_Button.text = string.IsNullOrEmpty(m_Value) ? (m_Choices != null && m_Choices.Count > 0 ? m_Choices[0] : "Select...") : m_Value;
            }
        }

        private void ShowMenu()
        {
            if (m_Choices == null || m_Choices.Count == 0) return;
            var menu = new UnityEditor.GenericMenu();
            for (int i = 0; i < m_Choices.Count; i++)
            {
                string choice = m_Choices[i];
                int choiceIndex = i;
                menu.AddItem(new GUIContent(choice), choice == m_Value, () =>
                {
                    index = choiceIndex;
                });
            }
            menu.ShowAsContext();
        }

        public void SetValueWithoutNotify(string val)
        {
            m_Value = val;
            m_Index = m_Choices != null ? m_Choices.IndexOf(val) : -1;
            UpdateDisplay();
        }

        public void RegisterValueChangedCallback(EventCallback<ChangeEvent<string>> callback)
        {
            if (callback != null && !m_Callbacks.Contains(callback))
                m_Callbacks.Add(callback);
        }

        public void UnregisterValueChangedCallback(EventCallback<ChangeEvent<string>> callback)
        {
            m_Callbacks.Remove(callback);
        }

        private void NotifyChanged(string oldVal, string newVal)
        {
            var evt = ChangeEvent<string>.GetPooled(oldVal, newVal);
            evt.target = this;
            foreach (var cb in m_Callbacks.ToArray())
            {
                cb(evt);
            }
        }
    }
}
#endif
