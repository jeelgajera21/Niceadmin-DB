using Microsoft.AspNetCore.Razor.TagHelpers;

namespace NiceAdmin.CustomTagHelper
{
    
    [HtmlTargetElement(Attributes = "background-color")]

    public class CustomBTN : TagHelper
    {
        public string BackgroundColor { get; set; }


        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.Attributes.SetAttribute("class", $"btn btn-{BackgroundColor} disabled");
        }

    
    }
}
