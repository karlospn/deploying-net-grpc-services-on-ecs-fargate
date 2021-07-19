using System;
using System.Collections.Generic;
using System.Text;
using Amazon.CDK.AWS.EC2;

namespace CdkGrpcStack.Props
{
    public class FargateClusterConstructProps
    {
        public string ClusterName { get; set; }
        public IVpc Vpc { get; set; }

    }
}
