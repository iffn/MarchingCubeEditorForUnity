using System;
using System.Collections;
using System.Collections.Generic;

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

    public delegate void BooleanFunction(bool value);

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

    public class Slider : UIElement
    {
        public string Title { get; private set; }
        public float Value { get; private set; }

        public Slider(string title, float defaultValue)
        {
            this.Title = title;
            this.Value = defaultValue;
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
}
