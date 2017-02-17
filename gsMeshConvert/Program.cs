using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using g3;

namespace gsMeshConvert
{
    class Program
    {
        //
        // [TODO]
        //   - binary output option
        //   - option to strip data from inputs (eg remove normals/colors/uv/material from obj)
        //   - option to remove material props from OBJ
        //   - option to combine input meshes
        //   - option to set float precision
        //   - option to estimate normals for writing (eg for obj)
        //   - option to set constant color for vertices
        //
        static void Main(string[] args)
        {
            if (args.Length != 2) {
                System.Console.WriteLine("gsMeshConvert v1.0 - Copyright gradientspace / Ryan Schmidt 2017");
                System.Console.WriteLine("Questions? Comments? www.gradientspace.com or @gradientspace");
                System.Console.WriteLine("usage: gsMeshConvert <input_mesh.format> (output_mesh.format)");
                return;
            }

            string sInputFile = args[0];
            if (!File.Exists(sInputFile)) {
                System.Console.WriteLine("cannot find file " + sInputFile);
                return;
            }

            string sOutputFile = args[1];
            // check that can write output file


            DMesh3Builder builder = new DMesh3Builder();
            StandardMeshReader reader = new StandardMeshReader() { MeshBuilder = builder };
            ReadOptions read_options = ReadOptions.Defaults;
            IOReadResult readOK = reader.Read(sInputFile, read_options);
            if ( readOK.code != IOCode.Ok ) {
                System.Console.WriteLine("Error reading " + sInputFile);
                System.Console.WriteLine(readOK.message);
                return;
            }

            if ( builder.Meshes.Count == 0 ) {
                System.Console.WriteLine("did not find any valid meshes in " + sInputFile);
                return;
            }

            List<WriteMesh> write_meshes = new List<WriteMesh>();
            foreach (DMesh3 mesh in builder.Meshes)
                write_meshes.Add(new WriteMesh(mesh));

            StandardMeshWriter writer = new StandardMeshWriter();
            WriteOptions write_options = WriteOptions.Defaults;
            IOWriteResult writeOK = writer.Write(sOutputFile, write_meshes, write_options);
            if ( writeOK.code != IOCode.Ok) {
                System.Console.WriteLine("Error writing " + sOutputFile);
                System.Console.WriteLine(writeOK.message);
                return;
            }

            // ok done!
            //System.Console.ReadKey();
        }
    }
}
