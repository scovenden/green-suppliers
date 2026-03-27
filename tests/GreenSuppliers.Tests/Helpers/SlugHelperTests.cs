using FluentAssertions;
using GreenSuppliers.Api.Helpers;

namespace GreenSuppliers.Tests.Helpers;

public class SlugHelperTests
{
    [Fact]
    public void Slugify_SimpleName_ReturnsLowercaseHyphenated()
    {
        SlugHelper.Slugify("Eco Solutions SA").Should().Be("eco-solutions-sa");
    }

    [Fact]
    public void Slugify_SpecialCharacters_AreRemoved()
    {
        SlugHelper.Slugify("Green & Clean (Pty) Ltd").Should().Be("green-clean-pty-ltd");
    }

    [Fact]
    public void Slugify_MultipleSpaces_CollapsedToSingleHyphen()
    {
        SlugHelper.Slugify("Solar   Energy   Corp").Should().Be("solar-energy-corp");
    }

    [Fact]
    public void Slugify_Underscores_ReplacedWithHyphens()
    {
        SlugHelper.Slugify("wind_turbine_solutions").Should().Be("wind-turbine-solutions");
    }

    [Fact]
    public void Slugify_LeadingAndTrailingSpaces_AreTrimmed()
    {
        SlugHelper.Slugify("  Eco Corp  ").Should().Be("eco-corp");
    }

    [Fact]
    public void Slugify_MixedCase_ConvertedToLowercase()
    {
        SlugHelper.Slugify("GREEN SUPPLIERS").Should().Be("green-suppliers");
    }

    [Fact]
    public void Slugify_NumbersPreserved()
    {
        SlugHelper.Slugify("ISO 14001 Certified").Should().Be("iso-14001-certified");
    }

    [Fact]
    public void Slugify_ConsecutiveHyphens_CollapsedToSingle()
    {
        SlugHelper.Slugify("eco--solutions---corp").Should().Be("eco-solutions-corp");
    }

    [Fact]
    public void Slugify_LeadingTrailingHyphens_AreRemoved()
    {
        SlugHelper.Slugify("-eco-corp-").Should().Be("eco-corp");
    }

    [Fact]
    public void Slugify_EmptyString_ReturnsEmpty()
    {
        SlugHelper.Slugify("").Should().Be("");
    }

    [Fact]
    public void Slugify_OnlySpecialCharacters_ReturnsEmpty()
    {
        SlugHelper.Slugify("@#$%^&*()").Should().Be("");
    }

    [Fact]
    public void Slugify_SingleWord_ReturnsLowercase()
    {
        SlugHelper.Slugify("Sustainability").Should().Be("sustainability");
    }
}
