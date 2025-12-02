using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Secs4Net;
using SECSClient.Config; // IniConfigLoader
using SECSClient.Contracts;
using SECSClient.Equipment;
using SECSClient.Equipment.GPIO;
using SECSClient.Equipment.Interface;
using SECSClient.Equipment.Modbus;
using SECSClient.Equipment.Socket;
using SECSClient.Logging;
using SECSClient.MediatRMessages;
using SECSClient.SECS.SECS2Handler;
using SECSClient.Services;
using System.IO;
using System.Windows;
using Scrutor;

namespace SECSClient
{
    public partial class App : Application
    {
        public static IHost AppHost { get; private set; } = null!;

        public App()
        {
            AppHost = Host.CreateDefaultBuilder()
                .ConfigureServices((ctx, services) =>
                {
                    // 1. 註冊 LogBuffer 基礎元件
                    services.AddSingleton<LogBuffer>();

                    // 2. 讀取 config.ini 或 appsettings.json
                    SecsGemOptions gemOptions;
                    var iniPath = Path.Combine(AppContext.BaseDirectory, "config.ini");
                    if (File.Exists(iniPath))
                        gemOptions = IniConfigLoader.LoadSecsGemOptions(iniPath);
                    else
                        gemOptions = ctx.Configuration.GetSection("secs4net").Get<SecsGemOptions>() ?? new SecsGemOptions();

                    services.AddSingleton<IOptions<SecsGemOptions>>(Options.Create(gemOptions));

                    // 3. Logger + Secs4Net
                    services.AddSingleton<ISecsGemLogger>(sp => new GuiSecsLogger(sp.GetRequiredService<LogBuffer>()));
                    services.AddSingleton<ISecsConnection, HsmsConnection>();
                    services.AddSingleton<ISecsGem, SecsGem>();

                    // 4. Controller 註冊
                    services.AddSingleton<SecsClientController>();
                    services.AddSingleton<IHostedService>(sp => sp.GetRequiredService<SecsClientController>());

                    services.AddSingleton<IPLCController, PLCController_Mitsubishi>();
                    services.AddSingleton<IHostedService>(sp => (PLCController_Mitsubishi)sp.GetRequiredService<IPLCController>());
                    services.AddSingleton<ISocketController, SocketController>();
                    services.AddSingleton<IHostedService>(sp => (SocketController)sp.GetRequiredService<ISocketController>());
                    services.AddSingleton<IModbusController, ModbusController>();
                    services.AddSingleton<IHostedService>(sp => (ModbusController)sp.GetRequiredService<IModbusController>());
                    services.AddSingleton<IGPIOController, GPIOController_FT232H>();
                    services.AddSingleton<IHostedService>(sp => (GPIOController_FT232H)sp.GetRequiredService<IGPIOController>());

                    // 5. 唯一的 ControllerActionRequest Handler → Dispatcher(可選，因為下面的Assembly Scan會將掃到的IRequestHandler<,>再註冊一次)
                    services.AddSingleton<IRequestHandler<MRRequestrMessage, bool>, EquipControllerDispatcher>();

                    // 6. Handler
                    // 原本一個一個單獨註冊
                    //services.AddSingleton<ISecsStreamHandler, S1InHandler>();
                    //services.AddSingleton<ISecsStreamHandler, S2InHandler>();
                    //services.AddSingleton<ISecsStreamHandler, S5InHandler>();

                    //services.AddSingleton<IRequestHandler<S6F11OutCommand>, S6OutHandler>();
                    //services.AddSingleton<IRequestHandler<S5F1OutCommand>, S5OutHandler>();

                    // 改用Scrutor的Assembly Scan自動註冊ISecsStreamHandler和IRequestHandler類別
                    services.Scan(scan => scan
                        .FromAssemblyOf<ISecsStreamHandler>()
                        .AddClasses(classes => classes.AssignableTo<ISecsStreamHandler>())
                        .AsImplementedInterfaces()
                        .WithSingletonLifetime()
                        .AddClasses(classes => classes.AssignableTo(typeof(IRequestHandler<,>)))
                        .AsImplementedInterfaces()
                        .WithSingletonLifetime());



                    // 7. 保留你原本的 MediatR 註冊（供其他 Handler/Notification）
                    services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(App).Assembly));

                    // 8. LogCleanupService
                    services.AddHostedService<LogCleanupController>();
                })
                .Build();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            // 檢查 OS 版本
            if (Environment.OSVersion.Version < new Version(10, 0, 14393)) // Windows 10 1607
            {
                MessageBox.Show("此應用程式需要 Windows 10 1607 或更新版本。", "系統不支援", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown(); // 安全結束應用程式
                return;
            }

            await AppHost.StartAsync();
            base.OnStartup(e);
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            await AppHost.StopAsync();
            var logBuffer = AppHost.Services.GetService<LogBuffer>();
            logBuffer?.Dispose();
            AppHost.Dispose();
            base.OnExit(e);
        }
    }
}