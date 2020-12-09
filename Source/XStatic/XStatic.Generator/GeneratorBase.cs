﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Web.Razor.Generator;
using Umbraco.Core;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors.ValueConverters;
using Umbraco.ModelsBuilder.Embedded;
using Umbraco.Web;
using Umbraco.Web.Models.ContentEditing;
using XStatic.Generator.Storage;
using XStatic.Generator.Transformers;

namespace XStatic.Generator
{
    public abstract class GeneratorBase : IGenerator
    {
        protected static readonly Encoding DefaultEncoder = Encoding.UTF8;

        protected IUmbracoContextFactory _umbracoContextFactory;
        protected readonly IStaticSiteStorer _storer;

        protected GeneratorBase(IUmbracoContextFactory umbracoContextFactory, IStaticSiteStorer storer)
        {
            _umbracoContextFactory = umbracoContextFactory;
            _storer = storer;
        }

        public virtual async Task<IEnumerable<string>> GenerateFolder(string folderPath, int staticSiteId)
        {
            var partialPath = folderPath;
            var absolutePath = System.Web.Hosting.HostingEnvironment.MapPath(partialPath);

            var files = Directory.GetFiles(absolutePath);
            var created = new List<string>();

            foreach (var file in files)
            {
                var outputPath = Path.Combine(partialPath, Path.GetFileName(file));
                var generatedFileLocation = await Copy(staticSiteId, file, outputPath);

                created.Add(generatedFileLocation);
            }

            return created;
        }

        public virtual async Task<string> GenerateFile(string filePath, int staticSiteId)
        {
            var partialPath = filePath;
            var absolutePath = System.Web.Hosting.HostingEnvironment.MapPath(partialPath);

            var generatedFileLocation = await Copy(staticSiteId, absolutePath, partialPath);

            return generatedFileLocation;
        }

        public virtual async Task<string> GenerateMedia(int id, int staticSiteId, IEnumerable<Crop> crops = null)
        {
            var mediaItem = GetMedia(id);

            if (mediaItem == null)
            {
                return null;
            }

            var url = mediaItem.Url();
            string absoluteUrl = mediaItem.Url(mode: UrlMode.Absolute);

            var partialPath = GetRelativeMediaPath(mediaItem);

            if (string.IsNullOrEmpty(partialPath))
            {
                return null;
            }

            var absolutePath = System.Web.Hosting.HostingEnvironment.MapPath(partialPath);

            var generatedFileLocation = await Copy(staticSiteId, absolutePath, partialPath);

            return generatedFileLocation;
        }

        public abstract Task<string> GeneratePage(int id, int staticSiteId, IFileNameGenerator fileNamer, IEnumerable<ITransformer> transformers = null);

        protected string RunTransformers(string fileData, IEnumerable<ITransformer> transformers)
        {
            if (transformers == null) return fileData;

            var context = GetContext();
            foreach (var transformer in transformers)
            {
                fileData = transformer.Transform(fileData, context);
            }

            return fileData;
        }

        protected async Task<string> Store(int staticSiteId, string filePath, string fileData)
        {
            return await _storer.StoreSiteItem(staticSiteId.ToString(), filePath, fileData, DefaultEncoder);
        }

        protected async Task<string> Copy(int staticSiteId, string absoluteFilePath, string filePath)
        {
            return await _storer.CopyFile(staticSiteId.ToString(), absoluteFilePath, filePath);
        }

        protected UmbracoContext GetContext()
        {
            using (var umbracoContextReference = _umbracoContextFactory.EnsureUmbracoContext())
            {
                return umbracoContextReference.UmbracoContext;
            }
        }

        protected IPublishedContent GetNode(int id)
        {
            using (var umbracoContextReference = _umbracoContextFactory.EnsureUmbracoContext())
            {
                var content = umbracoContextReference.UmbracoContext.Content;
                return content.GetById(id);
            }
        }

        protected IPublishedContent GetMedia(int id)
        {
            using (var umbracoContextReference = _umbracoContextFactory.EnsureUmbracoContext())
            {
                var media = umbracoContextReference.UmbracoContext.Media;
                return media.GetById(id);
            }
        }

        protected string GetRelativeMediaPath(IPublishedContent mediaItem)
        {
            if (!mediaItem.HasProperty(Constants.Conventions.Media.File))
            {
                return null;
            }

            var prop = mediaItem.GetProperty(Constants.Conventions.Media.File);

            var umbracoFileSource = prop?.Value<ImageCropperValue>()?.Src;

            if (umbracoFileSource == null)
            {
                umbracoFileSource = prop?.Value<string>();

                if (umbracoFileSource == null)
                {
                    return null;
                }
            }

            var relativeFilePath = umbracoFileSource;

            return relativeFilePath;
        }
    }
}