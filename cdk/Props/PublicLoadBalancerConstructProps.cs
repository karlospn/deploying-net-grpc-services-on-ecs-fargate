using Amazon.CDK.AWS.EC2;

namespace CdkGrpcStack.Props
{
    public class PublicLoadBalancerConstructProps
    {
        public IVpc Vpc { get; set; }
        public string SecretManagerHttpsCertificateArn { get; set; }
    }
}
