using Amazon.CDK;
using CdkGrpcStack.Props;

namespace CdkGrpcStack
{
    sealed class Program
    {
        public static void Main(string[] args)
        {
            var app = new App();
            
            new GrpcStack
            (
                app, 
                "GrpcStack",
                new GrpcStackProps
                {
                    VpcId = "vpc-127c5e74",
                    SubnetId = "subnet-928158c8",
                    EcrGrpcServiceArn = "arn:aws:ecr:eu-west-1:718694466383:repository/server.grpc",
                    EcrGrpcWebApiArn = "arn:aws:ecr:eu-west-1:718694466383:repository/server.apiwithgrpc",
                    SecretManagerHttpsCertificateArn = "arn:aws:acm:eu-west-1:718694466383:certificate/41b69798-8a4f-4ca9-a12a-cfa6fc59fe53"
                },
                new StackProps
                {
                    StackName = "grpc-stack",
                    Env = new Amazon.CDK.Environment
                    {
                        Account = "934045942927",
                        Region = "eu-west-1",
                    }
                }
            );
            
            app.Synth();
        }
    }
}
