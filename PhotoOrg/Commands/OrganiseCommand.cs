/*
* MIT License
*
* Copyright (c) 2024 Derek Goslin https://github.com/DerekGn
*
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
*
* The above copyright notice and this permission notice shall be included in all
* copies or substantial portions of the Software.
*
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
*/

using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace PhotoOrg.Commands
{
    internal class OrganiseCommand : Command<OrganiseCommand.Settings>
    {
        public override int Execute([NotNull] CommandContext context, [NotNull] Settings settings)
        {
            AnsiConsole.Progress()
                .Start(progressContext =>
                {
                    var task = progressContext.AddTask("[green]Files Processed[/]");

                    try
                    {
                        var files = new DirectoryInfo(settings.SourcePath!)
                            .GetFiles();

                        task.MaxValue = files.Length;

                        var result = OrgainseFiles(settings, task, files);

                        AnsiConsole.Write(new BarChart()
                            .Width(80)
                            .Label("[green bold underline]Processed Files[/]")
                            .CenterLabel()
                            .AddItem(nameof(result.Total), result.Total, Color.Yellow)
                            .AddItem(nameof(result.Processed), result.Processed, Color.Yellow)
                            .AddItem(nameof(result.Skip), result.Skip, Color.Yellow));
                    }
                    catch (Exception ex)
                    {
                        AnsiConsole.WriteException(ex);
                    }
                });

            return 0;
        }

        private static OrgainseResult OrgainseFiles(Settings settings, ProgressTask task, FileInfo[] files)
        {
            OrgainseResult result = new()
            {
                Total = files.Length
            };

            if (!System.IO.Directory.Exists(settings.TargetPath))
            {
                System.IO.Directory.CreateDirectory(settings.TargetPath!);
            }

            var unprocessedPath = Path.Combine(settings.TargetPath!, "Unprocessed");

            if (!System.IO.Directory.Exists(unprocessedPath))
            {
                System.IO.Directory.CreateDirectory(unprocessedPath);
            }

            foreach (var file in files)
            {
                try
                {
                    IEnumerable<MetadataExtractor.Directory> directories = ImageMetadataReader.ReadMetadata(file.FullName);

                    var subIfdDirectory = directories.OfType<ExifSubIfdDirectory>().FirstOrDefault();

                    if (subIfdDirectory != null && subIfdDirectory!.TryGetDateTime(ExifDirectoryBase.TagDateTimeOriginal, out var fileTime))
                    {
                        var path = Path.Combine(settings.TargetPath!, fileTime.Year.ToString());

                        System.IO.Directory.CreateDirectory(path);
                        File.Copy(file.FullName, Path.Combine(path, file.Name), settings.Overwrite);
                        result.Processed++;
                    }
                    else
                    {
                        result.Skip++;
                    }
                }
                catch (ImageProcessingException)
                {
                    result.Skip++;

                    var path = Path.Combine(settings.TargetPath!, "Unprocessed");
                    File.Copy(file.FullName, Path.Combine(path, file.Name), settings.Overwrite);
                }

                task.Increment(1);
            }

            return result;
        }

        public sealed class Settings : CommandSettings
        {
            [Description("Path to write files.")]
            [CommandArgument(1, "[targetPath]")]
            public string? TargetPath { get; init; }

            [Description("Path to search.")]
            [CommandArgument(0, "[sourcePath]")]
            public string? SourcePath { get; init; }

            [DefaultValue(true)]
            [CommandOption("-o|--overwrite")]
            public bool Overwrite { get; init; }
        }
    }
}