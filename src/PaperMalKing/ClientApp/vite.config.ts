import { defineConfig, splitVendorChunkPlugin} from 'vite'
import react from '@vitejs/plugin-react-swc'
import mkcert from 'vite-plugin-mkcert';
import {resolve} from 'path';


const c = await import('./config.json').then(x => x.default).catch(() => ({
    vite:{
        usePolling: false
    }
}));
// https://vitejs.dev/config/
export default defineConfig({
    base: '/app',
    build:{
        rollupOptions:{
            treeshake: "recommended",
        },
        outDir: "../wwwroot"
    },
    resolve: {
        alias: {
            '@': resolve(__dirname, './src'),
        }
    },
    plugins: [react(), mkcert(), splitVendorChunkPlugin()],
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
        },
        hmr: true,
        watch:{
            usePolling: c.vite.usePolling
        }
    }
})
