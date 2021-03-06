﻿using CmsCoreV2.Data;
using CmsCoreV2.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace CmsCoreV2.ViewComponents
{
    public class Gallery:ViewComponent
    {
        private readonly ApplicationDbContext _context;
      
        public Gallery(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IViewComponentResult> InvokeAsync(string name, int count = 10, string template = "Default")
        {
            var items = await GetItems(name, count);
            return View(template, items);
        }
        private async Task<List<CmsCoreV2.Models.GalleryItem>> GetItems(string galleryName, int count)
        {
            List<CmsCoreV2.Models.GalleryItem> galleries = GetGalleryItems(galleryName, count).Where(w => w.IsPublished == true).ToList();
            return await Task.FromResult(galleries);
        }
        public IEnumerable<GalleryItem> GetGalleryItems(string galleryName, int count)
        {
            galleryName = galleryName.ToLower();
            var gallery = Get(g => g.Name.ToLower() == galleryName && g.IsPublished == true, "GalleryItems", "GalleryItems.GalleryItemGalleryItemCategories", "GalleryItems.GalleryItemGalleryItemCategories.GalleryItemCategory");
            if (gallery != null)
            {
                var galleryItems = gallery.GalleryItems.Where(gi => gi.IsPublished == true).Take(count).ToList();
                return galleryItems;
            }
            return new List<GalleryItem>();

        }
        public Models.Gallery Get(Expression<Func<Models.Gallery, bool>> where, params string[] navigations)
        {
            var set = _context.Galleries.AsQueryable();
            foreach (string nav in navigations)
                set = set.Include(nav);
            return set.Where(where).FirstOrDefault<Models.Gallery>();
        }
       
    }
}
