using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace sdkmeshToOBJ
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 1) {
                System.Console.WriteLine("sdkmeshToOBJ v1.0 - Copyright gradientspace / Ryan Schmidt 2017");
                System.Console.WriteLine("Questions? Comments? www.gradientspace.com or @gradientspace");
                System.Console.WriteLine("usage: sdkmeshToOBJ.exe <file.sdkmesh>");
                return;
            }

            string sFilename = args[0];
            string sOutfilename = sFilename + ".obj";

            System.Console.WriteLine("Reading " + sFilename);
            SdkMesh sdk_mesh = new SdkMesh(sFilename);

            int nMeshes = sdk_mesh.Meshes.Count;
            System.Console.WriteLine("Found {0} Meshes", nMeshes);

            if ( nMeshes != 1 ) {
                System.Console.WriteLine("currently only support converting the first mesh! ignoring others.");
            }

            int mi = 0;
            foreach ( SdkMesh.SdkMeshMesh mesh in sdk_mesh.Meshes ) {

                string sMeshFilename = sOutfilename;
                if (nMeshes != 1) {
                    sMeshFilename.Insert(sMeshFilename.LastIndexOf('.'), string.Format("_{0}", mi));
                }
                mi++;

                int iIndexBuffer = (int)mesh.IndexBuffer;
                SdkMesh.SdkMeshIndexBuffer indices = sdk_mesh.IndexBuffers[iIndexBuffer];
                int nTriangles = (int)indices.NumIndices/3;

                if (mesh.NumVertexBuffers != 1) {
                    System.Console.WriteLine("currently only support a single vertex buffer! aborting.");
                    return;
                }
                int iVtxBuffer =  (int)mesh.VertexBuffers[0];
                SdkMesh.SdkMeshVertexBuffer vbuffer = sdk_mesh.VertexBuffers[iVtxBuffer];
                int nVertices = (int)vbuffer.NumVertices;

                StreamWriter writer = new StreamWriter(sOutfilename);

                for ( int i = 0; i < nVertices; ++i ) {
                    var v = vbuffer.Vertices[i].Pos;
                    writer.WriteLine("v {0} {1} {2}", v.X, v.Y, v.Z);
                    var uv = vbuffer.Vertices[i].Tex;
                    writer.WriteLine("vt {0} {1}", uv.X, uv.Y);
                }

                for ( int i = 0; i < nTriangles; ++i ) {
                    int a = indices.Indices[3*i];
                    a++;
                    int b = indices.Indices[3 * i + 1];
                    b++;
                    int c = indices.Indices[3 * i + 2];
                    c++;
                    writer.WriteLine("f {0}/{0} {1}/{1} {2}/{2}", a, b, c);
                }

                writer.Close();
            }

        }
    }
}
