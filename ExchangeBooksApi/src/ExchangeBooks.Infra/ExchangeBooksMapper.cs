using System;
using AutoMapper;
using ExchangeBooks.Infra.Enums;
using ExchangeBooks.Infra.Models.Domain;
using ExchangeBooks.Infra.Models.Request;
using System.Linq;
using System.Collections.Generic;
using ExchangeBooks.Infra.Models.Response;

namespace ExchangeBooks.Infra
{
    public class ExchangeBooksMapper : Profile
    {
        
        public ExchangeBooksMapper()
        {
            CreateMap<PostRequest, Post>()
            .ForMember(dest => dest.Id, opts => opts.MapFrom(src => Guid.NewGuid()))
            .ForMember(dest => dest.Status, opts => opts.MapFrom(src => PostStatus.Open))
            .ForMember(dest => dest.CreatedOn, opts => opts.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.ModifiedOn, opts => opts.MapFrom(src => DateTime.UtcNow));
            //.AfterMap<BookMapAction>();

            CreateMap<BookRequest, Book>()
            .ForMember(dest => dest.Id, opts => opts.MapFrom(src => Guid.NewGuid()))
            .ForMember(dest => dest.Status, opts => opts.MapFrom(src => BookStatus.Available));
        }
    }

    public class BookMapAction : IMappingAction<PostRequest, Post>
    {
        public void Process(PostRequest source, Post destination, ResolutionContext context)
        {
            destination.Books.ToList().ForEach(b => 
            {
                b.Id = Guid.NewGuid();
                b.Status = BookStatus.Available; 
            });
        }
    }
}