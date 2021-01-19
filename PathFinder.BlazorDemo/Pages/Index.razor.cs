using System.Numerics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace PathFinder.BlazorDemo.Pages
{
    public class IndexBase : ComponentBase
    {
        protected override async Task OnInitializedAsync()
        {
            SceneController = new PathFinderSceneController(new Vector2(Width, Height));

            await base.OnInitializedAsync();
        }

        #region Properties

        public PathFinderSceneController SceneController { get; set; }
        private const int Width = 200;
        private const int Height = 200;

        #endregion
    }
}