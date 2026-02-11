# Recipe Vault

A full-stack recipe management system with AI-powered meal planning capabilities, demonstrating enterprise-level architecture patterns and modern web development practices.

[![Live Demo](https://img.shields.io/badge/Demo-recipesvault.org-blue)](https://recipesvault.org)
[![AI Chatbot](https://img.shields.io/badge/AI-munchai.org-green)](https://munchai.org)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

## Project Overview

Recipe Vault is a comprehensive recipe management ecosystem consisting of two interconnected applications:

- **Recipe Vault** (ASP.NET Core MVC): Full-featured web application for recipe management, meal planning, and user profiles
- **MunchAI** (Python Flask): AI-powered chatbot using RAG architecture for intelligent recipe recommendations

Both applications share a PostgreSQL database with 280+ ingredients across 28+ categories and 50+ curated recipes.

### Key Features

- **Google OAuth Authentication** - Secure user authentication and profile management
- **AI-Powered Recommendations** - RAG-based chatbot for personalized recipe suggestions
- **Meal Planning** - Weekly meal planning with automated shopping list generation
- **Newsletter System** - Integrated email newsletters via Brevo API
- **Advanced Search** - Filter recipes by cuisine, ingredients, tags, and difficulty
- **High Performance** - Sub-second response times with optimized PostgreSQL queries
- **Responsive Design** - Mobile-friendly interface with modern UI/UX

## Architecture

### Tech Stack

**Frontend:**
- ASP.NET Core MVC 8.0
- Razor Views
- Bootstrap 5
- JavaScript/jQuery

**Backend:**
- C# / .NET 8.0
- Entity Framework Core
- Python Flask (AI Service)

**Database:**
- PostgreSQL (Production)
- Azure SQL Server (Legacy)

**AI/ML:**
- Claude API (Anthropic)
- RAG (Retrieval-Augmented Generation) Architecture
- Custom prompt engineering

**Cloud & Deployment:**
- Railway (Hosting)
- Custom Domain Configuration
- Azure Computer Vision (OCR)

**Authentication & Services:**
- Google OAuth 2.0
- Brevo Email API

### RAG Implementation

The AI chatbot uses a "database first, AI second" approach to prevent hallucinations:

1. **Query Structured Data**: User questions first query the PostgreSQL database for relevant recipes, ingredients, and nutritional information
2. **Context Assembly**: Retrieved data is formatted into structured context
3. **AI Generation**: Context is sent to Claude API with engineered prompts for natural language responses
4. **Grounded Responses**: AI generates answers based only on verified database information

This architectural pattern demonstrates enterprise-level AI integration applicable to healthcare systems, customer service, and other domains requiring reliable, data-backed conversational AI.

### Performance Optimization

- **Database Migration**: Migrated from Azure SQL Server to PostgreSQL, reducing response times from 20+ seconds to sub-second performance
- **Query Optimization**: Implemented efficient JOIN operations and indexing strategies
- **Connection Pooling**: Configured optimal database connection management
- **Caching**: Strategic caching of frequently accessed data

## Database Schema

The system uses a normalized relational database with the following core tables:

- `Recipes` - Recipe metadata and cooking information
- `Ingredients` - Comprehensive ingredient catalog (280+ items)
- `RecipeIngredients` - Many-to-many relationship with quantities
- `Instructions` - Step-by-step cooking instructions
- `Tags` - Recipe categorization and filtering
- `Cuisine` - Cultural cuisine classifications
- `Units` - Measurement units for ingredients
- `Category` - Ingredient categorization (28+ categories)

See [Data Dictionary](Data_Dictionary_Recipe_Database.docx) and [ERD](ERD_Recipe_Database.pdf) for complete schema documentation.

## Getting Started

### Prerequisites

- .NET 8.0 SDK
- PostgreSQL 14+
- Python 3.9+ (for AI service)
- Visual Studio 2022 or VS Code
- Git

### Installation

1. **Clone the repository**
```bash
git clone https://github.com/yourusername/recipe-vault.git
cd recipe-vault
```

2. **Set up the database**
```bash
# Create PostgreSQL database
createdb RecipeVaultDB

# Run migration scripts (in order)
psql -d RecipeVaultDB -f Database/01_Schema.sql
psql -d RecipeVaultDB -f Database/02_SeedData.sql
```

3. **Configure application settings**
```bash
# Copy example settings
cp appsettings.Example.json appsettings.Development.json

# Edit appsettings.Development.json with your credentials:
# - Database connection string
# - Google OAuth credentials
# - Claude API key
# - Brevo API key
```

4. **Install dependencies**
```bash
# .NET dependencies
dotnet restore

# Python dependencies (for AI service)
cd MunchAI
pip install -r requirements.txt
```

5. **Run the application**
```bash
# Start Recipe Vault
dotnet run

# In separate terminal, start MunchAI
cd MunchAI
python app.py
```

6. **Access the application**
- Recipe Vault: `https://localhost:5001`
- MunchAI: `http://localhost:5000`

### Configuration

Create `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=RecipeVaultDB;Username=your_user;Password=your_password"
  },
  "Authentication": {
    "Google": {
      "ClientId": "your-google-client-id.apps.googleusercontent.com",
      "ClientSecret": "your-google-client-secret"
    }
  },
  "Claude": {
    "ApiKey": "your-claude-api-key"
  },
  "Brevo": {
    "ApiKey": "your-brevo-api-key",
    "SenderEmail": "noreply@yourdomain.com",
    "SenderName": "Recipe Vault"
  }
}
```

## Project Structure

```
RecipeVault/
├── Controllers/          # MVC Controllers
│   ├── RecipesController.cs
│   ├── MealPlanController.cs
│   ├── AccountController.cs
│   └── NewsletterController.cs
├── Models/              # Entity models
│   ├── Recipe.cs
│   ├── Ingredient.cs
│   ├── MealPlan.cs
│   └── User.cs
├── ViewModels/          # View-specific models
├── Services/            # Business logic
│   ├── RecipeService.cs
│   ├── MealPlanService.cs
│   └── EmailService.cs
├── Views/               # Razor views
│   ├── Recipes/
│   ├── MealPlan/
│   └── Shared/
├── wwwroot/             # Static files
│   ├── css/
│   ├── js/
│   └── images/
├── Database/            # SQL scripts
│   ├── 01_Schema.sql
│   └── 02_SeedData.sql
├── MunchAI/             # Python AI service
│   ├── app.py
│   ├── rag_service.py
│   └── requirements.txt
└── appsettings.json     # Configuration
```

## Key Features Deep Dive

### Meal Planning System
- Drag-and-drop interface for weekly meal planning
- Automatic shopping list generation from selected recipes
- Ingredient deduplication and quantity aggregation
- Export to PDF or email

### AI Chatbot (RAG)
- Natural language recipe search
- Ingredient substitution suggestions
- Cooking technique explanations
- Dietary restriction filtering
- Nutritional information queries

### User Management
- Google OAuth integration
- User profile customization
- Favorite recipes management
- Meal plan history

### Newsletter System
- Automated recipe newsletters
- User subscription management
- Brevo API integration
- Template customization

## Performance Metrics

- **Response Time**: <1 second average (99th percentile)
- **Database Queries**: Optimized with proper indexing
- **Concurrent Users**: Tested up to 50 simultaneous users
- **Uptime**: 99.9% availability on Railway platform

## Security

- Google OAuth 2.0 authentication
- Secure password hashing (for future direct auth)
- SQL injection prevention via parameterized queries
- XSS protection through Razor encoding
- CSRF tokens on all forms
- HTTPS enforcement in production

## Development

### Running Tests
```bash
dotnet test
```

### Database Migrations
```bash
# Add migration
dotnet ef migrations add MigrationName

# Update database
dotnet ef database update
```

### Code Style
- Follow C# coding conventions
- Use async/await for I/O operations
- Implement repository pattern for data access
- Write XML documentation for public APIs

## Deployment

### Railway Deployment

1. **Connect GitHub repository**
2. **Configure environment variables**
3. **Deploy backend and AI service separately**
4. **Configure custom domains**

See [Deployment Guide](docs/DEPLOYMENT.md) for detailed instructions.

## Learning Outcomes

This project demonstrates proficiency in:

- **Full-Stack Development**: End-to-end application development with modern frameworks
- **AI Integration**: Implementing RAG architecture for enterprise applications
- **Cloud Deployment**: Production deployment with custom domain configuration
- **Database Design**: Normalized schema design and query optimization
- **API Integration**: Multiple third-party API integrations (Google, Claude, Brevo)
- **Authentication**: OAuth 2.0 implementation and user management
- **Performance Optimization**: Database migration and query tuning
- **DevOps**: CI/CD principles and cloud platform management

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Author

**Hesham**
- Portfolio: https://www.heshamrashid.org/
- LinkedIn: https://www.linkedin.com/in/hesham-rashid/ 
- Email: h.f.rashid@gmail.com

Master's in AI and Business Analytics - University of South Florida

## Acknowledgments

- Claude API by Anthropic
- Bootstrap framework
- PostgreSQL community
- Railway hosting platform

## Contact

For questions or collaboration opportunities, please reach out via [email] or LinkedIn.

---

**Note**: This is a portfolio project demonstrating technical capabilities in AI/ML integration, full-stack development, and cloud deployment. The architecture patterns used here are applicable to healthcare AI systems, customer service chatbots, and other enterprise applications requiring reliable, data-backed conversational AI.
