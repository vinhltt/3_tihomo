export default defineNuxtConfig({
    app: {
        head: {
            title: 'Sales Admin | VRISTO - Multipurpose Tailwind Dashboard Template',
            titleTemplate: '%s | VRISTO - Multipurpose Tailwind Dashboard Template',
            htmlAttrs: {
                lang: 'en',
            },
            meta: [
                { charset: 'utf-8' },
                {
                    name: 'viewport',
                    content: 'width=device-width, initial-scale=1, maximum-scale=1, shrink-to-fit=no',
                },
                { hid: 'description', name: 'description', content: '' },
                { name: 'format-detection', content: 'telephone=no' },
            ],
            link: [
                { rel: 'icon', type: 'image/x-icon', href: '/favicon.png' },
                {
                    rel: 'stylesheet',
                    href: 'https://fonts.googleapis.com/css2?family=Nunito:wght@400;500;600;700;800&display=swap',
                },
            ],
        },
    },

    css: ['~/assets/css/app.css'],

    postcss: {
        plugins: {
            tailwindcss: {},
            autoprefixer: {},
        },
    },

    modules: ['@pinia/nuxt', '@nuxtjs/i18n'],

    i18n: {
        locales: [
            { code: 'da', file: 'da.json' },
            { code: 'de', file: 'de.json' },
            { code: 'el', file: 'fr.json' },
            { code: 'en', file: 'en.json' },
            { code: 'es', file: 'es.json' },
            { code: 'fr', file: 'fr.json' },
            { code: 'hu', file: 'hu.json' },
            { code: 'it', file: 'it.json' },
            { code: 'ja', file: 'ja.json' },
            { code: 'pl', file: 'pl.json' },
            { code: 'pt', file: 'pt.json' },
            { code: 'ru', file: 'ru.json' },
            { code: 'sv', file: 'sv.json' },
            { code: 'tr', file: 'tr.json' },
            { code: 'zh', file: 'zh.json' },
            { code: 'ae', file: 'ae.json' },
        ],
        lazy: true,
        defaultLocale: 'en',
        strategy: 'no_prefix',
        langDir: 'locales/',
    },

    vite: {
        optimizeDeps: { include: ['quill'] },
    },

    router: {
        options: { linkExactActiveClass: 'active' },
    },

    compatibilityDate: '2024-09-21',

    devServer: {
        port: 3333, // Force Nuxt to run on port 3333 for consistency with OAuth configuration
    },

    runtimeConfig: {
        // Private keys (only available on server-side)

        // Public keys (exposed to client-side)
        public: {
            apiBase: process.env.NUXT_PUBLIC_API_BASE || 'https://localhost:5000', // API Gateway port
            appBase: process.env.NUXT_PUBLIC_APP_BASE || 'http://localhost:3333', // Frontend port
            identityApiBase: process.env.NUXT_PUBLIC_IDENTITY_API_BASE || 'https://localhost:5228',
            googleClientId: process.env.NUXT_PUBLIC_GOOGLE_CLIENT_ID,
            facebookAppId: process.env.NUXT_PUBLIC_FACEBOOK_APP_ID,
        },
    },
});
