using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace BlazorWebAppWebAssemblyRender.Components.Pages;

public partial class HomeBase: ComponentBase
{

    protected override Task OnAfterRenderAsync(bool firstRender)
    {
        if(this.RendererInfo.IsInteractive)
        {

        }
        Console.WriteLine($"{nameof(OnAfterRenderAsync)}");
        return base.OnAfterRenderAsync(firstRender);
    }

    protected override void OnAfterRender(bool firstRender)
    {
        Console.WriteLine($"{nameof(OnAfterRender)}");
        base.OnAfterRender(firstRender);
    }

    protected override void OnInitialized()
    {
        Console.WriteLine($"{nameof(OnInitialized)}");
        base.OnInitialized();
    }

    protected override Task OnInitializedAsync()
    {
        Console.WriteLine($"{nameof(OnInitializedAsync)}");
        return base.OnInitializedAsync();
    }


    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        Console.WriteLine("BuildRenderTree");
        base.BuildRenderTree(builder);
    }
}
