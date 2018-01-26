# gsMeshUtilities

Open-Souce (MIT License) command-line utilities for mesh and geometry things.

Questions? Contact Ryan Schmidt [@rms80](http://www.twitter.com/rms80) / [gradientspace](http://www.gradientspace.com)


# General Notes

You can find pre-built windows executables in the top-level **builds** folder.


# gsMeshSimplify

Reduce triangle count for a mesh. Currently supports *OBJ*, *STL*, and *OFF*. 

Usage:
```
gsMeshSimplify options <inputmesh>
options:
  -percent <N>        : reduce to this percentage (real-value)
  -tcount <N>         : reduce to this triangle count (int)
  -output <filename>  : output filename - default is inputmesh.reduced.fmt
  -v                  : verbose
```



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



# gsPolyFontGenerator

Generate polygon font file. Each font element is stored as a set of [geometry3Sharp](https://github.com/gradientspace/geometry3Sharp) **GeneralPolygon2d** objects. The set of available elements (ie letterforms) is stored in a g3Sharp **PolygonFont2d** object, which is directly serialized. Use **PolygonFont2d.ReadFont(filename)** to load the font file. The storage format is binary and subject to change if serialization of the above changes. A version number is stored in the binary file, it is the first 32-bit integer and currently is 3.

Usage: **gsPolyFontGenerator.exe** *options* *output_filename.bin*

Options:

* `-emsize <int>`    emSize of font. larger emSize results in more segments for curves
* `-font <fontname>`    string name of font. If it crashes, you don't have that font
* `-style <style>`      valid styles are `bold`, `italic`, `regular`
* `-string <s>`         font will contain a single "character" that is this entire string 

By default the font contains the characters in this string (you can modify it in the code) 
```
string characters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789.,;:-+=!?()[]<>{}|~@#$%^&*_/\\\"'`©™";
```
Each letter is generated separately in whatever coordinates System.Drawing.GraphicsPath uses. No character spacing info is provided, I tend to use this with monospace fonts and a constant spacing, and calculate my own geometric transformations of each character.

*However*, if you want proper layout for a specific string, then you can use the `-string` option. In this case the "font" will be a single "character" which contains polygons for the full string, with appropriate positioning. If you would like multiple such strings in a single font file, hardcode them into the `strings` array in Program.cs. *Or improve option handling and submit a PR!*

# sdkmeshToOBJ

Microsoft has released some awesome mesh-processing tools under the MIT license, in 
particular https://github.com/Microsoft/UVAtlas is a fantastic tool for auto-generating UV maps for
unstructured triangle meshes. However, this tool only outputs a weird *.sdkmesh* format that almost
no other tool supports. So, *sdkmeshToOBJ* converts this format to a standard OBJ with UV-coordinates.

Usage: **sdkmeshToOBJ.exe** *your_file.sdkmesh*

Output is **file.sdkmesh.obj**

Current Limitations: only the first mesh in the sdkmesh file is converted. And, it appears that sdkmesh files can contain multiple vertex buffers for a single mesh, but that isn't supported either.


