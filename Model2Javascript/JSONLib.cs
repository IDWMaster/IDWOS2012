using System;
using System.Collections.Generic;
using System.Text;

namespace ModelConverter
{
    public abstract class JSONObject
    {

        public abstract string Value
        {
            get;
        }
        public Dictionary<string, JSONObject> children = new Dictionary<string, JSONObject>();
        public string Serialize()
        {
            StringBuilder mbuilder = new StringBuilder();
            JSONObject currentObject = this;
            if (children.Count > 0)
            {
                mbuilder.AppendLine("{");
            }
            if (children.Count > 0)
            {
                mbuilder.AppendLine(Value);
            }
            else
            {
                mbuilder.Append(Value);
            }
            foreach (KeyValuePair<string, JSONObject> et in children)
            {

                mbuilder.AppendLine("\"" + et.Key + "\"" + ":" + et.Value.Serialize() + ",");

            }
            if (children.Count > 0)
            {
                mbuilder.Remove(mbuilder.Length - 3, 1);
            }
            if (children.Count > 0)
            {
                mbuilder.AppendLine("}");
            }
            return mbuilder.ToString();

        }
    }
    public class RootElement : JSONObject
    {
        public override string Value
        {
            get { return ""; }
        }
    }
    public class JSONString : JSONObject
    {
        public JSONString(string value)
        {
            _value = value;

        }
        string _value;

        public override string Value
        {
            get { return "\"" + _value + "\""; }
        }
    }
    public class JSONArray : JSONObject
    {
        public JSONArray(IEnumerable<JSONObject> _values)
        {
            values = _values;
        }
        public override string Value
        {
            get
            {
                StringBuilder mb = new StringBuilder();
                mb.Append("[");
                foreach (JSONObject et in values)
                {
                    mb.Append(et.Serialize() + ",");
                }
                mb.Remove(mb.Length - 1, 1);
                mb.Append("]");
                return mb.ToString();
            }
        }
        IEnumerable<JSONObject> values;
    }
}
