using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AppInsights
{
    internal class Program
    {
        /// <summary>
        /// Demonstration of ASP.NET console app logging to classic AppInsights
        /// and using Az App Config
        /// </summary>
        /// <param name="args"></param>
        /// <remarks>
        ///     TODO before running:
        ///         create Application Insights resource (workspace-based) in
        ///             a default workspace
        ///         copy App Insights connection string, paste into AzAppConfig
        ///             name AppInsightsConnectionString
        ///         copy Az App Configuration connection string, paste into
        ///             either the environment variable or the user-secrets
        ///             name AzAppConnectionString (env overrides user-secret)
        ///     After running:
        ///         go to the Application Insights resource, Investigate >
        ///             Transaction search to get Event time. Message,
        ///             Exception type (could
        ///             System.Text.Json.JsonSerializer.Serialize((Exception)ex))
        ///             into Message)
        /// </remarks>
        static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder();
            string connectionString = FindAzureAppConfigurationConnectionString();

            var config = builder.AddAzureAppConfiguration(connectionString)
                .AddEnvironmentVariables()
                .Build();

            Console.WriteLine(config["TestApp:Settings:Message"] ?? "Hello world!");
            foreach (KeyValuePair<string, string> item in config.AsEnumerable())
            {
                Console.WriteLine("[{0}]={1}", item.Key, item.Value);
            }

            using var channel = new InMemoryChannel();
            try
            {
                IServiceCollection services = new ServiceCollection();
                services.Configure<TelemetryConfiguration>(c =>
                        c.TelemetryChannel = channel);
                services.AddLogging(builder =>
                {
                    builder.AddApplicationInsights(
                        configureTelemetryConfiguration: (c => c.ConnectionString = config["AppInsightsConnectionString"]),
                        configureApplicationInsightsLoggerOptions: (opt => { })
                    );
                });

                IServiceProvider serviceProvider = services.BuildServiceProvider();
                ILogger<Program> logger = serviceProvider.GetRequiredService<ILogger<Program>>();

                logger.LogInformation("Logger is working...");

                logger.LogTrace(0, "Trace entry");
                logger.LogDebug(1, "Debug entry");
                logger.LogInformation(2, "Information entry");
                logger.LogWarning(3, "Warning entry");
                logger.LogError(4, "Error entry");
                var ex = new System.IO.FileNotFoundException("Not there", "foo.bar");
                logger.LogCritical(5, ex, "Logging FileNotFoundException");
                logger.LogCritical(6, ex, "Logging FileNotFoundException (nav): {nav}", LoggingHelpers.NavigateException.Navigate(ex));
                logger.LogCritical(7, ex, "Logging FileNotFoundException (json): {json}", System.Text.Json.JsonSerializer.Serialize((Exception)ex));
                logger.LogCritical(8, ex, "Logging {exceptionType} (json): {json}",
                    ex.GetType().FullName,
                    ((string)System.Text.Json.JsonSerializer.Serialize((dynamic)ex).ToString()));
            }
            finally
            {
                channel.Flush();  //escort logging out
                Task.Delay(TimeSpan.FromSeconds(5)).Wait(); //wait required time to complete
            }
        }

        /// <summary>
        /// Locate the AzureAppConfiguration connection string in either
        /// the user-secrets or from the environment variable. This doubles
        /// on the ConfigurationRoot creation due to the rework needed to get
        /// local override.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private static string FindAzureAppConfigurationConnectionString()
        {
            var config = (new ConfigurationBuilder())
                .AddUserSecrets<Program>()
                .AddEnvironmentVariables()
                .Build();
            string? appConfigConnectionString = config["AzAppConnectionString"];
            if (string.IsNullOrWhiteSpace(appConfigConnectionString))
            {
                throw new ArgumentOutOfRangeException(nameof(appConfigConnectionString), "No ConnectionString provided");
            }
            return appConfigConnectionString;
        }
    }
}
