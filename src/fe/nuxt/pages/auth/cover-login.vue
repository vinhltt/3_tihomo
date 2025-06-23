<template>
    <div>
        <div class="absolute inset-0">
            <img src="/assets/images/auth/bg-gradient.png" alt="image" class="h-full w-full object-cover" />
        </div>
        <div
            class="relative flex min-h-screen items-center justify-center bg-[url(/assets/images/auth/map.png)] bg-cover bg-center bg-no-repeat px-6 py-10 dark:bg-[#060818] sm:px-16"
        >
            <img src="/assets/images/auth/coming-soon-object1.png" alt="image" class="absolute left-0 top-1/2 h-full max-h-[893px] -translate-y-1/2" />
            <img src="/assets/images/auth/coming-soon-object2.png" alt="image" class="absolute left-24 top-0 h-40 md:left-[30%]" />
            <img src="/assets/images/auth/coming-soon-object3.png" alt="image" class="absolute right-0 top-0 h-[300px]" />
            <img src="/assets/images/auth/polygon-object.svg" alt="image" class="absolute bottom-0 end-[28%]" />
            <div
                class="relative flex w-full max-w-[1502px] flex-col justify-between overflow-hidden rounded-md bg-white/60 backdrop-blur-lg dark:bg-black/50 lg:min-h-[758px] lg:flex-row lg:gap-10 xl:gap-0"
            >
                <div
                    class="relative hidden w-full items-center justify-center bg-[linear-gradient(225deg,rgba(239,18,98,1)_0%,rgba(67,97,238,1)_100%)] p-5 lg:inline-flex lg:max-w-[835px] xl:-ms-28 ltr:xl:skew-x-[14deg] rtl:xl:skew-x-[-14deg]"
                >
                    <div
                        class="absolute inset-y-0 w-8 from-primary/10 via-transparent to-transparent ltr:-right-10 ltr:bg-gradient-to-r rtl:-left-10 rtl:bg-gradient-to-l xl:w-16 ltr:xl:-right-20 rtl:xl:-left-20"
                    ></div>
                    <div class="ltr:xl:-skew-x-[14deg] rtl:xl:skew-x-[14deg]">
                        <NuxtLink to="/" class="ms-10 block w-48 lg:w-72">
                            <img src="/assets/images/auth/logo-white.svg" alt="Logo" class="w-full" />
                        </NuxtLink>
                        <div class="mt-24 hidden w-full max-w-[430px] lg:block">
                            <img src="/assets/images/auth/login.svg" alt="Cover Image" class="w-full" />
                        </div>
                    </div>
                </div>
                <div class="relative flex w-full flex-col items-center justify-center gap-6 px-4 pb-16 pt-6 sm:px-6 lg:max-w-[667px]">
                    <div class="flex w-full max-w-[440px] items-center gap-2 lg:absolute lg:end-6 lg:top-6 lg:max-w-full">
                        <NuxtLink to="/" class="block w-8 lg:hidden">
                            <img src="/assets/images/logo.svg" alt="Logo" class="mx-auto w-10" />
                        </NuxtLink>
                        <div class="dropdown ms-auto w-max">
                            <client-only>
                                <Popper :placement="store.rtlClass === 'rtl' ? 'bottom-start' : 'bottom-end'" offsetDistance="8">
                                    <button
                                        type="button"
                                        class="flex items-center gap-2.5 rounded-lg border border-white-dark/30 bg-white px-2 py-1.5 text-white-dark hover:border-primary hover:text-primary dark:bg-black"
                                    >
                                        <div>
                                            <img :src="currentFlag" alt="image" class="h-5 w-5 rounded-full object-cover" />
                                        </div>
                                        <div class="text-base font-bold uppercase">{{ store.locale }}</div>
                                        <span class="shrink-0">
                                            <icon-caret-down />
                                        </span>
                                    </button>
                                    <template #content="{ close }">
                                        <ul
                                            class="grid w-[280px] grid-cols-2 gap-2 !px-2 font-semibold text-dark dark:text-white-dark dark:text-white-light/90"
                                        >
                                            <template v-for="item in store.languageList" :key="item.code">
                                                <li>
                                                    <button
                                                        type="button"
                                                        class="w-full hover:text-primary"
                                                        :class="{ 'bg-primary/10 text-primary': store.locale === item.code }"
                                                        @click="changeLanguage(item), close()"
                                                    >
                                                        <img
                                                            class="h-5 w-5 rounded-full object-cover"
                                                            :src="`/assets/images/flags/${item.code.toUpperCase()}.svg`"
                                                            alt=""
                                                        />
                                                        <span class="ltr:ml-3 rtl:mr-3">{{ item.name }}</span>
                                                    </button>
                                                </li>
                                            </template>
                                        </ul>
                                    </template>
                                </Popper>
                            </client-only>
                        </div>
                    </div>
                    <div class="w-full max-w-[440px] lg:mt-16">
                        <div class="mb-10">
                            <h1 class="text-3xl font-extrabold uppercase !leading-snug text-primary md:text-4xl">Sign in</h1>
                            <p class="text-base font-bold leading-normal text-white-dark">Enter your email and password to login</p>
                        </div>                        <form class="space-y-5 dark:text-white" @submit.prevent="handleLogin">
                            <div>
                                <label for="Email">Email</label>
                                <div class="relative text-white-dark">
                                    <input
                                        id="Email"
                                        v-model="loginForm.email"
                                        type="email"
                                        placeholder="Enter Email"
                                        class="form-input ps-10 placeholder:text-white-dark"
                                        :class="{ 'border-red-500': emailError }"
                                        required
                                    />
                                    <span class="absolute start-4 top-1/2 -translate-y-1/2">
                                        <icon-mail :fill="true" />
                                    </span>
                                </div>
                                <p v-if="emailError" class="mt-1 text-sm text-red-500">{{ emailError }}</p>
                            </div>
                            <div>
                                <label for="Password">Password</label>
                                <div class="relative text-white-dark">
                                    <input
                                        id="Password"
                                        v-model="loginForm.password"
                                        type="password"
                                        placeholder="Enter Password"
                                        class="form-input ps-10 placeholder:text-white-dark"
                                        :class="{ 'border-red-500': passwordError }"
                                        required
                                    />
                                    <span class="absolute start-4 top-1/2 -translate-y-1/2">
                                        <icon-lock-dots :fill="true" />
                                    </span>
                                </div>
                                <p v-if="passwordError" class="mt-1 text-sm text-red-500">{{ passwordError }}</p>
                            </div>
                            <div>
                                <label class="flex cursor-pointer items-center">
                                    <input
                                        v-model="rememberMe"
                                        type="checkbox"
                                        class="form-checkbox bg-white dark:bg-black"
                                    />
                                    <span class="text-white-dark">Remember me</span>
                                </label>
                            </div>

                            <!-- Error Message -->
                            <div v-if="error" class="rounded-md bg-red-50 p-4 dark:bg-red-900/10">
                                <div class="flex">
                                    <div class="ml-3">
                                        <h3 class="text-sm font-medium text-red-800 dark:text-red-200">
                                            Login Failed
                                        </h3>
                                        <div class="mt-2 text-sm text-red-700 dark:text-red-300">
                                            <p>{{ error }}</p>
                                        </div>
                                    </div>
                                </div>
                            </div>

                            <button
                                type="submit"
                                class="btn btn-gradient !mt-6 w-full border-0 uppercase shadow-[0_10px_20px_-10px_rgba(67,97,238,0.44)]"
                                :disabled="isLoading"
                            >
                                <span v-if="isLoading" class="mr-2">
                                    <svg class="animate-spin h-4 w-4" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                                        <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
                                        <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                                    </svg>
                                </span>
                                {{ isLoading ? 'Signing in...' : 'Sign in' }}
                            </button>
                        </form>

                        <div class="relative my-7 text-center md:mb-9">
                            <span class="absolute inset-x-0 top-1/2 h-px w-full -translate-y-1/2 bg-white-light dark:bg-white-dark"></span>
                            <span class="relative bg-white px-2 font-bold uppercase text-white-dark dark:bg-dark dark:text-white-light">or</span>
                        </div>                        <div class="mb-10 md:mb-[60px]">
                            <div class="space-y-3">
                                <!-- Google Login Button -->
                                <button
                                    type="button"
                                    @click="handleGoogleLogin"
                                    :disabled="socialLoading"
                                    class="btn w-full border border-gray-300 bg-white text-gray-700 hover:bg-gray-50 disabled:opacity-50 dark:border-gray-600 dark:bg-gray-800 dark:text-gray-300 dark:hover:bg-gray-700"
                                >
                                    <span v-if="socialLoading && currentSocialProvider === 'Google'" class="mr-2">
                                        <svg class="animate-spin h-4 w-4" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                                            <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
                                            <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                                        </svg>
                                    </span>
                                    <icon-google class="mr-2" />
                                    {{ socialLoading && currentSocialProvider === 'Google' ? 'Signing in with Google...' : 'Continue with Google' }}
                                </button>

                                <!-- Facebook Login Button -->
                                <button
                                    type="button"
                                    @click="handleFacebookLogin"
                                    :disabled="socialLoading"
                                    class="btn w-full border border-gray-300 bg-white text-gray-700 hover:bg-gray-50 disabled:opacity-50 dark:border-gray-600 dark:bg-gray-800 dark:text-gray-300 dark:hover:bg-gray-700"
                                >
                                    <span v-if="socialLoading && currentSocialProvider === 'Facebook'" class="mr-2">
                                        <svg class="animate-spin h-4 w-4" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                                            <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
                                            <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                                        </svg>
                                    </span>
                                    <icon-facebook-circle class="mr-2" />
                                    {{ socialLoading && currentSocialProvider === 'Facebook' ? 'Signing in with Facebook...' : 'Continue with Facebook' }}
                                </button>
                            </div>

                            <!-- Social Login Error -->
                            <div v-if="socialError" class="mt-4 rounded-md bg-red-50 p-4 dark:bg-red-900/10">
                                <div class="flex">
                                    <div class="ml-3">
                                        <h3 class="text-sm font-medium text-red-800 dark:text-red-200">
                                            Social Login Failed
                                        </h3>
                                        <div class="mt-2 text-sm text-red-700 dark:text-red-300">
                                            <p>{{ socialError }}</p>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                        <div class="text-center dark:text-white">
                            Don't have an account ?
                            <NuxtLink to="/auth/cover-signup" class="uppercase text-primary underline transition hover:text-black dark:hover:text-white">
                                SIGN UP
                            </NuxtLink>
                        </div>
                    </div>
                    <p class="absolute bottom-6 w-full text-center dark:text-white">© {{ new Date().getFullYear() }}.VRISTO All Rights Reserved.</p>
                </div>
            </div>
        </div>
    </div>
</template>
<script lang="ts" setup>
import { computed, reactive, ref, watch, onMounted } from 'vue'
import appSetting from '@/app-setting'
import { useAppStore } from '@/stores/index'
import { useAuth } from '@/composables/useAuth'
import { useSocialAuth } from '@/composables/useSocialAuth'
import { useAuthStore } from '@/stores/auth'
import type { SocialProvider } from '@/types/auth'

useHead({ title: 'Login with Google - Cover' })

definePageMeta({
    layout: 'auth-layout',
    title: 'Login with Google - Cover',
    auth: false // Disable auth middleware for this page
})

const store = useAppStore()
const { login, isLoading, error, clearError } = useAuth()
const { loginWith, isLoading: socialLoading, error: socialError, initializeProviders } = useSocialAuth()
const authStore = useAuthStore()
const router = useRouter()
const route = useRoute()
const { setLocale } = useI18n()

// Form state
const loginForm = reactive({
    email: '',
    password: '',
})

const rememberMe = ref(false)
const emailError = ref('')
const passwordError = ref('')
const currentSocialProvider = ref<SocialProvider | null>(null)

// Form validation
const validateForm = (): boolean => {
    emailError.value = ''
    passwordError.value = ''

    let isValid = true

    if (!loginForm.email) {
        emailError.value = 'Email is required'
        isValid = false
    } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(loginForm.email)) {
        emailError.value = 'Please enter a valid email address'
        isValid = false
    }

    if (!loginForm.password) {
        passwordError.value = 'Password is required'
        isValid = false
    } else if (loginForm.password.length < 6) {
        passwordError.value = 'Password must be at least 6 characters'
        isValid = false
    }

    return isValid
}

// Handle form submission
const handleLogin = async (): Promise<void> => {
    clearError()

    if (!validateForm()) {
        return
    }

    try {
        const success = await login({
            email: loginForm.email,
            password: loginForm.password,
        })

        if (success) {
            console.log('Login successful')
            
            // Redirect to returnUrl or home page
            const returnUrl = route.query.returnUrl as string
            const redirectTo = returnUrl && typeof returnUrl === 'string' ? returnUrl : '/'
            await router.push(redirectTo)
        }
    } catch (err) {
        console.error('Login error:', err)
    }
}

// Handle Google login
const handleGoogleLogin = async (): Promise<void> => {
    try {
        currentSocialProvider.value = 'Google'
        
        // Step 1: Authenticate with Google
        const socialResponse = await loginWith('Google')
        console.log('Google login successful')
        
        // Step 2: Store authentication data in auth store
        const success = await authStore.socialLogin(socialResponse)
        
        if (success) {
            console.log('✅ Authentication stored, redirecting...')
            
            // Step 3: Redirect to returnUrl or home page
            const returnUrl = route.query.returnUrl as string
            const redirectTo = returnUrl && typeof returnUrl === 'string' ? returnUrl : '/'
            await router.push(redirectTo)
        } else {
            throw new Error('Failed to store authentication data')
        }
    } catch (err) {
        console.error('Google login error:', err)
    } finally {
        currentSocialProvider.value = null
    }
}

// Handle Facebook login
const handleFacebookLogin = async (): Promise<void> => {
    try {
        currentSocialProvider.value = 'Facebook'
        await loginWith('Facebook')
        console.log('Facebook login successful')
        // Navigation will be handled by the auth store
    } catch (err) {
        console.error('Facebook login error:', err)
    } finally {
        currentSocialProvider.value = null
    }
}

// Multi language support
const changeLanguage = (item: any) => {
    appSetting.toggleLanguage(item, setLocale)
}

const currentFlag = computed(() => {
    return `/assets/images/flags/${store.locale?.toUpperCase()}.svg`
})

// Initialize social providers on mount
onMounted(async () => {
    try {
        await initializeProviders()
    } catch (error) {
        console.warn('Failed to initialize social providers:', error)
    }
})

// Clear errors when user starts typing
watch(() => loginForm.email, () => {
    if (emailError.value) emailError.value = ''
    if (error.value) clearError()
})

watch(() => loginForm.password, () => {
    if (passwordError.value) passwordError.value = ''
    if (error.value) clearError()
})
</script>
