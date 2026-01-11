using PdfSharp.Fonts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace barcode_gen.Fonts.Resolvers
{
    internal class CustomFontResolver : IFontResolver
    {
        public byte[] GetFont(string faceName)
        {
            var asm = Assembly.GetExecutingAssembly();

            // имя ресурса — смотри в свойствах файла!!!
            const string resourceName = "barcode_gen.Fonts.TIMES.TTF";

            using (var stream = asm.GetManifestResourceStream(resourceName))
            using (var ms = new MemoryStream())
            {
                stream.CopyTo(ms);
                return ms.ToArray();
            }
        }

        public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
        {
            if (familyName.Equals("Times New Roman", StringComparison.OrdinalIgnoreCase))
            {
                return new FontResolverInfo("TNR#Regular");
            }

            return null;
        }
    }
}
