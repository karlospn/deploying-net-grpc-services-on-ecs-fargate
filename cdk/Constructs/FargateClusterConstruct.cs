using Amazon.CDK;
using Amazon.CDK.AWS.ECS;
using CdkGrpcStack.Props;

namespace CdkGrpcStack.Constructs
{
    public class FargateClusterConstruct : Construct
    {
        public Cluster Cluster { get; }

        public FargateClusterConstruct(Construct scope,
            string id,
            FargateClusterConstructProps props)
            : base(scope, id)
        {
            Cluster = new Cluster(this,
                "fg-cluster",
                new ClusterProps
                {
                    ContainerInsights = true,
                    Vpc = props.Vpc,
                    ClusterName = props.ClusterName,
                });
        }
    }
}
