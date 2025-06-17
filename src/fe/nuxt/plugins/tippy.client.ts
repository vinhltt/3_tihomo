import { TippyPlugin } from 'tippy.vue';
export default defineNuxtPlugin((nuxtApp) => {
    nuxtApp.vueApp.use(TippyPlugin, {
        directive: 'tippy',
        component: 'tippy',
    });
});
