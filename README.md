# gsMeshUtilities

Open-Souce (MIT License) command-line utilities for mesh things.

Currently just one tool, in C#, but there is more to come, and probably there will be some C++ too...

Questions? Contact Ryan Schmidt [@rms80](http://www.twitter.com/rms80) / [gradientspace](http://www.gradientspace.com)


# General Notes

You can find pre-built windows executables in the top-level **builds** folder.


# gsMeshConvert

Convert between mesh formats. Currently supports *OBJ*, *STL*, and *OFF*. 

Usage: **gsMeshConvert.exe** *inputmesh.ext* *outputmesh.ext*

(pretty basic so far)

# gsMeshSplit

Some (most?) tools that support textured OBJ meshes have limitations in some way. For example, many (eg xNormal) do not support having multiple texture images (ie materials) for a single file. In addition many rendering tools do not support OBJs with multiple UV values per vertex. This utility takes an input OBJ/MTL pair with these properties and produces a set of "simple" output OBJ/MTL files - one material per OBJ, no shared vertices along UV seams.

Usage: **gsMeshSplit.exe** (options) input.obj

Options:
**-output** *directory_path* - output folder for set of OBJ files

The set of output files for input.obj will be named input_material0.obj, input_material1.obj, each with a .mtl file having the same name. 

Note that this utility does not handle the texture images at all. It doesn't open them, doesn't copy them, etc. Only the OBJ and MTL files are read and generated.


# sdkmeshToOBJ

Microsoft has released some awesome mesh-processing tools under the MIT license, in 
particular https://github.com/Microsoft/UVAtlas is a fantastic tool for auto-generating UV maps for
unstructured triangle meshes. However, this tool only outputs a weird *.sdkmesh* format that almost
no other tool supports. So, *sdkmeshToOBJ* converts this format to a standard OBJ with UV-coordinates.

Usage: **sdkmeshToOBJ.exe** *your_file.sdkmesh*

Output is **file.sdkmesh.obj**

Current Limitations: only the first mesh in the sdkmesh file is converted. And, it appears that sdkmesh files can contain multiple vertex buffers for a single mesh, but that isn't supported either.


