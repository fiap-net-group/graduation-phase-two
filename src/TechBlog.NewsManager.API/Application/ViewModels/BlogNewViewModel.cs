using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechBlog.NewsManager.API.Domain.Entities;

namespace TechBlog.NewsManager.API.Application.ViewModels
{
    public class BlogNewViewModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Body { get; set; }
        public string[] Tags { get; set; }
        public BlogUserViewModel Author { get; set; }
        public DateTime LastUpdateAt { get; set; }
    }
}
