# How to deploy a dotnet gRPC service on AWS ECS Fargate

This repository contains a couple of scenarios showing you how you can deploy a .NET gRPC service into AWS ECS Fargate.

# Scenario 1

We have built a .NET5 gRPC service and we want to deploy it into AWS ECS Fargate.

The source code can be found in the ``/src/StandAloneGrpcService`` folder. The app has the following features already built-in :

- It exposes a  gRPC service for health checking.
- It contains an Azure Devops YAML pipelines that pushes the app into an AWS ECR repository.


# Scenario 2

We have an existing WebAPI and we want to add a gRPC service endpoint. Afterwards we want to deploy it into AWS ECS Fargate.   

The source code can be found in the ``/src/WebApiWithGrpcService`` folder. The app has the following features already built-in:

- It exposes a Http endpoint for health checking.
- It exposes a gRPC service for health checking.
- Starts Kestrel on port 5001 and port 5002. The port 5001 is used for incoming Http1 requests and the port 5002 is used for Http2 requests.
- It contains an Azure Devops YAML pipelines that pushes the app into an AWS ECR repository.

# AWS Infrastructure

The ``/cdk`` folder contains everything necessary to ran both scenarios. It creates the following infrastructure:

- Fargate Cluster
- Public Application Load Balancer
- ECS Task Definition and ECS Service for the scenario 1 app
- ECS Task Definition and ECS Service for the scenario 2 app

![aws-infrastructure](https://github.com/karlospn/deploying-net-grpc-services-on-ecs-fargate/blob/main/docs/GrpcStack.template.json.png)

This CDK needs that a few resources are created beforehand. The resources that needs to be created beforehand are the following ones:

- ``VpcId``: The Id of the VPC where all the resources are going to be placed.
- ``SubnetId``: The Id of the subnet where the ECS Fargate Service is going to be placed.
- ``EcrGrpcServiceArn``: The Scenario 1 ECR repository Arn. The app needs to be pushed on this ECR repository before deploying the CDK.
- ``EcrGrpcWebApiArn``: The Scenario 2 ECR repository Arn. The app needs to be pushed on this ECR repository before deploying the CDK.
- ``SecretManagerHttpsCertificateArn``: A secret manager arn pointing to a certificate that is going to be used for the ALB Https Listener.
  
Every parameter the CDK needs is encapsulated in the ``GrpcStackProps`` DTO. Here's a code snippet of the ``Program.cs``:

```csharp
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
            Account = "718694466383",
            Region = "eu-west-1",
        }
    }
);
app.Synth();
```
