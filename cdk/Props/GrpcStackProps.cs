using System;
using System.Collections.Generic;
using System.Text;

namespace CdkGrpcStack.Props
{
    public class GrpcStackProps
    {
        public string VpcId { get; set; }
        public string SubnetId { get; set; }
        public string SecretManagerHttpsCertificateArn { get; set; }
        public string EcrGrpcServiceArn { get; set; }
        public string EcrGrpcWebApiArn { get; set; }
    }
}
