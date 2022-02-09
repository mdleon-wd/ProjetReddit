using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Linq;
using System.Collections.Generic;

#nullable disable

namespace ProjetReddit.Models
{
    public partial class ProjetRedditContext : DbContext
    {
        public ProjetRedditContext()
        {
        }

        public ProjetRedditContext(DbContextOptions<ProjetRedditContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Comment> Comments { get; set; }
        public virtual DbSet<Post> Posts { get; set; }
        public virtual DbSet<User> Users { get; set; }

//MÉTHODES DE MANIPULATION DE LA BASE DE DONNÉES

        //retourne l'enregistrement User dont l'Id a été reçu en paramètre
        //retourne null si aucun enregistrement ne correspond à ce Id (ou s'il y en a plusieurs (impossible: id est PK))
        // ou si une autre erreur est rencontrée
        public User GetUser(int userId)
        {
            User userRecord;
            try{
                userRecord = Users.Where(u => u.Id == userId).SingleOrDefault();
            }
            catch (System.Exception){
                userRecord = null;
            }

            return userRecord;
        }

        //retourne le 1er enregistrement User ayant le email reçu en paramètre. 
        //retourne null si aucun enregistrement User ne correspond à ce email 
        //ou si une autre erreur est rencontrée
        public User GetUserByEmail(string email){
            User user;
            try
            {
                user = Users.Where(u => u.Email == email).FirstOrDefault();
            }
            catch (System.Exception)
            {
                user = null;
            }

            return user;
        }

        public string GetPasswordByUser(int userId) {
             User userRecord = GetUser(userId);
            if (userRecord != null)
            {
                return userRecord.Pwd;
            }
            else{
                return null;
            }
            
        }

        //retourne le UserName de l'enregistrement User dont l'Id est reçu en paramètre
        //retourne null si le User n'existe pas ou s'il y en a plusieurs (impossible: id est PK)
        //ou si une autre erreur est rencontrée
        public string GetUserName(int userId){
            User user = GetUser(userId);
            if (user != null)
            {
                return user.UserName;
            }
            else{
                return null;
            }
        }

        //retourne l'enregistrement Post dont l'Id a été reçu en paramètre
        //retourne null si aucun enregistrement ne correspond à ce Id (ou s'il y en a plusieurs (impossible: id est PK))
        // ou si une autre erreur est rencontrée
        public Post GetPost(int postId)
        {
            Post post;
            try
            {
                post = Posts.Where(p => p.Id == postId).SingleOrDefault();
            }
            catch (System.Exception)
            {
                post = null;
            }
            return post;
        }

        //retourne la liste des enregistrements Post du user dont l'Id a été reçu en paramètre
        //retourne null si une erreur est rencontrée
        public List<Post> GetPostsForUserDateDesc(Int32 userId)
        {
            List<Post> postsForUser;
            try
            {
                postsForUser = Posts.Where(p => p.UserId == userId)
                                    .OrderByDescending(p => p.PublicationDate).ToList();
            }
            catch (System.Exception)
            {                
                postsForUser = null;
            }
            return postsForUser;
        }


        //retourne la liste des posts ayant au moins 1 interaction (vote ou commentaire)
        //en ordre décroissant de popularité: nbr upvote - nbr downvote + nbr de commentaires 
        //retourne null si une erreur est rencontrée
        public List<Post> GetPopularPosts()
        {
            List<Post> popularPosts;

            try
            {
                popularPosts = Posts.Where(p => !(p.UpVote == 0 && p.DownVote == 0 && p.Comments.Count == 0))
                                     .OrderByDescending(p => p.UpVote + p.DownVote + p.Comments.Count).ToList();
            }
            catch (System.Exception)
            {
                popularPosts = null;
            }
            return popularPosts;
        }


        public void AddUser(User user_p)
        {   
            Add<User>(user_p);
            SaveChanges();
        }

        public void AddPost(Post post)
            // ***************todo validation si ce lien existe déjà pour ce user
        {
            Add<Post>(post);
            SaveChanges();
        }

        public void AddComment(Comment comment)
        {
            Add<Comment>(comment);
            SaveChanges();
        }

        public void UpdatePost(Post post)
        {
            Update<Post>(post);
            SaveChanges();
        }

        public void RemovePost(int postId){
            Post pToRemove = Posts.Where(p => p.Id == postId).SingleOrDefault();
            if (pToRemove != null)
            {   //todo: statuer sur le On DELETE CASCADE sur la FK Comment TO Post  ce qui rendrait le remove des comments inutiles
                Comments.RemoveRange(pToRemove.Comments);
                SaveChanges();

                Remove<Post>(pToRemove);
                SaveChanges();
            }
        }


//Description de la bd générée par commande scafold
        
        /* 
                protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                {
                    if (!optionsBuilder.IsConfigured)
                    {
        #warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
                        optionsBuilder.UseMySql("server=mon-srv-bd.mysql.database.azure.com;port=3306;database=ProjetReddit;uid=Sylvie8544816@mon-srv-bd;pwd=Sylv138544816;sslmode=Preferred", Microsoft.EntityFrameworkCore.ServerVersion.Parse("8.0.15-mysql"));
                    }
                }
         */
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasCharSet("latin1")
                .UseCollation("latin1_swedish_ci");

            modelBuilder.Entity<Comment>(entity =>
            {
                entity.ToTable("comment");

                entity.HasIndex(e => e.PostId, "FK_COMMENT_POST");

                entity.HasIndex(e => e.UserId, "FK_COMMENT_USER");

                entity.Property(e => e.Id)
                    .HasColumnType("int(11)")
                    .HasColumnName("ID");

                entity.Property(e => e.Description).HasMaxLength(200);

                entity.Property(e => e.PostId)
                    .HasColumnType("int(11)")
                    .HasColumnName("PostID");

                entity.Property(e => e.PublicationDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.UserId)
                    .HasColumnType("int(11)")
                    .HasColumnName("UserID");

                entity.HasOne(d => d.Post)
                    .WithMany(p => p.Comments)
                    .HasForeignKey(d => d.PostId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_COMMENT_POST");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Comments)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_COMMENT_USER");
            });

            modelBuilder.Entity<Post>(entity =>
            {
                entity.ToTable("post");

                entity.HasIndex(e => e.UserId, "FK_POST_USER");

                entity.Property(e => e.Id)
                    .HasColumnType("int(11)")
                    .HasColumnName("ID");

                entity.Property(e => e.Descript).HasMaxLength(200);

                entity.Property(e => e.DownVote)
                    .HasColumnType("int(11)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.Link).HasMaxLength(100);

                entity.Property(e => e.PublicationDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.UpVote)
                    .HasColumnType("int(11)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.UserId)
                    .HasColumnType("int(11)")
                    .HasColumnName("UserID");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Posts)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_POST_USER");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("user");

                entity.HasIndex(e => e.Email, "Email")
                    .IsUnique();

                entity.Property(e => e.Id)
                    .HasColumnType("int(11)")
                    .HasColumnName("ID");

                entity.Property(e => e.Email).HasMaxLength(30);

                entity.Property(e => e.Pwd).HasMaxLength(30);

                entity.Property(e => e.UserName).HasMaxLength(30);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
