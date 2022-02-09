using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ProjetReddit.Models;
using Microsoft.AspNetCore.Http;

namespace ProjetReddit.Controllers
{
    public class HomeController : Controller
    {
        private readonly ProjetRedditContext dbContext;
        private readonly ISession session;
        public HomeController(ProjetRedditContext context, IHttpContextAccessor accessor)
        {
            this.session = accessor.HttpContext.Session;
            this.dbContext = context;
        }

       
        public IActionResult Index(string page = "Accueil")
        {
            if (session.Keys.Count() == 0)
            {
                return RedirectToAction("Login");
            }
            else
            {
                int userId = session.GetInt32("userId").GetValueOrDefault();
                ViewBag.UserName = dbContext.GetUserName(userId);

                IEnumerable<Post> posts;

                if (page == "Mes liens")
                {
                    posts = dbContext.GetPostsForUserDateDesc(userId);
                }
                else //if (page == "Accueil")
                {
                    posts = dbContext.GetPopularPosts();
                }

                ViewBag.Page = page;

                return View(posts);
            }

        }

        public IActionResult AddLink()
        {
            ViewBag.UserName = dbContext.GetUserName((int)session.GetInt32("userId"));
            ViewBag.Page = "Publier un lien";
            return View();
        }

        [HttpPost]
        public IActionResult SaveLink(string lien, string description)
        {
            Post nouveauPost = new Post()
            {
                Link = lien,
                Descript = description,
                UserId = session.GetInt32("userId")
            };
        
            
            dbContext.AddPost(nouveauPost);

            return RedirectToAction("MyLinks");
        }

        public IActionResult Link(int linkId)
        {
            if (session.Keys.Count() > 0){
                int userId = (int)session.GetInt32("userId");
                ViewBag.UserName = dbContext.GetUserName(userId);
            }
            


            Post post = dbContext.GetPost(linkId);
            if (post != null)
            {
                return View(post);
            }
            else
            {
                return RedirectToAction("Index");
            }

        }

        [HttpPost]
        public IActionResult SaveComment(int postId, string description)
        {
            Comment comment = new Comment()
            {
                Description = description,
                UserId = session.GetInt32("userId"),
                PostId = postId
            };

            dbContext.AddComment(comment);

            return RedirectToAction("Link", new { linkId = postId });
        }


        public IActionResult Vote(int postId, string type)
        {
            Post post = dbContext.GetPost(postId);
            if (post != null)
            {
                if (type == "up")
                {
                    post.UpVote += 1;
                }
                else if (type == "down")
                {
                    post.DownVote -= 1;
                }
                dbContext.UpdatePost(post);

                return RedirectToAction("Link", new { linkId = postId });
            }
            else
            {
                return RedirectToAction("Index");  
            }
 
        }

        public IActionResult MyLinks()
        {
            return RedirectToAction("Index", new { page = "Mes liens" });
        }

        public IActionResult RemoveLink(int linkToRemoveId)
        {
            dbContext.RemovePost(linkToRemoveId);

            return RedirectToAction("MyLinks");
        }

        public IActionResult Logout()
        {
            session.Clear();
            return RedirectToAction("Login");
        }

        public IActionResult Login(string emailPlaceholder_p="", string messageEmail_p = "", string messagePwd_p="")
        {
            ViewBag.emailPlaceholder = emailPlaceholder_p;
            ViewBag.messageEmail = messageEmail_p;
            ViewBag.messagePwd = messagePwd_p;
            return View();
        }

        

        [HttpPost]
        public IActionResult VerifyLogin(string email, string pwd)
            {
                //Lecture du user ayant ce email dans la base de donnée
                User userRecord = dbContext.GetUserByEmail(email);
                //Validation du email
                if (userRecord == null)
                {
                    
                    return RedirectToAction("Login", new { messageEmail_p = "Courriel invalide. Réessayez s'il vous plait." });
                }
                else
                {
                    string pwdContext = dbContext.GetPasswordByUser(userRecord.Id);
                    //Validation du password
                    if (pwdContext != pwd)
                    {
                        
                        return RedirectToAction("Login", new { emailPlaceholder_p = email, messagePwd_p = "Mot de passe invalide. Réessayez s'il vous plait." });
                    }
                }
                
                //enregistrement du user dans la session
                if (userRecord != null)
                {
                    session.SetInt32("userId", userRecord.Id);
                }
                else
                {
                    session.SetInt32("userId", 1);
                }
               
                return RedirectToAction("Index");
            }

        public IActionResult Register()
        {
            return View();
        }

     [HttpPost]
        public IActionResult VerifyRegistration(string username, string email, string pwd)
        {
            //Lecture du user ayant ce email dans la base de donnée            
            User userRecord = dbContext.GetUserByEmail(email);
            //Validation de l'existance de ce user
            if(userRecord != null){
                return RedirectToAction("Login", new { emailPlaceholder_p = email, messageEmail_p = "Vous êtes déjà inscrit! Réessayez s'il vous plait." });
            }
            else {
                User nouveauUser = new User()
                 {
                     UserName = username,
                     Email = email,
                     Pwd = pwd
                 };

                 dbContext.AddUser(nouveauUser);

                return RedirectToAction("Login", new { emailPlaceholder_p = email, messageEmail_p = "Inscription complétée! Veuillez vous connectez."});
            }
                
        }  
            



        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

    }
}
