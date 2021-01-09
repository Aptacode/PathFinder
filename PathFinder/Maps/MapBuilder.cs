using System;
using System.Collections.Generic;
using System.Numerics;
using Aptacode.Geometry.Blazor.Components.ViewModels.Components;
using Aptacode.PathFinder.Maps.Validation;
using Aptacode.PathFinder.Resources;

namespace Aptacode.PathFinder.Maps
{
    public class MapBuilder
    {
        private readonly List<ComponentViewModel> _obstacles;
        private Vector2 _dimensions;
        private Vector2 _end;
        private Vector2 _start;

        public MapBuilder()
        {
            _obstacles = new List<ComponentViewModel>();
            _dimensions = Vector2.Zero;
            _start = Vector2.Zero;
            _end = Vector2.Zero;
        }

        public MapBuilder SetStart(int x, int y)
        {
            _start = new Vector2(x, y);
            return this;
        }

        public MapBuilder SetEnd(int x, int y)
        {
            _end = new Vector2(x, y);
            return this;
        }

        public MapBuilder SetDimensions(int width, int height)
        {
            _dimensions = new Vector2(width, height);
            return this;
        }

        public MapBuilder SetStart(Vector2 position)
        {
            _start = position;
            return this;
        }

        public MapBuilder SetEnd(Vector2 position)
        {
            _end = position;
            return this;
        }

        public MapBuilder SetDimensions(Vector2 dimension)
        {
            _dimensions = dimension;
            return this;
        }

        public MapBuilder AddObstacle(ComponentViewModel obstacle)
        {
            var newObstacle = obstacle;
            _obstacles.Add(newObstacle);
            return this;
        }

        private MapResult CreateMap()
        {
            try
            {
                var map = new Map(_dimensions, _start, _end, _obstacles.ToArray());
                var mapValidationResult = map.IsValid();
                
                return mapValidationResult.Success ?
                    MapResult.Ok(map, GeneralMessages.Success) :
                    MapResult.Fail(mapValidationResult.Message);
            }
            catch (Exception ex)
            {
                return MapResult.Fail(ex.Message);
            }
        }

        public MapResult Build()
        {
            var mapResult = CreateMap();
            Reset();
            return mapResult;
        }

        public void Reset()
        {
            _obstacles.Clear();
            _dimensions = Vector2.Zero;
            _start = Vector2.Zero;
            _end = Vector2.Zero;
        }
    }
}