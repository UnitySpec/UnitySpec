namespace UnityFlow.BindingSkeletons
{
    public interface ISkeletonTemplateProvider
    {
        string GetStepDefinitionTemplate(ProgrammingLanguage language, bool withRegex);
        string GetStepDefinitionClassTemplate(ProgrammingLanguage language);
    }
}