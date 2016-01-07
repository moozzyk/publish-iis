using System;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Dnx.Runtime.Common.CommandLine;

namespace Microsoft.AspNet.Tools.PublishIIS
{
    public class Program
    {
        public static int Main(string[] args)
        {
            var app = new CommandLineApplication
            {
                Name = "dotnet(????) publish-iis",
                FullName = "Asp.Net IIS Publisher",
                Description = "IIS Publisher for the Asp.Net web applications",
            };
            app.HelpOption("-h|--help");

            var publishFolderOption = app.Option("--publish-folder", "The path to the publish output folder", CommandOptionType.SingleValue);
            var appNameOption = app.Option("--application-name", "The name of the application", CommandOptionType.SingleValue);

            app.OnExecute(() =>
            {
                var publishFolder = publishFolderOption.Value();
                var appName = appNameOption.Value();

                if (publishFolder == null || appName == null)
                {
                    app.ShowHelp();
                    return 2;
                }

                XDocument webConfigXml = null;
                var webConfigPath = Path.Combine(publishFolder, "wwwroot", "web.config");
                if (File.Exists(webConfigPath))
                {
                    try
                    {
                        webConfigXml = XDocument.Load(webConfigPath);
                    }
                    catch (XmlException) { }
                }

                var transformedConfig = WebConfigTransform.Transform(webConfigXml, appName);

                using (var f = new FileStream(webConfigPath, FileMode.Create))
                {
                    transformedConfig.Save(f);
                }

                return 0;
            });

            try
            {
                return app.Execute(args);
            }
            catch (Exception e)
            {
#if DEBUG
                Console.Error.WriteLine(e);
#else
                Console.Error.WriteLine(ex.Message);
#endif
            }

            return 1;

            /*
                var framework = app.Option("-f|--framework <FRAMEWORK>", "Target framework to compile for", CommandOptionType.SingleValue);
                var runtime = app.Option("-r|--runtime <RUNTIME_IDENTIFIER>", "Target runtime to publish for", CommandOptionType.SingleValue);
                var output = app.Option("-o|--output <OUTPUT_PATH>", "Path in which to publish the app", CommandOptionType.SingleValue);
                var configuration = app.Option("-c|--configuration <CONFIGURATION>", "Configuration under which to build", CommandOptionType.SingleValue);
                var projectPath = app.Argument("<PROJECT>", "The project to publish, defaults to the current directory. Can be a path to a project.json or a project directory");
                var nativeSubdirectories = app.Option("--native-subdirectory", "Temporary mechanism to include subdirectories from native assets of dependency packages in output", CommandOptionType.NoValue);

                app.OnExecute(() =>
                {
                    var publish = new PublishCommand();

                    publish.Framework = framework.Value();
                    // TODO: Remove default once xplat publish is enabled.
                    publish.Runtime = runtime.Value() ?? RuntimeIdentifier.Current;
                    publish.OutputPath = output.Value();
                    publish.Configuration = configuration.Value() ?? Constants.DefaultConfiguration;
                    publish.NativeSubdirectories = nativeSubdirectories.HasValue();

                    publish.ProjectPath = projectPath.Value;
                    if (string.IsNullOrEmpty(publish.ProjectPath))
                    {
                        publish.ProjectPath = Directory.GetCurrentDirectory();
                    }

                    if (!publish.TryPrepareForPublish())
                    {
                        return 1;
                    }

                    publish.PublishAllProjects();
                    Reporter.Output.WriteLine($"Published {publish.NumberOfPublishedProjects}/{publish.NumberOfProjects} projects successfully");
                    return (publish.NumberOfPublishedProjects == publish.NumberOfProjects) ? 0 : 1;
                });

                try
                {
                    return app.Execute(args);
                }
                catch (Exception ex)
                {
    #if DEBUG
                    Console.Error.WriteLine(ex);
    #else
                    Console.Error.WriteLine(ex.Message);
    #endif
                }
                        */
        }
    }
}
