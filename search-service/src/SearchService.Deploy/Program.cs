using Amazon.CDK;

namespace SearchService.Deploy
{
    sealed class Program
    {
        public static void Main(string[] args)
        {
            var configuration = new Configuration();
            var app = new App();
            
            new SearchServiceDeployStack(app, $"{configuration.Env}-SearchServiceDeployStack", configuration, new StackProps
            {
                StackName = $"{configuration.Env}-search-service-deploy-stack"
            });

            new PipelineStack(app, $"{configuration.Env}-PipelineStack", configuration, new StackProps
            {
                StackName = $"{configuration.Env}-pipeline-stack"
            });
            
            app.Synth();
        }
    }
}
