using DotnetAzureStarter.Api.Controllers;
using DotnetAzureStarter.Core.Entities;
using DotnetAzureStarter.Infrastructure.Data;
using FluentAssertions;
using NetArchTest.Rules;

namespace DotnetAzureStarter.Core.Tests.Architecture;

public sealed class ArchitectureTests
{
    private static readonly System.Reflection.Assembly CoreAssembly =
        typeof(BaseEntity).Assembly;

    private static readonly System.Reflection.Assembly InfraAssembly =
        typeof(AppDbContext).Assembly;

    private static readonly System.Reflection.Assembly ApiAssembly =
        typeof(TodosController).Assembly;

    [Fact]
    public void Core_ShouldNot_DependOn_Infrastructure()
    {
        var result = Types.InAssembly(CoreAssembly)
            .ShouldNot()
            .HaveDependencyOn("DotnetAzureStarter.Infrastructure")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "Core is the innermost layer and must not reference Infrastructure");
    }

    [Fact]
    public void Core_ShouldNot_DependOn_Api()
    {
        var result = Types.InAssembly(CoreAssembly)
            .ShouldNot()
            .HaveDependencyOn("DotnetAzureStarter.Api")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "Core must not reference the outer API layer");
    }

    [Fact]
    public void Infrastructure_ShouldNot_DependOn_Api()
    {
        var result = Types.InAssembly(InfraAssembly)
            .ShouldNot()
            .HaveDependencyOn("DotnetAzureStarter.Api")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "Infrastructure must not reference the API layer");
    }

    [Fact]
    public void Api_Controllers_ShouldNot_DependOn_Repositories_Directly()
    {
        var result = Types.InAssembly(ApiAssembly)
            .That().ResideInNamespace("DotnetAzureStarter.Api.Controllers")
            .ShouldNot()
            .HaveDependencyOn("DotnetAzureStarter.Infrastructure.Repositories")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "Controllers must use services, never repositories directly");
    }

    [Fact]
    public void Api_Controllers_ShouldNot_DependOn_DbContext_Directly()
    {
        var result = Types.InAssembly(ApiAssembly)
            .That().ResideInNamespace("DotnetAzureStarter.Api.Controllers")
            .ShouldNot()
            .HaveDependencyOn("DotnetAzureStarter.Infrastructure.Data")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "Controllers must not access DbContext directly — only via service interfaces");
    }
}
