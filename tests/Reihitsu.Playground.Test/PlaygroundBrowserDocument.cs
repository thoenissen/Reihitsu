using System.Linq;
using System.Reflection;

using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;

using Bunit;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.RenderTree;

namespace Reihitsu.Playground.Test;

/// <summary>
/// Builds the document a browser builds from a rendered component's current render tree, the way Blazor's browser
/// renderer inserts each frame: an element frame becomes an element, a text frame a text node holding exactly its text,
/// and a markup frame the nodes the HTML parser makes of its markup inside a <c>template</c> element.
/// <para>
/// bUnit's own markup writes a text frame unescaped and reads it back through the HTML parser, so a character the parser
/// drops or replaces, such as U+0000, is lost there although the browser keeps it in a text node. This document keeps a
/// text frame's text as it is and sends only markup frames through the parser, as the browser does.
/// </para>
/// </summary>
internal static class PlaygroundBrowserDocument
{
    #region Methods

    /// <summary>
    /// Builds the document a browser shows for a rendered component
    /// </summary>
    /// <typeparam name="TComponent">The type of the component</typeparam>
    /// <param name="context">The test context that rendered the component</param>
    /// <param name="component">The rendered component</param>
    /// <returns>A document whose body holds the component's nodes</returns>
    internal static IDocument Build<TComponent>(BunitContext context, IRenderedComponent<TComponent> component)
        where TComponent : IComponent
    {
        var document = new HtmlParser().ParseDocument("<!DOCTYPE html><html><head></head><body></body></html>");

        AppendComponent(context, component.ComponentId, document.Body, document);

        return document;
    }

    /// <summary>
    /// Appends the nodes of a component's current render tree
    /// </summary>
    /// <param name="context">The test context</param>
    /// <param name="componentId">The component</param>
    /// <param name="parent">The node the nodes are appended to</param>
    /// <param name="document">The document</param>
    private static void AppendComponent(BunitContext context, int componentId, INode parent, IDocument document)
    {
        var method = typeof(Renderer).GetMethod("GetCurrentRenderTreeFrames", BindingFlags.Instance | BindingFlags.NonPublic);
        var range = method.Invoke(context.Renderer, [componentId]);
        var array = (System.Array)ReadMember(range, "Array");
        var count = (int)ReadMember(range, "Count");
        var frames = new object[count];

        System.Array.Copy(array, frames, count);

        AppendFrames(context, frames, 0, count, parent, document);
    }

    /// <summary>
    /// Reads a public property or field of a render tree value by name
    /// </summary>
    /// <param name="value">The value, a render tree frame or a range of frames</param>
    /// <param name="name">The name of the property or field</param>
    /// <returns>The member's value</returns>
    /// <remarks>
    /// The render tree types are the only record of which content Blazor inserts as text and which as markup. They are read by
    /// name, not referenced at compile time, because they are not meant for use outside the framework; the tests read them to
    /// model the browser, not to drive the renderer.
    /// </remarks>
    private static object ReadMember(object value, string name)
    {
        var type = value.GetType();
        var property = type.GetProperty(name, BindingFlags.Instance | BindingFlags.Public);

        return property != null
                   ? property.GetValue(value)
                   : type.GetField(name, BindingFlags.Instance | BindingFlags.Public).GetValue(value);
    }

    /// <summary>
    /// Reads the name of a frame's type
    /// </summary>
    /// <param name="frame">The frame</param>
    /// <returns>The name of the frame's type, such as <c>Element</c> or <c>Text</c></returns>
    private static string GetFrameType(object frame)
    {
        return ReadMember(frame, "FrameType").ToString();
    }

    /// <summary>
    /// Appends the nodes of a range of sibling frames
    /// </summary>
    /// <param name="context">The test context</param>
    /// <param name="frames">The frames of the component</param>
    /// <param name="start">Index of the first frame</param>
    /// <param name="end">Index behind the last frame</param>
    /// <param name="parent">The node the nodes are appended to</param>
    /// <param name="document">The document</param>
    private static void AppendFrames(BunitContext context, object[] frames, int start, int end, INode parent, IDocument document)
    {
        var index = start;

        while (index < end)
        {
            var frame = frames[index];

            switch (GetFrameType(frame))
            {
                case "Element":
                    {
                        var element = document.CreateElement((string)ReadMember(frame, "ElementName"));
                        var childIndex = index + 1;
                        var subtreeEnd = index + (int)ReadMember(frame, "ElementSubtreeLength");

                        while (childIndex < subtreeEnd
                               && GetFrameType(frames[childIndex]) == "Attribute")
                        {
                            SetAttribute(element, frames[childIndex]);

                            childIndex++;
                        }

                        AppendFrames(context, frames, childIndex, subtreeEnd, element, document);
                        parent.AppendChild(element);

                        index = subtreeEnd;
                    }
                    break;

                case "Text":
                    {
                        parent.AppendChild(document.CreateTextNode((string)ReadMember(frame, "TextContent")));

                        index++;
                    }
                    break;

                case "Markup":
                    {
                        var template = (IHtmlTemplateElement)document.CreateElement("template");
                        var markup = (string)ReadMember(frame, "MarkupContent");

                        template.InnerHtml = string.IsNullOrEmpty(markup) ? " " : markup;

                        foreach (var node in template.Content.ChildNodes.ToList())
                        {
                            parent.AppendChild(node);
                        }

                        index++;
                    }
                    break;

                case "Component":
                    {
                        AppendComponent(context, (int)ReadMember(frame, "ComponentId"), parent, document);

                        index += (int)ReadMember(frame, "ComponentSubtreeLength");
                    }
                    break;

                case "Region":
                    {
                        var subtreeLength = (int)ReadMember(frame, "RegionSubtreeLength");

                        AppendFrames(context, frames, index + 1, index + subtreeLength, parent, document);

                        index += subtreeLength;
                    }
                    break;

                default:
                    {
                        index++;
                    }
                    break;
            }
        }
    }

    /// <summary>
    /// Sets an attribute frame on an element the way the browser renderer shows it: a text value as it is, <see langword="true"/>
    /// as an empty value, and nothing for <see langword="false"/>, an event handler, or an internal attribute
    /// </summary>
    /// <param name="element">The element</param>
    /// <param name="frame">The attribute frame</param>
    private static void SetAttribute(IElement element, object frame)
    {
        var name = (string)ReadMember(frame, "AttributeName");
        var value = ReadMember(frame, "AttributeValue");

        if (name.StartsWith("__", System.StringComparison.Ordinal))
        {
            return;
        }

        switch (value)
        {
            case string text:
                {
                    element.SetAttribute(name, text);
                }
                break;

            case true:
                {
                    element.SetAttribute(name, string.Empty);
                }
                break;

            case int or long or double or decimal:
                {
                    element.SetAttribute(name, System.Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture));
                }
                break;
        }
    }

    #endregion // Methods
}