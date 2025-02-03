# [Helene](https://science.nasa.gov/saturn/moons/helene/)

Tool to generate markdown from C# XML documentation.  
Fork of [xmldoc2md](https://github.com/charlesdevandiere/xmldoc2md) and [MarkdownBuilder](https://github.com/charlesdevandiere/markdown-builder-dotnet) by Charles de Vandière

## How to use

### Install tool

Not yet setup for this Fork

### Generate documentation

```shell
dotnet xmldoc2md <src> [options]
```

| Argument | Description |
|---|---|
| `<src>` | DLL source path |

| Option | Description |
|---|---|
| `-o, --output <output>` | Output directory |
| `--index-page-name <index-page-name>` | Name of the index page [default: index] |
| `--examples-path <examples-path>` | Path to the code examples to insert in the documentation |
| `--github-pages` | Remove '.md' extension from links for GitHub Pages |
| `--gitlab-wiki` | Remove '.md' extension and './' prefix from links for gitlab wikis |
| `--back-button` | Add a back button on each page |
| `--member-accessibility-level <internal\|private\|protected\|public>` | Minimum accessibility level of members to be documented. [default: protected] |
| `--structure <flat\|tree>` | Documentation structure. [default: flat] |
| `--version` | Show version information |
| `-?, -h, --help` | Show help and usage information |

#### Example

```shell
dotnet xmldoc2md Sample.dll --output docs --github-pages --back-button
```


