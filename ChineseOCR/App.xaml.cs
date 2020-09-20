using Common.WinApi;
using Gui.Configuration;
using Gui.Services;
using Gui.ViewModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

namespace Gui
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public IServiceProvider ServiceProvider { get; private set; }

        public IConfiguration Configuration { get; private set; }

        public new MainWindow MainWindow { get; set; }

        public App()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            Configuration = builder.Build();

            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);

            ServiceProvider = serviceCollection.BuildServiceProvider();
        }

        private void OnStartup(object sender, StartupEventArgs ev)
        {
            MainWindow = ServiceProvider.GetRequiredService<MainWindow>();
            MainWindow.Show();

            var helper = new WindowInteropHelper(MainWindow);
            var source = HwndSource.FromHwnd(helper.Handle);
            Interop.SetClipboardViewer(helper.Handle);
            source.AddHook(MainWindow.WndProc);
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<MainViewModel>();
            
            services
                .AddHttpClient<OCRSpaceService>((service, opts) =>
                {
                    var options = service.GetRequiredService<IOptions<OCRSpaceServiceOptions>>();
                    opts.BaseAddress = new Uri(options.Value.Url);
                    
                    opts
                        .DefaultRequestHeaders
                        .TryAddWithoutValidation("apikey", options.Value.ApiKey);
                });

            services.AddHttpClient<HttpClientService>();
            services.AddTransient<MainWindow>();

            services.Configure<OCRSpaceServiceOptions>(opts => Configuration.GetSection(nameof(OCRSpaceServiceOptions)).Bind(opts));
        }
    }
}
