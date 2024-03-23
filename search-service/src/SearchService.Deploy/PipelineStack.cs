using System.Collections.Generic;
using Amazon.CDK;
using Amazon.CDK.AWS.CodeBuild;
using Amazon.CDK.AWS.CodePipeline;
using Amazon.CDK.AWS.CodePipeline.Actions;
using Amazon.CDK.AWS.IAM;
using Constructs;

namespace SearchService.Deploy;

public class PipelineStack : Stack
{
    internal PipelineStack(Construct scope, string id, Configuration configuration, IStackProps props = null)
        : base(scope, id, props)
    {
        var pipeline = new Pipeline(this, $"{configuration.Env}-Pipeline", new PipelineProps
        {
            PipelineName = $"snd-documents-search-{configuration.Env}",
        });

        var artifactSource = new Artifact_();
        pipeline.AddStage(new StageOptions
        {
            StageName = "Source",
            Actions = new IAction[]
            {
                new GitHubSourceAction(new GitHubSourceActionProps
                {
                    ActionName = "Clone",
                    Owner = "mtudorr",
                    OauthToken = SecretValue.SecretsManager("GitHub-OAuth-Token"),
                    Repo = "snd-documents-search",
                    Branch = "develop",
                    Output = artifactSource
                })
            }
        });
        pipeline.AddStage(new StageOptions
        {
            StageName = "Build",
            Actions = new IAction[]
            {
                new CodeBuildAction(new CodeBuildActionProps
                {
                    ActionName = "Build",
                    Input = artifactSource,
                    Project = new Project(this, $"{configuration.Env}-Pipeline-Project-Build", new ProjectProps
                    {
                        ProjectName = $"snd-documents-search-build-{configuration.Env}",
                        Environment = new BuildEnvironment
                        {
                            BuildImage = LinuxBuildImage.STANDARD_7_0,
                            ComputeType = ComputeType.SMALL
                        },
                        BuildSpec = BuildSpec.FromObject(new Dictionary<string, object>
                        {
                            {"version", "0.2"},
                            {
                                "phases", 
                                new Dictionary<string, object>
                                { 
                                    {
                                        "build", 
                                        new Dictionary<string, object>
                                        {
                                            { 
                                                "commands", 
                                                new[]
                                                {
                                                    "ls -la", 
                                                    "dotnet --version", 
                                                    "dotnet build ./search-service/src/SearchService.sln -c Debug"
                                                } 
                                            }
                                        }
                                    }
                                }
                            }
                        })
                    })
                })
            }
        });

        var policyStatement = new PolicyStatement(new PolicyStatementProps
        {
            Effect = Effect.ALLOW
        });
        policyStatement.AddResources("*");
        policyStatement.AddActions("*");
        var managedPolicy = new ManagedPolicy(this, $"{configuration.Env}-Deployment-Policy", new ManagedPolicyProps
        {
            ManagedPolicyName = $"deployment-policy-{configuration.Env}",
            Statements = new[]
            {
                policyStatement
            }
        });
        
        var role = new Role(this, $"{configuration.Env}-Deployment-Role", new RoleProps
        {
            RoleName = $"deployment-role-{configuration.Env}",
            AssumedBy = new ServicePrincipal("codebuild.amazonaws.com"),
            ManagedPolicies = new[]
            {
                managedPolicy
            }
        });
        
        pipeline.AddStage(new StageOptions
        {
            StageName = "Deploy",
            Actions = new IAction[]{
                new CodeBuildAction(new CodeBuildActionProps
                {
                    ActionName = "Deploy",
                    Input = artifactSource,
                    Project = new Project(this, $"{configuration.Env}-Pipeline-Project-Deploy", new ProjectProps
                    {
                        Role = role,
                        ProjectName = $"snd-documents-search-deploy-{configuration.Env}",
                        Environment = new BuildEnvironment
                        {
                            BuildImage = LinuxBuildImage.STANDARD_7_0,
                            ComputeType = ComputeType.SMALL
                        },
                        BuildSpec = BuildSpec.FromObject(new Dictionary<string, object>
                        {
                            {"version", "0.2"},
                            {
                                "phases", 
                                new Dictionary<string, object>
                                { 
                                    {
                                        "install", 
                                        new Dictionary<string, object>
                                        {
                                            { 
                                                "commands", 
                                                new[]
                                                {
                                                    "npm ci",
                                                } 
                                            }
                                        }
                                    },
                                    {
                                        "build", 
                                        new Dictionary<string, object>
                                        {
                                            {
                                                "commands", 
                                                new[]
                                                {
                                                    "ls -la", 
                                                    "dotnet --version",
                                                    "cd search-service",
                                                    "npx cdk deploy *-SearchServiceDeployStack"
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        })
                    })
                })
            }
        });
    }
}