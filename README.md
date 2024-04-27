# Serverless Blazor Server

This project contains AWS CDK infra and simple blazor server which runs on AWS ECS container which makes the entire project totally serverless.


This projects consists of:
- A simple Blazor Server app
- AWS CDK infra:
  - ECS Cluster: To run the Blazor Server app
  - Security Group: To allow HTTP traffic to the Blazor Server app
  - Application Load Balancer: To route the HTTP traffic to the Blazor Server app
  - ECS Task Definition: To define the task to run the Blazor Server app
  - ECS Service: To run the Blazor Server app as a service


## Prerequisites
- `Node.js` installed
- `AWS CDK` installed
- `.NET Core SDK` installed
- `Docker` installed
- `AWS CLI` installed and configured
- `AWS Account` and `IAM User` with the necessary permissions

1. Install the AWS CDK CLI
```bash
npm install -g aws-cdk
```

2. Install the .NET Core SDK and verfy the installation
```bash
dotnet --version
```

3. install Docker server

4. Install the AWS CLI and configure it


## How to Deploy
To run the project you can follow the below steps:

1. Clone the repository
```bash
git clone git@github.com:emami-io/serverless-blazor.git
cd serverless-blazor
```

1. Install NPM packages
```bash
npm install
```

1. Test CDK infra
```bash
cdk synth
```

1. Deploy the CDK infra
```bash
cdk deploy
```

## How to run the Blazor Server app locally
To run the Blazor Server app locally you can follow the below steps:

1. Change the directory to the Blazor Server app
```bash
cd balzorApp
```

2. Run the Blazor Server app
```bash
dotnet run
```

3. (optional) You can run the server as docker container via:
```bash
docker compose up --build
```

## Infrastructure
The `cdk.json` file tells the CDK Toolkit how to execute your app.

It uses the [.NET CLI](https://docs.microsoft.com/dotnet/articles/core/) to compile and execute your project.

## Useful commands

* `dotnet build src` compile this app
* `cdk deploy`       deploy this stack to your default AWS account/region
* `cdk diff`         compare deployed stack with current state
* `cdk synth`        emits the synthesized CloudFormation template

