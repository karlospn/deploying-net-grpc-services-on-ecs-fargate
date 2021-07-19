using System;
using System.Collections.Generic;
using System.Text;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.ECS;
using Amazon.CDK.AWS.ElasticLoadBalancingV2;

namespace CdkGrpcStack.Props
{
    public class EcsServiceGrpcConstructProps
    {
        public IVpc Vpc { get; set; }
        public ICluster Cluster { get; set; }
        public IApplicationLoadBalancer Alb { get; set; }
        public IApplicationListener AlbListener { get; set; }
        public string EcsImageArn { get; set; }
        public string SubnetId { get; set; }
    }
}
