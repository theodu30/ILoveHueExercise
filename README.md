# I Love Hue
This project is an attempt at recreating I Love Hue by Zut Games.  
It was done in 3.5 hours as a class exercise.  

## Features
This project contains 5 different levels with various difficulties.  
The game is entirely done in UI.  
You can set all four corner colors, and the level handles the gradients.  
You can drag and drop to exchange two tiles.

## Limitations
If you rescale the game, the UI does not scale accordingly.

## Known Issues
Apparently, you can still exchange normal tiles with locked ones.  
The code needs refactoring, LevelGenerator.cs both generates the level  
and handles the gameplay.  
Tiles are sometimes misaligned and I don't know why.  
On levels where menu buttons and non-locked tiles overlap, dragging  
moves the tile to the new position without exchanging.

#
> Disclaimer:  
*This was a project made in class.  
It's an exercise to help us understand how to make good prototypes in a short time.  
This project is now on GitHub for accessibility and visibility.*
