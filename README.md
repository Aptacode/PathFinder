<p align="center">
   <div style="width:640;height:320">
       <img style="width: inherit" src="https://raw.githubusercontent.com/Aptacode/PathFinder/Production/Resources/Images/Banner.jpg">
</div>
</p>

PathFinder is an optimized C# implementation of the [A* Search Algorithm](https://en.wikipedia.org/wiki/A*_search_algorithm). In its current form it can be utilised to quickly find the shortest path between two points on a uniform grid with potential obstacles between the start and end point.

[Live Demo](https://aptacode.github.io/PathFinder/)

[![Codacy Badge](https://app.codacy.com/project/badge/Grade/1e520860e7f64f17bb523e6b8fae72b6)](https://www.codacy.com/gh/Aptacode/PathFinder/dashboard?utm_source=github.com&amp;utm_medium=referral&amp;utm_content=Aptacode/PathFinder&amp;utm_campaign=Badge_Grade)
[![NuGet](https://img.shields.io/nuget/v/Aptacode.PathFinder.svg?style=flat)](https://www.nuget.org/packages/Aptacode.PathFinder/)
![last commit](https://img.shields.io/github/last-commit/Aptacode/PathFinder?style=flat-square&cacheSeconds=86000)
## Overview

The main `PathFinder` class consists of a `Map` and `INeighbourFinder`. The `Map` is the class that contains all the information about the grid in which a path is to be found:

```csharp
var map = new Map(new Vector2(20, 20), //The dimensions of the map
                  new Vector2(0, 0), //The starting position of the path
                  new Vector2(19, 19), //The end/target position of the path
                  new Obstacle(Guid.NewGuid(), //An obstacle in the grid
                               new Vector2(5, 5), //The obstacle's position, this is set from the top lefthand corner.
                               new Vector2(10, 10))); //The dimensions of the obstacle.
```

Image of map above can go here.

`INeighbourFinder` is used by the `PathFinder` class to determine which nodes will be considered neighbours of the open node being acted on by the A* algorithm. It is also used to set the cost (sometimes called the **f** cost) of movement from a node to its neighbour. There are currently two implementations of `INeighbourFinder` in PathFinder: `DefaultNeighbourFinder` and `JumpPointSearchNeighbourFinder`.  The `DefaultNeighbourFinder` class can be used when the neighbouring nodes of node are the nodes adjacent to that node. The nodes considered to be adjacent will depend on the direction of movement allowed from one node to another.

```csharp
DefaultNeighbourFinder.Straight(1.0f) //Movement is only permitted horizontally and vertically, the cost of this movement is 1.
DefaultNeighbourFinder.Diagonal(1.0f) //Movement is only permitted diagonally, the cost of this movement is 1.
DefaultNeighbourFinder.All(1.0f, 1.41f) //Movement is permitted in all directions, the cost of horizontal or vertical movement is 1, the cost of diagonal movement is 1.41 (An approximation for the square root of 2).
```
Maybe some images to example the above here.



## Examples

PathFinder uses Aptacode's [FlowDesigner](https://github.com/Aptacode/FlowDesigner) to visualise the grid on which the path is found and the path itself. Here is an example of path on a grid where each node had a 20% chance of being obstructed:

Image will go here.




## License
[MIT](https://choosealicense.com/licenses/mit/)
