using PdfSharp.Fonts;
using System;
using System.IO;
using System.Reflection;

namespace barcode_gen.Fonts.Resolvers
{
    internal class CustomFontResolver : IFontResolver
    {
        public byte[] GetFont(string faceName)
        {
            var asm = Assembly.GetExecutingAssembly();
            string resourceName = "";

            // Выбираем нужный ресурс по ключу из ResolveTypeface
            switch (faceName)
            {
                // Кейсы для Times New Roman
                case "times new roman":
                    resourceName = "barcode_gen.Fonts.TIMES.TTF"; // Обычный
                    break;
                case "times new roman_bold":
                    resourceName = "barcode_gen.Fonts.TIMESBDD.TTF.ttf"; // Жирный (ваш файл)
                    break;

                default:
                    // Шрифт по умолчанию, если ничего не подошло
                    resourceName = "barcode_gen.Fonts.TIMES.TTF";
                    break;
            }

            using (var stream = asm.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                    throw new Exception($"Шрифт не найден в ресурсах: {resourceName}");

                using (var ms = new MemoryStream())
                {
                    stream.CopyTo(ms);
                    return ms.ToArray();
                }
            }
        }

        public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
        {
            // Приводим к нижнему регистру для надежности
            string name = familyName.ToLower();

            // Модифицируем ключ в зависимости от стиля (Жирный/Курсив)
            if (isBold && isItalic) name += "_bold_italic";
            else if (isBold) name += "_bold";
            else if (isItalic) name += "_italic";

            // Возвращаем уникальный идентификатор для PdfSharp
            return new FontResolverInfo(name);
        }
    }
}
