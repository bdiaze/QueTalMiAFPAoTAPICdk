using Amazon.CDK;
using Amazon.CDK.AWS.APIGateway;
using Amazon.CDK.AWS.Apigatewayv2;
using Amazon.CDK.AWS.DynamoDB;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.Logs;
using Amazon.CDK.AWS.S3;
using Amazon.CDK.AWS.SSM;
using Constructs;
using System;
using System.Collections.Generic;
using Attribute = Amazon.CDK.AWS.DynamoDB.Attribute;
using LogGroupLogDestination = Amazon.CDK.AWS.APIGateway.LogGroupLogDestination;
using StageOptions = Amazon.CDK.AWS.APIGateway.StageOptions;

namespace Cdk
{
    public class CdkStack : Stack
    {
        internal CdkStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
        {
            string appName = System.Environment.GetEnvironmentVariable("APP_NAME") ?? throw new ArgumentNullException("APP_NAME");
            string handler = System.Environment.GetEnvironmentVariable("HANDLER") ?? throw new ArgumentNullException("HANDLER");
            string timeout = System.Environment.GetEnvironmentVariable("TIMEOUT") ?? throw new ArgumentNullException("TIMEOUT");
            string memorySize = System.Environment.GetEnvironmentVariable("MEMORY_SIZE") ?? throw new ArgumentNullException("MEMORY_SIZE");
            string domainName = System.Environment.GetEnvironmentVariable("DOMAIN_NAME") ?? throw new ArgumentNullException("DOMAIN_NAME");
            string apiMappingKey = System.Environment.GetEnvironmentVariable("API_MAPPING_KEY") ?? throw new ArgumentNullException("API_MAPPING_KEY");
            string vpcId = System.Environment.GetEnvironmentVariable("VPC_ID") ?? throw new ArgumentNullException("VPC_ID");
            string subnetId1 = System.Environment.GetEnvironmentVariable("SUBNET_ID_1") ?? throw new ArgumentNullException("SUBNET_ID_1");
            string subnetId2 = System.Environment.GetEnvironmentVariable("SUBNET_ID_2") ?? throw new ArgumentNullException("SUBNET_ID_2");
            string rdsSecurityGroupId = System.Environment.GetEnvironmentVariable("RDS_SECURITY_GROUP_ID") ?? throw new ArgumentNullException("RDS_SECURITY_GROUP_ID");
            string secretArnConnectionString = System.Environment.GetEnvironmentVariable("SECRET_ARN_CONNECTION_STRING") ?? throw new ArgumentNullException("SECRET_ARN_CONNECTION_STRING");
            string allowedDomains = System.Environment.GetEnvironmentVariable("ALLOWED_DOMAINS") ?? throw new ArgumentNullException("ALLOWED_DOMAINS");
            string arnParHermesApiUrl = System.Environment.GetEnvironmentVariable("ARN_PARAMETER_HERMES_API_URL") ?? throw new ArgumentNullException("ARN_PARAMETER_HERMES_API_URL");
            string arnParHermesApiKeyId = System.Environment.GetEnvironmentVariable("ARN_PARAMETER_HERMES_API_KEY_ID") ?? throw new ArgumentNullException("ARN_PARAMETER_HERMES_API_KEY_ID");
            string arnParKairosApiUrl = System.Environment.GetEnvironmentVariable("ARN_PARAMETER_KAIROS_API_URL") ?? throw new ArgumentNullException("ARN_PARAMETER_KAIROS_API_URL");
            string arnParKairosApiKeyId = System.Environment.GetEnvironmentVariable("ARN_PARAMETER_KAIROS_API_KEY_ID") ?? throw new ArgumentNullException("ARN_PARAMETER_KAIROS_API_KEY_ID");
            string arnParNotifLambdaArn = System.Environment.GetEnvironmentVariable("ARN_PARAMETER_NOTIFICACIONES_LAMBDA_ARN") ?? throw new ArgumentNullException("ARN_PARAMETER_NOTIFICACIONES_LAMBDA_ARN");
            string arnParNotifEjecRoleArn = System.Environment.GetEnvironmentVariable("ARN_PARAMETER_NOTIFICACIONES_EJECUCION_ROLE_ARN") ?? throw new ArgumentNullException("ARN_PARAMETER_NOTIFICACIONES_EJECUCION_ROLE_ARN");

            // Se obtiene la VPC y subnets...
            IVpc vpc = Vpc.FromLookup(this, $"{appName}Vpc", new VpcLookupOptions {
                VpcId = vpcId
            });

            ISubnet subnet1 = Subnet.FromSubnetId(this, $"{appName}Subnet1", subnetId1);
            ISubnet subnet2 = Subnet.FromSubnetId(this, $"{appName}Subnet2", subnetId2);

            // Se crea security group para la lambda y se enlaza con security group de RDS...
            SecurityGroup securityGroup = new(this, $"{appName}LambdaSecurityGroup", new SecurityGroupProps {
                Vpc = vpc,
                SecurityGroupName = $"{appName}LambdaSecurityGroup",
                Description = $"{appName} Lambda Security Group",
                AllowAllOutbound = true,
            });

            ISecurityGroup rdsSecurityGroup = SecurityGroup.FromSecurityGroupId(this, $"{appName}RDSSecurityGroup", rdsSecurityGroupId);
            rdsSecurityGroup.AddIngressRule(securityGroup, Port.POSTGRES, $"Allow connection from {appName} Lambda to RDS");

            // Creación de log group lambda...
            LogGroup logGroup = new(this, $"{appName}APILogGroup", new LogGroupProps {
                LogGroupName = $"/aws/lambda/{appName}API/logs",
                Retention = RetentionDays.ONE_MONTH,
                RemovalPolicy = RemovalPolicy.DESTROY
            });

            StringParameter stringParameterApiAllowedDomains = new(this, $"{appName}StringParameterAllowedDomains", new StringParameterProps {
                ParameterName = $"/{appName}/ApiGateway/AllowedDomains",
                Description = $"Allowed Domains de la aplicacion {appName}",
                StringValue = allowedDomains,
                Tier = ParameterTier.STANDARD,
            });

            // Se crea bucket para almacenar respuestas muy grandes para API Gateway...
            Bucket bucket = new(this, $"{appName}APIBucketLargeResponses", new BucketProps {
                BucketName = $"{appName.ToLower()}api-large-responses",
                LifecycleRules = [
                    new LifecycleRule {
                        Id = $"{appName.ToLower()}-large-responses-removal",
                        Enabled = true,
                        Expiration = Duration.Days(1),
                    }
                ],
                BlockPublicAccess = BlockPublicAccess.BLOCK_ALL,
                Versioned = false,
                AutoDeleteObjects = true,
                RemovalPolicy = RemovalPolicy.DESTROY,
            });

            StringParameter stringParameterApiBucketName = new(this, $"{appName}StringParameterBucketName", new StringParameterProps {
                ParameterName = $"/{appName}/S3/BucketName",
                Description = $"Bucket Name de la aplicacion {appName}",
                StringValue = bucket.BucketName,
                Tier = ParameterTier.STANDARD,
            });

            StringParameter stringParameterApiBucketArn = new(this, $"{appName}StringParameterBucketArn", new StringParameterProps {
                ParameterName = $"/{appName}/S3/BucketArn",
                Description = $"Bucket ARN de la aplicacion {appName}",
                StringValue = bucket.BucketArn,
                Tier = ParameterTier.STANDARD,
            });

            // Se crea DynamoDB de la aplicación...
            /*
            Table singleTable = new(this, $"{appName}SingleTable", new TableProps {
                TableName = $"{appName}",
                PartitionKey = new Attribute {
                    Name = "PK",
                    Type = AttributeType.STRING
                },
                SortKey = new Attribute { 
                    Name = "SK",
                    Type = AttributeType.STRING,
                },
                DeletionProtection = true,
                BillingMode = BillingMode.PAY_PER_REQUEST,
                RemovalPolicy = RemovalPolicy.DESTROY,
            });
            */

            // Se obtiene ARN del API Key...
            IStringParameter strParHermesApiKeyId = StringParameter.FromStringParameterArn(this, $"{appName}StringParameterHermesApiKeyId", arnParHermesApiKeyId);
            IStringParameter strParKairosApiKeyId = StringParameter.FromStringParameterArn(this, $"{appName}StringParameterKairosApiKeyId", arnParKairosApiKeyId);
                                    
            // Creación de role para la función lambda...
            IRole roleLambda = new Role(this, $"{appName}APILambdaRole", new RoleProps {
                RoleName = $"{appName}APILambdaRole",
                Description = $"Role para API Lambda de {appName}",
                AssumedBy = new ServicePrincipal("lambda.amazonaws.com"),
                ManagedPolicies = [
                    ManagedPolicy.FromAwsManagedPolicyName("service-role/AWSLambdaVPCAccessExecutionRole"),
                    ManagedPolicy.FromAwsManagedPolicyName("service-role/AWSLambdaBasicExecutionRole"),
                ],
                InlinePolicies = new Dictionary<string, PolicyDocument> {
                    {
                        $"{appName}APILambdaPolicy",
                        new PolicyDocument(new PolicyDocumentProps {
                            Statements = [
                                new PolicyStatement(new PolicyStatementProps{
                                    Sid = $"{appName}AccessToSecretManager",
                                    Actions = [
                                        "secretsmanager:GetSecretValue"
                                    ],
                                    Resources = [
                                        secretArnConnectionString,
                                    ],
                                }),
                                new PolicyStatement(new PolicyStatementProps{
                                    Sid = $"{appName}AccessToParameterStore",
                                    Actions = [
                                        "ssm:GetParameter"
                                    ],
                                    Resources = [
                                        stringParameterApiAllowedDomains.ParameterArn,
                                        arnParHermesApiUrl,
                                        arnParHermesApiKeyId,
                                        arnParKairosApiUrl,
                                        arnParKairosApiKeyId,
                                        arnParNotifLambdaArn,
                                        arnParNotifEjecRoleArn,
                                    ],
                                }),
                                new PolicyStatement(new PolicyStatementProps{
                                    Sid = $"{appName}AccessToPutObject",
                                    Actions = [
                                        "s3:PutObject"
                                    ],
                                    Resources = [
                                        $"{bucket.BucketArn}/*",
                                    ],
                                }),
                                new PolicyStatement(new PolicyStatementProps{
                                    Sid = $"{appName}AccessToApiKey",
                                    Actions = [
                                        "apigateway:GET"
                                    ],
                                    Resources = [
                                        $"arn:aws:apigateway:{this.Region}::/apikeys/{strParHermesApiKeyId.StringValue}",
                                        $"arn:aws:apigateway:{this.Region}::/apikeys/{strParKairosApiKeyId.StringValue}",
                                    ],
                                }),
                                new PolicyStatement(new PolicyStatementProps{
                                    Sid = $"{appName}AccessToOwnApiKeys",
                                    Actions = [
                                        "apigateway:GET",
                                        "apigateway:POST",
                                        "apigateway:DELETE"
                                    ],
                                    Resources = [
                                        $"arn:aws:apigateway:{this.Region}::/usageplans/*",
                                        $"arn:aws:apigateway:{this.Region}::/apikeys/*",
                                        $"arn:aws:apigateway:{this.Region}::/usageplans/*/keys/*",
                                    ],
                                    Conditions = new Dictionary<string, object> {
                                        { "StringEquals",  new Dictionary<string, object> {
                                            { "aws:ResourceTag/AppName", $"{appName}" }
                                        } }
                                    }
                                }),
                                /*
                                new PolicyStatement(new PolicyStatementProps{
                                    Sid = $"{appName}AccessToDynamoDB",
                                    Actions = [
                                        "dynamodb:GetItem",
                                        "dynamodb:BatchGetItem",
                                        "dynamodb:Query",
                                        "dynamodb:Scan",
                                        "dynamodb:PutItem",
                                        "dynamodb:UpdateItem",
                                        "dynamodb:DeleteItem",
                                        "dynamodb:BatchWriteItem",
                                    ],
                                    Resources = [
                                        singleTable.TableArn,
                                        $"{singleTable.TableArn}/index/*",
                                    ],
                                }),
                                */
                            ]
                        })
                    }
                }
            });

            // Creación de la función lambda...
            Function function = new(this, $"{appName}APILambdaFunction", new FunctionProps {
                Runtime = Runtime.DOTNET_8,
                Handler = handler,
                Code = Code.FromAsset("../publish/publish.zip"),
                FunctionName = $"{appName}API",
                Timeout = Duration.Seconds(double.Parse(timeout)),
                MemorySize = double.Parse(memorySize),
                Architecture = Architecture.X86_64,
                LogGroup = logGroup,
                Environment = new Dictionary<string, string> {
                    { "APP_NAME", appName },
                    { "BUCKET_NAME_LARGE_RESPONSES", bucket.BucketName },
                    { "SECRET_ARN_CONNECTION_STRING", secretArnConnectionString },
                    { "PARAMETER_ARN_API_ALLOWED_DOMAINS", stringParameterApiAllowedDomains.ParameterArn },
                    { "ARN_PARAMETER_HERMES_API_URL", arnParHermesApiUrl },
                    { "ARN_PARAMETER_HERMES_API_KEY_ID", arnParHermesApiKeyId },
                    { "ARN_PARAMETER_KAIROS_API_URL", arnParKairosApiUrl },
                    { "ARN_PARAMETER_KAIROS_API_KEY_ID", arnParKairosApiKeyId },
                    { "ARN_PARAMETER_NOTIFICACIONES_LAMBDA_ARN", arnParNotifLambdaArn },
                    { "ARN_PARAMETER_NOTIFICACIONES_EJECUCION_ROLE_ARN", arnParNotifEjecRoleArn },
                    // { "NAME_DYNAMODB_SINGLE_TABLE", singleTable.TableName }
                    { "ARN_PARAMETER_APIGATEWAY_API_ID", $"arn:aws:ssm:{this.Region}:{this.Account}:parameter/{appName}/ApiGateway/ApiId" },
                    { "ARN_PARAMETER_APIGATEWAY_STAGE_NAME", $"arn:aws:ssm:{this.Region}:{this.Account}:parameter/{appName}/ApiGateway/StageName" },
                },
                Vpc = vpc,
                VpcSubnets = new SubnetSelection {
                    Subnets = [subnet1, subnet2]
                },
                SecurityGroups = [securityGroup],
                Role = roleLambda,
            });

            // Creación de access logs...
            LogGroup logGroupAccessLogs = new(this, $"{appName}APILambdaFunctionLogGroup", new LogGroupProps {
                LogGroupName = $"/aws/lambda/{appName}API/access_logs",
                Retention = RetentionDays.ONE_MONTH,
                RemovalPolicy = RemovalPolicy.DESTROY
            });

            // Creación de la LambdaRestApi...
            LambdaRestApi lambdaRestApi = new(this, $"{appName}APILambdaRestApi", new LambdaRestApiProps {
                Handler = function,
                DefaultCorsPreflightOptions = new CorsOptions {
                    AllowOrigins = stringParameterApiAllowedDomains.StringValue.Split(","),
                },
                DeployOptions = new StageOptions {
                    AccessLogDestination = new LogGroupLogDestination(logGroupAccessLogs),
                    AccessLogFormat = AccessLogFormat.Custom("'{\"requestTime\":\"$context.requestTime\",\"requestId\":\"$context.requestId\",\"httpMethod\":\"$context.httpMethod\",\"path\":\"$context.path\",\"resourcePath\":\"$context.resourcePath\",\"status\":$context.status,\"responseLatency\":$context.responseLatency,\"xrayTraceId\":\"$context.xrayTraceId\",\"integrationRequestId\":\"$context.integration.requestId\",\"functionResponseStatus\":\"$context.integration.status\",\"integrationLatency\":\"$context.integration.latency\",\"integrationServiceStatus\":\"$context.integration.integrationStatus\",\"authorizeStatus\":\"$context.authorize.status\",\"authorizerStatus\":\"$context.authorizer.status\",\"authorizerLatency\":\"$context.authorizer.latency\",\"authorizerRequestId\":\"$context.authorizer.requestId\",\"ip\":\"$context.identity.sourceIp\",\"userAgent\":\"$context.identity.userAgent\",\"principalId\":\"$context.authorizer.principalId\"}'"),
                    StageName = "prod",
                    Description = $"Stage para produccion de la aplicacion {appName}",
                },
                RestApiName = $"{appName}API",
                DefaultMethodOptions = new MethodOptions {
                    ApiKeyRequired = true,
                },
            });

            // Se crean parámetros que se usarán para crear los API Keys...
            StringParameter stringParameterApiId = new(this, $"{appName}StringParameterApiId", new StringParameterProps {
                ParameterName = $"/{appName}/ApiGateway/ApiId",
                Description = $"API ID de la aplicacion {appName}",
                StringValue = lambdaRestApi.RestApiId,
                Tier = ParameterTier.STANDARD,
            });
            StringParameter stringParameterStageName = new(this, $"{appName}StringParameterStageName", new StringParameterProps {
                ParameterName = $"/{appName}/ApiGateway/StageName",
                Description = $"Stage Name de la aplicacion {appName}",
                StringValue = lambdaRestApi.DeploymentStage.StageName,
                Tier = ParameterTier.STANDARD,
            });

            _ = new ManagedPolicy(this, $"{appName}APIManagedPolicy", new ManagedPolicyProps{ 
                ManagedPolicyName = $"{appName}APIManagedPolicy",
                Description = $"Politica para acceder a los parametros de API Gateway de {appName}",
                Roles = [roleLambda],
                Statements = [
                    new PolicyStatement(new PolicyStatementProps {
                        Sid = $"{appName}AccessToParameterStore",
                        Actions = [ 
                            "ssm:GetParameter"
                        ],
                        Resources = [
                            stringParameterApiId.ParameterArn,
                            stringParameterStageName.ParameterArn,
                        ],
                    }),
                ],
            });

            // Creación de la CfnApiMapping para el API Gateway...
            CfnApiMapping apiMapping = new(this, $"{appName}APIApiMapping", new CfnApiMappingProps {
                DomainName = domainName,
                ApiMappingKey = apiMappingKey,
                ApiId = lambdaRestApi.RestApiId,
                Stage = lambdaRestApi.DeploymentStage.StageName,
            });

            // Se crea Usage Plan para configurar API Key...
            UsagePlan usagePlan = new(this, $"{appName}APIUsagePlan", new UsagePlanProps {
                Name = $"{appName}APIUsagePlan",
                Description = $"Usage Plan de {appName} API",
                ApiStages = [
                    new UsagePlanPerApiStage() {
                        Api = lambdaRestApi,
                        Stage = lambdaRestApi.DeploymentStage
                    }
                ],
            });

            // Se crea API Key...
            ApiKey apiGatewayKey = new(this, $"{appName}APIAPIKey", new ApiKeyProps {
                ApiKeyName = $"{appName}APIKey",
                Description = $"API Key de {appName} API",
            });
            usagePlan.AddApiKey(apiGatewayKey);

            // Se configura permisos para la ejecucíon de la Lambda desde el API Gateway...
            ArnPrincipal arnPrincipal = new("apigateway.amazonaws.com");
            Permission permission = new() {
                Scope = this,
                Action = "lambda:InvokeFunction",
                Principal = arnPrincipal,
                SourceArn = $"arn:aws:execute-api:{this.Region}:{this.Account}:{lambdaRestApi.RestApiId}/*/*/*",
            };
            function.AddPermission($"{appName}APIPermission", permission);

            // Se configuran parámetros para ser rescatados por consumidores...
            _ = new StringParameter(this, $"{appName}StringParameterApiUrl", new StringParameterProps {
                ParameterName = $"/{appName}/Api/Url",
                Description = $"API URL de la aplicacion {appName}",
                StringValue = $"https://{apiMapping.DomainName}/{apiMapping.ApiMappingKey}/",
                Tier = ParameterTier.STANDARD,
            });

            _ = new StringParameter(this, $"{appName}StringParameterApiKeyId", new StringParameterProps {
                ParameterName = $"/{appName}/Api/KeyId",
                Description = $"API Key ID de la aplicacion {appName}",
                StringValue = apiGatewayKey.KeyId,
                Tier = ParameterTier.STANDARD,
            });
        }
    }
}
