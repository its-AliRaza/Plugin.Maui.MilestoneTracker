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
                new Milestone { ImageSource = "abc", CompletedImageSource="xyz", IsCompleted = true },
                new Milestone { ImageSource = "abc", CompletedImageSource="xyz",IsCompleted = false },
                new Milestone { ImageSource = "abc", CompletedImageSource="xyz",IsCompleted = false },
                new Milestone { ImageSource = "abc", CompletedImageSource="xyz",IsCompleted = false },
                new Milestone { ImageSource = "abc", CompletedImageSource="xyz", IsCompleted = false }
            };

    public MainPage(IFeature feature)
    {
        InitializeComponent();
        this.feature = feature;

    }

    protected override async void OnAppearing()
    {
        MilestoneTracker.Milestones = Milestones;
        MilestoneTracker.InitializeMilestones();
    }

    private void Button_Clicked(object sender, EventArgs e)
    {
        Milestones[3].IsCompleted = true;
    }
}
