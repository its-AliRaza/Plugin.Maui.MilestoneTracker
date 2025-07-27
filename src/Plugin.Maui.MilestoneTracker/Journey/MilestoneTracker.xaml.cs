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
    #region Private Fields

    private readonly List<(float x, float y)> _pathPoints = new();
    private readonly Stopwatch _pathAnimationTimer = new();
    private readonly Dictionary<Milestone, Image> _milestoneImageMap = new();
    private float _animationProgress = 0f;
    private bool _isAnimationComplete = false;

    private SKPath _cachedPath;
    private float _lastAnimationProgress = -1;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the MilestoneTracker control.
    /// </summary>
    public MilestoneTracker()
    {
        InitializeComponent();
    }

    #endregion

    #region Bindable Properties

    /// <summary>
    /// Defines the type of path to draw for the milestone journey.
    /// </summary>
    public static readonly BindableProperty PathTypeProperty = BindableProperty.Create(
        propertyName: nameof(PathType),
        returnType: typeof(MilestonePathType),
        declaringType: typeof(MilestoneTracker),
        defaultValue: MilestonePathType.Wave);

    public MilestonePathType PathType
    {
        get => (MilestonePathType)GetValue(PathTypeProperty);
        set => SetValue(PathTypeProperty, value);
    }



    /// <summary>
    /// Bindable property to trigger the path animation.
    /// Set to true to start the animation, automatically resets to false after starting.
    /// </summary>
    public static readonly BindableProperty StartAnimationProperty = BindableProperty.Create(
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

    /// <summary>
    /// Collection of milestones to display along the animated path.
    /// </summary>
    public static readonly BindableProperty MilestonesProperty = BindableProperty.Create(
        propertyName: nameof(Milestones),
        returnType: typeof(ObservableCollection<Milestone>),
        defaultValue: new ObservableCollection<Milestone>(),
        declaringType: typeof(MilestoneTracker));

    public ObservableCollection<Milestone> Milestones
    {
        get => (ObservableCollection<Milestone>)GetValue(MilestonesProperty);
        set => SetValue(MilestonesProperty, value);
    }

    /// <summary>
    /// Controls the speed of the path animation.
    /// </summary>
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

    /// <summary>
    /// Defines the color of the animated path.
    /// </summary>
    public static readonly BindableProperty PathColorProperty = BindableProperty.Create(
        propertyName: nameof(PathColor),
        returnType: typeof(Color),
        declaringType: typeof(MilestoneTracker),
        defaultValue: Color.FromArgb("#3498db"));

    public Color PathColor
    {
        get => (Color)GetValue(PathColorProperty);
        set => SetValue(PathColorProperty, value);
    }


    #endregion

    #region Private Methods

    private void InitializeMilestones()
    {
        _pathAnimationTimer.Reset();
        _isAnimationComplete = false;
        _animationProgress = 0;
        RoadCanvas.InvalidateSurface();
    }

    private static void OnStartAnimationChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is not MilestoneTracker view) return;
        if (newValue is not bool shouldStart || !shouldStart) return;
        if (oldValue is not bool wasStarted || wasStarted) return;

        view.InitializeMilestones();
        view.StartAnimation = false; // Reset for future triggers
    }

    private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        canvas.Clear();

        int width = e.Info.Width;
        int height = e.Info.Height;

#if IOS || MACCATALYST
        DrawPathOnIOS(canvas, width, height);
#elif ANDROID || WINDOWS
        UpdateAnimationProgress();
        DrawAnimatedPath(canvas, width, height);
        HandleAnimationCompletion(sender, width, height);
#endif
    }

    private void UpdateAnimationProgress()
    {
        if (_isAnimationComplete) return;

        if (!_pathAnimationTimer.IsRunning)
            _pathAnimationTimer.Start();

        float elapsed = _pathAnimationTimer.ElapsedMilliseconds;
        _animationProgress = Math.Min(elapsed / GetAnimationDurationMs(), 1f);
    }

    private void DrawAnimatedPath(SKCanvas canvas, int width, int height)
    {
        // Only recalculate path if animation progress changed
        if (_lastAnimationProgress != _animationProgress)
        {
            _cachedPath?.Dispose();
            _cachedPath = new SKPath();
            _pathPoints.Clear();

            using var pathPaint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 6,
                Color = PathColor.ToSKColor(),
                IsAntialias = true,
                StrokeCap = SKStrokeCap.Round
            };

            switch (PathType)
            {
                case MilestonePathType.Horizontal:
                    _cachedPath.MoveTo(0, height / 2f);
                    for (float x = 0; x <= width * _animationProgress; x += 10)
                    {
                        _cachedPath.LineTo(x, height / 2f);
                        _pathPoints.Add((x, height / 2f));
                    }
                    break;
                case MilestonePathType.Wave:
                    _cachedPath.MoveTo(0, height / 2f);
                    for (float x = 0; x <= width * _animationProgress; x += 10)
                    {
                        float y = (float)(height / 2 + Math.Sin(x * 0.01) * 50);
                        _cachedPath.LineTo(x, y);
                        _pathPoints.Add((x, y));
                    }
                    break;
                case MilestonePathType.Diagonal:
                    _cachedPath.MoveTo(0, height);
                    for (float t = 0; t <= _animationProgress; t += 0.01f)
                    {
                        float x = width * t;
                        float y = height - (height * t);
                        _cachedPath.LineTo(x, y);
                        _pathPoints.Add((x, y));
                    }
                    break;
                case MilestonePathType.DiagonalWave:
                    // Start position adjusted to extend before first milestone
                    _cachedPath.MoveTo(-width * 0.05f, height * 0.9f);
                    float waveAmplitude = height * 0.1f;
                    float waveFrequency = 3.5f;

                    // Adjust padding to allow for path extension
                    float Dpadding = 0.05f; // 5% padding on each end
                    float effectiveWidth = width * (1 + 2 * Dpadding); // Extended width

                    for (float t = 0; t <= _animationProgress; t += 0.01f)
                    {
                        // Adjust x to include extended path
                        float x = -width * Dpadding + (effectiveWidth * t);

                        // Calculate base diagonal line with adjusted start/end points
                        float baseY = height * 0.9f - (height * 0.8f * t);

                        // Add wave pattern
                        float wave = waveAmplitude * (float)Math.Sin(t * Math.PI * 2 * waveFrequency);
                        float amplitudeFactor = (float)(Math.Sin(t * Math.PI));
                        wave *= amplitudeFactor;

                        float y = Math.Max(height * 0.1f, Math.Min(height * 0.9f, baseY + wave));

                        // Only add points within the visible canvas area
                        if (x >= -width * 0.05f && x <= width * 1.05f)
                        {
                            _cachedPath.LineTo(x, y);
                            _pathPoints.Add((x, y));
                        }
                    }

                    // Ensure the path extends beyond the last milestone
                    if (_animationProgress >= 1f)
                    {
                        _cachedPath.LineTo(width * 1.05f, height * 0.1f);
                        _pathPoints.Add((width * 1.05f, height * 0.1f));
                    }
                    break;
                case MilestonePathType.ZigZag:
                    _cachedPath.MoveTo(0, height / 2f);
                    float zAmplitude = 40f;
                    float period = 60f;
                    for (float x = 0; x <= width * _animationProgress; x += 10)
                    {
                        float y = (float)(height / 2 + zAmplitude * Math.Sign(Math.Sin(x / period)));
                        _cachedPath.LineTo(x, y);
                        _pathPoints.Add((x, y));
                    }
                    break;
            }

            _lastAnimationProgress = _animationProgress;
        }

        // Always draw the cached path
        using var paint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 6,
            Color = PathColor.ToSKColor(),
            IsAntialias = true,
            StrokeCap = SKStrokeCap.Round
        };

        canvas.DrawPath(_cachedPath, paint);
    }

    private void DrawPathOnIOS(SKCanvas canvas, int width, int height)
    {

        _cachedPath?.Dispose();
        _cachedPath = new SKPath();
        _pathPoints.Clear();

        using var pathPaint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 6,
            Color = PathColor.ToSKColor(),
            IsAntialias = true,
            StrokeCap = SKStrokeCap.Round
        };


        switch (PathType)
        {
            case MilestonePathType.Horizontal:
                _cachedPath.MoveTo(0, height / 2f);
                for (float x = 0; x <= width; x += 10)
                {
                    _cachedPath.LineTo(x, height / 2f);
                    _pathPoints.Add((x, height / 2f));
                }
                break;

            case MilestonePathType.Wave:
                _cachedPath.MoveTo(0, height / 2f);
                for (float x = 0; x <= width; x += 10)
                {
                    float y = (float)(height / 2 + Math.Sin(x * 0.01) * 50);
                    _cachedPath.LineTo(x, y);
                    _pathPoints.Add((x, y));
                }
                break;

            case MilestonePathType.Diagonal:
                _cachedPath.MoveTo(0, height);
                for (float t = 0; t <= 1f; t += 0.01f)
                {
                    float x = width * t;
                    float y = height - (height * t);
                    _cachedPath.LineTo(x, y);
                    _pathPoints.Add((x, y));
                }
                break;

            case MilestonePathType.DiagonalWave:
                _cachedPath.MoveTo(-width * 0.05f, height * 0.9f);
                float waveAmplitude = height * 0.1f;
                float waveFrequency = 3.5f;
                float Dpadding = 0.05f;
                float effectiveWidth = width * (1 + 2 * Dpadding);

                for (float t = 0; t <= 1f; t += 0.01f)
                {
                    float x = -width * Dpadding + (effectiveWidth * t);
                    float baseY = height * 0.9f - (height * 0.8f * t);

                    float wave = waveAmplitude * (float)Math.Sin(t * Math.PI * 2 * waveFrequency);
                    wave *= (float)(Math.Sin(t * Math.PI));

                    float y = Math.Max(height * 0.1f, Math.Min(height * 0.9f, baseY + wave));

                    if (x >= -width * 0.05f && x <= width * 1.05f)
                    {
                        _cachedPath.LineTo(x, y);
                        _pathPoints.Add((x, y));
                    }
                }

                _cachedPath.LineTo(width * 1.05f, height * 0.1f);
                _pathPoints.Add((width * 1.05f, height * 0.1f));
                break;

            case MilestonePathType.ZigZag:
                _cachedPath.MoveTo(0, height / 2f);
                float zAmplitude = 40f;
                float period = 60f;
                for (float x = 0; x <= width; x += 10)
                {
                    float y = (float)(height / 2 + zAmplitude * Math.Sign(Math.Sin(x / period)));
                    _cachedPath.LineTo(x, y);
                    _pathPoints.Add((x, y));
                }
                break;
        }

        // Always draw the cached path
        using var paint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 6,
            Color = PathColor.ToSKColor(),
            IsAntialias = true,
            StrokeCap = SKStrokeCap.Round
        };

        canvas.DrawPath(_cachedPath, paint);
        if (_milestoneImageMap.Count == 0)
            PlaceMilestoneImages(width, height);
    }
    private void HandleAnimationCompletion(object sender, int width, int height)
    {
        if (_isAnimationComplete) return;

        if (_animationProgress < 1f)
        {
            ((SKCanvasView)sender).InvalidateSurface();
        }
        else
        {
            _isAnimationComplete = true;
            _pathAnimationTimer.Stop();
            _pathAnimationTimer.Reset();

            if (_milestoneImageMap.Count == 0)
                PlaceMilestoneImages(width, height);
        }
    }

    private async Task PlaceMilestoneImages(int canvasWidth, int canvasHeight)
    {
        UnsubscribeAllMilestoneEvents();

        if (Milestones.Count == 0) return;

        float dipPadding = 25f;
        float density = (float)DeviceDisplay.MainDisplayInfo.Density;
        float pixelPadding = dipPadding * density;
        float drawableWidth = canvasWidth - 2 * pixelPadding;

        var animationTasks = new List<Task>();

        for (int i = 0; i < Milestones.Count; i++)
        {
            float progress = Milestones.Count > 1 ? (float)i / (Milestones.Count - 1) : 0.5f;
            float x = pixelPadding + drawableWidth * progress;
            var closest = GetClosestPathPoint(x);
            double dipX = closest.x / density;
            double dipY = closest.y / density;

            var milestone = Milestones[i];
            var image = CreateMilestoneImage(milestone);

            AbsoluteLayout.SetLayoutBounds(image, new Rect(dipX - 16, dipY - 16, 32, 32));
            AbsoluteLayout.SetLayoutFlags(image, AbsoluteLayoutFlags.None);
            RootLayout.Children.Add(image);

            _milestoneImageMap[milestone] = image;
            milestone.PropertyChanged += OnMilestoneChanged;

            await Task.Delay(100);
            animationTasks.Add(image.FadeTo(1, 300, Easing.CubicIn));
            animationTasks.Add(image.ScaleTo(1, 300, Easing.BounceOut));
        }

        await Task.WhenAll(animationTasks);
    }

    private static Image CreateMilestoneImage(Milestone milestone)
    {
        return new Image
        {
            Source = milestone.Icon,
            WidthRequest = 35,
            HeightRequest = 35,
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Center,
            Opacity = 0,
            Scale = 0.5
        };
    }

    private (float x, float y) GetClosestPathPoint(float targetX)
    {
        // Original logic for other path types
        (float x, float y) closest = _pathPoints[0];
        float minDist = Math.Abs(closest.x - targetX);

        foreach (var point in _pathPoints)
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

    #region Public Methods

    /// <summary>
    /// Cleans up all event handlers and resets the control state.
    /// Call this method when the control is being removed from the visual tree.
    /// </summary>
    public void CleanAllEvent()
    {
        if (_pathAnimationTimer.IsRunning)
        {
            _pathAnimationTimer.Stop();
            _pathAnimationTimer.Reset();
        }

        // Clean up SKPath
        _cachedPath?.Dispose();
        _cachedPath = null;

        // Clear path points
        _pathPoints.Clear();

        // Clean up images
        foreach (var image in _milestoneImageMap.Values)
        {
            image.Handler?.DisconnectHandler();
        }

        UnsubscribeAllMilestoneEvents();

        // Remove all images from the layout
        for (int i = RootLayout.Children.Count - 1; i >= 0; i--)
        {
            if (RootLayout.Children[i] is Image)
                RootLayout.Children.RemoveAt(i);
        }

        _milestoneImageMap.Clear();
        _isAnimationComplete = false;
        _animationProgress = 0;
        _lastAnimationProgress = -1;
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

    /// <summary>
    /// Animation speed options.
    /// </summary>
    public enum AnimatedDuration
    {
        Slow,
        Normal,
        Fast
    }

    /// <summary>
    /// Path style options for the milestone journey.
    /// </summary>
    public enum MilestonePathType
    {
        Horizontal,      // Straight horizontal line
        Wave,            // Sinusoidal wave from left to right
        Diagonal,        // Diagonal from bottom left to top right
        DiagonalWave,        // Diagonal from bottom left to top right
        ZigZag           // Zig-zag pattern
    }

    #endregion
}