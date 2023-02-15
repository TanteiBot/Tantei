import { createRoot } from 'react-dom/client';
import App from './components/App';
// Import our custom CSS
import './styles/index.scss'

createRoot(document.getElementById('root')!).render(
        <App />
);
