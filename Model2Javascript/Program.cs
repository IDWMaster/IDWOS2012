using System;
using System.Collections.Generic;
using System.Text;
using _3DAPI;
using System.Windows.Forms;
namespace ModelConverter
{

    class Program
    {
        class FloatingPointValue : JSONObject
        {
            public FloatingPointValue(float val)
            {
                _val = val;
            }
            public override string Value
            {
                get { return _val.ToString(); }
            }
            float _val;
        }
        [STAThread]
        static void Main(string[] args)
        {



            Mesh[] meshes = Primitives.LoadMesh("simplemodel.obj", true);
            RootElement main = new RootElement();
            ulong id = 0;
            List<RootElement> elems = new List<RootElement>();

            foreach (Mesh et in meshes)
            {

                RootElement meshmain = new RootElement();
                elems.Add(meshmain);
                List<FloatingPointValue> vlas = new List<FloatingPointValue>();
                foreach (Vector3D ett in et.meshverts)
                {
                    vlas.Add(new FloatingPointValue(ett.X));
                    vlas.Add(new FloatingPointValue(ett.Y));
                    vlas.Add(new FloatingPointValue(ett.Z));
                }
                JSONArray vertices = new JSONArray(vlas.ToArray());
                vlas.Clear();
                foreach (Vector3D ett in et.meshnorms)
                {
                    vlas.Add(new FloatingPointValue(ett.X));
                    vlas.Add(new FloatingPointValue(ett.Y));
                    vlas.Add(new FloatingPointValue(ett.Z));
                }
                JSONArray normals = new JSONArray(vlas.ToArray());
                vlas.Clear();
                
                foreach (Vector2D ett in et.meshtexas)
                {
                    vlas.Add(new FloatingPointValue(ett.X));
                    vlas.Add(new FloatingPointValue(ett.Y));
                }
                JSONArray texcoords = new JSONArray(vlas.ToArray());
                meshmain.children.Add("vertices", vertices);
                meshmain.children.Add("texcoords", texcoords);
                meshmain.children.Add("normals", normals);
                id++;

            }
            main.children.Add("meshes", new JSONArray(elems.ToArray()));

            Clipboard.SetText(main.Serialize());
            Console.WriteLine("Copied to clipboard");
            Console.WriteLine(main.Serialize());
            Console.ReadKey();
        }
    }
}
