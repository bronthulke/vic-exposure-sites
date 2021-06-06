using AWD.VicExposureSites;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(Startup))]
namespace AWD.VicExposureSites
{
   public class Startup : FunctionsStartup
   {      
    public override void Configure(IFunctionsHostBuilder builder)
      {
         builder.Services.AddTransient<IExposureDataService, ExposureDataService>();
         // or one of the options below
         // builder.Services.AddScoped<IRepository, Repository>();
         // builder.Services.AddSingleton<IRepository, Repository>();
         builder.Services.AddLogging();
      }
   }
}
