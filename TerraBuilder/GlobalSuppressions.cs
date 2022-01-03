// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1623:Property summary documentation should match accessors", Justification = "日本語でコメントを書くため無効化。", Scope = "namespaceanddescendants", Target = "~N:TerraBuilder")]
[assembly: SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1642:Constructor summary documentation should begin with standard text", Justification = "日本語でコメントを書くため無効化。", Scope = "namespaceanddescendants", Target = "~N:TerraBuilder")]
[assembly: SuppressMessage("Style", "IDE0003:Remove qualification", Justification = "SA1623を優先。https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1623.md", Scope = "namespaceanddescendants", Target = "~N:TerraBuilder")]
