using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MyFirstIdentity.Models;
using MyFirstIdentity.ViewModels;
using System.Runtime.CompilerServices;

namespace MyFirstIdentity.Controllers
{
    public class UserController : Controller
    {
        private readonly UserManager<User> _userManager;        //These are dependency Injectors that injects two services into the program 
        private readonly SignInManager<User> _signInManager;

        public UserController(SignInManager<User> signInManager, UserManager<User> userManager)     //The program does work without this constructor as well
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }
        public async Task<IActionResult> Index()
        {
            var users = _userManager.Users.ToList(); //It return all the users registered in the db and store it in users

            var userViewModels = new List<UserViewModel>(); //Creating a list in which we will add all the users

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);  //Extracting the role of the user

                userViewModels.Add(new UserViewModel { Id = user.Id,UserName= user.UserName, Email=user.Email, Name=user.Name, Address= user.Address, Roles = roles });
                //Adding all the data into the list we created above
            }
            return View(userViewModels);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(RegisterVM model, string role)      //When in HttpPost, The ViewModel is passed into the parameter in the beginning
        {
            if (ModelState.IsValid)
            {
                var user = new User { Name = model.Name, Email = model.Email, Address = model.Address, Password = model.Password, UserName = model.Email };

                var result = await _userManager.CreateAsync(user, model.Password!);

                if (result.Succeeded)
                {
                    if (!string.IsNullOrEmpty(role) && (role == "Admin" || role == "User")) //Checking if role is admin or user or if left empty
                    {
                        await _userManager.AddToRoleAsync(user, role);  //The role will be added to the user logged in
                    }

                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "Users");
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            return View(model);
        }

        
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user= await _userManager.FindByIdAsync(id);     //searches for the user using the his id
            if (user == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);         //retrieves the user's role and stores in roles

            var model = new UserViewModel { Id = user.Id, UserName = user.UserName, Email = user.Email, Name = user.Name, Address = user.Address, Password = user.Password, Roles = roles };
            
            return View(model); 
        }

        [HttpPost]
        public async Task<IActionResult> Edit(UserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(model.Id);
                if (user == null)
                {
                    return NotFound();
                }

                user.UserName = model.UserName;
                user.Email = model.Email;
                user.Name = model.Name;
                user.Address = model.Address;
                user.Password = model.Password;

                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            return View(model);
        }

        public async Task<IActionResult> Delete(string id) 
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _userManager.FindByNameAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            
            return View(new UserViewModel {Id= user.Id, Address= user.Address, Email=user.Email, Name=user.Name, UserName=user.UserName});
        }


        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(new UserViewModel
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                Name = user.Name,
                Address = user.Address
            });
        }
    }

    
}
