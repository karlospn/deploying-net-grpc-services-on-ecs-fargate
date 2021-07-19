using System;
using Amazon.CDK;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.ECR;
using Amazon.CDK.AWS.ECS;
using Amazon.CDK.AWS.ElasticLoadBalancingV2;
using CdkGrpcStack.Props;
using Protocol = Amazon.CDK.AWS.EC2.Protocol;
using HealthCheck = Amazon.CDK.AWS.ElasticLoadBalancingV2.HealthCheck;


namespace CdkGrpcStack.Constructs
{
    public class EcsServiceGrpcConstruct: Construct
    {

        internal ApplicationTargetGroup TargetGroup { get; set; }
        internal FargateService FargateService { get; set; }

        public EcsServiceGrpcConstruct(Construct scope,
            string id,
            EcsServiceGrpcConstructProps props)
            : base(scope, id)
        {
            var task = CreateTaskDefinition(props);
            FargateService = CreateEcsService(props, task);
            CreateTargetGroup(props, FargateService);
        }

        private FargateTaskDefinition CreateTaskDefinition(
            EcsServiceGrpcConstructProps props)
        {

            var task = new FargateTaskDefinition(this,
                "td-app",
                new FargateTaskDefinitionProps
                {
                    Cpu = 512,
                    Family = "ecs-td-grpc-service",
                    MemoryLimitMiB = 1024
                });

            task.AddContainer("container-app-grpc-service",
                new ContainerDefinitionOptions
                {
                    Cpu = 512,
                    MemoryLimitMiB = 1024,
                    Image = ContainerImage.FromEcrRepository(Repository.FromRepositoryArn(this,
                            "ecs-container-service-image", props.EcsImageArn), "latest"),
                    Logging = LogDriver.AwsLogs(new AwsLogDriverProps
                    {
                        StreamPrefix = "ecs"
                    }),
                }).AddPortMappings(new PortMapping
            {
                ContainerPort = 80,
            });

            return task;
        }

        private FargateService CreateEcsService(EcsServiceGrpcConstructProps props,
            FargateTaskDefinition task)
        {
            var sg = new SecurityGroup(this,
                "sg-grpc-service",
                new SecurityGroupProps
                {
                    SecurityGroupName = "sec-group-pub-alb-to-ecs-grpc-service",
                    Description = "Allow traffic from ALB to gRPC service",
                    AllowAllOutbound = true,
                    Vpc = props.Vpc
                });

            sg.Connections.AllowFrom(props.Alb.Connections, new Port(new PortProps
            {
                FromPort = 80,
                ToPort = 80,
                Protocol = Protocol.TCP,
                StringRepresentation = string.Empty
            }));


            var service = new FargateService(this,
                "ecs-grpc-service",
                new FargateServiceProps
                {
                    TaskDefinition = task,
                    Cluster = props.Cluster,
                    DesiredCount = 1,
                    MinHealthyPercent = 100,
                    MaxHealthyPercent = 200,
                    AssignPublicIp = true,
                    VpcSubnets = new SubnetSelection
                    {
                        Subnets = new[]
                        {
                            Subnet.FromSubnetId(this, "subnet-id", props.SubnetId)
                        }
                    },
                    SecurityGroups = new ISecurityGroup[] { sg },
                    ServiceName = "grpc-service"    
                });
            return service;
        }

        private void CreateTargetGroup(EcsServiceGrpcConstructProps props,
            FargateService service)
        {
            TargetGroup = new ApplicationTargetGroup(this,
                "tg-grpc-service",
                new ApplicationTargetGroupProps
                {
                    TargetGroupName = "ecs-tg-grpc-service",
                    Vpc = props.Vpc,
                    TargetType = TargetType.IP,
                    ProtocolVersion = ApplicationProtocolVersion.GRPC,
                    HealthCheck = new HealthCheck
                    {
                        Protocol = Amazon.CDK.AWS.ElasticLoadBalancingV2.Protocol.HTTP,
                        HealthyThresholdCount = 3,
                        Path = "/grpc.health.v1.Health/Check",
                        Port = "80",
                        Interval = Duration.Millis(10000),
                        Timeout = Duration.Millis(8000),
                        UnhealthyThresholdCount = 10,
                        HealthyGrpcCodes = "0"
                    },
                    Port = 80,
                    Targets = new IApplicationLoadBalancerTarget[] { service }
                });

            //Path routing using packagename.**
            props.AlbListener.AddTargetGroups(
                "listener-svc-greet",
                new AddApplicationTargetGroupsProps
                {
                    TargetGroups = new IApplicationTargetGroup[] { TargetGroup },
                    Conditions = new[] { ListenerCondition.PathPatterns(new[] { $"/greet.*" })},
                    Priority = new Random().Next(1, 1000)
                });

            //Path routing using packagename.servicename/methodname*
            props.AlbListener.AddTargetGroups(
                "listener-svc-replyer",
                new AddApplicationTargetGroupsProps
                {
                    TargetGroups = new IApplicationTargetGroup[] { TargetGroup },
                    Conditions = new[] { ListenerCondition.PathPatterns(new[] { $"/reply.Replyer/SaySomething" }) },
                    Priority = new Random().Next(1, 1000)
                });

            //Path routing using packagename.servicename/**
            props.AlbListener.AddTargetGroups(
                "listener-svc-weather",
                new AddApplicationTargetGroupsProps
                {
                    TargetGroups = new IApplicationTargetGroup[] { TargetGroup },
                    Conditions = new[] { ListenerCondition.PathPatterns(new[] { $"/weather.Weather/*" }) },
                    Priority = new Random().Next(1, 1000)
                });
        }

    }
}
