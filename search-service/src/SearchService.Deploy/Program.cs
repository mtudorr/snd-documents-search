using Amazon.CDK;

namespace SearchService.Deploy
{
    sealed class Program
    {
        public static void Main(string[] args)
        {
            var app = new App();
            var configuration = new Configuration(app.Node);
            
            new SearchServiceDeployStack(app, $"{configuration.Env}-SearchServiceDeployStack", configuration, new StackProps
            {
                StackName = $"{configuration.Env}-search-service-deploy-stack"
            });
            
            app.Synth();
        }
    }
}
