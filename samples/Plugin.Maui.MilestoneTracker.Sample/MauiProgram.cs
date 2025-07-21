using Microsoft.Extensions.DependencyInjection;
using Plugin.Maui.MilestoneTracker;
using SkiaSharp.Views.Maui.Controls.Hosting;

namespace Plugin.Maui.MilestoneTracker.Sample;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.UseSkiaSharp()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		builder.Services.AddTransient<MainPage>();
		builder.Services.AddSingleton<IFeature>(Feature.Default);

		return builder.Build();
	}
}