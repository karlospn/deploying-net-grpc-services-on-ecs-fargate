using Amazon.CDK;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.ElasticLoadBalancingV2;
using CdkGrpcStack.Props;

namespace CdkGrpcStack.Constructs
{
    public class PublicLoadBalancerConstruct: Construct
    {
        public ApplicationLoadBalancer Alb { get; }
        public ApplicationListener HttpsListener { get; }
        public SecurityGroup SecurityGroup { get; }

        public PublicLoadBalancerConstruct(Construct scope,
            string id,
            PublicLoadBalancerConstructProps props)
            : base(scope, id)
        {
            SecurityGroup = new SecurityGroup(this,
                "sg-alb-fg",
                new SecurityGroupProps()
                {
                    Vpc = props.Vpc,
                    AllowAllOutbound = true,
                    Description = "Security group for the public ALB",
                    SecurityGroupName = "sec-group-public-alb"
                });

            SecurityGroup.AddIngressRule(Peer.AnyIpv4(),
                Port.Tcp(443),
                "Allow port 443 ingress traffic");

            Alb = new ApplicationLoadBalancer(this,
                "pub-alb-fg-cluster",
                new ApplicationLoadBalancerProps
                {
                    InternetFacing = true,
                    Vpc = props.Vpc,
                    Http2Enabled = true,
                    VpcSubnets = new SubnetSelection
                    {
                        OnePerAz = true,
                        SubnetType = SubnetType.PUBLIC,
                    },
                    SecurityGroup = SecurityGroup,
                    LoadBalancerName = "alb-pub-grpc"
                });

            HttpsListener = Alb.AddListener("alb-https", new ApplicationListenerProps
            {
                Protocol = ApplicationProtocol.HTTPS,
                LoadBalancer = Alb,
                DefaultAction = ListenerAction.FixedResponse(500),
                Certificates = new[] { new ListenerCertificate(props.SecretManagerHttpsCertificateArn) }
            });
        }
    }
}
