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
                new Milestone { ImageSource = "silver_medal", CompletedImageSource="ui_cup", IsCompleted = true },
                new Milestone { ImageSource = "silver_medal", CompletedImageSource="ui_cup",IsCompleted = false },
                new Milestone { ImageSource = "silver_medal", CompletedImageSource="ui_cup",IsCompleted = false },
                new Milestone { ImageSource = "silver_medal", CompletedImageSource="ui_cup",IsCompleted = false },
                new Milestone { ImageSource = "asilver_medalbc", CompletedImageSource="ui_cup", IsCompleted = false }
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
