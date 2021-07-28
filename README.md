# textureAtlasTools
A set of tools for slicing a texture atlas to individual components and merging back

## Description
These tools simplify the approach for upscaling a texture atlas.
The workflow would be something like this:
- manually select some tiles in the texture atlas (that are placed in difficult positions)
- add them to a json file following this pattern: [json file](tex1_512x256_B20814E2D6573DFE_0.json); you can use any image editing tool such as GIMP to manually select and inspect the location, width and height of each selection
- run the `textureAtlas-slicer` tool; this will automatically create a folder with separate files from the selections, as well a `[basefilename]_sliced.png` file containing the texture without the sliced files
- manually upscale all the resulting files with the correct transparency / seamless mode, including the `[basefilename]_sliced.png` from the previous step. it is important to use the same scale factor for all the files. it is also important to scale all the files, even if using a nearest neighbor filtering
- run the `textureAtlas-merger` tool; this will automatically add back the separated upscaled tiles to the upscaled texture atlas. `[basefilename]_final.png` will be created

## TODO
- proper error handling
- code cleanup

## _Use it at your own risk_
