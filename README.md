# Plugin.Maui.MilestoneTracker
A lightweight and customizable milestone tracking UI component for .NET MAUI apps. Ideal for visualizing user progress, achievements, onboarding steps, or any sequence of tasks in a beautiful and animated path-based layout.

✨ Features
- 🔄 Dynamic progress animation along a smooth path

- 📌 Custom milestone icons (completed + default)

- ⚙️ MVVM-friendly with full data-binding support

- 🧩 Easy to integrate

- 🚀 Optimized for performance – renders only what’s needed

- 📱 Works on Android, iOS, Windows and macOS via .NET MAUI

## 📦 Installation

Install via NuGet:

<pre> dotnet add package Plugin.Maui.MilestoneTracker </pre>

Or via Visual Studio NuGet UI:

<pre> Plugin.Maui.MilestoneTracker </pre>


## 🚀 Getting Started


1. Add Namespace

<pre> xmlns:milestone="clr-namespace:Plugin.Maui.MilestoneTracker;assembly=Plugin.Maui.MilestoneTracker"
</pre>

2. Use in XAML

```
 <milestone:MilestoneTrackerView
 x:Name="MilestoneView"
 ItemsSource="{Binding Milestones}"
 PathColor="DeepSkyBlue"
 HeightRequest="150" />

```
3. Page.cs

```
protected override async void OnAppearing()
{
        MilestoneTracker.Milestones = Milestones;
        MilestoneTracker.InitializeMilestones();
}

 ```
## 🧩 Example ViewModel

```
public ObservableCollection<Milestone> Milestones { get; set; } = new()
{
    new Milestone { ImageSource = "milestone1.png", CompletedImageSource = "milestone1_done.png", IsCompleted = true },
    new Milestone { ImageSource = "milestone2.png", CompletedImageSource = "milestone2_done.png", IsCompleted = false },
    new Milestone { ImageSource = "milestone3.png", CompletedImageSource = "milestone3_done.png", IsCompleted = false }
};


```

## 🧼 Best Practices

- Use ObservableCollection<T> for dynamic updates.

- Bind directly to your ViewModel using BindingContext.

- Avoid animating the full layout repeatedly — the control is optimized to redraw only when IsCompleted changes.

## 🧪 Planned Features

- Vertical layout mode

- Horizontal scrollable milestones

- Milestone click events (e.g., show detail on tap)

- Custom path shapes (curves, waves, etc.)

- Dark mode support

## 📃 License

MIT License © 2025 Ali Raza
