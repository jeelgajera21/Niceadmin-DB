using Microsoft.AspNetCore.Razor.TagHelpers;

namespace NiceAdmin.CustomTagHelper
{
    [HtmlTargetElement("jeel")]
   
    public class MyTagHelper : TagHelper
    {
        public string path { get; set; }
        public string altText { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "img";
            output.TagMode = TagMode.StartTagAndEndTag;

            output.Attributes.SetAttribute("src",path);
            output.Attributes.SetAttribute("alt", altText);

        }
    }
}
