using System;
using System.IO;

namespace UnityFlow.BindingSkeletons
{
    public class ResourceSkeletonTemplateProvider : FileBasedSkeletonTemplateProvider
    {
        protected override string GetTemplateFileContent()
        {
            string path = "Assets\\Editor\\Runner\\Context\\BindingSkeletons\\DefaultSkeletonTemplates.sftemplate";
            using (StreamReader streamReader  = new StreamReader(path))
            {
                return streamReader.ReadToEnd();
            }

            //var resourceStream = GetType().Assembly.GetManifestResourceStream("UnityFlow.Bindingskeletons.DefaultSkeletonTemplates.sftemplate");
            //if (resourceStream == null)
            //    throw new SpecFlowException("Missing resource: DefaultSkeletonTemplates.sftemplate");

            //using (var reader = new StreamReader(resourceStream))
            //    return reader.ReadToEnd();
        }
    }

    public class DefaultSkeletonTemplateProvider : FileBasedSkeletonTemplateProvider
    {
        private readonly ResourceSkeletonTemplateProvider resourceSkeletonTemplateProvider;

        public DefaultSkeletonTemplateProvider(ResourceSkeletonTemplateProvider resourceSkeletonTemplateProvider)
        {
            this.resourceSkeletonTemplateProvider = resourceSkeletonTemplateProvider;
        }

        protected override string GetTemplateFileContent()
        {
            string templateFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"SpecFlow\SkeletonTemplates.sftemplate");
            if (!File.Exists(templateFilePath))
                return "";

            return File.ReadAllText(templateFilePath);
        }

        protected internal override string GetTemplate(string key)
        {
            return base.GetTemplate(key) ?? resourceSkeletonTemplateProvider.GetTemplate(key);
        }
    }
}