import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import { VitePWA } from 'vite-plugin-pwa'

const API = 'http://localhost:5000' 

export default defineConfig({
  plugins: [
    react(),
    VitePWA({
      registerType: 'autoUpdate',
      manifest: {
        name: 'Adopta+',
        short_name: 'Adopta+',
        description: 'Plataforma de adopción y seguimiento de mascotas',
        start_url: '/',
        display: 'standalone',
        theme_color: '#5E9EA0',
        background_color: '#ffffff',
        icons: [
          { src: '/icons/pwa-192x192.png', sizes: '192x192', type: 'image/png' },
          { src: '/icons/pwa-512x512.png', sizes: '512x512', type: 'image/png' }
        ]
      },
      workbox: {
        runtimeCaching: [{
          urlPattern: ({ url }) => url.pathname.startsWith('/api'),
          handler: 'NetworkFirst',
          options: { cacheName: 'api-cache' }
        }]
      }
    })
  ],
  server:  { host: true, proxy: { '/api': API } },
  preview: { host: true, proxy: { '/api': API } }
})