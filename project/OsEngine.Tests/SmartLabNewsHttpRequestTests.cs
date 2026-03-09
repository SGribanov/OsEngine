#nullable enable

using System;
using OsEngine.Market.Servers.SmartLabNews;

namespace OsEngine.Tests;

public sealed class SmartLabNewsHttpRequestTests
{
    [Fact]
    public void CreateFullNewsRequestUri_AbsoluteHttpsUrl_ShouldPreserveRequestUri()
    {
        Uri requestUri = SmartLabNewsServerRealization.CreateFullNewsRequestUri(
            "https://smart-lab.ru/blog/123.php?utm_source=rss");

        Assert.Equal(
            "https://smart-lab.ru/blog/123.php?utm_source=rss",
            requestUri.AbsoluteUri);
    }

    [Fact]
    public void CreateFullNewsRequestUri_RelativeUrl_ShouldThrowArgumentException()
    {
        ArgumentException exception = Assert.Throws<ArgumentException>(() =>
            SmartLabNewsServerRealization.CreateFullNewsRequestUri("/blog/123.php"));

        Assert.Equal("urlPost", exception.ParamName);
    }

    [Fact]
    public void ExtractFullNewsContent_ValidTopicHtml_ShouldDecodeAndNormalizeMarkup()
    {
        const string html =
            "<div class=\"topic topic_type_blog\" tid=\"123\">" +
            "<div class=\"content\">Line 1<br/>Line &amp; 2<p>Tail</p></div>" +
            "</div>";

        string? content = SmartLabNewsServerRealization.ExtractFullNewsContent(html, "123");

        Assert.Equal("Line 1\r\nLine & 2Tail", content);
    }

    [Fact]
    public void ExtractFullNewsContent_MissingTopic_ShouldReturnNull()
    {
        string? content = SmartLabNewsServerRealization.ExtractFullNewsContent(
            "<div class=\"topic\" tid=\"999\"><div class=\"content\">Ignored</div></div>",
            "123");

        Assert.Null(content);
    }
}
