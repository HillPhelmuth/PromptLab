# Jodit Editor Integration in Blazor

## Prerequisites

1. Ensure you have a Blazor WebAssembly or Blazor Server project
2. Install Jodit via npm or include via CDN

## Installation Steps

### 1. Install NPM Packages
```bash
npm install jodit
```

### 2. Configure bundling (webpack/vite)
Ensure Jodit is properly imported in your JavaScript bundling process.

### 3. Add to _Host.cshtml or index.html
```html
<link href="_content/jodit/jodit.min.css" rel="stylesheet" />
<script src="_content/jodit/jodit.min.js"></script>
```

### 4. Component Implementation

#### JoditEditor.razor
- Handles Jodit editor initialization
- Provides methods to get/set content
- Supports content change events

#### Usage in Razor Page
```csharp
<JoditEditor @ref="joditEditor" 
             InitialContent="@initialContent" 
             ContentChanged="HandleContentChanged"/>
```

## Key Features
- Dynamic editor initialization
- Content get/set methods
- Change event handling
- Proper JavaScript interop
- Cleanup on component disposal

## Configuration
Customize Jodit options in `initializeJoditEditor` function:
```javascript
const editor = Jodit.make(element, {
    preset: 'full',
    height: 400,
    buttons: [/* custom buttons */]
})
```

## Dependency Injection
- Uses `IJSRuntime` for JavaScript interoperability
- Supports dynamic content management

## Notes
- Requires modern browser support
- Performance may vary with large content
- Customize error handling as needed
