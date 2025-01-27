using System;
using System.Collections.Generic;

namespace Jodit.Models
{
    public enum ButtonVariant
    {
        Initial,
        Outline,
        Default,
        Primary,
        Secondary,
        Success,
        Danger
    }

    public enum ButtonSize
    {
        Tiny,
        XSmall,
        Small,
        Middle,
        Large
    }

    public enum ButtonGroup
    {
        Source,
        FontStyle,
        Script,
        List,
        Indent,
        Font,
        Color,
        Media,
        State,
        Clipboard,
        Insert,
        History,
        Search,
        Other,
        Info
    }

    public class ControlType
    {
        public string Name { get; set; }
        public string Text { get; set; }
        public Dictionary<string, object> Data { get; set; }
        public bool IsInput { get; set; }
        public string Component { get; set; }
        // Additional properties from IControlType can be added as needed
    }

    public class ButtonsGroup
    {
        public ButtonGroup Group { get; set; }
        public IList<object> Buttons { get; set; } // Can be string or ControlType
    }

    public class Attributes
    {
        public Dictionary<string, string> Properties { get; set; }
    }
}