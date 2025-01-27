using Jodit.Models;

namespace BlazorJoditEditor;

public class ViewOptions
{
    // ILanguageOptions properties
    public string Language { get; set; }
    public bool? DebugLanguage { get; set; }
    public Dictionary<string, Dictionary<string, string>> I18n { get; set; }

    // IToolbarOptions properties
    public object Toolbar { get; set; } // Can be bool, string, or HTMLElement
    public string Theme { get; set; }
    public ButtonSize? ToolbarButtonSize { get; set; }
    public object TextIcons { get; set; } // Can be bool or Func<string, bool>
    public IList<object> ExtraButtons { get; set; }
    public IList<string> RemoveButtons { get; set; }
    public Dictionary<string, string> ExtraIcons { get; set; }
    public object Buttons { get; set; } // ButtonsOption type
    public bool? ShowTooltip { get; set; }
    public int? ShowTooltipDelay { get; set; }
    public bool? UseNativeTooltip { get; set; }
    public string Direction { get; set; }

    // IViewOptions specific properties
    public bool? Cache { get; set; }
    public Func<string, string, string> GetIcon { get; set; }
    public object HeaderButtons { get; set; } // String or Array of controls
    public string BasePath { get; set; }
    public int? DefaultTimeout { get; set; }
    public bool? Disabled { get; set; }
    public bool? ReadOnly { get; set; }
    public bool? Iframe { get; set; }
    public string Namespace { get; set; }
    public IList<string> ActiveButtonsInReadOnly { get; set; }
    public bool? AllowTabNavigation { get; set; }
    public object ZIndex { get; set; } // Can be number or string
    public bool? Fullsize { get; set; }
    public bool? GlobalFullSize { get; set; }
    public Dictionary<string, ControlType> Controls { get; set; }
    public Dictionary<string, object> CreateAttributes { get; set; } // Can be Attributes or Action<HTMLElement>
    public Dictionary<string, Delegate> Events { get; set; }
    public object ShadowRoot { get; set; }
    public object OwnerWindow { get; set; }
}