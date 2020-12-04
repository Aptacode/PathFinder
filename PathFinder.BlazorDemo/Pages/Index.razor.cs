using Aptacode.FlowDesigner.Core.ViewModels;
using Microsoft.AspNetCore.Components;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace PathFinder.BlazorDemo.Pages
{
    public class IndexBase : ComponentBase
    {

        public DesignerViewModel Designer { get; set; }

        protected async override Task OnInitializedAsync()
        {
            Designer = new DesignerViewModel();
            Designer.CreateItem.Execute(("State 1", new Vector2(8, 8), new Vector2(8, 4)));
            Designer.CreateItem.Execute(("State 2", new Vector2(8, 16), new Vector2(8, 4)));
            Designer.CreateItem.Execute(("State 3", new Vector2(25, 30), new Vector2(8, 4)));
            Designer.CreateItem.Execute(("State 4", new Vector2(10, 40), new Vector2(8, 4)));
            Designer.CreateItem.Execute(("State 5", new Vector2(10, 50), new Vector2(8, 4)));
            Designer.CreateItem.Execute(("State 6", new Vector2(25, 60), new Vector2(8, 4)));

            var items = Designer.Items.ToList();
            Designer.Connect.Execute((items[0], items[1]));
            Designer.Connect.Execute((items[0], items[2]));
            Designer.Connect.Execute((items[0], items[3]));
            Designer.Connect.Execute((items[0], items[4]));

            await base.OnInitializedAsync();
        }
    }
}
