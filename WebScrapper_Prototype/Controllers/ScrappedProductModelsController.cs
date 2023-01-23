using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebScrapper_Prototype.Data;
using WebScrapper_Prototype.Models;

namespace WebScrapper_Prototype.Controllers
{
    public class ScrappedProductModelsController : Controller
    {
        private readonly WebScrapper_PrototypeContext _context;

        public ScrappedProductModelsController(WebScrapper_PrototypeContext context)
        {
            _context = context;
        }

        // GET: ScrappedProductModels
        public async Task<IActionResult> Index()
        {
              return View(await _context.ScrappedProductModel.ToListAsync());
        }

        // GET: ScrappedProductModels/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.ScrappedProductModel == null)
            {
                return NotFound();
            }

            var scrappedProductModel = await _context.ScrappedProductModel
                .FirstOrDefaultAsync(m => m.ID == id);
            if (scrappedProductModel == null)
            {
                return NotFound();
            }

            return View(scrappedProductModel);
        }

        // GET: ScrappedProductModels/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: ScrappedProductModels/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,ProductName,ProductDescription,ProductType,ProductCategory,ProductPrice,ProductDiscount,ProductCreated")] ScrappedProductModel scrappedProductModel)
        {
            if (ModelState.IsValid)
            {
                _context.Add(scrappedProductModel);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(scrappedProductModel);
        }

        // GET: ScrappedProductModels/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.ScrappedProductModel == null)
            {
                return NotFound();
            }

            var scrappedProductModel = await _context.ScrappedProductModel.FindAsync(id);
            if (scrappedProductModel == null)
            {
                return NotFound();
            }
            return View(scrappedProductModel);
        }

        // POST: ScrappedProductModels/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,ProductName,ProductDescription,ProductType,ProductCategory,ProductPrice,ProductDiscount,ProductCreated")] ScrappedProductModel scrappedProductModel)
        {
            if (id != scrappedProductModel.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(scrappedProductModel);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ScrappedProductModelExists(scrappedProductModel.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(scrappedProductModel);
        }

        // GET: ScrappedProductModels/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.ScrappedProductModel == null)
            {
                return NotFound();
            }

            var scrappedProductModel = await _context.ScrappedProductModel
                .FirstOrDefaultAsync(m => m.ID == id);
            if (scrappedProductModel == null)
            {
                return NotFound();
            }

            return View(scrappedProductModel);
        }

        // POST: ScrappedProductModels/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.ScrappedProductModel == null)
            {
                return Problem("Entity set 'WebScrapper_PrototypeContext.ScrappedProductModel'  is null.");
            }
            var scrappedProductModel = await _context.ScrappedProductModel.FindAsync(id);
            if (scrappedProductModel != null)
            {
                _context.ScrappedProductModel.Remove(scrappedProductModel);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ScrappedProductModelExists(int id)
        {
          return _context.ScrappedProductModel.Any(e => e.ID == id);
        }
    }
}
