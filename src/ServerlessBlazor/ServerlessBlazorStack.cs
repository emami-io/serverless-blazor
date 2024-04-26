using Amazon.CDK;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.Ecr.Assets;
using Amazon.CDK.AWS.ECS;
using Amazon.CDK.AWS.IAM;
using Constructs;

namespace ServerlessBlazor
{
    public class ServerlessBlazorStack : Stack
    {
        internal ServerlessBlazorStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
        {
            // The code that defines your stack goes here

            var cluster = new Cluster(this, "BlazorAppCluster");

            var sg = new SecurityGroup(this, "BlazorAppSecurityGroup", new SecurityGroupProps
            {
                Vpc = cluster.Vpc
            });

            sg.AddIngressRule(Peer.AnyIpv4(), Port.Tcp(80));

            var exect_role = new Role(this, "BlazorAppExecutionRole", new RoleProps
            {
                AssumedBy = new ServicePrincipal("ecs-tasks.amazonaws.com"),
                ManagedPolicies = new[]
                {
                    ManagedPolicy.FromAwsManagedPolicyName("service-role/AmazonECSTaskExecutionRolePolicy")
                }
            });

            var task_role = new Role(this, "BlazorAppTaskRole", new RoleProps
            {
                AssumedBy = new ServicePrincipal("ecs-tasks.amazonaws.com"),
            });


            var task_definition = new FargateTaskDefinition(this, "BlazorAppTaskDefinition", new FargateTaskDefinitionProps
            {
                Cpu = 256,
                MemoryLimitMiB = 512,
                ExecutionRole = exect_role,
                TaskRole = task_role,
            });


            task_definition.AddContainer("BlazorAppContainer", new ContainerDefinitionOptions
            {
                ContainerName = "BlazorAppContainer",
                Cpu = 256,
                MemoryLimitMiB = 512,
                Image = ContainerImage.FromDockerImageAsset(new DockerImageAsset(this, "BlazorAppImage", new DockerImageAssetProps
                {
                    Directory = "blazorApp",
                    File = "Dockerfile",
                    Platform = Platform_.LINUX_AMD64
                })),
                PortMappings = new[]
                {
                    new PortMapping
                    {
                        ContainerPort = 80,
                        HostPort = 80
                    }
                }
            });

            var fargate_service = new FargateService(this, "BlazorAppService", new FargateServiceProps
            {
                Cluster = cluster,
                TaskDefinition = task_definition,
                SecurityGroups = new[] { sg },
                DesiredCount = 1,
                
            });
        }
    }
}
