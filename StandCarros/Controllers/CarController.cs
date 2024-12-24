using System.Net.Mime;
using System.Text.Json;
using ImageMagick;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using StandCarros.Data;
using StandCarros.Models;

namespace StandCarros.Controllers
{
    [Authorize]
    public class CarController : Controller
    {
        private readonly StandCarrosContext _context;

        public CarController(StandCarrosContext context)
        {
            _context = context;
        }

        // GET: Car
        [AllowAnonymous]
        public async Task<IActionResult> Index(string Marca, string Combustivel, string Caixa)
        {
            var cars = _context.Car.AsQueryable();
            
            ViewBag.Marcas = await _context.Car.Select(c => c.Marca).Distinct().ToListAsync();
            ViewBag.Combustiveis = await _context.Car.Select(c => c.Combustível).Distinct().ToListAsync();
            ViewBag.Caixas = await _context.Car.Select(c => c.Caixa).Distinct().ToListAsync();
            
            ViewBag.SelectedMarca = Marca;
            ViewBag.SelectedCombustivel = Combustivel;
            ViewBag.SelectedCaixa = Caixa;
            
            if (!string.IsNullOrEmpty(Marca))
            {
                cars = cars.Where(c => c.Marca == Marca);
            }

            if (!string.IsNullOrEmpty(Combustivel))
            {
                cars = cars.Where(c => c.Combustível == Combustivel);
            }

            if (!string.IsNullOrEmpty(Caixa))
            {
                cars = cars.Where(c => c.Caixa == Caixa);
            }

            return View(await cars.Include(c => c.Photos).ToListAsync());
        }
        
        // GET: Car/Details/5
        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var car = await _context.Car.Include(c => c.Photos)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (car == null)
            {
                return NotFound();
            }
            
            return View(car);
        }
        
        public async Task<byte[]> ResizeImageAsync(byte[] imageData, int width, int height)
        {
            using (var inputStream = new MemoryStream(imageData))
            using (var image = await Image.LoadAsync(inputStream))
            {
                image.Mutate(x => x.Resize(width, height));

                using (var outputStream = new MemoryStream())
                {
                    await image.SaveAsJpegAsync(outputStream); 
                    return outputStream.ToArray();
                }
            }
        }

        // GET: Car/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {   
            var combustiveis = new List<SelectListItem>
            {
                new SelectListItem { Value = "Diesel", Text = "Diesel" },
                new SelectListItem { Value = "Gasolina", Text = "Gasolina" },
                new SelectListItem { Value = "Elétrico", Text = "Elétrico" },
                new SelectListItem { Value = "Hibrido (Diesel)", Text = "Híbrido (Diesel)" },
                new SelectListItem { Value = "Hibrido (Gasolina)", Text = "Híbrido (Gasolina)" },
                new SelectListItem { Value = "Outro", Text = "Outro" }
            };

            var caixas = new List<SelectListItem>
            {
                new SelectListItem { Value = "Manual", Text = "Manual" },
                new SelectListItem { Value = "Automática", Text = "Automática" },
                new SelectListItem { Value = "Semi-automática", Text = "Semi-automática" }
            };

            var tracoes = new List<SelectListItem>()
            {
                new SelectListItem { Value = "Dianteira", Text = "Dianteira" },
                new SelectListItem { Value = "Traseira", Text = "Traseira" },
                new SelectListItem { Value = "Integral", Text = "Integral" }
            };
            
            ViewBag.caixas = new SelectList(caixas, "Value", "Text");
            ViewBag.Combustiveis = new SelectList(combustiveis, "Value", "Text");
            ViewBag.tracao = new SelectList(tracoes, "Value", "Text");
            
            return View();
        }

        // POST: Car/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(Car car)
        {
            if (!ModelState.IsValid)
            {
                // Debugging output: print all validation errors
                foreach (var key in ModelState.Keys)
                {
                    var state = ModelState[key];
                    foreach (var error in state.Errors)
                    {
                        // You can log these errors to a file or console
                        Console.WriteLine($"Key: {key}, Error: {error.ErrorMessage}");
                    }
                }

                // Return the view to display the errors to the user
                return View(car);
            }
            if (ModelState.IsValid)
            {
                Car newCar = new()  
                {  
                    Name = car.Name,
                    Marca = car.Marca.ToLower(),
                    Modelo = car.Modelo,
                    Combustível = car.Combustível,
                    DataRegisto = car.DataRegisto,
                    Quilometros = car.Quilometros,
                    Cilindrada = car.Cilindrada,
                    Potencia = car.Potencia,
                    Cor = car.Cor,
                    Caixa = car.Caixa,
                    NumMudancas = car.NumMudancas,
                    NumPortas = car.NumPortas,
                    Tracao = car.Tracao,
                    Descricao = car.Descricao,
                    Preco = car.Preco
                };  
                List<Photo> photolist = new List<Photo>();  
                if (car.Files != null)  
                {                  
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".heic" };
                    foreach(IFormFile formFile in car.Files)  
                    {  
                        if (formFile.Length > 0)  
                        {    
                            var fileExtension = Path.GetExtension(formFile.FileName);
                            var lenght = formFile.Length;
                            
                            if (!allowedExtensions.Contains(fileExtension.ToLower()))
                            {
                                ModelState.AddModelError("Files", $"File {formFile.FileName} has an unsupported extension. Only .jpg, .jpeg, .png, .gif, and .heic are allowed.");
                                continue;
                            }
                            using (var memoryStream = new MemoryStream())  
                            {  
                                await formFile.CopyToAsync(memoryStream);  
                                if (memoryStream.Length < 2097152)
                                {
                                    byte[] fileBytes = memoryStream.ToArray();
                                    if (fileExtension.ToUpper().Equals(".HEIC"))
                                    {
                                        try
                                        {
                                            using (MagickImage image = new MagickImage(fileBytes))
                                            {
                                                image.Format = MagickFormat.Jpeg; 
                                                fileBytes = image.ToByteArray();
                                                lenght = image.ToByteArray().Length;
                                                fileExtension = ".jpg";
                                            }
                                        }
                                        catch (Exception e)
                                        {
                                            Console.WriteLine($"Error converting HEIC file to JPEG: {e.Message}");
                                            ModelState.AddModelError("Files", "An error occurred while converting HEIC file to JPEG. Please try again.");
                                        }
                                    }
                                    var newphoto = new Photo()  
                                    {  
                                        Bytes = fileBytes,  
                                        Description = formFile.FileName,  
                                        FileExtension = fileExtension,  
                                        Size = lenght,
                                    };  
                                    //add the photo instance to the list.
                                    photolist.Add(newphoto);  
                                }  
                                else  
                                {  
                                    ModelState.AddModelError("File", "The file is too large.");  
                                }  
                            }  
                        }  
                    }  
                }  
                newCar.Photos = photolist;  
                _context.Add(newCar);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(car);
        }

        // GET: Car/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            
            var combustiveis = new List<SelectListItem>
            {
                new SelectListItem { Value = "Diesel", Text = "Diesel" },
                new SelectListItem { Value = "Gasolina", Text = "Gasolina" },
                new SelectListItem { Value = "Elétrico", Text = "Elétrico" },
                new SelectListItem { Value = "Hibrido (Diesel)", Text = "Híbrido (Diesel)" },
                new SelectListItem { Value = "Hibrido (Gasolina)", Text = "Híbrido (Gasolina)" },
                new SelectListItem { Value = "Outro", Text = "Outro" }
            };

            var caixas = new List<SelectListItem>
            {
                new SelectListItem { Value = "Manual", Text = "Manual" },
                new SelectListItem { Value = "Automática", Text = "Automática" },
                new SelectListItem { Value = "Semi-automática", Text = "Semi-automática" }
            };
            
            ViewBag.caixas = new SelectList(caixas, "Value", "Text");
            ViewBag.Combustiveis = new SelectList(combustiveis, "Value", "Text");

            var car = await _context.Car.Include(c => c.Photos).FirstOrDefaultAsync(c => c.Id == id);
            //var car = await _context.Car.FindAsync(id);
            if (car == null)
            {
                return NotFound();
            }
            return View(car);
        }

        // POST: Car/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(Car car, string? removePhotos)
        {
            ModelState.Remove("removePhotos");
            
            if (ModelState.IsValid)
            {
                try
                {
                    // Fetch the existing car including photos for comparison
                    var oldCar = await _context.Car
                        .Include(c => c.Photos)
                        .FirstOrDefaultAsync(c => c.Id == car.Id);

                    if (oldCar == null)
                    {
                        return NotFound();
                    }

                    // Remove selected photos
                    if (!string.IsNullOrEmpty(removePhotos))
                    {
                        var removedPhotoIds = removePhotos.Split(',')
                            .Select(id => int.TryParse(id, out var photoId) ? photoId : (int?)null)
                            .Where(id => id.HasValue)
                            .Select(id => id.Value)
                            .ToList();
                        
                        var photosToRemove = oldCar.Photos
                            .Where(p => removedPhotoIds.Contains(p.Id))
                            .ToList();

                        _context.Photos.RemoveRange(photosToRemove);
                    }

                    // Update existing car properties
                    oldCar.Name = car.Name;
                    oldCar.Preco = car.Preco;
                    oldCar.Marca = car.Marca.ToLower();
                    oldCar.Modelo = car.Modelo;
                    oldCar.Combustível = car.Combustível;
                    oldCar.DataRegisto = car.DataRegisto;
                    oldCar.Quilometros = car.Quilometros;
                    oldCar.Cilindrada = car.Cilindrada;
                    oldCar.Potencia = car.Potencia;
                    oldCar.Cor = car.Cor;
                    oldCar.Caixa = car.Caixa;
                    oldCar.NumMudancas = car.NumMudancas;
                    oldCar.NumPortas = car.NumPortas;
                    oldCar.Descricao = car.Descricao;
                    oldCar.Tracao = car.Tracao;

                    // Add new photos
                    if (car.Files != null)
                    {
                        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".heic" };
                        foreach (var formFile in car.Files)
                        {
                            if (formFile.Length > 0)
                            {
                                var fileExtension = Path.GetExtension(formFile.FileName);
                                var length = formFile.Length;

                                if (!allowedExtensions.Contains(fileExtension.ToLower()))
                                {
                                    ModelState.AddModelError("Files", $"File {formFile.FileName} has an unsupported extension. Only .jpg, .jpeg, .png, .gif, and .heic are allowed.");
                                    continue;
                                }

                                using (var memoryStream = new MemoryStream())
                                {
                                    await formFile.CopyToAsync(memoryStream);
                                    if (memoryStream.Length < 2097152)
                                    {
                                        byte[] fileBytes = memoryStream.ToArray();
                                        if (fileExtension.ToUpper().Equals(".HEIC"))
                                        {
                                            try
                                            {
                                                using (MagickImage image = new MagickImage(fileBytes))
                                                {
                                                    image.Format = MagickFormat.Jpeg;
                                                    fileBytes = image.ToByteArray();
                                                    length = fileBytes.Length;
                                                    fileExtension = ".jpg";
                                                }
                                            }
                                            catch (Exception e)
                                            {
                                                Console.WriteLine($"Error converting HEIC file to JPEG: {e.Message}");
                                                ModelState.AddModelError("Files", "An error occurred while converting HEIC file to JPEG. Please try again.");
                                            }
                                        }

                                        var newPhoto = new Photo()
                                        {
                                            Bytes = fileBytes,
                                            Description = formFile.FileName,
                                            FileExtension = fileExtension,
                                            Size = length,
                                        };
                                        oldCar.Photos.Add(newPhoto);
                                    }
                                    else
                                    {
                                        ModelState.AddModelError("Files", "The file is too large.");
                                    }
                                }
                            }
                        }
                    }

                    _context.Update(oldCar);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CarExists(car.Id))
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
            return View(car);
        }

        // GET: Car/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var car = await _context.Car
                .Include(c => c.Photos)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (car == null)
            {
                return NotFound();
            }

            return View(car);
        }

        // POST: Car/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var car = await _context.Car.FindAsync(id);
            if (car != null)
            {
                _context.Car.Remove(car);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CarExists(int id)
        {
            return _context.Car.Any(e => e.Id == id);
        }
    }
}
