using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using System.Drawing.Drawing2D;
using g3;

namespace gsPolyFontGenerator
{
    class Program
    {

        static void print_usage()
        {
            System.Console.WriteLine("gsPolyFontGenerator v1.0 - Copyright gradientspace / Ryan Schmidt 2017");
            System.Console.WriteLine("Questions? Comments? www.gradientspace.com or @gradientspace");
            System.Console.WriteLine("usage: gsPolyFontGenerator options <output_filename.bin>");
            System.Console.WriteLine("options:");
            System.Console.WriteLine("  -emSize <int>    : emSize of font. larger emSize results in more segments for curves");
            System.Console.WriteLine("  -font <fontname> : string name of font. If it crashes, you don't have that font");
            System.Console.WriteLine("  -style <style>   : valid styles are 'bold', 'italic', 'regular'");
            System.Console.WriteLine("  -string <s>      : \"font\" will contain a single character that is this entire string");
        }


        static void Main(string[] args)
        {
            // [RMS] if true, we try to read back the file we wrote to make sure that works
            bool bTestRead = true;


            CommandArgumentSet arguments = new CommandArgumentSet();
            arguments.Register("-string", "");
            arguments.Register("-font", "Consolas");
            arguments.Register("-emSize", 64);
            arguments.Register("-style", "regular");
            if (arguments.Parse(args) == false) {
                return;
            }

            if (arguments.Filenames.Count != 1) {
                print_usage();
                return;
            }
            string outputFilename = arguments.Filenames[0];


            string fontName = "Consolas";
            if (arguments.Saw("-font"))
                fontName = arguments.Strings["-font"];


            int emSize = 64;
            if ( arguments.Saw("-emSize")) 
                emSize = arguments.Integers["-emSize"];


            FontStyle style = FontStyle.Regular;
            if ( arguments.Saw("-style") ) {
                string s = arguments.Strings["-style"];
                if (s.Equals("bold", StringComparison.OrdinalIgnoreCase))
                    style = FontStyle.Bold;
                else if (s.Equals("italic", StringComparison.OrdinalIgnoreCase))
                    style = FontStyle.Italic;
                else if (s.Equals("regular", StringComparison.OrdinalIgnoreCase) == false) {
                    System.Console.WriteLine("Unknown font style '{0}'", s);
                    return;
                }
            }



            // [RMS] these are the characters that will be in the output, unless...
            string characters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789.,;:-+=!?()[]<>{}|~@#$%^&*_/\\\"'`©™";
            string[] strings = new string[characters.Length];
            for (int k = 0; k < characters.Length; ++k)
                strings[k] = characters.Substring(k, 1);

            // override with a fixed string
            if ( arguments.Saw("-string") ) {
                strings = new string[1];
                strings[0] = arguments.Strings["-string"];
            }


            // generate polygons for each character and add to font
            PolygonFont2d Font = new PolygonFont2d();
            try {
                for (int k = 0; k < strings.Length; ++k) {
                    string s = strings[k];
                    List<GeneralPolygon2d> loops = GetPolygons(s, fontName, style, emSize);
                    Font.AddCharacter(s, loops.ToArray());
                }
            } catch (Exception e) {
                System.Console.WriteLine("Exception generating font: " + e.Message);
            }

            // write font to output file
            try {
                using (FileStream file_stream = File.Open(outputFilename, FileMode.Create)) {
                    BinaryWriter w = new BinaryWriter(file_stream);
                    PolygonFont2d.Store(Font, w);
                }
            } catch(Exception e) {
                System.Console.WriteLine("Exception serializing font: " + e.Message);
            }

            // read font back in to test
            if (bTestRead) {
                try {
                    using (FileStream file_stream = File.Open(outputFilename, FileMode.Open)) {
                        BinaryReader binReader = new BinaryReader(file_stream);
                        PolygonFont2d newfont = new PolygonFont2d();
                        PolygonFont2d.Restore(newfont, binReader);
                    }
                } catch (Exception e) {
                    System.Console.WriteLine("Error verifying font read: " + e.Message);
                }
            }

            // print status
            if ( strings.Length == 1 )
                System.Console.WriteLine("Wrote string \"{0}\" to {1}", strings[0], outputFilename);
            else
                System.Console.WriteLine("Wrote {0} font characters to {1}", Font.Characters.Count, outputFilename);
        }


        /// <summary>
        /// Construct the set of polygons-with-holes that represents the input string
        /// </summary>
        static List<GeneralPolygon2d> GetPolygons(string s, string fontName = "Consolas", FontStyle style = FontStyle.Regular, int emSize = 128)
        {
            GraphicsPath path = new GraphicsPath();

            string text = s;
            FontFamily font = new FontFamily(fontName);
            Point origin = new Point(0, 0);
            StringFormat format = new StringFormat();

            path.AddString(text, font, (int)style, emSize, origin, format);
            path.Flatten();

            PlanarComplex complex = new PlanarComplex();


            Polygon2d cur_poly = new Polygon2d();
            for (int i = 0; i < path.PathPoints.Length; ++i) {
                PointF pt = path.PathPoints[i];
                Vector2d v = new Vector2d(pt.X, pt.Y);

                int type = path.PathTypes[i] & 0x7;
                int flags = path.PathTypes[i] & 0xF8;

                if (type == 0) {
                    cur_poly = new Polygon2d();
                    cur_poly.AppendVertex(v);
                } else if (type == 1) {
                    cur_poly.AppendVertex(v);
                    if ((flags & 0x80) != 0) {
                        // [RMS] some characters have a duplicate start/end point after Flatten(). Some do not. Clusterfuck!
                        if ( cur_poly.Start.Distance(cur_poly.End) < 0.001 )
                            cur_poly.RemoveVertex(cur_poly.VertexCount - 1);  // we just duplicated first point
                        complex.Add(cur_poly);
                    }
                }
            }


            PlanarComplex.SolidRegionInfo solids = complex.FindSolidRegions(0.0f, false);
            List<GeneralPolygon2d> result = solids.Polygons;
            foreach (var gp in result)
                gp.Transform((v) => { return new Vector2d(v.x, emSize - v.y); });

            return result;
        }

    }
}
