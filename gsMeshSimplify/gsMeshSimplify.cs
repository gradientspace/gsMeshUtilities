using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using System.Drawing.Drawing2D;
using g3;

namespace gsMeshSimplify
{
    class gsMeshSimplify
    {

        static void print_usage()
        {
            System.Console.WriteLine("gsMeshSimplify v1.0 - Copyright gradientspace / Ryan Schmidt 2018");
            System.Console.WriteLine("Questions? Comments? www.gradientspace.com or @gradientspace");
            System.Console.WriteLine("usage: gsMeshSimplify options <inputmesh>");
            System.Console.WriteLine("options:");
            System.Console.WriteLine("  -percent <N>        : reduce to this percentage (real-value)");
            System.Console.WriteLine("  -tcount <N>         : reduce to this triangle count (int)");
            System.Console.WriteLine("  -output <filename>  : output filename - default is inputmesh.reduced.fmt");
            System.Console.WriteLine("  -v                  : verbose ");
        }


        static void Main(string[] args)
        {
            CommandArgumentSet arguments = new CommandArgumentSet();
            arguments.Register("-tcount", int.MaxValue);
            arguments.Register("-percent", 50.0f);
            arguments.Register("-v", false);
            arguments.Register("-output", "");
            if (arguments.Parse(args) == false) {
                return;
            }

            if (arguments.Filenames.Count != 1) {
                print_usage();
                return;
            }
            string inputFilename = arguments.Filenames[0];
            if (! File.Exists(inputFilename) ) {
                System.Console.WriteLine("File {0} does not exist", inputFilename);
                return;
            }


            string outputFilename = Path.GetFileNameWithoutExtension(inputFilename);
            string format = Path.GetExtension(inputFilename);
            outputFilename = outputFilename + ".reduced" + format;
            if (arguments.Saw("-output")) {
                outputFilename = arguments.Strings["-output"];
            }


            int triCount = int.MaxValue;
            if ( arguments.Saw("-tcount"))
                triCount = arguments.Integers["-tcount"];

            float percent = 50.0f;
            if (arguments.Saw("-percent"))
                percent = arguments.Floats["-percent"];

            bool verbose = false;
            if (arguments.Saw("-v"))
                verbose = arguments.Flags["-v"];


            List<DMesh3> meshes;
            try {
                DMesh3Builder builder = new DMesh3Builder();
                IOReadResult result = StandardMeshReader.ReadFile(inputFilename, ReadOptions.Defaults, builder);
                if (result.code != IOCode.Ok) {
                    System.Console.WriteLine("Error reading {0} : {1}", inputFilename, result.message);
                    return;
                }
                meshes = builder.Meshes;
            } catch (Exception e) {
                System.Console.WriteLine("Exception reading {0} : {1}", inputFilename, e.Message);
                return;
            }
            if ( meshes.Count == 0 ) {
                System.Console.WriteLine("file did not contain any valid meshes");
                return;
            }

            DMesh3 mesh = meshes[0];
            for (int k = 1; k < meshes.Count; ++k)
                MeshEditor.Append(mesh, meshes[k]);
            if ( mesh.TriangleCount == 0 ) {
                System.Console.WriteLine("mesh does not contain any triangles");
                return;
            }

            if (verbose)
                System.Console.WriteLine("initial mesh contains {0} triangles", mesh.TriangleCount);

            Reducer r = new Reducer(mesh);

            if (triCount < int.MaxValue) {
                if (verbose)
                    System.Console.Write("reducing to {0} triangles...", triCount);
                r.ReduceToTriangleCount(triCount);
            } else {
                int nT = (int)((float)mesh.TriangleCount * percent / 100.0f);
                nT = MathUtil.Clamp(nT, 1, mesh.TriangleCount);
                if ( verbose )
                    System.Console.Write("reducing to {0} triangles...", nT);
                r.ReduceToTriangleCount(nT);
            }

            if (verbose)
                System.Console.WriteLine("done!");

            try {
                IOWriteResult wresult =
                    StandardMeshWriter.WriteMesh(outputFilename, mesh, WriteOptions.Defaults);
                if (wresult.code != IOCode.Ok) {
                    System.Console.WriteLine("Error writing {0} : {1}", inputFilename, wresult.message);
                    return;
                }
            } catch (Exception e) {
                System.Console.WriteLine("Exception reading {0} : {1}", inputFilename, e.Message);
                return;
            }

            return;
        }
        
    }
}
