﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CCS.Models;
using Microsoft.AspNetCore.Identity;
using CCS.Repositories;

namespace CCS.Controllers
{
    //[Authorize]
    public class AccountController : Controller
    {
        // CREATIVE CYBER SOLUTIONS
        // CREATED: 04/10/2018
        // CREATED BY: JOHN BELL contact@conquest-marketing.com
        // UPDATED: 05/02/2018
        // UPDATED BY: JOHN BELL contact@conquest-marketing.com

        #region var and constructor


        private IProjectRepository project;
            private IProductRepository product;
            private UserManager<User> userManager;
            private SignInManager<User> signInManager;
            private IUserValidator<User> userValidator;
            private IPasswordValidator<User> passwordValidator;
            private IPasswordHasher<User> passwordHasher;
            private IMessageRepository message;
            private IProjectProductsRepository prodProj;
            private INoteRepository note;

            public AccountController(UserManager<User> usrMgr,
                SignInManager<User> signinMgr,
                IUserValidator<User> userValid,
                IPasswordValidator<User> passValid,
                IPasswordHasher<User> passwordHash,
                IMessageRepository repos,
                IProjectRepository proj,
                IProductRepository prod,
                IProjectProductsRepository prop,
                INoteRepository nor)
            {
                signInManager = signinMgr;
                userManager = usrMgr;
                userValidator = userValid;
                passwordValidator = passValid;
                passwordHasher = passwordHash;
                message = repos;
                project = proj;
                product = prod;
                prodProj = prop;
                note = nor;
            }

        #endregion

        #region Account Functions
        [Authorize]
            public IActionResult Index() => View(GetData(nameof(Index)));

            [Authorize(Roles = "Users")]
            public IActionResult OtherAction() => View("Index",
                GetData(nameof(OtherAction)));

            private Dictionary<string, object> GetData(string actionName) =>
                new Dictionary<string, object>
                {
                    ["Action"] = actionName,
                    ["User"] = HttpContext.User.Identity.Name,
                    ["Authenticated"] = HttpContext.User.Identity.IsAuthenticated,
                    ["Auth Type"] = HttpContext.User.Identity.AuthenticationType,
                    ["In Users Role"] = HttpContext.User.IsInRole("Users")
                };

            [AllowAnonymous]
            public IActionResult Login(string returnUrl)
            {
                ViewBag.returnUrl = returnUrl;
                return View();
            }

            [HttpPost]
            [AllowAnonymous]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> Login(LoginModel details,
                string returnUrl)
            {
                if (ModelState.IsValid)
                {
                    User user = await userManager.FindByEmailAsync(details.Email);
                    if (user != null)
                    {
                        await signInManager.SignOutAsync();
                        Microsoft.AspNetCore.Identity.SignInResult result =
                            await signInManager.PasswordSignInAsync(
                                user, details.Password, false, false);
                        if (result.Succeeded)
                        {
                            return Redirect(returnUrl ?? "/Account");
                        }
                    }
                    ModelState.AddModelError(nameof(LoginModel.Email),
                        "Invalid user or password");
                }
                return View(details);
            }

            [Authorize]
            public async Task<IActionResult> Logout()
            {
                await signInManager.SignOutAsync();
                return RedirectToAction("Index", "Home");
            }

            [AllowAnonymous]
            public IActionResult AccessDenied()
            {
                return View();
            }


            [HttpGet]
            public ViewResult Register() => View();


        [HttpPost]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            if (ModelState.IsValid)

            {
                User user = new User
                {
                    UserName = model.Name,
                    Email = model.Email
                };
                IdentityResult result
                    = await userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    await signInManager.PasswordSignInAsync(
                                user, model.Password, false, false);
                    return RedirectToAction("Index");
                }
                else
                {
                    foreach (IdentityError error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
            }
            return View(model);
        }

      
        #endregion

        #region Message System Views

        public IActionResult MessageList()
            {
            List<Message> mess = new List<Message>();
                ViewBag.UserId = GetCurrentUserId();
                 mess = message.GetMessagesToAndFromUser(GetCurrentUserId()).OrderByDescending(a => a.Date).ToList<Message>();
                foreach (Message m in mess)
                {
                if (!userManager.FindByIdAsync(m.FromID).IsCompletedSuccessfully) m.FromName = "[Deleted]";
                    else m.FromName = userManager.FindByIdAsync(m.FromID).Result.UserName;
                if (!userManager.FindByIdAsync(m.ToID).IsCompletedSuccessfully) m.ToUser = "[Deleted]";
                else m.ToUser = userManager.FindByIdAsync(m.ToID).Result.UserName;
                }
            ViewBag.Message = TempData["Message"];
                return View(mess);
            }


        public IActionResult MessageView(int id)
        {
            ViewBag.UserId = GetCurrentUserId();

            var messages = message.GetMessages(id, GetCurrentUserId());
            foreach (Message m in messages)
            {
                if (userManager.Users.FirstOrDefault(a => a.UserName == m.ToUser)==null) m.FromName = "[Deleted]";
                else m.FromName = userManager.Users.FirstOrDefault(a => a.UserName == m.ToUser).UserName;
            }
            ViewBag.Item = messages.Last().ID;
            return View(messages);
        }

            [HttpGet]
            public IActionResult MessageSend() => View(new Message() { FromID = GetCurrentUserId() });


            [HttpPost]
            public IActionResult MessageSend(Message m)
            {

            
                if(userManager.Users.FirstOrDefault(a => a.UserName == m.ToUser) != null)

            {
                m.ToID = userManager.Users.FirstOrDefault(a => a.UserName == m.ToUser).Id;
                m.Date = DateTime.Now;
                message.Add(m);
                TempData["Message"] = "Message Sent to " + m.ToUser + "!";
                return RedirectToAction("MessageList");
            }
            else { 
                ViewBag.Message = "User not Found, Please Check your Recipient and Try Again";
                return View(m);
            }
            }

        [HttpPost]
        public IActionResult MessageReply(Message m)
        {
            
            //Message m = new Message();
            //m.FromID = FromId;
            //m.ToID = ToId;
            //m.Parent = Parent;
            //m.Text = Text;
            //m.Subject = Subject;
            m.Date = DateTime.Now;
            m.Status = Read.Unread;
            message.Add(m);
            ViewBag.Message = "Message Sent to " + m.ToUser + "!";
            return RedirectToAction("MessageList");
        }
        #endregion

        #region Project Views
        [HttpGet]
        public IActionResult ProjectRequest() {

            @ViewBag.UserId = GetCurrentUserId();
            return View();
        }

        public IActionResult ProjectList() => View(project.ShowProjectsByCustomer(GetCurrentUserId()));

        [HttpPost]
        public IActionResult ProjectRequest(Project p)
        {
            project.Add(p);
            ViewBag.Message = "Project Requested!";
            return View("ProjectList", project.ShowProjectsByCustomer(GetCurrentUserId()));
        }

        public IActionResult ProjectView(int id) => View(project.ShowProjectByID(id));




        #endregion

        #region Note Views

        [HttpGet]
        public IActionResult NoteAdd(int id)
        {
            if (userManager.GetUserAsync(HttpContext.User).Result.Id is null)
            {
                ViewBag.From = 0;
            }
            else ViewBag.From = userManager.GetUserAsync(HttpContext.User).Result.Id;
            Note n = new Note();
            n.ProjectID = id;
            return View(n);
        }

        [HttpPost]
        public IActionResult NoteAdd(Note n)
        {
            n.Date = DateTime.Now;
            note.AddNote(n.ProjectID, n);
            ViewBag.Message = "Note Added!";
            return RedirectToAction("ProjectView", n.ProjectID);
        }

        [HttpGet]
        public IActionResult NoteEdit(int id) => View(note.GetNote(id));
        [HttpPost]
        public IActionResult NoteEdit(Note n)
        {
            note.UpdateNote(n);
            ViewBag.Message = "Message Updated!";
            return RedirectToAction("ProjectView", n.ProjectID);
        }
        #endregion

        public string GetCurrentUserId() => userManager.GetUserAsync(HttpContext.User).Result.Id ?? 0.ToString();

        }
    }
