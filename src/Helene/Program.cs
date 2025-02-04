using System.CommandLine;
using System.CommandLine.Invocation;
using System.Reflection;
using Markdown;
using Microsoft.Extensions.Logging;
using Serilog;
using XMLDoc2Markdown;
using XMLDoc2Markdown.Utils;

Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Debug()
        .WriteTo.Console()
        .WriteTo.Debug()
        .CreateLogger();
ILoggerFactory loggerFactory = LoggerFactory.Create(loggingBuilder => loggingBuilder.AddSerilog());
Microsoft.Extensions.Logging.ILogger logger = loggerFactory.CreateLogger("helene");

Argument<string> srcArgument = new(
    name: "src",
    description: "DLL source path");

Option<string> outputOption = new(
    aliases: ["--output", "-o"],
    description: "Output directory");

Option<string> indexPageNameOption = new(
    name: "--index-page-name",
    description: "Name of the index page",
    getDefaultValue: () => "index");

Option<string> examplesPathOption = new(
    name: "--examples-path",
    description: "Path to the code examples to insert in the documentation");

Option<bool> gitHubPagesOption = new(
    name: "--github-pages",
    description: "Remove '.md' extension from links for GitHub Pages");

Option<bool> mkDocsOption = new(
    name: "--mkdocs",
    description: "Adjust links and output to work well with mkdocs.");

Option<bool> gitlabWikiOption = new(
    name: "--gitlab-wiki",
    description: "Remove '.md' extension and './' prefix from links for gitlab wikis");

Option<bool> backButtonOption = new(
    name: "--back-button",
    description: "Add a back button on each page");

Option<string> memberAccessibilityLevelOption = new(
    name: "--member-accessibility-level",
    description: "Minimum accessibility level of members to be documented.",
    getDefaultValue: () => "protected");
memberAccessibilityLevelOption.AddCompletions("public", "protected", "internal", "private");

Option<string> structureOption = new(
    name: "--structure",
    description: "Documentation structure.",
    getDefaultValue: () => "flat");
structureOption.AddCompletions("flat", "tree");

RootCommand rootCommand = new(description: "Tool to generate markdown from C# XML documentation.")
{
    srcArgument,
    outputOption,
    indexPageNameOption,
    examplesPathOption,
    gitHubPagesOption,
    mkDocsOption,
    gitlabWikiOption,
    backButtonOption,
    memberAccessibilityLevelOption,
    structureOption
};

rootCommand.SetHandler((InvocationContext context) =>
    {
        try
        {
            string src = context.ParseResult.GetValueForArgument(srcArgument);
            string @out = context.ParseResult.GetValueForOption(outputOption) ?? ".";
            string indexPageName = context.ParseResult.GetValueForOption(indexPageNameOption)!;

            bool mkDocsFlag = context.ParseResult.GetValueForOption(mkDocsOption);
            DocumentationStructure structureOptionValue = context.ParseResult.GetValueForOption(structureOption) switch
            {
                "tree" => DocumentationStructure.Tree,
                _ => DocumentationStructure.Flat,
            };
            if (mkDocsFlag)
            {
                structureOptionValue = DocumentationStructure.Tree;
            }

            TypeDocumentationOptions options = new()
            {
                ExamplesDirectory = context.ParseResult.GetValueForOption(examplesPathOption),
                GitHubPages = context.ParseResult.GetValueForOption(gitHubPagesOption),
                GitlabWiki = context.ParseResult.GetValueForOption(gitlabWikiOption),
                BackButton = context.ParseResult.GetValueForOption(backButtonOption),
                MkDocs = mkDocsFlag,
                MemberAccessibilityLevel = context.ParseResult.GetValueForOption(memberAccessibilityLevelOption) switch
                {
                    "private" => Accessibility.Private,
                    "internal" => Accessibility.Internal,
                    "protected" => Accessibility.Protected,
                    _ => Accessibility.Public,
                },
                Structure = structureOptionValue
                };
            int succeeded = 0;
            int failed = 0;

            Assembly assembly = new AssemblyLoadContext(src)
                .LoadFromAssemblyName(new AssemblyName(Path.GetFileNameWithoutExtension(src)));

            string? assemblyName = assembly.GetName().Name;
            XmlDocumentation documentation = new(src);
            logger.LogInformation($"Generation started: Assembly: {assemblyName}");
            
            IMarkdownDocument indexPage = new MarkdownDocument().AppendHeader(assemblyName, 1);

            IEnumerable<Type> types = assembly.GetTypes()
                .Where(type => type.IsPublic && !typeof(Delegate).IsAssignableFrom(type));
            IEnumerable<IGrouping<string?, Type>> typesByNamespace = types.GroupBy(type => type.Namespace).OrderBy(g => g.Key);
            foreach (IGrouping<string?, Type> namespaceTypes in typesByNamespace)
            {
                indexPage.AppendHeader(namespaceTypes.Key ?? "No namespace", 2);

                foreach (Type type in namespaceTypes.OrderBy(x => x.Name))
                {
                    string fileName = type.GetDocsFileName(options.Structure);
                    logger.LogInformation($"  {fileName}.md");

                    indexPage.AppendParagraph(type.GetDocsLink(assembly, options.Structure, noExtension: options.GitHubPages));

                    try
                    {
                        string filePath = Path.Combine(@out, $"{fileName}.md");
                        string? directory = Path.GetDirectoryName(filePath);

                        if (directory != null)
                        {
                            Directory.CreateDirectory(directory);
                        }

                        File.WriteAllText(
                            filePath,
                            new TypeDocumentation(assembly, type, documentation, logger, options, fileName).ToString()
                        );
                        succeeded++;
                    }
                    catch (Exception exception)
                    {
                        logger.LogError(exception.Message);
                        failed++;
                    }
                }
            }

            File.WriteAllText(Path.Combine(@out, $"{indexPageName}.md"), indexPage.ToString());

            logger.LogInformation($"Generation: {succeeded} succeeded, {failed} failed");
        }
        catch (Exception ex)
        {
            logger.LogError("Unable to generate documentation:");
            logger.LogError(ex.Message);
            context.ExitCode = 1;
        }
    });

return await rootCommand.InvokeAsync(args);
