using Microsoft.Maui.Layouts;
using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;

namespace Plugin.Maui.MilestoneTracker.Journey;

public partial class MilestoneTracker : AbsoluteLayout
{
    #region Properties

    private List<(float x, float y)> pathPoints = new();
    Stopwatch pathAnimationTimer = new();
    float animationProgress = 0f;
    private readonly Dictionary<Milestone, Image> _milestoneImageMap = new();

    #endregion

    public MilestoneTracker()
    {
        InitializeComponent();
    }

    #region BindableProperties

    public static readonly BindableProperty StartAnimationProperty =
       BindableProperty.Create(
           nameof(StartAnimation),
           typeof(bool),
           typeof(MilestoneTracker),
           false,
           propertyChanged: OnStartAnimationChanged);

    public bool StartAnimation
    {
        get => (bool)GetValue(StartAnimationProperty);
        set => SetValue(StartAnimationProperty, value);
    }

    public static readonly BindableProperty MilestonesProperty = BindableProperty.Create(
      propertyName: nameof(Milestones),
      returnType: typeof(ObservableCollection<Milestone>),
      defaultValue: new ObservableCollection<Milestone>(),
      declaringType: typeof(MilestoneTracker));

    public ObservableCollection<Milestone> Milestones
    {
        get => (ObservableCollection<Milestone>)GetValue(MilestonesProperty);
        set { SetValue(MilestonesProperty, value); }
    }

    public static readonly BindableProperty AnimationSpeedProperty = BindableProperty.Create(
    propertyName: nameof(AnimationSpeed),
    returnType: typeof(AnimatedDuration),
    declaringType: typeof(MilestoneTracker),
    defaultValue: AnimatedDuration.Normal);

    public AnimatedDuration AnimationSpeed
    {
        get => (AnimatedDuration)GetValue(AnimationSpeedProperty);
        set => SetValue(AnimationSpeedProperty, value);
    }

    public static readonly BindableProperty PathColorProperty = BindableProperty.Create(
    propertyName: nameof(PathColor),
    returnType: typeof(Color),
    declaringType: typeof(MilestoneTracker),
    defaultValue: Color.FromArgb("#3498db")); // default to #3498db

    public Color PathColor
    {
        get => (Color)GetValue(PathColorProperty);
        set => SetValue(PathColorProperty, value);
    }

    #endregion

    #region Methods

    private void InitializeMilestones()
    {
        // Restart animation
        pathAnimationTimer.Reset();
        _isAnimationComplete = false;
        animationProgress = 0;
        RoadCanvas.InvalidateSurface(); // Triggers drawing
    }

    private static void OnStartAnimationChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is MilestoneTracker view &&
        newValue is bool shouldStart &&
        shouldStart &&
        oldValue is bool wasStarted &&
        !wasStarted) // Trigger only on false → true
        {
            view.InitializeMilestones();

            // Reset to allow future triggers
            view.StartAnimation = false;
        }
    }

    private bool _isAnimationComplete = false;

    private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        canvas.Clear();

        int width = e.Info.Width;
        int height = e.Info.Height;

        if (!_isAnimationComplete)
        {
            if (!pathAnimationTimer.IsRunning)
                pathAnimationTimer.Start();

            float elapsed = pathAnimationTimer.ElapsedMilliseconds;
            animationProgress = Math.Min(elapsed / GetAnimationDurationMs(), 1f);
        }

        using var pathPaint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 6,
            Color = PathColor.ToSKColor(),
            IsAntialias = true,
            StrokeCap = SKStrokeCap.Round
        };

        var path = new SKPath();
        path.MoveTo(0, height / 2f);
        pathPoints.Clear();

        for (float x = 0; x <= width * animationProgress; x += 10)
        {
            float y = (float)(height / 2 + Math.Sin(x * 0.01) * 50);
            path.LineTo(x, y);
            pathPoints.Add((x, y));
        }

        canvas.DrawPath(path, pathPaint);

        if (!_isAnimationComplete)
        {
            if (animationProgress < 1f)
            {
                ((SKCanvasView)sender).InvalidateSurface(); // keep drawing
            }
            else
            {
                _isAnimationComplete = true;
                pathAnimationTimer.Stop();
                pathAnimationTimer.Reset();

                // ✅ Only place milestones once
                if (_milestoneImageMap.Count == 0)
                    PlaceMilestoneImages(width, height);
            }
        }
    }

    private async void PlaceMilestoneImages(int canvasWidth, int canvasHeight)
    {
        UnsubscribeAllMilestoneEvents(); // Clear old events and image map

        float dipPadding = 25f;
        float density = (float)DeviceDisplay.MainDisplayInfo.Density;
        float pixelPadding = dipPadding * density;
        float drawableWidth = canvasWidth - 2 * pixelPadding;

        for (int i = 0; i < Milestones.Count; i++)
        {
            float progress = (float)i / (Milestones.Count - 1);
            float x = pixelPadding + drawableWidth * progress;
            var closest = GetClosestPathPoint(x);
            double dipX = closest.x / density;
            double dipY = closest.y / density;

            var milestone = Milestones[i];

            var image = new Image
            {
                Source = milestone.Icon,
                WidthRequest = 35,
                HeightRequest = 35,
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center,
                Opacity = 0,
                Scale = 0.5
            };

            AbsoluteLayout.SetLayoutBounds(image, new Rect(dipX - 16, dipY - 16, 32, 32));
            AbsoluteLayout.SetLayoutFlags(image, AbsoluteLayoutFlags.None);
            RootLayout.Children.Add(image);

            _milestoneImageMap[milestone] = image;
            milestone.PropertyChanged += OnMilestoneChanged;

            // Animate only on first appearance
            await Task.Delay(100);
            _ = image.FadeTo(1, 300, Easing.CubicIn);
            _ = image.ScaleTo(1, 300, Easing.BounceOut);
        }
    }

    private (float x, float y) GetClosestPathPoint(float targetX)
    {
        (float x, float y) closest = pathPoints[0];
        float minDist = Math.Abs(closest.x - targetX);

        foreach (var point in pathPoints)
        {
            float dist = Math.Abs(point.x - targetX);
            if (dist < minDist)
            {
                minDist = dist;
                closest = point;
            }
        }

        return closest;
    }

    private void UnsubscribeAllMilestoneEvents()
    {
        foreach (var kv in _milestoneImageMap)
        {
            kv.Key.PropertyChanged -= OnMilestoneChanged;
            RootLayout.Children.Remove(kv.Value); // Clean up UI too
        }

        _milestoneImageMap.Clear();
    }

    public void CleanAllEvent()
    {
        // Stop animation timer
        if (pathAnimationTimer != null && pathAnimationTimer.IsRunning)
        {
            pathAnimationTimer.Stop();
            pathAnimationTimer.Reset();
        }

        // Clean milestone event subscriptions and UI
        UnsubscribeAllMilestoneEvents();

        // Optionally remove images and clear layout
        for (int i = RootLayout.Children.Count - 1; i >= 0; i--)
        {
            if (RootLayout.Children[i] is Image)
                RootLayout.Children.RemoveAt(i);
        }

        // Reset animation flag (optional if page can reappear)
        _isAnimationComplete = false;
        animationProgress = 0;
    }


    private void OnMilestoneChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(Milestone.IsCompleted))
            return;

        if (sender is Milestone milestone &&
            _milestoneImageMap.TryGetValue(milestone, out var image))
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                // Update only image source
                image.Source = milestone.Icon;
            });
        }
    }

    private int GetAnimationDurationMs()
    {
        return AnimationSpeed switch
        {
            AnimatedDuration.Slow => 1200,
            AnimatedDuration.Normal => 800,
            AnimatedDuration.Fast => 400,
            _ => 800
        };
    }


    #endregion

    #region Models

    public class Milestone : INotifyPropertyChanged
    {
        private bool _isCompleted;

        public string Name { get; set; }                // Name of the milestone
        public string Description { get; set; }         // Description of the milestone
        public int Id { get; set; }                  // Unique identifier for the milestone
        public string ImageSource { get; set; }              // Default (not completed) image
        public string CompletedImageSource { get; set; }     // Image shown when completed

        public bool IsCompleted
        {
            get => _isCompleted;
            set
            {
                if (_isCompleted != value)
                {
                    _isCompleted = value;
                    OnPropertyChanged(nameof(IsCompleted));
                    OnPropertyChanged(nameof(Icon)); // Notify change for UI-binding (if used)
                }
            }
        }

        // Dynamically returns the correct image
        public string Icon => IsCompleted ? CompletedImageSource : ImageSource;

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }



    #endregion

    #region Enum

    public enum AnimatedDuration
    {
        Slow,
        Normal,
        Fast
    }

    #endregion
}