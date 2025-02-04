#addin nuget:?package=AWSSDK.Core&version=3.7.101
#addin nuget:?package=AWSSDK.Ecr&version=3.7.100.28
#addin nuget:?package=AWSSDK.IdentityManagement&version=3.7.100.29
#addin nuget:?package=AWSSDK.Lambda&version=3.7.102.4
#addin nuget:?package=AWSSDK.S3&version=3.7.101.28
#addin nuget:?package=AWSSDK.SecurityToken&version=3.7.100.28
#addin nuget:?package=AWSSDK.SQS&version=3.7.100.28
#addin nuget:?package=AWSSDK.SSO&version=3.7.100.28
#addin nuget:?package=AWSSDK.SSOOIDC&version=3.7.100.28
#addin nuget:?package=Cake.Docker&version=0.11.1
#addin nuget:?package=Cake.FileHelpers&version=3.4.0
#addin nuget:?package=Cake.Http&version=0.7.0
#addin nuget:?package=log4net&version=2.0.15
#addin nuget:?package=Microsoft.Bcl.AsyncInterfaces&version=7.0.0
#addin nuget:?package=System.Configuration.ConfigurationManager&version=7.0.0
#addin nuget:?package=YamlDotNet&version=12.0.2
#addin nuget:?package=Newtonsoft.Json&version=10.0.3

#load "./tasks/**/*.cake"

RunTarget(Targets.GetTargetToRun(Context, "help"));