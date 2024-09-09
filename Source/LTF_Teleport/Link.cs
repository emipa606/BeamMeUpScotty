using Verse;

namespace LTF_Teleport;

public static class Link
{
    public enum LinkOptions
    {
        [Description("Orphan")] Orphan,
        [Description("Linked")] Linked
    }

    public static bool ValidLink(LinkOptions val)
    {
        var num = 0;
        var num2 = 1;
        return (int)val >= num && (int)val <= num2;
    }

    public static LinkOptions NextLink(this LinkOptions link)
    {
        return link != LinkOptions.Linked ? LinkOptions.Linked : LinkOptions.Orphan;
    }

    public static string LinkNaming(this LinkOptions link)
    {
        return !ValidLink(link) ? "link labeling outbound" : link.DescriptionAttr();
    }

    public static void NextLinkNaming(this LinkOptions link)
    {
        var linkOptions = link.NextLink();
        if (!ValidLink(linkOptions))
        {
            return;
        }

        linkOptions.LinkNaming();
    }
}