<template>
  <div v-if="modelValue" class="fixed inset-0 z-[999] flex items-center justify-center overflow-y-auto bg-black bg-opacity-50">
    <div class="relative mx-4 w-full max-w-2xl bg-white rounded-lg shadow-lg dark:bg-[#0e1726]">
      <!-- Header -->
      <div class="flex items-center justify-between border-b border-gray-200 px-6 py-4 dark:border-gray-700">
        <h3 class="text-lg font-semibold text-gray-900 dark:text-white">
          {{ isEdit ? 'Edit Account' : 'Create New Account' }}
        </h3>
        <button
          type="button"
          class="text-gray-400 hover:text-gray-600 dark:hover:text-gray-300"
          @click="closeModal"
        >
          <icon-x class="h-6 w-6" />
        </button>
      </div>

      <!-- Form -->
      <form @submit.prevent="handleSubmit" class="p-6">
        <div class="grid grid-cols-1 gap-6 sm:grid-cols-2">
          <!-- Account Name -->
          <div class="sm:col-span-2">
            <label for="name" class="form-label">Account Name *</label>
            <input
              id="name"
              v-model="form.name"
              type="text"
              class="form-input"
              :class="{ 'border-red-500': errors.name }"
              placeholder="Enter account name"
              required
            />
            <div v-if="errors.name" class="mt-1 text-sm text-red-500">
              {{ errors.name }}
            </div>
          </div>

          <!-- Account Type -->
          <div>
            <label for="type" class="form-label">Account Type *</label>
            <select
              id="type"
              v-model="form.type"
              class="form-select"
              :class="{ 'border-red-500': errors.type }"
              required
            >
              <option value="">Select account type</option>
              <option value="Bank">Bank Account</option>
              <option value="Wallet">Digital Wallet</option>
              <option value="CreditCard">Credit Card</option>
              <option value="DebitCard">Debit Card</option>
              <option value="Cash">Cash</option>
            </select>
            <div v-if="errors.type" class="mt-1 text-sm text-red-500">
              {{ errors.type }}
            </div>
          </div>

          <!-- Currency -->
          <div>
            <label for="currency" class="form-label">Currency *</label>
            <select
              id="currency"
              v-model="form.currency"
              class="form-select"
              :class="{ 'border-red-500': errors.currency }"
              required
            >
              <option value="">Select currency</option>
              <option value="VND">VND - Vietnamese Dong</option>
              <option value="USD">USD - US Dollar</option>
              <option value="EUR">EUR - Euro</option>
              <option value="JPY">JPY - Japanese Yen</option>
              <option value="GBP">GBP - British Pound</option>
            </select>
            <div v-if="errors.currency" class="mt-1 text-sm text-red-500">
              {{ errors.currency }}
            </div>
          </div>

          <!-- Card Number -->
          <div v-if="showCardNumber" class="sm:col-span-2">
            <label for="cardNumber" class="form-label">Card Number</label>
            <input
              id="cardNumber"
              v-model="form.cardNumber"
              type="text"
              class="form-input"
              :class="{ 'border-red-500': errors.cardNumber }"
              placeholder="Enter card number"
              maxlength="19"
              @input="formatCardNumber"
            />
            <div v-if="errors.cardNumber" class="mt-1 text-sm text-red-500">
              {{ errors.cardNumber }}
            </div>
            <div class="mt-1 text-sm text-gray-500">
              Card number will be masked for security
            </div>
          </div>

          <!-- Initial Balance -->
          <div v-if="!isEdit">
            <label for="initialBalance" class="form-label">Initial Balance *</label>
            <div class="relative">
              <input
                id="initialBalance"
                v-model.number="form.initialBalance"
                type="number"
                step="0.01"
                class="form-input pl-8"
                :class="{ 'border-red-500': errors.initialBalance }"
                placeholder="0.00"
                required
              />
              <span class="absolute left-3 top-1/2 -translate-y-1/2 text-gray-500">
                {{ getCurrencySymbol(form.currency) }}
              </span>
            </div>
            <div v-if="errors.initialBalance" class="mt-1 text-sm text-red-500">
              {{ errors.initialBalance }}
            </div>
          </div>

          <!-- Available Limit -->
          <div v-if="showAvailableLimit">
            <label for="availableLimit" class="form-label">Available Limit</label>
            <div class="relative">
              <input
                id="availableLimit"
                v-model.number="form.availableLimit"
                type="number"
                step="0.01"
                class="form-input pl-8"
                :class="{ 'border-red-500': errors.availableLimit }"
                placeholder="0.00"
              />
              <span class="absolute left-3 top-1/2 -translate-y-1/2 text-gray-500">
                {{ getCurrencySymbol(form.currency) }}
              </span>
            </div>
            <div v-if="errors.availableLimit" class="mt-1 text-sm text-red-500">
              {{ errors.availableLimit }}
            </div>
            <div class="mt-1 text-sm text-gray-500">
              Credit limit for credit cards or spending limit for other accounts
            </div>
          </div>
        </div>

        <!-- Form Actions -->
        <div class="mt-8 flex justify-end gap-3">
          <button
            type="button"
            class="btn btn-outline-secondary"
            @click="closeModal"
          >
            Cancel
          </button>
          <button
            type="submit"
            class="btn btn-primary"
            :disabled="isSubmitting"
          >
            <span v-if="isSubmitting" class="mr-2">
              <div class="h-4 w-4 animate-spin rounded-full border-2 border-white border-t-transparent"></div>
            </span>
            {{ isEdit ? 'Update Account' : 'Create Account' }}
          </button>
        </div>
      </form>
    </div>
  </div>
</template>

<script setup lang="ts">
import { useAccounts } from '@/composables/useAccounts'
import type { Account, AccountCreateRequest, AccountUpdateRequest, AccountType } from '~/types'
import Swal from 'sweetalert2'

const { createAccount, updateAccount, getCurrencySymbol, validateAccountForm } = useAccounts()

type AccountForm = {
  name: string
  type: AccountType | ''
  cardNumber: string
  currency: string
  initialBalance: number
  availableLimit?: number
}

type FormErrors = {
  [key: string]: string
}

interface Props {
  modelValue: boolean
  account?: Account | null
  isEdit?: boolean
}

interface Emits {
  (e: 'update:modelValue', value: boolean): void
  (e: 'saved'): void
}

const props = withDefaults(defineProps<Props>(), {
  account: null,
  isEdit: false
})

const emit = defineEmits<Emits>()

// Reactive data
const isSubmitting = ref(false)
const errors = ref<FormErrors>({})

const form = ref<AccountForm>({
  name: '',
  type: '',
  cardNumber: '',
  currency: '',
  initialBalance: 0,
  availableLimit: undefined
})

// Computed
const showCardNumber = computed(() => {
  return ['CreditCard', 'DebitCard'].includes(form.value.type as string)
})

const showAvailableLimit = computed(() => {
  return ['CreditCard', 'Wallet'].includes(form.value.type as string)
})

// Methods
const closeModal = () => {
  emit('update:modelValue', false)
  resetForm()
}

const resetForm = () => {
  form.value = {
    name: '',
    type: '',
    cardNumber: '',
    currency: '',
    initialBalance: 0,
    availableLimit: undefined
  }
  errors.value = {}
}

const formatCardNumber = (event: Event) => {
  const input = event.target as HTMLInputElement
  const value = input.value.replace(/\s+/g, '').replace(/[^0-9]/gi, '')
  const formattedValue = value.match(/.{1,4}/g)?.join(' ') || value
  form.value.cardNumber = formattedValue
}

const validateForm = (): boolean => {
  const formData = {
    name: form.value.name,
    type: form.value.type,
    currency: form.value.currency,
    cardNumber: form.value.cardNumber,
    initialBalance: form.value.initialBalance,
    availableLimit: form.value.availableLimit
  }
  
  errors.value = validateAccountForm(formData)
  return Object.keys(errors.value).length === 0
}

const handleSubmit = async () => {
  if (!validateForm()) {
    return
  }

  try {
    isSubmitting.value = true

    const baseData = {
      name: form.value.name.trim(),
      type: form.value.type as AccountType,
      currency: form.value.currency,
      cardNumber: form.value.cardNumber.replace(/\s/g, '') || undefined,
      availableLimit: form.value.availableLimit || undefined
    }

    if (props.isEdit && props.account?.id) {
      // Update existing account
      const updateData: AccountUpdateRequest = {
        id: props.account.id,
        ...baseData
      }
      await updateAccount(props.account.id, updateData)
    } else {
      // Create new account
      const createData: AccountCreateRequest = {
        ...baseData,
        initialBalance: form.value.initialBalance
        // userId will be added later when authentication is implemented
      }
      await createAccount(createData)
    }

    emit('saved')
  } catch (error: any) {
    console.error('Error saving account:', error)
    
    let errorMessage = 'Failed to save account'
    if (error.data?.message) {
      errorMessage = error.data.message
    } else if (error.message) {
      errorMessage = error.message
    }

    Swal.fire({
      icon: 'error',
      title: 'Error',
      text: errorMessage
    })
  } finally {
    isSubmitting.value = false
  }
}

// getCurrencySymbol is now imported from useAccounts composable

// Initialize form when account prop changes
watch(
  () => props.account,
  (newAccount) => {
    if (newAccount && props.isEdit) {
      form.value = {
        name: newAccount.name || '',
        type: newAccount.type || '',
        cardNumber: newAccount.cardNumber || '',
        currency: newAccount.currency || '',
        initialBalance: newAccount.initialBalance || 0,
        availableLimit: newAccount.availableLimit
      }
    } else {
      resetForm()
    }
  },
  { immediate: true }
)

// Reset form when modal closes
watch(
  () => props.modelValue,
  (isOpen) => {
    if (!isOpen) {
      resetForm()
    }
  }
)
</script> 