using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenericPersistentUI
{
    public List<UIElement> Elements { get; private set; } = new List<UIElement>();

    public void AddElement(UIElement element)
    {
        Elements.Add(element);
    }

    public void Clear()
    {
        Elements.Clear();
    }

    public abstract class UIElement
    {
        
    }

    public static class UIBinding
    {
        public static (Func<T> getter, Action<T> setter) FromProperty<T>(
            Func<T> getter,
            Action<T> setter) => (getter, setter);
    }

    public class Heading : UIElement
    {
        public string Title { get; private set; }

        public Heading(string title)
        {
            this.Title = title;
        }
    }

    public class RefLabel : UIElement
    {
        public string Title { get; private set; }
        private readonly Func<string> getter;

        public RefLabel(string title, Func<string> getter)
        {
            this.Title = title;
            this.getter = getter;
        }

        public string Value
        {
            get => getter();
        }
    }

    public class Button : UIElement
    {
        public string Title {get; private set;}
        readonly Action clickAction;

        public Button(string title, Action clickAction)
        {
            this.Title = title;
            this.clickAction = clickAction;
        }

        public void Invoke()
        {
            clickAction.Invoke();
        }
    }

    public class Toggle : UIElement
    {
        public string Title { get; private set; }
        private readonly Func<bool> getter;
        private readonly Action<bool> setter;

        public Toggle(string title, Func<bool> getter, Action<bool> setter)
        {
            this.Title = title;
            this.getter = getter;
            this.setter = setter;
        }

        public bool Value
        {
            get => getter();
            set
            {
                if (value == getter()) return;
                setter(value);
            }
        }
    }

    public class IntField : UIElement
    {
        public string Title { get; private set; }
        private readonly Func<int> getter;
        private readonly Action<int> setter;

        public IntField(string title, Func<int> getter, Action<int> setter)
        {
            Title = title;
            this.getter = getter;
            this.setter = setter;
        }

        public int Value
        {
            get => getter();
            set
            {
                if (value == getter()) return;
                setter(value);
            }
        }
    }

    public class FloatField : UIElement
    {
        public string Title { get; private set; }
        private readonly Func<float> getter;
        private readonly Action<float> setter;

        public FloatField(string title, Func<float> getter, Action<float> setter)
        {
            Title = title;
            this.getter = getter;
            this.setter = setter;
        }

        public float Value
        {
            get => getter();
            set
            {
                if (Mathf.Approximately(value, getter())) return;
                setter(value);
            }
        }
    }

    public class Slider : UIElement
    {
        public string Title { get; private set; }
        private readonly Func<float> getter;
        private readonly Action<float> setter;
        public float min;
        public float max;

        public Slider(string title, float min, float max, Func<float> getter, Action<float> setter)
        {
            this.Title = title;
            this.min = min;
            this.max = max;
            this.getter = getter;
            this.setter = setter;
        }

        public float Value
        {
            get => getter();
            set
            {
                if (value == getter()) return;
                setter(value);
            }
        }
    }

    public class Dropdown : UIElement
    {
        public string Title { get; private set; }
        public string[] Options { get; private set; }
        private readonly Func<int> getter;
        private readonly Action<int> setter;

        public Dropdown(string title, string[] options, Func<int> getter, Action<int> setter)
        {
            Title = title;
            Options = options;
            this.getter = getter;
            this.setter = setter;
        }

        public Dropdown(string title, List<string> options, Func<int> getter, Action<int> setter)
            : this(title, options.ToArray(), getter, setter)
        {
            // Not needed
        }

        public int Value
        {
            get => getter();
            set
            {
                if (value == getter()) return;
                setter(value);
            }
        }
    }

    // Unity specific stuff
    public class ColorField : UIElement
    {
        public string Title { get; private set; }
        private readonly Func<Color> getter;
        private readonly Action<Color> setter;

        public ColorField(string title, Func<Color> getter, Action<Color> setter)
        {
            Title = title;
            this.getter = getter;
            this.setter = setter;
        }

        public Color Value
        {
            get => getter();
            set
            {
                if (value.Equals(getter())) return;
                setter(value);
            }
        }
    }

    public class MaterialField : UIElement
    {
        public string Title { get; private set; }

        private readonly Func<Material> getter;
        private readonly Action<Material> setter;

        public MaterialField(string title, Func<Material> getter, Action<Material> setter)
        {
            Title = title;
            this.getter = getter;
            this.setter = setter;
        }

        public Material Value
        {
            get => getter();
            set
            {
                if (value == getter()) return;
                setter(value);
            }
        }
    }

    public class ScriptableObjectField<T> : GenericPersistentUI.UIElement where T : UnityEngine.Object
    {
        public string Title { get; private set; }
        private readonly Func<T> getter;
        private readonly Action<T> setter;

        public ScriptableObjectField(string title, Func<T> getter, Action<T> setter)
        {
            Title = title;
            this.getter = getter;
            this.setter = setter;
        }

        public T Value
        {
            get => getter();
            set
            {
                if (value == getter()) return;
                setter(value);
            }
        }
    }

    // Organizing
    public class DisplayIfTrue : UIElement
    {
        private readonly Func<bool> getter;
        public List<UIElement> Elements { get; private set; }

        public DisplayIfTrue(Func<bool> getter, List<UIElement> elements)
        {
            this.getter = getter;
            this.Elements = elements;
        }

        public bool ShouldDisplay
        {
            get => getter();
        }
    }

    public class HorizontalArrangement : UIElement
    {
        public List<UIElement> Elements { get; private set; }

        public HorizontalArrangement(List<UIElement> elements)
        {
            this.Elements = elements;
        }
    }

    public class Foldout : UIElement
    {
        public string Title { get; private set; }
        public List<UIElement> Elements { get; private set; }
        public bool Open;

        public Foldout (string title, List<UIElement> elements, bool openByDefault)
        {
            this.Title = title;
            this.Elements = elements;
            this.Open = openByDefault;
        }
    }

    public class TogglePanel : UIElement
    {
        public List<string> Titles { get; private set; }
        public List<List<UIElement>> ContentLists { get; private set; }

        public int? SelectedIndex { get; private set; }

        public TogglePanel(List<string> titles, List<List<UIElement>> contentLists)
        {
            if (titles == null || contentLists == null || titles.Count != contentLists.Count)
                throw new ArgumentException("TogglePanel requires equal-length title and content lists.");

            Titles = titles;
            ContentLists = contentLists;
            SelectedIndex = null; // Nothing selected by default
        }

        public void Toggle(int index)
        {
            if (SelectedIndex == index)
                SelectedIndex = null; // Deselect if already selected
            else if (index >= 0 && index < Titles.Count)
                SelectedIndex = index;
        }

        public List<UIElement> ActiveElements =>
            SelectedIndex.HasValue ? ContentLists[SelectedIndex.Value] : new List<UIElement>();
    }
}
