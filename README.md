# UINavigator Overview

**UINavigator** is a modular and extensible UI navigation system for Unity that helps you manage screens, pages, and modals efficiently.  
It supports stack-based navigation, smooth transitions, caching, and dynamic view handling—all while remaining easy to integrate and extend.

---
v1.1.0
- Add feature Bind Data UI
- Auto get Refs easy to UI

- v1.2.0
- Add OpenAddStack

## ✨ Key Features

- **Root UI Management**  
  Handles a centralized `RootUI` with canvas and camera access.

- **Dynamic View Control**
  - `Spawn<T>()`: Instantiate and optionally cache views at runtime.
  - `Open<T>()`: Open a view with optional data and hide previous UI.
  - `Hide(View)`: Hide a specific view.
  - `HideAll()`, `HideAllIgnoreView<T>()`: Close all UI views with optional exclusions.
  - `OpenAddStack<T>()`: Adds multiple specified popups to the stack, displayed one by one until the last popup is added to the stack
  - `OpenAlert<T>()`: Open an alert modal with custom setup.

- **View Caching**  
  Cache views to avoid re-instantiation and improve performance.

- **View Navigation Stack**  
  Support for `OpenPrevious()` to return to the last UI view.

- **Utility Functions**
  - `Get<T>()`, `TryOpen<T>()`, `GetOrOpen<T>()`: Flexible access to views.
  - `Delete<T>()`: Properly remove view instances.
  - `IsShowing(View)`: Check view visibility state.
  - `GetAll()`: Retrieve all active or initialized views.

---

## 🧩 RootUI Integration

`UINavigator` requires a reference to a `RootUI` object which manages the canvas and UI camera.  
If not set manually, it attempts to find it in the scene on initialization (`autoInit` enabled by default).

---

## 🧪 Example Usage

```csharp
// Open a popup view with data
UINavigator.Open<MyPopupView>(new object[] { "Hello", 123 });

// Hide a specific view
UINavigator.Hide(myPopupView);

// Open an alert
UINavigator.OpenAlert<MyAlertView>(new AlertSetup
{
    title = "Warning",
    message = "Are you sure you want to quit?"
});

// Open and add Stack
 UINavigator.OpenAddStack<Notif1UI>();
 UINavigator.OpenAddStack<Notif2UI>();
```
---

## **📞 Support**
- **Email**: gamecoding1999@gmail.com  
- **Facebook**: [OSK UINavigator](https://www.facebook.com/xOskx/)
