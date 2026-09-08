# OneSource Solutions

Full-stack sales/catalog site for OneSource Solutions Limited (ICT company - TIMS, eTIMS, POS, IT, Security).

## Structure

- `web/` — Static frontend (React via in-browser Babel, no build step). `index.html` is the entire site.
- `api/` — ASP.NET Core Web API (.NET) backend with PostgreSQL, providing product/category CRUD, image upload, and JWT-based admin authentication.

## Backend setup (`api/`)

1. Requires .NET SDK and a PostgreSQL database.
2. Copy `appsettings.json` and fill in your real values:
   - `ConnectionStrings:DefaultConnection` — your PostgreSQL connection string.
   - `Jwt:Key` — a long random secret used to sign login tokens (change the placeholder!).
   - `AllowedOrigins` — add the URL(s) where the frontend is hosted.
3. Run:
   ```
   cd api
   dotnet restore
   dotnet run
   ```
   On first run, the app auto-applies EF Core migrations and seeds:
   - Product categories & sample products
   - A default admin user: **username `admin`, password `Admin@123`** — change this password immediately after first login.

### Key endpoints

- `GET /api/ProductCategories` — list categories
- `GET /api/Products/by-category/{id}` — list products (id = 0 for all)
- `POST /api/Auth/login` — returns a JWT for `admin` role
- Admin-only (require `Authorization: Bearer <token>`): create/delete categories, create/update/delete products, upload/delete product images

## Frontend setup (`web/`)

`index.html` is a static file — no build step required. It calls the API at the base URL configured in the `API_BASE` constant near the top of the `SalesPage` component. Update this to point at your deployed API before publishing.

You can serve it via GitHub Pages, or simply open it directly in a browser.

## Deployment notes

- The frontend (`web/`) can be hosted for free via GitHub Pages.
- The backend (`api/`) needs a host that can run a .NET app with a PostgreSQL database (e.g. Render, Railway, Azure App Service). Update the frontend's `API_BASE` and the backend's `AllowedOrigins`/connection string accordingly once deployed.
