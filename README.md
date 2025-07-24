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

<pre> xmlns:milestone="clr-namespace:Plugin.Maui.MilestoneTracker.Journey;assembly=Plugin.Maui.MilestoneTracker"
</pre>

2. Use in XAML

```
<Border
    Padding="10"
    StrokeShape="RoundRectangle 7,7,7,7"
    VerticalOptions="Center">

    <Border.Background>
        <LinearGradientBrush StartPoint="0,0" EndPoint="1,0">
            <GradientStop Color="yourcolor" Offset="0.0" />
            <!-- 0.1 opacity -->
            <GradientStop Color="yourcolor" Offset="1.0" />
            <!-- 0.1 opacity -->
        </LinearGradientBrush>
    </Border.Background>

    <milestone:MilestoneTracker
        AnimationSpeed="Normal"
        StartAnimation="{Binding StartAnimation}"
        Milestones="{Binding Milestones}"
        Grid.Row="1"
        Grid.ColumnSpan="2"
        x:Name="milestoneView"
        PathColor="{StaticResource Primary}"
        HeightRequest="150" />
</Border>


```
3. Page.cs

```
protected override async void OnAppearing()
{
    MilestoneTracker.Milestones = Milestones;
    MilestoneTracker.StartAnimation = true;
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

## 🧪 Planned Features

- Vertical layout mode

- Horizontal scrollable milestones

- Milestone click events (e.g., show detail on tap)

- Custom path shapes (curves, waves, etc.)

- Dark mode support

## 📃 License

MIT License © 2025 Ali Raza
