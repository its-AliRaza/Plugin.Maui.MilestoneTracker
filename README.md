# Plugin.Maui.MilestoneTracker
A lightweight and customizable milestone tracking UI component for .NET MAUI apps. Ideal for visualizing user progress, achievements, onboarding steps, or any sequence of tasks in a beautiful and animated path-based layout.

## 📖 Milestone Tracker Guide

For a complete step-by-step installation and usage guide, visit:  
[Milestone Tracker Guide](https://mauiwithali.hashnode.dev/installing-and-using-pluginmauimilestonetracker)

---

### Preview

![Preview](https://cdn.hashnode.com/res/hashnode/image/upload/v1753527768729/aec3c9b6-e0cb-465c-a0c7-b47bc0d33c4d.png?w=1600&h=840&fit=crop&crop=entropy&auto=compress,format&format=webp)

✨ Features

- 🔄 Dynamic progress animation along a smooth paths

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
        PathType="Wave" 
        StartAnimation="{Binding StartAnimation}"
        Milestones="{Binding Milestones}"
        IndicatorImage="anyimg" // Animated indicator image - Optional
        x:Name="milestoneView"
        PathColor="{StaticResource Primary}"
        HeightRequest="150" />
</Border>

PathType = 
       Horizontal,      // Straight horizontal line
       Wave,            // Sinusoidal wave from left to right
       Diagonal,        // Diagonal from bottom left to top right
       DiagonalWave,    // Diagonal Wave from bottom left to top right
       ZigZag           // Zig-zag pattern

```
3. Page.cs

```
protected override async void OnAppearing()
{
    milestoneView.Milestones = Milestones;
    milestoneView.StartAnimation = true;
}

private void Button_Clicked(object sender, EventArgs e)
{
    milestoneView[3].IsCompleted = true;
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

- Dark mode support

## 📃 License

MIT License © 2025 Ali Raza
