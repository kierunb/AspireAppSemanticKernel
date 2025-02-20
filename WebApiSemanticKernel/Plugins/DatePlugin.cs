using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace WebApiSemanticKernel.Plugins;

public class DatePlugin
{
    [KernelFunction("get_current_date")]
    [Description("Gets a current date")]
    [return: Description("current date")]
    public string GetCurrentDate()
    {
        return DateTime.Now.ToString("yyyy-MM-dd");
    }
}
