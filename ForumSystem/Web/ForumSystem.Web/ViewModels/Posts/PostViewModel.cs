﻿namespace ForumSystem.Web.ViewModels.Posts
{
    using System;

    using AutoMapper;

    using ForumSystem.Data.Models;
    using ForumSystem.Web.Infrastructure.Mapping;

    public class PostViewModel : IMapFrom<Post>, IHaveCustomMappings
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Content { get; set; }

        public int CategoryId { get; set; }

        public string Category { get; set; }

        public string AuthorId { get; set; }

        public string AuthorPictureUrl { get; set; }

        public string Author { get; set; }

        public DateTime CreatedOn { get; set; }

        public DateTime? ModifiedOn { get; set; }

        public void CreateMappings(IMapperConfiguration configuration)
        {
            configuration.CreateMap<Post, PostViewModel>()
                .ForMember(p => p.Author, config => config.MapFrom(p => p.Author.UserName));
            configuration.CreateMap<Post, PostViewModel>()
                .ForMember(p => p.Category, config => config.MapFrom(p => p.Category.Title));
            configuration.CreateMap<Post, PostViewModel>()
                .ForMember(p => p.AuthorPictureUrl, config => config.MapFrom(p => p.Author.PictureUrl));
        }
    }
}