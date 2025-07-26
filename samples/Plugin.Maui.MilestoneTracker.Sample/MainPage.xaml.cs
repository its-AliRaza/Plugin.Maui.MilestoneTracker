using Plugin.Maui.MilestoneTracker;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using static Plugin.Maui.MilestoneTracker.Journey.MilestoneTracker;

namespace Plugin.Maui.MilestoneTracker.Sample;

public partial class MainPage : ContentPage
{
    readonly IFeature feature;
    public ObservableCollection<Milestone> Milestones { get; } = new()
    {
        new Milestone
        {
            Name = "Sunrise Explorer",
            Description = "Begin your journey with the first light.",
            Id = 1,
            ImageSource = "yourimg", // gray lock
            CompletedImageSource = "yourimg", // green tick
            IsCompleted = true
        },
        new Milestone
        {
            Name = "Mountain Climber",
            Description = "Reach the peak of your first challenge.",
            Id = 2,
            ImageSource = "yourimg", // gray lock
            CompletedImageSource = "yourimg", // green tick
            IsCompleted = false
        },
        new Milestone
        {
            Name = "Forest Guardian",
            Description = "Navigate through the lush green woods.",
            Id = 3,
            ImageSource = "yourimg", // gray lock
            CompletedImageSource = "yourimg", // green tick
            IsCompleted = false
        },
        new Milestone
        {
            Name = "Starlit Voyager",
            Description = "Travel under a sky full of stars.",
            Id = 4,
            ImageSource = "yourimg", // gray lock
            CompletedImageSource = "yourimg", // green tick
            IsCompleted = false
        },
        new Milestone
        {
            Name = "Champion's Cup",
            Description = "Achieve the ultimate milestone.",
            Id = 5,
            ImageSource = "yourimg", // gray cup
            CompletedImageSource = "yourimg", // gold cup
            IsCompleted = false
        }
    };

    public MainPage(IFeature feature)
    {
        InitializeComponent();
        this.feature = feature;

    }

    protected override async void OnAppearing()
    {
        MilestoneTracker.Milestones = Milestones;
        MilestoneTracker.StartAnimation = true;
    }

    private void Button_Clicked(object sender, EventArgs e)
    {
        Milestones[3].IsCompleted = true;
    }
}
