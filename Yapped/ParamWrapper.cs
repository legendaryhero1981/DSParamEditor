using SoulsFormats;
using System;
using System.Collections.Generic;

namespace Yapped
{
    internal class ParamWrapper : IComparable<ParamWrapper>
    {
        public bool Error { get; }

        public string Name { get; }

        public string Description { get; }

        public PARAM Param;
        [Obsolete]
        public PARAM.Layout Layout;

        public PARAMDEF Paramdef;

        public List<PARAMTDF> Paramtdfs;

        public List<PARAM.Row> Rows => Param.Rows;

        [Obsolete]
        public ParamWrapper(string name, PARAM param, PARAM.Layout layout, string description)
        {
            if (layout == null || layout.Size != param.DetectedSize)
            {
                layout = new PARAM.Layout
                {
                    new PARAM.Layout.Entry(PARAM.CellType.dummy8, "Unknown", (int) param.DetectedSize, null)
                };
                Error = true;
            }

            Name = name;
            Param = param;
            Layout = layout;
            Paramdef = Layout.ToParamdef(name, out Paramtdfs);
            Param.ApplyParamdef(Paramdef);
            Description = description;
        }

        public int CompareTo(ParamWrapper other) => string.Compare(Name, other.Name, StringComparison.Ordinal);
    }
}
