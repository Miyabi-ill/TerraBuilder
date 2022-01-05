#pragma warning disable SA1636 // File header copyright text should match
#pragma warning disable SA1515 // Single-line comment should be preceded by blank line
#pragma warning disable SA1512 // Single-line comments should not be followed by blank line
// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;
#pragma warning restore SA1512 // Single-line comments should not be followed by blank line
#pragma warning restore SA1515 // Single-line comment should be preceded by blank line
#pragma warning restore SA1636 // File header copyright text should match

[assembly: SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1623:Property summary documentation should match accessors", Justification = "日本語でコメントを書くため無効化.", Scope = "namespaceanddescendants", Target = "~N:TerraBuilder")]
[assembly: SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1642:Constructor summary documentation should begin with standard text", Justification = "日本語でコメントを書くため無効化.", Scope = "namespaceanddescendants", Target = "~N:TerraBuilder")]
[assembly: SuppressMessage("Style", "IDE0003:Remove qualification", Justification = "SA1623を優先.https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1623.md", Scope = "namespaceanddescendants", Target = "~N:TerraBuilder")]
[assembly: SuppressMessage("Style", "IDE0010:Add missing cases", Justification = "RCS1070を優先.https://github.com/JosefPihrt/Roslynator/blob/master/docs/analyzers/RCS1070.md", Scope = "namespaceanddescendants", Target = "~N:TerraBuilder")]
