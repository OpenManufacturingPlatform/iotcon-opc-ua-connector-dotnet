using System;
using System.Diagnostics;
using System.IO;
using OMP.Connector.Domain.Schema.Interfaces;

namespace OMP.Connector.Domain.Schema.Extentions
{
    public static class ModelExtensions
    {
        private const char PartPaddingChar = '0';
        private const string VersionSeparator = "-";
        private const string SchemaPostFix = ".schema.json";
        private const string BusinessObjectRootFolder = "business_object_schemas";
        private const string MessageRootFolder = "message_schemas";
        private const string ApiRootFolder = "api_schemas";
        private const string GenericTypePostFixChar = "`";

        public static string GetModelVersionPath(this Type type)
        {
            var assemblyLocation = type.Assembly.Location;
            var fileVersionInfo = FileVersionInfo.GetVersionInfo(assemblyLocation);

            var fileMajorPart = fileVersionInfo.FileMajorPart.ToString();
            var fileMinorPart = FormatVersionPart(fileVersionInfo.FileMinorPart);
            var fileBuildPart = FormatVersionPart(fileVersionInfo.FileBuildPart);

            return $"{fileMajorPart}{VersionSeparator}{fileMinorPart}{VersionSeparator}{fileBuildPart}";
        }

        public static string GetModelSchemaPath(this Type type)
            =>
                $"{type.GetSchemaRootFolderName()}{Path.AltDirectorySeparatorChar}{type.GetModelVersionPath()}{Path.AltDirectorySeparatorChar}{type.GetModelNameSpace()}{SchemaPostFix}";

        public static string GetModelNameSpace(this Type type)
        {
            var fullName = type.FullName ?? string.Empty;
            var length = fullName.IndexOf(GenericTypePostFixChar, StringComparison.OrdinalIgnoreCase);
            if (length <= 0)
                length = fullName.Length;

            return fullName.Substring(0, length).ToLower();
        }

        public static string GetSchemaId(string schemaContainerPath, Type type)
        {
            schemaContainerPath = schemaContainerPath.EndsWith(Path.AltDirectorySeparatorChar.ToString())
                ? schemaContainerPath
                : $"{schemaContainerPath}{Path.AltDirectorySeparatorChar}";

            var schemaUrl = $"{schemaContainerPath}{type.GetModelSchemaPath()}";

            if (!Uri.TryCreate(schemaUrl, UriKind.Absolute, out var uriResult) &&
                uriResult?.Scheme != Uri.UriSchemeHttp && uriResult?.Scheme != Uri.UriSchemeHttps)
                throw new ArgumentException($"{schemaUrl} is not a valid Uri", nameof(schemaContainerPath));


            return schemaUrl;
        }

        private static string FormatVersionPart(int part)
            => part.ToString().PadLeft(2, PartPaddingChar);

        private static string GetSchemaRootFolderName(this Type type)
        {
            var folder = string.Empty;
            if (typeof(IBusinessObject).IsAssignableFrom(type))
                folder = BusinessObjectRootFolder;
            else if (typeof(IMessage).IsAssignableFrom(type))
                folder = MessageRootFolder;
            else if (typeof(IApi).IsAssignableFrom(type))
                folder = ApiRootFolder;

            return folder;
        }
    }
}