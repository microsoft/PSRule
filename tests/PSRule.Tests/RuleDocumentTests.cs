// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using PSRule.Definitions;
using PSRule.Help;

namespace PSRule;

public sealed class RuleDocumentTests
{
    /// <summary>
    /// Try to parse the markdown document using English with Windows line endings.
    /// </summary>
    [Fact]
    public void ReadDocument_Windows_en()
    {
        var document = GetDocument(GetToken(nx: false, suffix: ""), culture: null);
        var expected = GetExpected_en();

        Assert.Equal(expected.Name, document.Name);
        Assert.Equal(expected.Synopsis.Text, document.Synopsis.Text);
        Assert.Equal(expected.Description.Text, document.Description.Text);
        Assert.Equal(expected.Recommendation.Text, document.Recommendation.Text);
        Assert.Equal(expected.Notes.Text, document.Notes.Text);
        Assert.Equal(expected.Annotations["severity"], document.Annotations["severity"]);
        Assert.Equal(expected.Annotations["category"], document.Annotations["category"]);
        Assert.Equal(expected.Links.Length, document.Links.Length);
        Assert.Equal(expected.Links[0].Name, document.Links[0].Name);
        Assert.Equal(expected.Links[0].Uri, document.Links[0].Uri);
        Assert.Equal(expected.Links[1].Name, document.Links[1].Name);
        Assert.Equal(expected.Links[1].Uri, document.Links[1].Uri);
    }

    /// <summary>
    /// Try to parse the markdown document using Spanish with Windows line endings.
    /// </summary>
    [Fact]
    public void ReadDocument_Windows_es()
    {
        var document = GetDocument(GetToken(nx: false, suffix: "-es"), culture: "es");
        var expected = GetExpected_es();

        Assert.Equal(expected.Name, document.Name);
        Assert.Equal(expected.Synopsis.Text, document.Synopsis.Text);
        Assert.Equal(expected.Description.Text, document.Description.Text);
        Assert.Equal(expected.Recommendation.Text, document.Recommendation.Text);
        Assert.Equal(expected.Annotations["severity"], document.Annotations["severity"]);
        Assert.Equal(expected.Annotations["category"], document.Annotations["category"]);
        Assert.Equal(expected.Links.Length, document.Links.Length);
        Assert.Equal(expected.Links[0].Name, document.Links[0].Name);
        Assert.Equal(expected.Links[0].Uri, document.Links[0].Uri);
        Assert.Equal(expected.Links[1].Name, document.Links[1].Name);
        Assert.Equal(expected.Links[1].Uri, document.Links[1].Uri);
    }

    /// <summary>
    /// Try to parse the markdown document using English with Linux line endings.
    /// </summary>
    [Fact]
    public void ReadDocument_Linux_en()
    {
        var document = GetDocument(GetToken(nx: true, suffix: ""), culture: null);
        var expected = GetExpected_en();

        Assert.Equal(expected.Name, document.Name);
        Assert.Equal(expected.Synopsis.Text, document.Synopsis.Text);
        Assert.Equal(expected.Description.Text, document.Description.Text);
        Assert.Equal(expected.Recommendation.Text, document.Recommendation.Text);
        Assert.Equal(expected.Notes.Text, document.Notes.Text);
        Assert.Equal(expected.Annotations["severity"], document.Annotations["severity"]);
        Assert.Equal(expected.Annotations["category"], document.Annotations["category"]);
        Assert.Equal(expected.Links.Length, document.Links.Length);
        Assert.Equal(expected.Links[0].Name, document.Links[0].Name);
        Assert.Equal(expected.Links[0].Uri, document.Links[0].Uri);
        Assert.Equal(expected.Links[1].Name, document.Links[1].Name);
        Assert.Equal(expected.Links[1].Uri, document.Links[1].Uri);
    }

    #region Helper methods

    #endregion Helper methods

    private static RuleDocument GetExpected_en()
    {
        var annotations = new Hashtable
        {
            ["severity"] = "Critical",
            ["category"] = "Security"
        };

        var links = new List<Link>
        {
            new() { Name = "PSRule", Uri = "https://github.com/Microsoft/PSRule" },
            new() { Name = "Stable tags", Uri = "https://docs.microsoft.com/en-us/azure/container-registry/container-registry-image-tag-version#stable-tags" }
        };

        var result = new RuleDocument(name: "Use specific tags")
        {
            Synopsis = new InfoString("Containers should use specific tags instead of latest."),
            Description = new InfoString(@"Containers should use specific tags instead of latest. This is because:

- Latest can be updated."),
            Annotations = ResourceTags.FromHashtable(annotations),
            Recommendation = new InfoString(@"Deployments or pods should identify a specific tag to use for container images instead of latest. When latest is used it may be hard to determine which version of the image is running.
When using variable tags such as v1.0 (which may refer to v1.0.0 or v1.0.1) consider using imagePullPolicy: Always to ensure that the an out-of-date cached image is not used.
The latest tag automatically uses imagePullPolicy: Always instead of the default imagePullPolicy: IfNotPresent."),
            Notes = new TextBlock(@"Test that [isIgnored].

{
    ""type"": ""Microsoft.Network/virtualNetworks"",
    ""name"": ""[parameters('VNETName')]"",
    ""apiVersion"": ""2020-06-01"",
    ""location"": ""[parameters('location')]"",
    ""properties"": {}
}"),
            Links = links.ToArray()
        };
        return result;
    }

    private static RuleDocument GetExpected_es()
    {
        var annotations = new Hashtable
        {
            ["severity"] = "Critico",
            ["category"] = "Autenticación"
        };

        var links = new List<Link>
        {
            new() { Name = "Uso de la autenticación basada en identidad", Uri = "https://docs.microsoft.com/azure/architecture/framework/security/design-identity-authentication#use-identity-based-authentication" },
            new() { Name = "Autenticación con un registro de contenedor de Azure", Uri = "https://docs.microsoft.com/azure/container-registry/container-registry-authentication?tabs=azure-cli" }
        };

        var result = new RuleDocument(name: "Deshabilitar el usuario adminstrador para ACR")
        {
            Synopsis = new InfoString("Usar identidades de Azure AD en lugar de usar el usuario administrador del registro."),
            Description = new InfoString(@"Azure Container Registry (ACR) incluye una cuenta de usuario administrador incorporada. La cuenta de usuario administrador es una cuenta de usuario única con acceso administrativo al registro. Esta cuenta proporciona acceso de usuario único para pruebas y desarrollo tempranos. La cuenta de usuario administrador no está diseñada para usarse con registros de contenedores de producción.
En su lugar, utilice el control de acceso basado en roles (RBAC). RBAC se puede usar para delegar permisos de registro a una identidad de Azure AD (AAD)."),
            Annotations = ResourceTags.FromHashtable(annotations),
            Recommendation = new InfoString(@"Considere deshabilitar la cuenta de usuario administrador y solo use la autenticación basada en identidad para las operaciones de registro."),
            Links = links.ToArray()
        };
        return result;
    }

    private static RuleDocument GetDocument(TokenStream stream, string culture)
    {
        var lexer = new RuleHelpLexer(culture);
        return lexer.Process(stream);
    }

    private static TokenStream GetToken(bool nx, string suffix)
    {
        var reader = new MarkdownReader(yamlHeaderOnly: false);
        var content = GetMarkdownContent(suffix);
        content = nx ? content.Replace("\r\n", "\n") : content.Replace("\r\n", "\n").Replace("\n", "\r\n");
        return reader.Read(content, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "RuleDocument.md"));
    }

    private static string GetMarkdownContent(string suffix)
    {
        var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"RuleDocument{suffix}.md");
        return File.ReadAllText(path);
    }
}
