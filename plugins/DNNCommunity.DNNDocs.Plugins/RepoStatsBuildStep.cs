﻿using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using Microsoft.DocAsCode.Plugins;
using Microsoft.DocAsCode.Build.ConceptualDocuments;
using DNNCommunity.DNNDocs.Plugins.Models;
using DNNCommunity.DNNDocs.Plugins.Providers;
using System;

namespace DNNCommunity.DNNDocs.Plugins
{
    [Export(nameof(ConceptualDocumentProcessor), typeof(IDocumentBuildStep))]
    public class RepoStatsBuildStep : IDocumentBuildStep
    {
        #region Build
        public void Build(FileModel model, IHostService host)
        {
            // do nothing
        }
        #endregion

        public int BuildOrder => 2;

        public string Name => nameof(RepoStatsBuildStep);

        #region Postbuild
        public void Postbuild(ImmutableList<FileModel> models, IHostService host)
        {
            Console.WriteLine($"Processing Repo Stats for {models.Count()} models");
            var rootPath = models[0].BaseDir;
            
            List<Contributor> gitContributors = GitHubApi.Instance(rootPath).GetContributors(models);
            Console.WriteLine($"Found {gitContributors.Count()} contributors");

            List<Commits> gitCommits = GitHubApi.Instance(rootPath).GetCommits(models, "");
            Console.WriteLine($"Found {gitCommits.Count()} commits");

            if (gitContributors != null && gitCommits != null)
            {
                foreach (var model in models)
                {
                    if (model.Type == DocumentType.Article)
                    {
                        var content = (Dictionary<string, object>)model.Content;

                        for (var i = 1; i < 6; i++)
                        {
                            try
                            {
                                content["gitContributor" + i + "Contributions"] = gitContributors[i - 1].Contributions;
                                content["gitContributor" + i + "Login"] = gitContributors[i - 1].Login;
                                content["gitContributor" + i + "AvatarUrl"] = gitContributors[i - 1].AvatarUrl;
                                content["gitContributor" + i + "HtmlUrl"] = gitContributors[i - 1].HtmlUrl;
                            }
                            catch (Exception)
                            {
                                // Ignore failures with an empty string
                                content["gitContributor" + i + "Contributions"] = String.Empty;
                                content["gitContributor" + i + "Login"] = String.Empty;
                                content["gitContributor" + i + "AvatarUrl"] = String.Empty;
                                content["gitContributor" + i + "HtmlUrl"] = String.Empty;
                            }
                        }

                        var commits = gitCommits
                            .GroupBy(x => x.Author.Login)
                            .OrderByDescending(x => x.Count())
                            .Select(x => x.FirstOrDefault())
                            .Take(5);

                        int index = 1;
                        foreach (var commit in commits)
                        {
                            try
                            {
                                content["gitRecentContributor" + index + "Login"] = commit.Author.Login;
                                content["gitRecentContributor" + index + "AvatarUrl"] = commit.Author.AvatarUrl;
                                content["gitRecentContributor" + index + "HtmlUrl"] = commit.Author.HtmlUrl;
                            }
                            catch (Exception)
                            {
                                // Ignore failures with an empty string
                                content["gitRecentContributor" + index + "Login"] = String.Empty;
                                content["gitRecentContributor" + index + "AvatarUrl"] = String.Empty;
                                content["gitRecentContributor" + index + "HtmlUrl"] = String.Empty;
                            }
                            finally{
                                index++;
                            }
                        }
                    }
                }
            }
        }
        #endregion

        #region Prebuild
        public IEnumerable<FileModel> Prebuild(ImmutableList<FileModel> models, IHostService host)
        {
            return models;
        }
        #endregion

    }
}
