using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;

public class YamlTestCaseFilter
{
    public static IEnumerable<TestCase> FilterTestCases(IEnumerable<TestCase> tests, IEnumerable<string> criteria)
    {
        // example 1: "ai init openai" "ai init speech" -skip -nightly
        // > test must contain either:
        // >   * "ai", "init", and "openai" in EXACTLY that order in any one single field/property, or
        // >   * "ai", "init", and "speech" in EXACTLY that order in any one single field/property
        // > test must not contain "skip" in any field/property
        // > test must not contain "nightly" in any field/property

        // example 2: +ai +init +openai -skip -nightly
        // > test must contain ALL three of "ai", "init", and "openai" in any field/property
        // >   * they do NOT need to be in the same field/property
        // > test must not contain "skip" in any field/property
        // > test must not contain "nightly" in any field/property

        // example 3: "ai dev new" "ai init speech" +java +build -skip
        // > test must contain, either:
        // >   * "ai", "init", and "openai" in EXACTLY that order in any one single field/property, or
        // >   * "ai", "init", and "speech" in EXACTLY that order in any one single field/property
        // > test must contain "java" in any field/property
        // > test must contain "build" in any field/property
        // > test must not contain "skip" in any field/property

        var sourceCriteria = new List<string>();
        var mustMatchCriteria = new List<string>();
        var mustNotMatchCriteria = new List<string>();

        foreach (var criterion in criteria)
        {
            var isMustMatch = criterion.StartsWith("+");
            var isMustNotMatch = criterion.StartsWith("-");
            var isSource = !isMustMatch && !isMustNotMatch;

            if (isSource) sourceCriteria.Add(criterion);
            if (isMustMatch) mustMatchCriteria.Add(criterion.Substring(1));
            if (isMustNotMatch) mustNotMatchCriteria.Add(criterion.Substring(1));
        }

        var unfiltered = sourceCriteria.Count > 0
            ? tests.Where(test =>
                sourceCriteria.Any(criterion =>
                    TestContainsText(test, criterion)))
            : tests;

        if (mustMatchCriteria.Count > 0)
        {
            unfiltered = unfiltered.Where(test =>
                mustMatchCriteria.All(criterion =>
                    TestContainsText(test, criterion)));
        }

        if (mustNotMatchCriteria.Count > 0)
        {
            unfiltered = unfiltered.Where(test =>
                mustNotMatchCriteria.All(criterion =>
                    !TestContainsText(test, criterion)));
        }

        return unfiltered;
    }

    public static IEnumerable<TestCase> FilterTestCases(IEnumerable<TestCase> tests, IRunContext runContext)
    {
        tests = tests.ToList(); // force enumeration
        
        var names = GetSupportedFilterableNames(tests);
        var filter = runContext.GetTestCaseFilter(names, null);
        return tests.Where(test => filter == null || filter.MatchTestCase(test, name => GetPropertyValue(test, name))).ToList();
    }

    private static HashSet<string> GetSupportedFilterableNames(IEnumerable<TestCase> tests)
    {
        var filterable = new HashSet<string>(supportedFilterProperties);
        foreach (var test in tests)
        {
            foreach (var trait in test.Traits)
            {
                filterable.Add(trait.Name);
            }
        }

        if (filterable.Contains("tag")) filterable.Add("tags");

        return filterable;
    }

    private static object? GetPropertyValue(TestCase test, string name)
    {
        switch (name.ToLower())
        {
            case "name":
            case "displayname": return test.DisplayName;

            case "fqn":
            case "fullyqualifiedname": return test.FullyQualifiedName;

            case "cli": return YamlTestProperties.Get(test, "cli");
            case "run": return YamlTestProperties.Get(test, "run");
            case "script": return YamlTestProperties.Get(test, "script");
            case "bash": return YamlTestProperties.Get(test, "bash");

            case "matrix": return YamlTestProperties.Get(test, "matrix");
            case "foreach": return YamlTestProperties.Get(test, "foreach");
            case "arguments": return YamlTestProperties.Get(test, "arguments");
            case "input": return YamlTestProperties.Get(test, "input");

            case "expect": return YamlTestProperties.Get(test, "expect");
            case "expect-regex": return YamlTestProperties.Get(test, "expect-regex");
            case "not-expect-regex": return YamlTestProperties.Get(test, "not-expect-regex");
            case "expect-exit-code": return YamlTestProperties.Get(test, "expect-exit-code");

            case "parallelize": return YamlTestProperties.Get(test, "parallelize");
            case "skipOnFailure": return YamlTestProperties.Get(test, "skipOnFailure");

            case "timeout": return YamlTestProperties.Get(test, "timeout");
            case "working-directory": return YamlTestProperties.Get(test, "working-directory");
        }

        var tags = test.Traits.Where(x => x.Name == name || name == "tags");
        if (tags.Count() == 0) return null;

        return tags.Select(x => x.Value).ToArray();
    }

    private static bool TestContainsText(TestCase test, string text)
    {
        return test.DisplayName.Contains(text)
            || test.FullyQualifiedName.Contains(text)
            || test.Traits.Any(x => x.Name == text || x.Value.Contains(text))
            || supportedFilterProperties.Any(property => GetPropertyValue(test, property)?.ToString()?.Contains(text) == true);
    }


    private static readonly string[] supportedFilterProperties = { "DisplayName", "FullyQualifiedName", "Category", "cli", "run", "script", "bash", "foreach", "arguments", "input", "expect", "expect-regex", "not-expect-regex", "expect-exit-code", "parallelize", "skipOnFailure" };
}
