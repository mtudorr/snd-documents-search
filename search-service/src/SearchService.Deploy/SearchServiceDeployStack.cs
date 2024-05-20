using System;
using System.Text;
using Amazon.CDK;
using Amazon.CDK.AWS.APIGateway;
using Amazon.CDK.AWS.Lambda;
using Constructs;
using SearchService.Deploy.LambdaHandlers;
using AssetOptions = Amazon.CDK.AWS.S3.Assets.AssetOptions;

namespace SearchService.Deploy
{
    public class SearchServiceDeployStack : Stack
    {
        internal SearchServiceDeployStack(Construct scope, string id, Configuration configuration, IStackProps props = null) : base(scope, id, props)
        {
            var buildConfiguration = "Debug";
            var code = new AssetCode(".", new AssetOptions
            {
                Bundling = new BundlingOptions
                {
                    Image = Runtime.DOTNET_6.BundlingImage,
                    OutputType = BundlingOutput.ARCHIVED,
                    Command = new []
                    {
                        "/bin/sh",
                        "-c",
                        " dotnet tool install -g Amazon.Lambda.Tools" +
                        " && dotnet lambda package --output-package /asset-output/function.zip -pl src/SearchService.Deploy.Lambda -c " + System.Environment.GetEnvironmentVariable("DOTNET_BUILD_CONFIGURATION")
                    }
                }
            });

            var lambdaSearch = new Function(this, $"{configuration.Env}-lambda-search", new FunctionProps
            {
                FunctionName = $"snd-documents-search-search-{configuration.Env}",
                Runtime = Runtime.DOTNET_6,
                Timeout = Duration.Seconds(5),
                EphemeralStorageSize = Size.Gibibytes(1),
                MemorySize = 1024,
                Code = code,
                Handler = HandlerFrom(typeof(Search), nameof(Search.ExecuteAsync))
            });

            var api = new RestApi(this, $"{configuration.Env}-api-search", new RestApiProps
            {
                RestApiName = $"snd-document-search-api-{configuration.Env}",
                DeployOptions = new StageOptions
                {
                    StageName = configuration.Env
                }
            });

            var resourceV1 = api.Root.AddResource("v1");
            var resourceV1Search = resourceV1.AddResource("search");

            var integrationOfV1Search = new LambdaIntegration(lambdaSearch);
            resourceV1Search.AddMethod("GET", integrationOfV1Search);
        }

        public string HandlerFrom(Type handlerType, string handlerMethod)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append(handlerType.Assembly.GetName().Name);
            stringBuilder.Append("::");
            stringBuilder.Append(handlerType.FullName);
            stringBuilder.Append("::");
            stringBuilder.Append(handlerMethod);
        
            return stringBuilder.ToString();
        }
    }
}
