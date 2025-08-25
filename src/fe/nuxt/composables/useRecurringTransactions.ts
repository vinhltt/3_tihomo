import type {
    RecurringTransactionTemplateViewModel,
    RecurringTransactionTemplateCreateRequest,
    RecurringTransactionTemplateUpdateRequest,
    RecurringTransactionCalendarEvent,
    IBasePaging
} from '~/types/recurring-transaction'
import type { FilterBodyRequest } from '~/types/api'
import { useApi } from './useApi'

export const useRecurringTransactions = () => {
    const { get, post, postForm, putForm, delete: del, apiCall } = useApi()

    /**
     * Get paginated templates (filter body request)
     */
    const getTemplates = async (request: FilterBodyRequest): Promise<IBasePaging<RecurringTransactionTemplateViewModel>> => {
        return post<IBasePaging<RecurringTransactionTemplateViewModel>>('/api/core-finance/RecurringTransactionTemplate/filter', request)
    }

    const getActiveTemplates = async (userId: string): Promise<RecurringTransactionTemplateViewModel[]> => {
        return get<RecurringTransactionTemplateViewModel[]>(`/api/core-finance/RecurringTransactionTemplate/active/${userId}`)
    }

    const getTemplatesByAccount = async (accountId: string): Promise<RecurringTransactionTemplateViewModel[]> => {
        return get<RecurringTransactionTemplateViewModel[]>(`/api/core-finance/RecurringTransactionTemplate/account/${accountId}`)
    }

    const createTemplate = async (request: RecurringTransactionTemplateCreateRequest): Promise<RecurringTransactionTemplateViewModel> => {
        return postForm<RecurringTransactionTemplateViewModel>('/api/core-finance/RecurringTransactionTemplate', request)
    }

    const updateTemplate = async (id: string, request: RecurringTransactionTemplateUpdateRequest): Promise<RecurringTransactionTemplateViewModel> => {
        return putForm<RecurringTransactionTemplateViewModel>(`/api/core-finance/RecurringTransactionTemplate/${id}`, request)
    }

    const deleteTemplate = async (id: string): Promise<void> => {
        return del<void>(`/api/core-finance/RecurringTransactionTemplate/${id}`)
    }

    const toggleActiveStatus = async (templateId: string, isActive: boolean): Promise<void> => {
        // apiCall used because useApi doesn't expose PATCH directly
        return apiCall(`/api/core-finance/RecurringTransactionTemplate/${templateId}/toggle-active`, {
            method: ('PATCH' as any),
            body: isActive
        })
    }

    const calculateNextExecutionDate = async (templateId: string): Promise<string | null> => {
        const resp = await get<{ nextExecutionDate: string }>(`/api/core-finance/RecurringTransactionTemplate/${templateId}/next-execution-date`)
        return resp?.nextExecutionDate || null
    }

    const generateExpectedTransactions = async (templateId: string, daysInAdvance = 30): Promise<void> => {
        return post<void>(`/api/core-finance/RecurringTransactionTemplate/${templateId}/generate-expected-transactions?daysInAdvance=${daysInAdvance}`)
    }

    const generateAllExpectedTransactions = async (): Promise<void> => {
        return post<void>('/api/core-finance/RecurringTransactionTemplate/generate-all-expected-transactions')
    }

    const getCalendarEvents = async (month: string): Promise<RecurringTransactionCalendarEvent[]> => {
        return get<RecurringTransactionCalendarEvent[]>(`/api/core-finance/RecurringTransactionTemplate/calendar?month=${month}`)
    }

    return {
        getTemplates,
        getActiveTemplates,
        getTemplatesByAccount,
        createTemplate,
        updateTemplate,
        deleteTemplate,
        toggleActiveStatus,
        calculateNextExecutionDate,
        generateExpectedTransactions,
        generateAllExpectedTransactions,
        getCalendarEvents
    }
}