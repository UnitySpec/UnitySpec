namespace UnitySpec.Generator.Roslyn
{
    public enum ProviderLanguage
    {
        CSharp,
        VB,
        Other
    }

    public static class ProviderLanguageExtensions
    {
        public static string GetLanguage(this ProviderLanguage language)
        {
            return language switch
            {
                ProviderLanguage.CSharp => "ProgrammingLanguage.CSharp",
                ProviderLanguage.VB => "ProgrammingLanguage.VB",
                ProviderLanguage.Other => "ProgrammingLanguage.Other",
                _ => language.ToString(),
            };
        }
    }
}