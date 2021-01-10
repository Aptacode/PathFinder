using System.Drawing;
using System.Numerics;
using System.Threading.Tasks;
using Aptacode.Geometry.Blazor.Components.ViewModels.Components.Primitives;
using Aptacode.Geometry.Primitives;
using Microsoft.AspNetCore.Components;
using Rectangle = Aptacode.Geometry.Primitives.Polygons.Rectangle;

namespace PathFinder.BlazorDemo.Pages
{
    public class IndexBase : ComponentBase
    {
        #region Properties

        public PathFinderSceneController SceneController { get; set; }
        private const int Width = 100;
        private const int Height = 100;
        
        #endregion

        protected override async Task OnInitializedAsync()
        {
            SceneController = new PathFinderSceneController(new Vector2(Width, Height));

            await base.OnInitializedAsync();
        }
    }
}