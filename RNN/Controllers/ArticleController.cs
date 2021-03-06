﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using RNN.Controllers.Common;
using RNN.Models;
using RNN.Models.ViewModels.Data;
using RNN.Models.ViewModels.Pages;
using RNN.Models.ViewModels.ViewComponents;
using RNN.Services;
using RNN.Services.Impl;

namespace RNN.Controllers
{
    [AllowAnonymous]
    public class ArticleController : BaseController
    {
        private readonly ITopicService _topicService;
        private readonly IArticleService _articleService;
        private readonly IMemoryCache _cache;

        public ArticleController(
            IWebHostEnvironment environment,
            IArticleService articleService,
            ITopicService topicService,
            IMemoryCache cache) : base(environment)
        {
            _topicService = topicService;
            _articleService = articleService;
            _cache = cache;
        }

        /// <summary>
        /// Display an article
        /// </summary>
        /// <param name="slug"></param>
        /// <returns></returns>
        [Route("article/{slug}", Name = "display")]
        [HttpGet]
        public async Task<IActionResult> Get(
            [FromRoute] string slug)
        {
            // get article from cache
            if(!_cache.TryGetValue(slug, out FullArticle article))
            {
                article = await _articleService.GetArticleBySlugAsync(slug);
                _cache.Set(slug, article, DateTimeOffset.Now.AddDays(1));
            }

            if (!_cache.TryGetValue("trending", out IEnumerable<Topic> trending))
            {
                trending = (await _articleService.GetHeadlineTopics(5))
                    .GroupBy(t => t)
                    .OrderByDescending(g => g.Count())
                    .Select(g => g.Key)
                    .Take(5)
                    .ToList();

                _cache.Set("trending", trending, DateTimeOffset.Now.AddDays(1));
            }

            ViewData["Trending"] = trending;

            ViewData["Title"] = article.HeadLine;
            ViewData["Description"] = article.Paragraph;
            ViewData["OGImage"] = string.Concat("https://www.renegadenews.net/images/uploads/", article.Img);
            ViewData["OGUrl"] = string.Concat("https://www.renegadenews.net/article/", article.Slug);

            return View("Index", article);
        }

        /// <summary>
        /// Load reccomendations for an article
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("article/{id}/reccomendations")]
        public async Task<IActionResult> GetReccomendations(
            [FromRoute] int id)
        {
            var topics = _articleService.GetArticleTopics(id);

            var reccomendations = _articleService.GetReccomendedArticlesAsync(
                (await topics).Select(t => t.Id).ToList(),
                id);

            var model = (await reccomendations)
                .Select(r => ReccomendationBlockViewComponent.ToViewModel(r))
                .ToList();

            return PartialView("ReccomendationsPartial", model);
        }

        /// <summary>
        /// Get articles by topic
        /// </summary>
        /// <param name="topicId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("articles/topic/{topicId}", Name = "topiclist")]
        public async Task<IActionResult> GetByTopic(
            [FromRoute] int topicId)
        {
            var topic = await _topicService.GetById(topicId);

            ViewData["Topic"] = topic.Name;
            ViewData["Title"] = topic.Name;
            ViewData["Description"] = "Latest " + topic.Name + " news & opinions from Renegade News";

            var list = _articleService.GetArticlesByTopicAsync(topicId);

            return View("Topic", (await list).Select(e => HorizontalMediumBlockViewComponent.ToViewModel(e, false)).ToList());
        }

        [HttpPut]
        [Route("article/{id}/viewed")]
        public async Task IncreaseViews(
            [FromRoute] int id)
        {
            await _articleService.IncreaseViews(id);
        }
    }
}