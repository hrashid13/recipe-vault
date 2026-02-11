using Microsoft.EntityFrameworkCore;
using RecipeManager.Models;
using RecipesVault.Models;

namespace RecipeManager.Data
{
    public class RecipeDbContext : DbContext
    {
        public RecipeDbContext(DbContextOptions<RecipeDbContext> options)
            : base(options)
        {
            // Disable lazy loading to prevent N+1 query problems
            this.ChangeTracker.LazyLoadingEnabled = false;

            // Disable automatic query splitting for better performance
            //  this.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        public DbSet<Recipe> Recipes { get; set; }
        public DbSet<Ingredient> Ingredients { get; set; }
        public DbSet<Instruction> Instructions { get; set; }
        public DbSet<RecipeIngredient> RecipeIngredients { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<RecipeTag> RecipeTags { get; set; }
        public DbSet<Unit> Units { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Cuisine> Cuisines { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserRecipe> UserRecipes { get; set; }
        public DbSet<NewsletterLog> NewsletterLogs { get; set; }
        public DbSet<UserNewsletterHistory> UserNewsletterHistories { get; set; }
        public DbSet<NewsletterSubscriber> NewsletterSubscribers { get; set; }

        // Meal Planner & Shopping List
        public DbSet<UserMealPlan> UserMealPlans { get; set; }
        public DbSet<ShoppingList> ShoppingLists { get; set; }
        public DbSet<ShoppingListItem> ShoppingListItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Recipe entity
            modelBuilder.Entity<Recipe>(entity =>
            {
                entity.ToTable("recipes");
                entity.HasKey(r => r.RecipeID);
                entity.Property(r => r.RecipeID).HasColumnName("recipeid");
                entity.Property(r => r.RecipeName).HasColumnName("recipename");
                entity.Property(r => r.Description).HasColumnName("description");
                entity.Property(r => r.PrepTime).HasColumnName("preptime");
                entity.Property(r => r.CookTime).HasColumnName("cooktime");
                entity.Property(r => r.Servings).HasColumnName("servings");
                entity.Property(r => r.DifficultyLevel).HasColumnName("difficultylevel");
                entity.Property(r => r.DateAdded).HasColumnName("dateadded");
                entity.Property(r => r.CuisineID).HasColumnName("cuisineid");

                // Configure relationships - explicitly prevent cascade delete issues
                entity.HasOne(r => r.Cuisine)
                    .WithMany()
                    .HasForeignKey(r => r.CuisineID)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Ingredient entity
            modelBuilder.Entity<Ingredient>(entity =>
            {
                entity.ToTable("ingredients");
                entity.HasKey(i => i.IngredientID);
                entity.Property(i => i.IngredientID).HasColumnName("ingredientid");
                entity.Property(i => i.IngredientName).HasColumnName("ingredientname");
                entity.Property(i => i.CategoryID).HasColumnName("categoryid");
            });

            // Instruction entity
            modelBuilder.Entity<Instruction>(entity =>
            {
                entity.ToTable("instructions");
                entity.HasKey(i => i.InstructionsID);
                entity.Property(i => i.InstructionsID).HasColumnName("instructionsid");
                entity.Property(i => i.StepNumber).HasColumnName("stepnumber");
                entity.Property(i => i.InstructionText).HasColumnName("instructiontext");
                entity.Property(i => i.RecipeID).HasColumnName("recipeid");

                // Relationship
                entity.HasOne(i => i.Recipe)
                    .WithMany(r => r.Instructions)
                    .HasForeignKey(i => i.RecipeID)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // RecipeIngredient entity
            modelBuilder.Entity<RecipeIngredient>(entity =>
            {
                entity.ToTable("recipeingredients");
                entity.HasKey(ri => ri.RecipeIngredientID);
                entity.Property(ri => ri.RecipeIngredientID).HasColumnName("recipeingredientid");
                entity.Property(ri => ri.Quantity).HasColumnName("quantity");
                entity.Property(ri => ri.Notes).HasColumnName("notes");
                entity.Property(ri => ri.UnitID).HasColumnName("unitid");
                entity.Property(ri => ri.RecipeID).HasColumnName("recipeid");
                entity.Property(ri => ri.IngredientID).HasColumnName("ingredientid");

                // Relationships
                entity.HasOne(ri => ri.Recipe)
                    .WithMany(r => r.RecipeIngredients)
                    .HasForeignKey(ri => ri.RecipeID)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ri => ri.Ingredient)
                    .WithMany()
                    .HasForeignKey(ri => ri.IngredientID)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(ri => ri.Unit)
                    .WithMany()
                    .HasForeignKey(ri => ri.UnitID)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Tag entity
            modelBuilder.Entity<Tag>(entity =>
            {
                entity.ToTable("tags");
                entity.HasKey(t => t.TagID);
                entity.Property(t => t.TagID).HasColumnName("tagid");
                entity.Property(t => t.TagName).HasColumnName("tagname");
            });

            // RecipeTag entity
            modelBuilder.Entity<RecipeTag>(entity =>
            {
                entity.ToTable("recipetags");
                entity.HasKey(rt => rt.RecipeTagID);
                entity.Property(rt => rt.RecipeTagID).HasColumnName("recipetagid");
                entity.Property(rt => rt.RecipeID).HasColumnName("recipeid");
                entity.Property(rt => rt.TagID).HasColumnName("tagid");

                // Relationships
                entity.HasOne(rt => rt.Recipe)
                    .WithMany(r => r.RecipeTags)
                    .HasForeignKey(rt => rt.RecipeID)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(rt => rt.Tag)
                    .WithMany()
                    .HasForeignKey(rt => rt.TagID)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Unit entity
            modelBuilder.Entity<Unit>(entity =>
            {
                entity.ToTable("units");
                entity.HasKey(u => u.UnitID);
                entity.Property(u => u.UnitID).HasColumnName("unitid");
                entity.Property(u => u.UnitName).HasColumnName("unitname");
            });

            // Category entity
            modelBuilder.Entity<Category>(entity =>
            {
                entity.ToTable("categories");
                entity.HasKey(c => c.CategoryID);
                entity.Property(c => c.CategoryID).HasColumnName("categoryid");
                entity.Property(c => c.CategoryName).HasColumnName("categoryname");
            });

            // Cuisine entity
            modelBuilder.Entity<Cuisine>(entity =>
            {
                entity.ToTable("cuisine");
                entity.HasKey(c => c.CuisineID);
                entity.Property(c => c.CuisineID).HasColumnName("cuisineid");
                entity.Property(c => c.CuisineType).HasColumnName("cuisinetype");
            });
            // User entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("users");
                entity.HasKey(u => u.UserID);
                entity.Property(u => u.UserID).HasColumnName("userid");
                entity.Property(u => u.GoogleID).HasColumnName("googleid");
                entity.Property(u => u.Email).HasColumnName("email");
                entity.Property(u => u.DisplayName).HasColumnName("displayname");
                entity.Property(u => u.ProfilePictureUrl).HasColumnName("profilepictureurl");
                entity.Property(u => u.DateJoined).HasColumnName("datejoined");
                entity.Property(u => u.LastLogin).HasColumnName("lastlogin");
                entity.Property(u => u.IsNewsletterSubscribed).HasColumnName("isnewslettersubscribed");
                entity.Property(u => u.IsActive).HasColumnName("isactive");
                entity.Property(u => u.IsAdmin).HasColumnName("isadmin");
            });

            // UserRecipe entity
            modelBuilder.Entity<UserRecipe>(entity =>
            {
                entity.ToTable("userrecipes");
                entity.HasKey(ur => ur.UserRecipeID);
                entity.Property(ur => ur.UserRecipeID).HasColumnName("userrecipeid");
                entity.Property(ur => ur.UserID).HasColumnName("userid");
                entity.Property(ur => ur.RecipeID).HasColumnName("recipeid");
                entity.Property(ur => ur.DateSaved).HasColumnName("datesaved");
                entity.Property(ur => ur.Notes).HasColumnName("notes");

                // Relationships
                entity.HasOne(ur => ur.User)
                    .WithMany(u => u.UserRecipes)
                    .HasForeignKey(ur => ur.UserID)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ur => ur.Recipe)
                    .WithMany()
                    .HasForeignKey(ur => ur.RecipeID)
                    .OnDelete(DeleteBehavior.Cascade);

                // Unique constraint
                entity.HasIndex(ur => new { ur.UserID, ur.RecipeID }).IsUnique();
            });

            // NewsletterLog entity
            modelBuilder.Entity<NewsletterLog>(entity =>
            {
                entity.ToTable("newsletterlogs");
                entity.HasKey(n => n.NewsletterID);
                entity.Property(n => n.NewsletterID).HasColumnName("newsletterid");
                entity.Property(n => n.RecipeID).HasColumnName("recipeid");
                entity.Property(n => n.SentDate).HasColumnName("sentdate");
                entity.Property(n => n.SubjectLine).HasColumnName("subjectline");
                entity.Property(n => n.RecipientCount).HasColumnName("recipientcount");

                // Relationship
                entity.HasOne(n => n.Recipe)
                    .WithMany()
                    .HasForeignKey(n => n.RecipeID)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // UserNewsletterHistory entity
            modelBuilder.Entity<UserNewsletterHistory>(entity =>
            {
                entity.ToTable("usernewsletterhistories");
                entity.HasKey(h => h.HistoryID);
                entity.Property(h => h.HistoryID).HasColumnName("historyid");
                entity.Property(h => h.UserID).HasColumnName("userid");
                entity.Property(h => h.NewsletterID).HasColumnName("newsletterid");
                entity.Property(h => h.SentDate).HasColumnName("sentdate");
                entity.Property(h => h.WasOpened).HasColumnName("wasopened");
                entity.Property(h => h.OpenedDate).HasColumnName("openeddate");

                // Relationships
                entity.HasOne(h => h.User)
                    .WithMany(u => u.NewsletterHistory)
                    .HasForeignKey(h => h.UserID)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(h => h.Newsletter)
                    .WithMany(n => n.UserHistories)
                    .HasForeignKey(h => h.NewsletterID)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // NewsletterSubscriber entity
            modelBuilder.Entity<NewsletterSubscriber>(entity =>
            {
                entity.ToTable("newslettersubscribers");
                entity.HasKey(ns => ns.SubscriberID);
                entity.Property(ns => ns.SubscriberID).HasColumnName("subscriberid");
                entity.Property(ns => ns.Email).HasColumnName("email");
                entity.Property(ns => ns.UserID).HasColumnName("userid");
                entity.Property(ns => ns.SubscribedDate).HasColumnName("subscribeddate");
                entity.Property(ns => ns.IsActive).HasColumnName("isactive");
                entity.Property(ns => ns.UnsubscribeToken).HasColumnName("unsubscribetoken");
                entity.Property(ns => ns.LastEmailSent).HasColumnName("lastemailsent");

                // Indexes
                entity.HasIndex(ns => ns.Email).IsUnique();
                entity.HasIndex(ns => ns.UnsubscribeToken).IsUnique();
                entity.HasIndex(ns => ns.IsActive);
            });

            // UserMealPlan entity
            modelBuilder.Entity<UserMealPlan>(entity =>
            {
                entity.ToTable("usermealplans");
                entity.HasKey(ump => ump.MealPlanID);
                entity.Property(ump => ump.MealPlanID).HasColumnName("mealplanid");
                entity.Property(ump => ump.UserID).HasColumnName("userid");
                entity.Property(ump => ump.RecipeID).HasColumnName("recipeid");
                entity.Property(ump => ump.PlannedDate).HasColumnName("planneddate");
                entity.Property(ump => ump.DateCreated).HasColumnName("datecreated");

                // Relationship
                entity.HasOne(ump => ump.Recipe)
                    .WithMany()
                    .HasForeignKey(ump => ump.RecipeID)
                    .OnDelete(DeleteBehavior.Cascade);

                // Indexes
                entity.HasIndex(ump => ump.UserID);
                entity.HasIndex(ump => ump.PlannedDate);
            });

            // ShoppingList entity
            modelBuilder.Entity<ShoppingList>(entity =>
            {
                entity.ToTable("shoppinglists");
                entity.HasKey(sl => sl.ShoppingListID);
                entity.Property(sl => sl.ShoppingListID).HasColumnName("shoppinglistid");
                entity.Property(sl => sl.UserID).HasColumnName("userid");
                entity.Property(sl => sl.ListName).HasColumnName("listname");
                entity.Property(sl => sl.StartDate).HasColumnName("startdate");
                entity.Property(sl => sl.EndDate).HasColumnName("enddate");
                entity.Property(sl => sl.DateCreated).HasColumnName("datecreated");
                entity.Property(sl => sl.IsCompleted).HasColumnName("iscompleted");

                // Relationship
                entity.HasMany(sl => sl.ShoppingListItems)
                    .WithOne(sli => sli.ShoppingList)
                    .HasForeignKey(sli => sli.ShoppingListID)
                    .OnDelete(DeleteBehavior.Cascade);

                // Index
                entity.HasIndex(sl => sl.UserID);
            });

            // ShoppingListItem entity
            modelBuilder.Entity<ShoppingListItem>(entity =>
            {
                entity.ToTable("shoppinglistitems");
                entity.HasKey(sli => sli.ShoppingListItemID);
                entity.Property(sli => sli.ShoppingListItemID).HasColumnName("shoppinglistitemid");
                entity.Property(sli => sli.ShoppingListID).HasColumnName("shoppinglistid");
                entity.Property(sli => sli.IngredientID).HasColumnName("ingredientid");
                entity.Property(sli => sli.TotalQuantity).HasColumnName("totalquantity");
                entity.Property(sli => sli.UnitID).HasColumnName("unitid");
                entity.Property(sli => sli.CategoryID).HasColumnName("categoryid");
                entity.Property(sli => sli.IsChecked).HasColumnName("ischecked");

                // Relationships
                entity.HasOne(sli => sli.Ingredient)
                    .WithMany()
                    .HasForeignKey(sli => sli.IngredientID)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(sli => sli.Unit)
                    .WithMany()
                    .HasForeignKey(sli => sli.UnitID)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(sli => sli.Category)
                    .WithMany()
                    .HasForeignKey(sli => sli.CategoryID)
                    .OnDelete(DeleteBehavior.Restrict);

                // Indexes
                entity.HasIndex(sli => sli.ShoppingListID);
                entity.HasIndex(sli => sli.CategoryID);
            });
        }
    }
}