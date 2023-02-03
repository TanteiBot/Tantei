import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react-swc'
import mkcert from 'vite-plugin-mkcert';
// https://vitejs.dev/config/


export default defineConfig({
    plugins: [react(), mkcert()],
    server: {
        port: 44428,
        https: true,
        strictPort : true,
        proxy: {
            '/api' : {
                target: 'https://localhost:7131',
                changeOrigin: true,
                secure: false,
                rewrite: (path) => path.replace(/^\/api/, '/api')
            }
        }
    }
})