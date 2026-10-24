# OneSource Solutions

Full-stack sales/catalog site for OneSource Solutions Limited (ICT company - TIMS, eTIMS, POS, IT, Security).

## Structure

- `web/` — Static frontend (React via in-browser Babel, no build step). `index.html` is the entire site.
- `api/` — ASP.NET Core Web API (.NET) backend with PostgreSQL, providing product/category CRUD, image upload, and JWT-based admin authentication.

## Backend setup (`api/`)

1. Requires .NET SDK and a PostgreSQL database.
2. Set `ConnectionStrings__DefaultConnection` to your PostgreSQL connection string. Do not put database credentials in the tracked `appsettings.json`. For a local PowerShell session:
   ```powershell
   $env:ConnectionStrings__DefaultConnection = "Host=localhost;Port=5432;Database=onesource_db;Username=postgres;Password=<your-password>"
   ```
   Set `Jwt:Key` to a long random secret used to sign login tokens, and configure `AllowedOrigins` with the URL(s) where the frontend is hosted.
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
- `PUT /api/ProductCategories/{id}` — Admin-only category rename/description update
- `GET /api/Products/by-category/{id}` — list products (id = 0 for all)
- `PUT /api/Products/{id}` — Admin-only product name, description, category, and code update
- `GET /api/Products/template` — Admin-only Excel import template (`.xlsx`)
- `POST /api/Products/import` — Admin-only Excel import; creates missing categories and upserts products by product code
- `GET /api/Products/export` — Admin-only Excel export of active products and categories
- `GET /api/Contact` — read the public contact details
- `PUT /api/Contact` — Admin-only replacement of the contact detail list (`details` entries have `label`, `value`, `type`, and optional `isEnquiry`; enquiry targets can be phone, WhatsApp, or email; a removed WhatsApp number is retained as the enquiry fallback)
- `GET /api/Carousel` — read the carousel slides
- `PUT /api/Carousel` — Admin-only replacement of carousel slides (image URL, alt text, label, description, and title)
- `POST /api/Carousel/upload-image` — Admin-only upload of a local JPEG, PNG, WebP, or GIF carousel image (up to 10 MB); returns an image URL for the carousel slide
- `POST /api/Auth/login` — returns a JWT for `admin` role
- Admin-only (require `Authorization: Bearer <token>`): create/update/delete categories, create/update/delete products, upload/delete product images, and import/export product spreadsheets. New products and imported rows without a code receive the next numeric product code (`001`, `002`, ...); product codes already assigned during backfill are preserved. Spreadsheet columns are `Product Cate(*)`, `Product Cate Desc`, `Product Code`, `Product(*)`, and `Product Desc`; supplied product codes match case-insensitively.

## Frontend setup (`web/`)

`index.html` is a static file — no build step required. It calls the API at the base URL configured in the `API_BASE` constant near the top of the `SalesPage` component. Update this to point at your deployed API before publishing.

You can serve it via GitHub Pages, or simply open it directly in a browser.

## Deployment notes

- The frontend (`web/`) can be hosted for free via GitHub Pages.
- The backend (`api/`) needs a host that can run a .NET app with a PostgreSQL database (e.g. Render, Railway, Azure App Service). Update the frontend's `API_BASE` and the backend's `AllowedOrigins`/connection string accordingly once deployed.
