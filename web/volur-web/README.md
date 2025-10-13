# Volur Web

React + TypeScript frontend for the Volur market data platform.

## Tech Stack

- **React 18** - UI library
- **TypeScript** - Type safety
- **Vite** - Build tool and dev server
- **TanStack Query** - Data fetching and caching
- **React Router** - Client-side routing
- **Tailwind CSS** - Utility-first CSS

## Getting Started

### Install dependencies
```bash
npm install
```

### Run development server
```bash
npm run dev
```

The app will be available at http://localhost:5173

### Build for production
```bash
npm run build
```

### Preview production build
```bash
npm run preview
```

### Lint code
```bash
npm run lint
```

## Project Structure

```
src/
├── api/              # API client and types
│   └── client.ts     # HTTP client with typed methods
├── components/       # Reusable UI components
│   ├── Layout.tsx    # App layout with header/footer
│   ├── LoadingSpinner.tsx
│   ├── ErrorMessage.tsx
│   └── EmptyState.tsx
├── pages/           # Page components
│   ├── ExchangesPage.tsx
│   └── SymbolsPage.tsx
├── hooks/           # Custom React hooks
│   └── useDebounce.ts
├── types/           # TypeScript type definitions
│   └── api.ts       # API response types
├── App.tsx          # Root component with routes
├── main.tsx         # Application entry point
└── index.css        # Global styles with Tailwind
```

## Key Features

### Data Fetching with TanStack Query
```tsx
const { data, isLoading, error, refetch } = useQuery({
  queryKey: ['exchanges'],
  queryFn: () => api.getExchanges(),
})
```

Benefits:
- Automatic caching
- Background refetching
- Stale-while-revalidate
- Error handling
- Loading states

### Debounced Search
```tsx
const [searchTerm, setSearchTerm] = useState('')
const debouncedSearch = useDebounce(searchTerm, 300)

// Use debouncedSearch in query
useQuery({
  queryKey: ['symbols', code, debouncedSearch],
  queryFn: () => api.getSymbols(code, { q: debouncedSearch }),
})
```

### Responsive Design
- Mobile-first approach with Tailwind
- Responsive tables and navigation
- Touch-friendly UI elements

## API Integration

The app communicates with the backend API at `/api`. In development, Vite proxies API requests to `http://localhost:5000`.

### API Client
```typescript
// src/api/client.ts
export const api = {
  getExchanges: () => Promise<ExchangesResponse>
  getSymbols: (code, params) => Promise<SymbolsResponse>
  refreshSymbols: (code) => Promise<void>
}
```

### Type Safety
All API responses are typed using TypeScript interfaces that match the backend DTOs.

## Environment Variables

Create a `.env.local` file for local overrides:

```bash
VITE_API_URL=http://localhost:5000
```

## Development Tips

### Hot Module Replacement (HMR)
Vite provides instant HMR - your changes appear immediately without full page reload.

### Browser DevTools
- React DevTools for component inspection
- Network tab for API debugging
- Console for errors and logs

### Code Organization
- Keep components small and focused
- Extract reusable logic into hooks
- Use TypeScript for type safety
- Follow React best practices

## Styling with Tailwind

### Utility Classes
```tsx
<div className="flex items-center justify-between px-4 py-2 bg-white shadow rounded-lg">
  {/* content */}
</div>
```

### Responsive Design
```tsx
<div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
  {/* responsive grid */}
</div>
```

### Custom Theme
Extend the theme in `tailwind.config.js`:
```js
theme: {
  extend: {
    colors: {
      primary: '#...',
    },
  },
}
```

## Performance Optimization

### Code Splitting
React Router automatically code-splits routes. For lazy loading:
```tsx
const HeavyComponent = lazy(() => import('./HeavyComponent'))
```

### Memoization
```tsx
const MemoizedComponent = React.memo(ExpensiveComponent)
```

### Query Optimization
- Set appropriate `staleTime` for queries
- Use query keys effectively for caching
- Prefetch data on hover/interaction

## Testing

### Unit Tests (Coming Soon)
```bash
npm run test
```

### E2E Tests with Playwright (Coming Soon)
```bash
npm run test:e2e
```

## Build Output

The production build is optimized:
- Minified JavaScript and CSS
- Tree-shaking to remove unused code
- Asset hashing for cache busting
- Gzip compression ready

Build artifacts are output to `dist/`:
```
dist/
├── assets/
│   ├── index-[hash].js
│   └── index-[hash].css
└── index.html
```

## Deployment

### Static Hosting
Deploy the `dist/` folder to any static host:
- Netlify
- Vercel
- AWS S3 + CloudFront
- Azure Static Web Apps
- GitHub Pages

### Docker
Use the provided Dockerfile:
```bash
docker build -t volur-web .
docker run -p 3000:80 volur-web
```

### Nginx Configuration
The app includes nginx config for:
- SPA routing (fallback to index.html)
- Gzip compression
- Cache headers
- API proxying

## Browser Support

- Chrome (latest)
- Firefox (latest)
- Safari (latest)
- Edge (latest)

## Resources

- [React Documentation](https://react.dev)
- [TypeScript Handbook](https://www.typescriptlang.org/docs/)
- [TanStack Query](https://tanstack.com/query)
- [Tailwind CSS](https://tailwindcss.com)
- [Vite Guide](https://vitejs.dev/guide/)

