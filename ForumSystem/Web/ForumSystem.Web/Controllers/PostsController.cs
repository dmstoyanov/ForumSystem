﻿namespace ForumSystem.Web.Controllers
{
    using System.Linq;
    using System.Net;
    using System.Web.Mvc;

    using AutoMapper;
    using AutoMapper.QueryableExtensions;

    using ForumSystem.Data.Models;
    using ForumSystem.Data.UnitOfWork;
    using ForumSystem.Web.Controllers.Base;
    using ForumSystem.Web.Infrastructure.Extensions;
    using ForumSystem.Web.InputModels.Posts;
    using ForumSystem.Web.ViewModels.Answers;
    using ForumSystem.Web.ViewModels.Posts;

    using Microsoft.AspNet.Identity;

    using PagedList;

    public class PostsController : BaseController
    {
        private const int AnswersPerPageDefaultValue = 10;

        public PostsController(IForumSystemData data)
            : base(data)
        {
        }

        [HttpGet]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var post = this.Data.Posts.GetById(id);
            if (post == null)
            {
                return this.HttpNotFound();
            }

            var viewModel = 
                this.Data.Posts.All()
                    .Where(p => p.Id == id)
                    .ProjectTo<PostViewModel>()
                    .FirstOrDefault();

            return this.View(viewModel);
        }

        [ChildActionOnly]
        public ActionResult Answers(int? id, int? page)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var posts = this.Data.Posts.GetById(id);
            if (posts == null)
            {
                return this.HttpNotFound();
            }

            var pageNumber = page ?? 1;

            var answers =
                this.Data.Answers.All()
                    .Where(a => a.PostId == id)
                    .OrderBy(x => x.CreatedOn)
                    .ProjectTo<AnswerViewModel>();
            var model = answers.ToPagedList(pageNumber, AnswersPerPageDefaultValue);

            return this.PartialView("_PostAnswersPartial", model);
        }

        [ChildActionOnly]
        public ActionResult GetById(int id)
        {
            var post = this.Data.Posts.GetById(id);
            if (post == null)
            {
                return this.HttpNotFound();
            }

            var model = 
                this.Data.Posts.All()
                    .Where(p => p.Id == id)
                    .ProjectTo<PostViewModel>()
                    .FirstOrDefault();

            return this.PartialView("_PostDetailPartial", model);
        }

        [HttpGet]
        [Authorize]
        public ActionResult Create(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var category = this.Data.Categories.GetById(id);
            if (category == null)
            {
                return this.HttpNotFound();
            }

            var inputModel = new PostInputModel { CategoryId = category.Id, Category = category.Title };

            return this.View(inputModel);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Title,Content,CategoryId,Category")] PostInputModel input)
        {
            if (input != null && this.ModelState.IsValid)
            {
                var userId = this.User.Identity.GetUserId();
                var post = new Post
                               {
                                   Title = input.Title, 
                                   Content = input.Content, 
                                   AuthorId = userId, 
                                   CategoryId = input.CategoryId
                               };

                this.Data.Posts.Add(post);
                this.Data.SaveChanges();

                return this.RedirectToAction("Details", "Posts", new { area = string.Empty, id = post.Id });
            }

            return this.View(input);
        }

        [HttpGet]
        [Authorize]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var userId = this.User.Identity.GetUserId();
            var post = this.Data.Posts.GetById(id);

            if (post == null)
            {
                return this.HttpNotFound();
            }

            if (post.AuthorId != userId)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var model = new PostEditModel { Id = post.Id, Title = post.Title, Content = post.Content };

            return this.View(model);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Title,Content")] PostEditModel model)
        {
            if (model != null && this.ModelState.IsValid)
            {
                var userId = this.User.Identity.GetUserId();
                var post = this.Data.Posts.GetById(model.Id);

                if (post.AuthorId != userId)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }

                post.Title = model.Title;
                post.Content = model.Content;

                this.Data.Posts.Update(post);
                this.Data.SaveChanges();

                return this.RedirectToAction("Details", "Posts", new { area = string.Empty, id = post.Id });
            }

            return this.View(model);
        }

        [HttpGet]
        [Authorize]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var post = this.Data.Posts.GetById(id);
            if (post == null || post.IsDeleted)
            {
                return this.HttpNotFound();
            }

            var userId = this.User.Identity.GetUserId();
            if (post.AuthorId != userId && !this.User.IsModerator() && !this.User.IsAdmin())
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var model = Mapper.Map<PostViewModel>(post);

            return this.PartialView(model);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            var post = this.Data.Posts.GetById(id);
            if (post == null || post.IsDeleted)
            {
                return this.HttpNotFound();
            }

            var userId = this.User.Identity.GetUserId();
            if (post.AuthorId != userId && !this.User.IsModerator() && !this.User.IsAdmin())
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            this.Data.Posts.Delete(id);
            this.Data.SaveChanges();

            return this.RedirectToAction("Details", "Categories", new { area = string.Empty, id = post.CategoryId });
        }
    }
}