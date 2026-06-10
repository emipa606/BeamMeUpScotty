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

    extension(LinkOptions link)
    {
        private LinkOptions NextLink()
        {
            return link != LinkOptions.Linked ? LinkOptions.Linked : LinkOptions.Orphan;
        }

        public string LinkNaming()
        {
            return !ValidLink(link) ? "link labeling outbound" : link.DescriptionAttr();
        }

        public void NextLinkNaming()
        {
            var linkOptions = link.NextLink();
            if (!ValidLink(linkOptions))
            {
                return;
            }

            linkOptions.LinkNaming();
        }
    }
}