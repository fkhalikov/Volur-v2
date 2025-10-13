import { Routes, Route } from 'react-router-dom'
import Layout from './components/Layout'
import ExchangesPage from './pages/ExchangesPage'
import SymbolsPage from './pages/SymbolsPage'

function App() {
  return (
    <Routes>
      <Route path="/" element={<Layout />}>
        <Route index element={<ExchangesPage />} />
        <Route path="exchanges" element={<ExchangesPage />} />
        <Route path="exchanges/:code/symbols" element={<SymbolsPage />} />
      </Route>
    </Routes>
  )
}

export default App

