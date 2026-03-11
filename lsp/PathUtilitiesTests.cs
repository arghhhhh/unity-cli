using UnityCli.Lsp.Core;
using Xunit;

public sealed class PathUtilitiesTests
{
    [Fact]
    public void NormalizeRelative_ConvertsBackslashes()
    {
        Assert.Equal("Assets/Scripts/Player.cs", LspPathUtilities.NormalizeRelative(@"Assets\Scripts\Player.cs"));
    }

    [Fact]
    public void NormalizeNamePath_NormalizesDotsAndSlashes()
    {
        Assert.Equal("Player/Config/Value", LspPathUtilities.NormalizeNamePath("Player.Config/Value"));
    }

    [Fact]
    public void ContainerHelpers_ReturnExpectedValues()
    {
        Assert.Equal("Player/Config", LspPathUtilities.ContainerPath("Player/Config/Value"));
        Assert.Equal("Config", LspPathUtilities.ContainerName("Player/Config/Value"));
        Assert.Null(LspPathUtilities.ContainerPath("Player"));
    }

    [Fact]
    public void ContainerName_ReturnsNullForEmptyContainer()
    {
        Assert.Null(LspPathUtilities.ContainerName(""));
    }

    [Fact]
    public void ToRel_ReturnsPathRelativeToRoot()
    {
        Assert.Equal(
            "Assets/Scripts/Player.cs",
            LspPathUtilities.ToRel("/repo/Assets/Scripts/Player.cs", "/repo"));
    }

    [Fact]
    public void ToRel_ReturnsOriginalWhenOutsideRoot()
    {
        Assert.Equal(
            "/other/Assets/Scripts/Player.cs",
            LspPathUtilities.ToRel("/other/Assets/Scripts/Player.cs", "/repo"));
    }

    [Fact]
    public void Path2Uri_UsesFilePrefix()
    {
        Assert.Equal("file:///repo/Assets/Scripts/Player.cs", LspPathUtilities.Path2Uri("/repo/Assets/Scripts/Player.cs"));
    }
}
