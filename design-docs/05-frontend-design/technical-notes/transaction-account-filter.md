# Transaction Account Filter Implementation

## 📋 Overview

Tài liệu này mô tả cách implementation của account filter trong Transaction Management page, đảm bảo rằng khi user chọn "Tất cả tài khoản" thì API sẽ không gửi accountId filter.

## 🔧 Technical Implementation

### Frontend Logic Flow

```typescript
// 1. User selects account in dropdown
selectedAccountId.value = "" // "Tất cả tài khoản"
// OR
selectedAccountId.value = "account-uuid" // Specific account

// 2. handleAccountChange() được trigger
const handleAccountChange = () => {
  // Convert empty string to undefined để clear filter
  const accountIdFilter = selectedAccountId.value === '' ? undefined : selectedAccountId.value
  
  // Log để debug
  console.log('Account filter changed:', { 
    selectedValue: selectedAccountId.value, 
    filterValue: accountIdFilter,
    willFilterByAccount: accountIdFilter !== undefined 
  })
  
  // Call API với filter
  getTransactions({ accountId: accountIdFilter })
}

// 3. getTransactions() xử lý filter
const getTransactions = async (filter) => {
  // Merge với current filter
  const mergedFilter = { ...currentFilter.value, ...filter }
  
  // Clear undefined/null/empty values
  Object.keys(filter).forEach(key => {
    const value = filter[key]
    if (value === '' || value === null || value === undefined) {
      delete mergedFilter[key] // Remove from filter object
    }
  })
  
  // Build filter details array
  const filterDetails = []
  
  // CHỈ add accountId filter khi có giá trị valid
  if (mergedFilter.accountId && mergedFilter.accountId.trim() !== '') {
    filterDetails.push({
      attributeName: 'accountId',
      filterType: FilterType.Equal,
      value: mergedFilter.accountId
    })
    console.log('Adding account filter:', mergedFilter.accountId)
  } else {
    console.log('No account filter - loading all accounts transactions')
  }
  
  // Build API request
  const filterRequest = {
    filter: filterDetails.length > 0 ? {
      logicalOperator: FilterLogicalOperator.And,
      details: filterDetails
    } : {} // Empty filter object = no filtering
  }
}
```

### API Request Examples

#### Case 1: "Tất cả tài khoản" được chọn
```json
{
  "langId": "",
  "searchValue": "",
  "filter": {}, // ← Empty filter = load all accounts
  "orders": [
    {
      "field": "transactionDate", 
      "direction": "Descending"
    }
  ],
  "pagination": {
    "pageIndex": 0,
    "pageSize": 20
  }
}
```

#### Case 2: Specific account được chọn
```json
{
  "langId": "",
  "searchValue": "",
  "filter": {
    "logicalOperator": "And",
    "details": [
      {
        "attributeName": "accountId",
        "filterType": "Equal", 
        "value": "550e8400-e29b-41d4-a716-446655440000"
      }
    ]
  },
  "orders": [
    {
      "field": "transactionDate",
      "direction": "Descending"
    }
  ],
  "pagination": {
    "pageIndex": 0,
    "pageSize": 20
  }
}
```

## 🎯 Key Features

### 1. **Smart Filter Clearing**
- Khi user chọn "Tất cả tài khoản" → `selectedAccountId.value = ""`
- Logic convert empty string thành `undefined` để remove từ filter object
- API request không chứa accountId filter → Backend load all accounts

### 2. **Debug Logging**
- Console logs hiển thị filter state changes
- Network tab shows exact API requests being sent
- Easy để verify logic hoạt động đúng

### 3. **UX Improvements**
- Loading spinner khi filter đang process
- Dropdown disabled during loading
- Clear visual feedback cho user

### 4. **Type Safety**
- TypeScript interfaces đảm bảo type consistency
- Proper handling của optional filter properties
- No runtime type errors

## 🔍 Testing Scenarios

### Manual Testing Steps

1. **Test "Tất cả tài khoản"**:
   - Mở Transaction page
   - Chọn "Tất cả tài khoản" trong dropdown
   - Check console logs: `willFilterByAccount: false`
   - Check Network tab: API request không có accountId filter
   - Verify: All transactions được load

2. **Test Specific Account**:
   - Chọn một account cụ thể trong dropdown  
   - Check console logs: `willFilterByAccount: true`
   - Check Network tab: API request có accountId filter
   - Verify: Chỉ transactions của account đó được load

3. **Test Account Switching**:
   - Switch giữa "Tất cả tài khoản" và specific accounts
   - Verify smooth transitions và correct filtering
   - Check loading states hoạt động

## 🐛 Common Issues & Solutions

### Issue 1: Filter không clear khi chọn "Tất cả tài khoản"
**Solution**: Đảm bảo logic convert empty string thành `undefined`:
```typescript
const accountIdFilter = selectedAccountId.value === '' ? undefined : selectedAccountId.value
```

### Issue 2: TypeScript errors với null/undefined
**Solution**: Use `undefined` thay vì `null` cho optional filter properties

### Issue 3: API vẫn gửi empty accountId
**Solution**: Check logic clear filter trong `getTransactions()`:
```typescript
if (value === '' || value === null || value === undefined) {
  delete mergedFilter[key]
}
```

## 📝 Code Locations

- **Frontend Filter Logic**: `src/fe/nuxt/pages/apps/transactions/index.vue` - `handleAccountChange()`
- **API Filter Processing**: `src/fe/nuxt/composables/useTransactions.ts` - `getTransactions()`
- **Backend Filter Processing**: `src/be/Shared.EntityFramework/EntityFrameworkUtilities/ExpressionBuilder.cs`

## ✅ Verification Checklist

- [ ] "Tất cả tài khoản" không gửi accountId trong API filter
- [ ] Specific account selection gửi đúng accountId
- [ ] Console logs hiển thị filter state correctly
- [ ] Loading states hoạt động smooth
- [ ] No TypeScript compilation errors
- [ ] Network requests match expected format
- [ ] Backend correctly processes filter requests

---

*Last updated: June 24, 2025*
*Implementation: Transaction Account Filter Logic*
