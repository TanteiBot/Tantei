import { defineConfig, type PluginOption, splitVendorChunkPlugin} from 'vite'
import react from '@vitejs/plugin-react-swc'
import mkcert from 'vite-plugin-mkcert';
import { visualizer } from "rollup-plugin-visualizer";
import {resolve} from 'path';
import checker from 'vite-plugin-checker';

// https://vitejs.dev/config/
export default defineConfig({
    build:{
        rollupOptions:{
            treeshake: "recommended",
        }
    },
    resolve: {
        alias: {
            '@': resolve(__dirname, './src'),
            '~': resolve(__dirname, 'node_modules')
        }
    },
    plugins: [react(), mkcert(), visualizer() as PluginOption, splitVendorChunkPlugin(), checker({typescript:true, enableBuild: false})],
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