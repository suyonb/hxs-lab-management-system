using HxsAiSystem.Application.Auth;
using HxsAiSystem.Application.Auth.Authorization;
using HxsAiSystem.Application.Common;
using HxsAiSystem.Application.LabFoundation;
using HxsAiSystem.Application.LabInventory;
using HxsAiSystem.Application.LabExperiment;
using HxsAiSystem.Application.Files;
using HxsAiSystem.Application.LabVisualization;
using Xunit;

namespace HxsAiSystem.Tests;

public sealed class FoundationTests
{
    [Fact]
    public void PasswordHasher_RoundTripsAndRejectsWrongPassword()
    {
        var hasher = new PasswordHasher();
        var hash = hasher.Hash("Stage1@Password");
        Assert.True(hasher.Verify("Stage1@Password", hash));
        Assert.False(hasher.Verify("wrong-password", hash));
    }

    [Fact]
    public void PageRequest_ClampsUnsafeValues()
    {
        var request = new PageRequest { PageIndex = -1, PageSize = 5000 };
        Assert.Equal(1, request.PageIndex);
        Assert.Equal(200, request.PageSize);
    }

    [Fact]
    public void PermissionAttribute_BuildsDynamicPolicyName()
    {
        var attribute = new PermissionAuthorizeAttribute("lab:booking:approve");
        Assert.Equal("Permission:lab:booking:approve", attribute.Policy);
    }

    [Fact]
    public void LabFoundationRequests_DefaultToEnabledBusinessValues()
    {
        Assert.True(new LabRequest().IsActive);
        Assert.True(new LocationRequest().IsActive);
        Assert.Equal("room", new LocationRequest().LocationType);
        Assert.Equal("member", new GroupMemberRequest().MemberRole);
    }

    [Fact]
    public void InventoryRequests_HaveSafeDefaults()
    {
        var material = new MaterialRequest();
        var requisition = new RequisitionRequest();
        Assert.True(material.IsActive);
        Assert.Equal("reagent", material.MaterialType);
        Assert.Empty(requisition.Items);
    }

    [Fact]
    public void ExperimentAndFileRequests_HaveSafeDefaults()
    {
        var experiment = new ExperimentRequest();
        var record = new ExperimentRecordRequest();
        var files = new FileStorageOptions();
        Assert.Empty(experiment.Instruments);
        Assert.Empty(experiment.Materials);
        Assert.Equal("process", record.RecordType);
        Assert.True(files.MaxBusinessSizeBytes > files.MaxFileSizeBytes);
        Assert.Contains(".txt", files.AllowedExtensions);
        Assert.Contains(".glb", files.AllowedExtensions);
    }

    [Fact]
    public void VisualizationRules_AcceptValidSceneAndNode()
    {
        LabVisualizationRules.ValidateScene(new Lab3dSceneRequest(Guid.NewGuid(),"中心实验室","#e9f0f2"));
        LabVisualizationRules.ValidateNode(new Lab3dNodeRequest("INS-01","液相色谱仪","instrument",0,1,2));
    }

    [Theory]
    [InlineData("blue")]
    [InlineData("#fff")]
    [InlineData("#12GG00")]
    public void VisualizationRules_RejectInvalidBackgroundColor(string color)
    {
        Assert.Throws<InvalidOperationException>(()=>LabVisualizationRules.ValidateScene(new Lab3dSceneRequest(Guid.NewGuid(),"场景",color)));
    }

    [Fact]
    public void VisualizationRules_RejectInvalidNodeScale()
    {
        Assert.Throws<InvalidOperationException>(()=>LabVisualizationRules.ValidateNode(new Lab3dNodeRequest("N-1","节点","instrument",0,0,0,0,1,1)));
    }
}
