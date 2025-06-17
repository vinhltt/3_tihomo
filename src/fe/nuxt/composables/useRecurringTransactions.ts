import { ref, reactive } from 'vue'
import type { 
    RecurringTransactionTemplateViewModel, 
    RecurringTransactionTemplateCreateRequest,
    RecurringTransactionTemplateUpdateRequest,
    FilterBodyRequest,
    IBasePaging 
} from '~/types'

export const useRecurringTransactions = () => {
    const { $api } = useNuxtApp()
    
    // Reactive state
    const templates = ref<RecurringTransactionTemplateViewModel[]>([])
    const loading = ref(false)
    const pagination = reactive({
        pageIndex: 1,
        pageSize: 10,
        totalRow: 0,
        pageCount: 0
    })

    // Build filter request
    const buildFilterRequest = (filters: any = {}): FilterBodyRequest => {
        const filterDetails = []
        
        if (filters.accountId) {
            filterDetails.push({
                attributeName: 'AccountId',
                value: filters.accountId,
                filterType: 0 // Equal
            })
        }
        
        if (filters.isActive !== undefined) {
            filterDetails.push({
                attributeName: 'IsActive',
                value: filters.isActive.toString(),
                filterType: 0 // Equal
            })
        }
        
        if (filters.frequency !== undefined) {
            filterDetails.push({
                attributeName: 'Frequency',
                value: filters.frequency.toString(),
                filterType: 0 // Equal
            })
        }

        return {
            langId: '',
            searchValue: '',
            filter: {
                logicalOperator: 0, // And
                details: filterDetails
            },
            orders: [
                {
                    field: 'CreatedAt',
                    direction: 1 // Descending
                }
            ],
            pagination: {
                pageIndex: filters.pageIndex || pagination.pageIndex,
                pageSize: filters.pageSize || pagination.pageSize,
                totalRow: 0,
                pageCount: 0
            }
        }
    }

    // Get templates with pagination and filtering
    const getTemplates = async (filters: any = {}) => {
        try {
            loading.value = true
            const request = buildFilterRequest(filters)
            
            const response = await $api.post<IBasePaging<RecurringTransactionTemplateViewModel>>(
                '/api/RecurringTransactionTemplate/filter',
                request
            )
            
            if (response.data) {
                templates.value = response.data.data || []
                pagination.pageIndex = response.data.pageIndex
                pagination.pageSize = response.data.pageSize
                pagination.totalRow = response.data.totalRow
                pagination.pageCount = response.data.pageCount
            }
        } catch (error) {
            console.error('Error fetching recurring transaction templates:', error)
            templates.value = []
        } finally {
            loading.value = false
        }
    }

    // Get active templates for a user
    const getActiveTemplates = async (userId: string) => {
        try {
            const response = await $api.get<RecurringTransactionTemplateViewModel[]>(
                `/api/RecurringTransactionTemplate/active/${userId}`
            )
            return response.data || []
        } catch (error) {
            console.error('Error fetching active templates:', error)
            return []
        }
    }

    // Get templates by account
    const getTemplatesByAccount = async (accountId: string) => {
        try {
            const response = await $api.get<RecurringTransactionTemplateViewModel[]>(
                `/api/RecurringTransactionTemplate/account/${accountId}`
            )
            return response.data || []
        } catch (error) {
            console.error('Error fetching templates by account:', error)
            return []
        }
    }

    // Create template
    const createTemplate = async (request: RecurringTransactionTemplateCreateRequest) => {
        try {
            const formData = new FormData()
            Object.entries(request).forEach(([key, value]) => {
                if (value !== null && value !== undefined) {
                    formData.append(key, value.toString())
                }
            })

            const response = await $api.postForm<RecurringTransactionTemplateViewModel>(
                '/api/RecurringTransactionTemplate',
                formData
            )
            return response.data
        } catch (error) {
            console.error('Error creating recurring transaction template:', error)
            throw error
        }
    }

    // Update template
    const updateTemplate = async (id: string, request: RecurringTransactionTemplateUpdateRequest) => {
        try {
            const formData = new FormData()
            Object.entries(request).forEach(([key, value]) => {
                if (value !== null && value !== undefined) {
                    formData.append(key, value.toString())
                }
            })

            const response = await $api.putForm<RecurringTransactionTemplateViewModel>(
                `/api/RecurringTransactionTemplate/${id}`,
                formData
            )
            return response.data
        } catch (error) {
            console.error('Error updating recurring transaction template:', error)
            throw error
        }
    }

    // Delete template
    const deleteTemplate = async (id: string) => {
        try {
            await $api.delete(`/api/RecurringTransactionTemplate/${id}`)
            return true
        } catch (error) {
            console.error('Error deleting recurring transaction template:', error)
            return false
        }
    }

    // Toggle active status
    const toggleActiveStatus = async (templateId: string, isActive: boolean) => {
        try {
            const response = await $api.patch(
                `/api/RecurringTransactionTemplate/${templateId}/toggle-active`,
                isActive
            )
            return response.status === 200
        } catch (error) {
            console.error('Error toggling template status:', error)
            return false
        }
    }

    // Calculate next execution date
    const calculateNextExecutionDate = async (templateId: string) => {
        try {
            const response = await $api.get<{ nextExecutionDate: string }>(
                `/api/RecurringTransactionTemplate/${templateId}/next-execution-date`
            )
            return response.data?.nextExecutionDate
        } catch (error) {
            console.error('Error calculating next execution date:', error)
            return null
        }
    }

    // Generate expected transactions for a template
    const generateExpectedTransactions = async (templateId: string, daysInAdvance: number = 30) => {
        try {
            const response = await $api.post(
                `/api/RecurringTransactionTemplate/${templateId}/generate-expected-transactions?daysInAdvance=${daysInAdvance}`
            )
            return response.status === 200
        } catch (error) {
            console.error('Error generating expected transactions:', error)
            return false
        }
    }

    // Generate expected transactions for all active templates
    const generateAllExpectedTransactions = async () => {
        try {
            const response = await $api.post(
                '/api/RecurringTransactionTemplate/generate-all-expected-transactions'
            )
            return response.status === 200
        } catch (error) {
            console.error('Error generating all expected transactions:', error)
            return false
        }
    }

    return {
        // State
        templates,
        loading,
        pagination,
        
        // Methods
        getTemplates,
        getActiveTemplates,
        getTemplatesByAccount,
        createTemplate,
        updateTemplate,
        deleteTemplate,
        toggleActiveStatus,
        calculateNextExecutionDate,
        generateExpectedTransactions,
        generateAllExpectedTransactions
    }
} 