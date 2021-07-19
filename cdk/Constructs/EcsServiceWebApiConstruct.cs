using System;
using Amazon.CDK;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.ECR;
using Amazon.CDK.AWS.ECS;
using Amazon.CDK.AWS.ElasticLoadBalancingV2;
using Amazon.CDK.AWS.Logs;
using CdkGrpcStack.Props;
using Protocol = Amazon.CDK.AWS.EC2.Protocol;
using HealthCheck = Amazon.CDK.AWS.ElasticLoadBalancingV2.HealthCheck;


namespace CdkGrpcStack.Constructs
{
    public class EcsServiceWebApiConstruct: Construct
    {
        internal ApplicationTargetGroup TargetGroup { get; set; }
        internal FargateService FargateService { get; set; }

        public EcsServiceWebApiConstruct(Construct scope,
            string id,
            EcsServiceGrpcConstructProps props)
            : base(scope, id)
        {
            var task = CreateTaskDefinition(props);
            FargateService = CreateEcsService(props, task);
            CreateGrpcTargetGroup(props, FargateService);
            CreateHttpTargetGroup(props, FargateService);
        }

        private FargateTaskDefinition CreateTaskDefinition(
            EcsServiceGrpcConstructProps props)
        {

            var task = new FargateTaskDefinition(this,
                "td-api-app",
                new FargateTaskDefinitionProps
                {
                    Cpu = 512,
                    Family = "ecs-td-grpc-api",
                    MemoryLimitMiB = 1024
                });

            task.AddContainer("container-app-grpc-api",
                new ContainerDefinitionOptions
                {
                    ContainerName = "grpc-webapi",
                    Cpu = 512,
                    MemoryLimitMiB = 1024,
                    Image = ContainerImage.FromEcrRepository(Repository.FromRepositoryArn(this,
                            "ecs-container-api-image", props.EcsImageArn), "latest"),
                    Logging = LogDriver.AwsLogs(new AwsLogDriverProps
                    {
                        StreamPrefix = "ecs"
                    }),
                }).AddPortMappings(new PortMapping
            {
                ContainerPort = 5001,
                HostPort = 5001
            }, new PortMapping
            {
                ContainerPort = 5002,
                HostPort = 5002
            });

            return task;
        }

        private FargateService CreateEcsService(EcsServiceGrpcConstructProps props,
            FargateTaskDefinition task)
        {
            var sg = new SecurityGroup(this,
                "sg-grpc-api",
                new SecurityGroupProps
                {
                    SecurityGroupName = "sec-group-pub-alb-to-ecs-grpc-api",
                    Description = "Allow traffic from ALB to gRPC api",
                    AllowAllOutbound = true,
                    Vpc = props.Vpc
                });

            sg.Connections.AllowFrom(props.Alb.Connections, new Port(new PortProps
            {
                FromPort = 5001,
                ToPort = 5002,
                Protocol = Protocol.TCP,
                StringRepresentation = string.Empty
            }));


            var service = new FargateService(this,
                "ecs-api-service",
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
                    ServiceName = "grpc-api"
                });
            return service;
        }

        //Target group for gRPC
        private void CreateGrpcTargetGroup(EcsServiceGrpcConstructProps props,
            FargateService service)
        {
            TargetGroup = new ApplicationTargetGroup(this,
                "tg-grpc-api",
                new ApplicationTargetGroupProps
                {
                    TargetGroupName = "ecs-tg-grpc-api",
                    Vpc = props.Vpc,
                    TargetType = TargetType.IP,
                    ProtocolVersion = ApplicationProtocolVersion.GRPC,
                    Protocol = ApplicationProtocol.HTTP,
                    HealthCheck = new HealthCheck
                    {
                        Protocol = Amazon.CDK.AWS.ElasticLoadBalancingV2.Protocol.HTTP,
                        HealthyThresholdCount = 3,
                        Path = "/grpc.health.v1.Health/Check",
                        Port = "5002",
                        Interval = Duration.Millis(10000),
                        Timeout = Duration.Millis(8000),
                        UnhealthyThresholdCount = 10,
                        HealthyGrpcCodes = "0"
                    },
                    Port = 5002,
                    Targets = new IApplicationLoadBalancerTarget[]
                    {
                        service.LoadBalancerTarget(new LoadBalancerTargetOptions
                        {
                            ContainerPort = 5002,
                            ContainerName = "grpc-webapi"
                        })
                    }
                });

            //Path routing using packagename.**
            props.AlbListener.AddTargetGroups(
                "listener-api-greet",
                new AddApplicationTargetGroupsProps
                {
                    TargetGroups = new IApplicationTargetGroup[] { TargetGroup },
                    Conditions = new[] { ListenerCondition.PathPatterns(new[] { $"/greet.*" })},
                    Priority = new Random().Next(1, 1000)
                });

        }


        //Target group for HTTP
        private void CreateHttpTargetGroup(EcsServiceGrpcConstructProps props,
            FargateService service)
        {
            TargetGroup = new ApplicationTargetGroup(this,
                "tg-http-api",
                new ApplicationTargetGroupProps
                {
                    TargetGroupName = "ecs-tg-http-api",
                    Vpc = props.Vpc,
                    TargetType = TargetType.IP,
                    ProtocolVersion = ApplicationProtocolVersion.HTTP1,
                    Protocol = ApplicationProtocol.HTTP,
                    HealthCheck = new HealthCheck
                    {
                        Protocol = Amazon.CDK.AWS.ElasticLoadBalancingV2.Protocol.HTTP,
                        HealthyThresholdCount = 3,
                        Path = "/health",
                        Port = "5001",
                        Interval = Duration.Millis(10000),
                        Timeout = Duration.Millis(8000),
                        UnhealthyThresholdCount = 10,
                        HealthyHttpCodes = "200"
                    },
                    Port = 5001,
                    Targets = new IApplicationLoadBalancerTarget[] 
                    {
                        service.LoadBalancerTarget(new LoadBalancerTargetOptions
                        {
                            ContainerPort = 5001,
                            ContainerName = "grpc-webapi"
                        })
                    }
                });

            props.AlbListener.AddTargetGroups(
                "listener-api-http",
                new AddApplicationTargetGroupsProps
                {
                    TargetGroups = new IApplicationTargetGroup[] { TargetGroup },
                    Conditions = new[] { ListenerCondition.PathPatterns(new[] { $"/api/*" }) },
                    Priority = new Random().Next(1, 1000)
                });

        }

    }
}
