using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using g3;

namespace gsMeshSplit
{


    class Program
    {

        static void print_usage()
        {
            System.Console.WriteLine("gsMeshSplit v1.0 - Copyright gradientspace / Ryan Schmidt 2017");
            System.Console.WriteLine("Questions? Comments? www.gradientspace.com or @gradientspace");
            System.Console.WriteLine("usage: gsMeshSplit options <input_mesh.obj>");
            System.Console.WriteLine("options:");
            System.Console.WriteLine("  -output <directory_path>     : path to output directory");
        }

        //
        // [TODO]
        //
        static void Main(string[] args)
        {
            CommandArgumentSet arguments = new CommandArgumentSet();
            arguments.Register("-output", "");
            if ( arguments.Parse(args) == false ) {
                return;
            }
            if ( arguments.Filenames.Count != 1 ) {
                print_usage();
                return;
            }

            string sInputFile = arguments.Filenames[0];
			string sFilenameRoot = Path.GetFileNameWithoutExtension(sInputFile);
            if (!File.Exists(sInputFile)) {
                System.Console.WriteLine("cannot find file " + sInputFile);
                return;
            }


            DMesh3Builder builder = new DMesh3Builder();
            StandardMeshReader reader = new StandardMeshReader() { MeshBuilder = builder };
            ReadOptions read_options = ReadOptions.Defaults;
			read_options.ReadMaterials = true;
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

            // [TODO] out if count == 0

            string sOutRoot = arguments.Strings["-output"];
            if (sOutRoot.Length > 0) {
                bool bOutIsFolder = Directory.Exists(sOutRoot);
                if (!bOutIsFolder) {
                    System.Console.WriteLine("-output folder {0} does not exist", sOutRoot);
                    return;
                }
            }

			Dictionary<int, List<int>> MeshesByMaterial = new Dictionary<int, List<int>>();
			MeshesByMaterial[-1] = new List<int>();
			for ( int i = 0; i < builder.Materials.Count; ++i )
				MeshesByMaterial[i] = new List<int>();

			int N = builder.Meshes.Count;
			for ( int i = 0; i < N; ++i ) {
				int mati = builder.MaterialAssignment[i];
				if ( mati >= builder.Materials.Count )
					mati = -1;
				MeshesByMaterial[mati].Add(i);
			}

			int file_i = 0;
			foreach ( int mat_i in MeshesByMaterial.Keys ) {

				List<int> mesh_idxs = MeshesByMaterial[mat_i];
				if ( mesh_idxs.Count == 0 )
					continue;
				
				WriteMesh[] write_meshes = new WriteMesh[mesh_idxs.Count];
				for ( int i = 0; i < mesh_idxs.Count; ++i )
					write_meshes[i] = new WriteMesh(builder.Meshes[mesh_idxs[i]]);

				string suffix = string.Format("_material{0}", file_i++);
				string sOutPath = Path.Combine(sOutRoot, sFilenameRoot + suffix + ".obj");

				StandardMeshWriter writer = new StandardMeshWriter();
				WriteOptions write_options = WriteOptions.Defaults;
				if ( mat_i != -1 ) {
					write_options.bWriteMaterials = true;
					write_options.bPerVertexUVs = true;
					write_options.MaterialFilePath = Path.Combine(sOutRoot, sFilenameRoot + suffix + ".mtl");

					GenericMaterial mat = builder.Materials[mat_i];
					List<GenericMaterial> matList = new List<GenericMaterial>() { mat };
					ConstantIndexMap idxmap = new ConstantIndexMap(0);

					for (int i = 0; i < write_meshes.Length; ++i ) {
						write_meshes[i].Materials = matList;
						write_meshes[i].TriToMaterialMap = idxmap;
					}
				}
				IOWriteResult writeOK = writer.Write(sOutPath, new List<WriteMesh>(write_meshes), write_options);
				if ( writeOK.code != IOCode.Ok) {
					System.Console.WriteLine("Error writing " + sOutPath);
				    System.Console.WriteLine(writeOK.message);
				}
			}


            // ok done!
            //System.Console.ReadKey();
        }
    }
}
