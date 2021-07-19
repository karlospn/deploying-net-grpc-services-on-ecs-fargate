using Amazon.CDK;
using Amazon.CDK.AWS.EC2;
using CdkGrpcStack.Constructs;
using CdkGrpcStack.Props;

namespace CdkGrpcStack
{
    public class GrpcStack : Stack
    {

        internal GrpcStack(Construct scope, 
            string id,
            GrpcStackProps stackProps,
            IStackProps props = null) : base(scope, id, props)
        {
            var vpc = GetVpc(stackProps.VpcId);
            
            var fg = new FargateClusterConstruct(this,
                "fg-cluster-grpc-construct",
                new FargateClusterConstructProps
                {
                    ClusterName = "grpc-cluster",
                    Vpc = vpc
                });

            var alb = new PublicLoadBalancerConstruct(this,
                "public-alb-grpc-construct",
                new PublicLoadBalancerConstructProps
                {
                    Vpc = vpc,
                    SecretManagerHttpsCertificateArn = stackProps.SecretManagerHttpsCertificateArn
                });

            new EcsServiceGrpcConstruct(this,
                "ecs-grpc-service-construct",
                new EcsServiceGrpcConstructProps
                {
                    Alb = alb.Alb,
                    AlbListener = alb.HttpsListener,
                    Vpc = vpc,
                    SubnetId = stackProps.SubnetId,
                    Cluster = fg.Cluster,
                    EcsImageArn = stackProps.EcrGrpcServiceArn
                });

            new EcsServiceWebApiConstruct(this,
                "ecs-grpc-webapi-construct",
                new EcsServiceGrpcConstructProps
                {
                    Alb = alb.Alb,
                    AlbListener = alb.HttpsListener,
                    Vpc = vpc,
                    SubnetId = stackProps.SubnetId,
                    Cluster = fg.Cluster,
                    EcsImageArn = stackProps.EcrGrpcWebApiArn
                });
        }

        private IVpc GetVpc(string vpcId)
        {
            return Vpc.FromLookup(this,
                "vpc-grpc",
                new VpcLookupOptions
                {
                    VpcId = vpcId
                });
        }
    }
}
