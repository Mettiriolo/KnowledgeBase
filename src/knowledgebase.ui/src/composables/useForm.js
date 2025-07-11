import { ref, reactive, computed, watch } from 'vue'
import { validateForm } from '@/utils/validation'
import { debounce } from '@/utils/performance'

export function useForm(initialData = {}, validationRules = {}, options = {}) {
  const {
    validateOnChange = true,
    validateOnBlur = true,
    debounceTime = 300,
    resetOnSubmit = false,
    transform = null
  } = options

  const formData = reactive({ ...initialData })
  const errors = ref({})
  const touched = ref({})
  const isSubmitting = ref(false)
  const isDirty = ref(false)

  const validate = () => {
    const result = validateForm(formData, validationRules)
    errors.value = result.errors
    return result.isValid
  }

  const validateField = (field) => {
    if (validationRules[field]) {
      const rules = Array.isArray(validationRules[field]) 
        ? validationRules[field] 
        : [validationRules[field]]
      
      for (const rule of rules) {
        const error = rule(formData[field])
        if (error) {
          errors.value[field] = error
          return false
        }
      }
      
      delete errors.value[field]
      return true
    }
    return true
  }

  const debouncedValidateField = debounce(validateField, debounceTime)

  const setValue = (field, value) => {
    formData[field] = value
    isDirty.value = true
    
    if (validateOnChange) {
      debouncedValidateField(field)
    }
  }

  const setValues = (data) => {
    Object.assign(formData, data)
    isDirty.value = true
    
    if (validateOnChange) {
      validate()
    }
  }

  const setTouched = (field, value = true) => {
    touched.value[field] = value
    
    if (validateOnBlur && value) {
      validateField(field)
    }
  }

  const reset = (data = initialData) => {
    Object.assign(formData, data)
    errors.value = {}
    touched.value = {}
    isDirty.value = false
    isSubmitting.value = false
  }

  const clearErrors = (field) => {
    if (field) {
      delete errors.value[field]
    } else {
      errors.value = {}
    }
  }

  const getFieldProps = (field) => {
    return {
      value: formData[field],
      error: errors.value[field],
      touched: touched.value[field],
      onChange: (value) => setValue(field, value),
      onBlur: () => setTouched(field, true),
      onFocus: () => setTouched(field, false)
    }
  }

  const submit = async (submitFn) => {
    if (isSubmitting.value) return

    isSubmitting.value = true
    
    try {
      const isValid = validate()
      if (!isValid) {
        throw new Error('表单验证失败')
      }

      const dataToSubmit = transform ? transform(formData) : formData
      const result = await submitFn(dataToSubmit)

      if (resetOnSubmit) {
        reset()
      }

      return result
    } catch (error) {
      throw error
    } finally {
      isSubmitting.value = false
    }
  }

  const isValid = computed(() => {
    return Object.keys(errors.value).length === 0
  })

  const hasErrors = computed(() => {
    return Object.keys(errors.value).length > 0
  })

  const canSubmit = computed(() => {
    return isValid.value && isDirty.value && !isSubmitting.value
  })

  // 监听表单数据变化
  watch(formData, () => {
    isDirty.value = true
  }, { deep: true })

  return {
    formData,
    errors,
    touched,
    isSubmitting,
    isDirty,
    isValid,
    hasErrors,
    canSubmit,
    validate,
    validateField,
    setValue,
    setValues,
    setTouched,
    reset,
    clearErrors,
    getFieldProps,
    submit
  }
}

export function useFormField(value, rules = [], options = {}) {
  const {
    validateOnChange = true,
    validateOnBlur = true,
    debounceTime = 300
  } = options

  const fieldValue = ref(value)
  const error = ref(null)
  const touched = ref(false)
  const isDirty = ref(false)

  const validate = () => {
    for (const rule of rules) {
      const result = rule(fieldValue.value)
      if (result) {
        error.value = result
        return false
      }
    }
    error.value = null
    return true
  }

  const debouncedValidate = debounce(validate, debounceTime)

  const setValue = (newValue) => {
    fieldValue.value = newValue
    isDirty.value = true
    
    if (validateOnChange) {
      debouncedValidate()
    }
  }

  const setTouched = (value = true) => {
    touched.value = value
    
    if (validateOnBlur && value) {
      validate()
    }
  }

  const reset = (newValue = value) => {
    fieldValue.value = newValue
    error.value = null
    touched.value = false
    isDirty.value = false
  }

  const isValid = computed(() => error.value === null)

  return {
    value: fieldValue,
    error,
    touched,
    isDirty,
    isValid,
    validate,
    setValue,
    setTouched,
    reset
  }
}

export function useFormPersistence(formData, key, options = {}) {
  const {
    storage = localStorage,
    debounceTime = 500,
    exclude = []
  } = options

  const save = debounce(() => {
    try {
      const dataToSave = { ...formData }
      exclude.forEach(field => delete dataToSave[field])
      storage.setItem(key, JSON.stringify(dataToSave))
    } catch (error) {
      console.warn('Failed to save form data:', error)
    }
  }, debounceTime)

  const load = () => {
    try {
      const saved = storage.getItem(key)
      if (saved) {
        const parsedData = JSON.parse(saved)
        Object.assign(formData, parsedData)
      }
    } catch (error) {
      console.warn('Failed to load form data:', error)
    }
  }

  const clear = () => {
    try {
      storage.removeItem(key)
    } catch (error) {
      console.warn('Failed to clear form data:', error)
    }
  }

  watch(formData, save, { deep: true })

  return {
    save,
    load,
    clear
  }
}