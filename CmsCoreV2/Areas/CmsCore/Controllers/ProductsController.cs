using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CmsCoreV2.Data;
using CmsCoreV2.Models;
using Microsoft.AspNetCore.Authorization;
using SaasKit.Multitenancy;

namespace CmsCoreV2.Areas.CmsCore.Controllers
{
    [Authorize(Roles = "ADMIN")]
    [Area("CmsCore")]
    public class ProductsController : ControllerBase
    {
 
        public ProductsController(ApplicationDbContext context, ITenant<AppTenant> tenant) : base(context, tenant)
        {
                
        }

        // GET: CmsCore/Products
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Products.Include(p => p.CrossSell).Include(p => p.GroupedProduct).Include(p => p.Language).Include(p => p.UpSell);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: CmsCore/Products/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.CrossSell)
                .Include(p => p.GroupedProduct)
                .Include(p => p.Language)
                .Include(p => p.UpSell)
                .SingleOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: CmsCore/Products/Create
        public IActionResult Create()
        {
            var product = new Product();
            product.CreatedBy = User.Identity.Name ?? "username";
            product.CreateDate = DateTime.Now;
            product.UpdatedBy = User.Identity.Name ?? "username";
            product.UpdateDate = DateTime.Now;
            product.AppTenantId = tenant.AppTenantId;
            ViewData["CrossSellId"] = new SelectList(_context.Products, "Id", "Name");
            ViewData["GroupedProductId"] = new SelectList(_context.Products, "Id", "Name");
            ViewData["LanguageId"] = new SelectList(_context.Languages, "Id", "Culture");
            ViewData["UpSellId"] = new SelectList(_context.Products, "Id", "Name");
            return View(product);
        }

        // POST: CmsCore/Products/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Slug,Description,LanguageId,UnitPrice,SalePrice,TaxStatus,TaxClass,StockCode,StockCount,StockStatus,Weight,Length,Height,Width,ProductType,ProductUrl,UpSellId,CrossSellId,GroupedProductId,PurchaseNote,MenuOrder,ProductImage,ShortDescription,ViewCount,SaleCount,CatalogVisibility,IsFeatured,Id,CreateDate,CreatedBy,UpdateDate,UpdatedBy,AppTenantId")] Product product)
        {
            if (ModelState.IsValid)
            {
                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewData["CrossSellId"] = new SelectList(_context.Products, "Id", "Name", product.CrossSellId);
            ViewData["GroupedProductId"] = new SelectList(_context.Products, "Id", "Name", product.GroupedProductId);
            ViewData["LanguageId"] = new SelectList(_context.Languages, "Id", "Culture", product.LanguageId);
            ViewData["UpSellId"] = new SelectList(_context.Products, "Id", "Name", product.UpSellId);
            return View(product);
        }

        // GET: CmsCore/Products/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products.SingleOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }
            ViewData["CrossSellId"] = new SelectList(_context.Products, "Id", "Name", product.CrossSellId);
            ViewData["GroupedProductId"] = new SelectList(_context.Products, "Id", "Name", product.GroupedProductId);
            ViewData["LanguageId"] = new SelectList(_context.Languages, "Id", "Culture", product.LanguageId);
            ViewData["UpSellId"] = new SelectList(_context.Products, "Id", "Name", product.UpSellId);
            return View(product);
        }

        // POST: CmsCore/Products/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Name,Slug,Description,LanguageId,UnitPrice,SalePrice,TaxStatus,TaxClass,StockCode,StockCount,StockStatus,Weight,Length,Height,Width,ProductType,ProductUrl,UpSellId,CrossSellId,GroupedProductId,PurchaseNote,MenuOrder,ProductImage,ShortDescription,ViewCount,SaleCount,CatalogVisibility,IsFeatured,Id,CreateDate,CreatedBy,UpdateDate,UpdatedBy,AppTenantId")] Product product)
        {
            if (id != product.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(product);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Index");
            }
            ViewData["CrossSellId"] = new SelectList(_context.Products, "Id", "Name", product.CrossSellId);
            ViewData["GroupedProductId"] = new SelectList(_context.Products, "Id", "Name", product.GroupedProductId);
            ViewData["LanguageId"] = new SelectList(_context.Languages, "Id", "Culture", product.LanguageId);
            ViewData["UpSellId"] = new SelectList(_context.Products, "Id", "Name", product.UpSellId);
            return View(product);
        }

        // GET: CmsCore/Products/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.CrossSell)
                .Include(p => p.GroupedProduct)
                .Include(p => p.Language)
                .Include(p => p.UpSell)
                .SingleOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: CmsCore/Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var product = await _context.Products.SingleOrDefaultAsync(m => m.Id == id);
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        private bool ProductExists(long id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
    }
}
