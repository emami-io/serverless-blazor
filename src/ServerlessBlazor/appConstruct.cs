using Amazon.CDK;
using Amazon.CDK.AWS.CloudFront;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.Ecr.Assets;
using Amazon.CDK.AWS.ECS;
using Amazon.CDK.AWS.ElasticLoadBalancingV2;
using Amazon.CDK.AWS.IAM;
using Constructs;
using HealthCheck = Amazon.CDK.AWS.ElasticLoadBalancingV2.HealthCheck;
using Protocol = Amazon.CDK.AWS.ElasticLoadBalancingV2.Protocol;

namespace ServerlessBlazor
{
    public class AppConstruct : Constructs.Construct
    {
        public AppConstruct(Construct scope, string id) : base(scope, id)
        {

            var cluster = new Cluster(this, "BlazorAppCluster");

            var sg = new SecurityGroup(this, "BlazorAppSecurityGroup",
            new SecurityGroupProps
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


            var lb = new ApplicationLoadBalancer(this, "BlazorAppLoadBalancer", new ApplicationLoadBalancerProps
            {
                Vpc = cluster.Vpc,
                InternetFacing = true,
            });

            var lb_target_group = new ApplicationTargetGroup(this, "BlazorAppTargetGroup", new ApplicationTargetGroupProps
            {
                Port = 80,
                Vpc = cluster.Vpc,
                Protocol = ApplicationProtocol.HTTP,
                TargetType = TargetType.IP,
                StickinessCookieDuration = Duration.Days(1),
                HealthCheck = new HealthCheck
                {
                    Path = "/",
                    Protocol = Protocol.HTTP,
                    Port = "80",
                },
                Targets = new[] {
                    fargate_service.LoadBalancerTarget(new LoadBalancerTargetOptions
                    {
                        ContainerName = "BlazorAppContainer",
                        ContainerPort = 80
                    })
                }
            });


            var listener = lb.AddListener("BlazorAppListener", new BaseApplicationListenerProps
            {
                Port = 80,
                DefaultTargetGroups = new[] { lb_target_group },
                Protocol = ApplicationProtocol.HTTP,
            });
        }
    }

}